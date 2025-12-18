using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    // Sadece 'Trainer' rolüne sahip olanlar buraya girebilir
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ANTRENÖR PANELİ (DASHBOARD)
        public async Task<IActionResult> Index()
        {
            // 1. Giriş yapan kullanıcının emailini al
            var userEmail = User.Identity.Name;

            // 2. Bu email hangi Antrenöre ait? (Trainer tablosunda ara)
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Email == userEmail);

            if (trainer == null)
            {
                // Eğer email eşleşmezse hata ver (Admin email girmeyi unutmuş olabilir)
                return View("Error", new { message = "Antrenör kaydınız bulunamadı. Lütfen yöneticiyle iletişime geçin." });
            }

            // 3. Sadece BU antrenöre ait randevuları getir
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Where(a => a.TrainerId == trainer.Id) // Filtreleme burada!
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // ONAYLA
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Onaylandı";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // REDDET
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Reddedildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ÇALIŞMA SAATLERİ YÖNETİMİ
        // ==========================================

        // 1. Saatleri Listeleme Sayfası
        public async Task<IActionResult> WorkHours()
        {
            var userEmail = User.Identity.Name;
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (trainer == null) return RedirectToAction("Index");

            // Hocanın eklediği saatleri getir ve güne göre sırala
            var hours = await _context.TrainerWorkHours
                .Where(w => w.TrainerId == trainer.Id)
                .OrderBy(w => w.DayOfWeek)
                .ThenBy(w => w.StartTime)
                .ToListAsync();

            return View(hours);
        }

        // 2. Yeni Saat Ekleme (POST)
        [HttpPost]
        public async Task<IActionResult> AddWorkHour(int dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var userEmail = User.Identity.Name;
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (trainer == null) return RedirectToAction("Index");

            if (startTime >= endTime)
            {
                TempData["Error"] = "Başlangıç saati bitiş saatinden büyük olamaz.";
                return RedirectToAction("WorkHours");
            }

            var newHour = new TrainerWorkHour
            {
                TrainerId = trainer.Id,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            };

            _context.TrainerWorkHours.Add(newHour);
            await _context.SaveChangesAsync();

            return RedirectToAction("WorkHours");
        }

        // 3. Saat Silme (POST)
        [HttpPost]
        public async Task<IActionResult> DeleteWorkHour(int id)
        {
            var workHour = await _context.TrainerWorkHours.FindAsync(id);
            if (workHour != null)
            {
                _context.TrainerWorkHours.Remove(workHour);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("WorkHours");
        }
    }
}