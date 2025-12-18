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

        // 1. Randevularım Sayfası (Kullanıcının geçmiş ve gelecek randevuları)
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

        // 2. Randevu Al Sayfası (GET) - Formu Gösterir
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

        // 3. Randevu Kaydetme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment, int gymId, DateTime date, TimeSpan time)
        {
            // 1. Tarih ve Saati Birleştiriyoruz
            appointment.AppointmentDate = date.Date + time;
            appointment.GymId = gymId;

            // 2. Validation Temizliği (Bu alanları formdan beklemiyoruz, kodla dolduruyoruz)
            ModelState.Remove("Member");
            ModelState.Remove("MemberId");
            ModelState.Remove("Gym");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");

            // --- ZORUNLU ALAN KONTROLLERİ ---
            // Eğer dropdown'dan seçim yapılmadıysa ID'ler 0 gelir.
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

            // ============================================================
            // !!! BURASI YENİ: EĞİTMEN MÜSAİTLİK KONTROLÜ (KRİTİK KISIM) !!!
            // ============================================================

            if (appointment.TrainerId != 0) // Eğer eğitmen seçildiyse kontrol et
            {
                // A) EĞİTMEN O GÜN/SAAT ÇALIŞIYOR MU?
                // C#'ta Pazar=0, Pazartesi=1... Veritabanımız da buna uyumlu.
                int gunNo = (int)appointment.AppointmentDate.DayOfWeek;

                // Veritabanına soruyoruz:
                // "Bu hocanın, bu günde, bu saati kapsayan bir çalışma kaydı var mı?"
                var isWorking = await _context.TrainerWorkHours
                    .AnyAsync(w => w.TrainerId == appointment.TrainerId
                                && w.DayOfWeek == gunNo
                                && w.StartTime <= time    // Başlangıç saati randevudan önce veya eşit mi?
                                && w.EndTime > time);     // Bitiş saati randevudan sonra mı?

                // Eğer böyle bir kayıt YOKSA -> Hoca çalışmıyordur.
                if (!isWorking)
                {
                    ModelState.AddModelError("", "Seçtiğiniz eğitmen, talep ettiğiniz gün ve saatte hizmet vermemektedir. Lütfen eğitmenin çalışma saatlerine (Profilinden veya Bilgi kısmından) dikkat ediniz.");
                }

                // B) EĞİTMEN O SAATTE DOLU MU? (ÇAKIŞMA KONTROLÜ)
                // Veritabanına soruyoruz:
                // "Bu hocanın, tam bu tarihte ve saatte, iptal edilmemiş başka bir randevusu var mı?"
                var isBusy = await _context.Appointments
                    .AnyAsync(a => a.TrainerId == appointment.TrainerId
                                && a.AppointmentDate == appointment.AppointmentDate
                                && a.Status != "İptal"       // İptal edilenler engel teşkil etmez
                                && a.Status != "Reddedildi"); // Reddedilenler engel teşkil etmez

                if (isBusy)
                {
                    ModelState.AddModelError("", "Bu saatte seçtiğiniz eğitmenin başka bir üye ile randevusu mevcut. Lütfen farklı bir saat seçiniz.");
                }
            }
            // ============================================================


            // --- KAYIT İŞLEMİ ---
            // Eğer yukarıdaki kontrollerden hiçbiri hata fırlatmadıysa (ModelState.IsValid true ise)
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                appointment.MemberId = userId;
                appointment.Status = "Bekliyor"; // Onay mekanizması için durumu 'Bekliyor' yapıyoruz

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Başarılı olursa listeye gönder
                return RedirectToAction(nameof(Index));
            }

            // --- HATA VARSA SAYFAYI TEKRAR DOLDUR (Dropdownlar boşalmasın) ---
            ViewBag.Services = new SelectList(_context.Services.Where(s => s.GymId == gymId), "Id", "Name");
            ViewBag.Trainers = new SelectList(_context.Trainers.Where(t => t.GymId == gymId), "Id", "FullName");

            if (gym != null)
            {
                ViewBag.GymName = gym.Name;
                ViewBag.OpenHour = gym.OpenHour;
                ViewBag.CloseHour = gym.CloseHour;
            }
            ViewBag.GymId = gymId;

            // Hatalı sayfayı geri döndür (Kırmızı uyarılar çıkacak)
            return View(appointment);
        }
    }
}