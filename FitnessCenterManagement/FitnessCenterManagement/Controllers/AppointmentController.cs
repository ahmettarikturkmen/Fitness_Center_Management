using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    // Sadece giriş yapmış üyeler randevu alabilir
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Randevularım Sayfası (Kullanıcının geçmiş ve gelecek randevuları)
        public async Task<IActionResult> Index()
        {
            // Şu anki giriş yapmış kullanıcının ID'sini bul
            var userId = _userManager.GetUserId(User);

            // Sadece bu kullanıcıya ait randevuları getir
            var appointments = await _context.Appointments
                .Include(a => a.Gym)      // Salon bilgisini getir
                .Include(a => a.Trainer)  // Hoca bilgisini getir
                .Include(a => a.Service)  // Ders bilgisini getir
                .Where(a => a.MemberId == userId) // Filtrele
                .OrderByDescending(a => a.AppointmentDate) // En yeniler üstte
                .ToListAsync();

            return View(appointments);
        }

        //  Randevu Al Sayfası (GET) - Formu Gösterir
        [HttpGet]
        public async Task<IActionResult> Create(int? gymId)
        {
            if (gymId == null) return RedirectToAction("Index", "Home");

            // Seçilen salonun bilgilerini alalım (Açılış/Kapanış saatleri için lazım olacak)
            var gym = await _context.Gyms.FindAsync(gymId);
            if (gym == null) return NotFound();

            // Sadece O Salona ait Hizmetleri ve Hocaları Getir
            var services = _context.Services.Where(s => s.GymId == gymId).ToList();
            var trainers = _context.Trainers.Where(t => t.GymId == gymId).ToList();

            // Dropdownlar için ViewBag'e atıyoruz
            ViewBag.Services = new SelectList(services, "Id", "Name");
            ViewBag.Trainers = new SelectList(trainers, "Id", "FullName");

            // Hangi salona randevu alındığını sayfaya taşıyalım
            ViewBag.GymName = gym.Name;
            ViewBag.GymId = gym.Id;

            // Salonun çalışma saatlerini de bilgi olarak gönderelim
            ViewBag.OpenHour = gym.OpenHour;
            ViewBag.CloseHour = gym.CloseHour;

            return View();
        }

        //  Randevu Kaydetme İşlemi (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment, int gymId, DateTime date, TimeSpan time)
        {
            // Tarih ve Saati Birleştiriyoruz
            appointment.AppointmentDate = date.Date + time;
            appointment.GymId = gymId;

            // Validation Temizliği (Bu alanları formdan beklemiyoruz, kodla dolduruyoruz)
            ModelState.Remove("Member");
            ModelState.Remove("MemberId");
            ModelState.Remove("Gym");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");

            // --- ZORUNLU ALAN KONTROLLERİ ---
            if (appointment.TrainerId == 0) ModelState.AddModelError("TrainerId", "Lütfen bir eğitmen seçiniz.");
            if (appointment.ServiceId == 0) ModelState.AddModelError("ServiceId", "Lütfen bir hizmet seçiniz.");

            // --- KONTROL 1: GEÇMİŞ ZAMAN ---
            if (appointment.AppointmentDate < DateTime.Now)
            {
                ModelState.AddModelError("", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            // --- KONTROL 2: SALON AÇIK MI? ---
            var gym = await _context.Gyms.FindAsync(gymId);
            if (gym != null && (time < gym.OpenHour || time >= gym.CloseHour))
            {
                ModelState.AddModelError("", $"Salon {gym.OpenHour} - {gym.CloseHour} saatleri arasında açıktır.");
            }

            if (appointment.TrainerId != 0 && appointment.ServiceId != 0)
            {
                //  HİZMET SÜRESİNİ BULALIM
                var selectedService = await _context.Services.FindAsync(appointment.ServiceId);
                if (selectedService != null)
                {
                    // Yeni Randevunun Başlangıç ve Bitiş Zamanını Hesapla
                    DateTime newStart = appointment.AppointmentDate;
                    DateTime newEnd = newStart.AddMinutes(selectedService.Duration); // Örn: 09:00 + 90dk = 10:30

                    // EĞİTMEN O GÜN ÇALIŞIYOR MU? (Vardiya Kontrolü)
                    int gunNo = (int)newStart.DayOfWeek;
                    var isWorking = await _context.TrainerWorkHours
                        .AnyAsync(w => w.TrainerId == appointment.TrainerId
                                    && w.DayOfWeek == gunNo
                                    && w.StartTime <= time
                                    && w.EndTime > time); 

                    if (!isWorking)
                    {
                        
                        ModelState.AddModelError("", "Seçtiğiniz eğitmen, talep ettiğiniz gün ve saatte hizmet vermemektedir. Lütfen eğitmenin çalışma saatlerine dikkat ediniz.");
                    }
                    else
                    {
                        //  ÇAKIŞMA KONTROLÜ 
                        // Sadece o günkü randevuları çekelim (Performans için)
                        var existingAppointments = await _context.Appointments
                            .Include(a => a.Service) 
                            .Where(a => a.TrainerId == appointment.TrainerId
                                        && a.AppointmentDate.Date == newStart.Date 
                                        && a.Status != "İptal"
                                        && a.Status != "Reddedildi")
                            .ToListAsync();

                        bool hasOverlap = false;
                        foreach (var existing in existingAppointments)
                        {
                            // Mevcut randevunun başlangıç ve bitişi
                            DateTime existingStart = existing.AppointmentDate;
                            // Eğer veritabanında eski kayıtlarda service null ise varsayılan 60dk ekle (hata almamak için)
                            int existingDuration = existing.Service != null ? existing.Service.Duration : 60;
                            DateTime existingEnd = existingStart.AddMinutes(existingDuration);

                            // ÇAKIŞMA FORMÜLÜ: (StartA < EndB) && (EndA > StartB)
                            // Bu formül; içine almayı, üstüne binmeyi, kesişmeyi yakalar.
                            if (newStart < existingEnd && newEnd > existingStart)
                            {
                                hasOverlap = true;
                                break;
                            }
                        }

                        if (hasOverlap)
                        {
                            ModelState.AddModelError("", $"Seçtiğiniz saat aralığı ({newStart:HH:mm} - {newEnd:HH:mm}) dolu. Eğitmenin mevcut bir randevusu ile çakışıyor.");
                        }
                    }
                }
            }
            // ============================================================


            // --- KAYIT İŞLEMİ ---
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                appointment.MemberId = userId;
                appointment.Status = "Bekliyor";

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // --- HATA VARSA SAYFAYI TEKRAR DOLDUR ---
            ViewBag.Services = new SelectList(_context.Services.Where(s => s.GymId == gymId), "Id", "Name", appointment.ServiceId);
            ViewBag.Trainers = new SelectList(_context.Trainers.Where(t => t.GymId == gymId), "Id", "FullName", appointment.TrainerId);

            if (gym != null)
            {
                ViewBag.GymName = gym.Name;
                ViewBag.OpenHour = gym.OpenHour;
                ViewBag.CloseHour = gym.CloseHour;
            }
            ViewBag.GymId = gymId;

            return View(appointment);
        }

        [HttpGet]
        public IActionResult GetTrainerWorkHours(int trainerId)
        {
            var hours = _context.TrainerWorkHours
                .Where(x => x.TrainerId == trainerId)
                .Select(x => new
                {
                    day = x.DayOfWeek,
                    start = x.StartTime.ToString(@"hh\:mm"),
                    end = x.EndTime.ToString(@"hh\:mm")
                })
                .ToList();

            return Json(hours);
        }
    }
}