using FitnessCenterManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    // Bu etiket, bu sınıfın bir API olduğunu belirtir
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // İSTER: "API üzerinden LINQ sorguları ile filtreleme"
        // ÖRNEK: İki tarih arasındaki randevuları getir
        // ÇAĞRILIŞI: /api/reportsapi/GetAppointments?startDate=2023-01-01&endDate=2023-12-31
        [HttpGet("GetAppointments")]
        public IActionResult GetAppointments(DateTime startDate, DateTime endDate)
        {
            // --- LINQ SORGUSU BURADA ---
            var reportData = _context.Appointments
                // 1. Filtreleme (Where)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                // 2. Sıralama (OrderBy)
                .OrderBy(a => a.AppointmentDate)
                // 3. Seçim (Select) - Sadece lazım olan veriyi alıyoruz (Projeksiyon)
                // Bunu yapmazsak "Döngüsel Başvuru" hatası alabiliriz.
                .Select(a => new
                {
                    Tarih = a.AppointmentDate.ToString("dd.MM.yyyy HH:mm"),
                    UyeAdi = a.Member.FullName,
                    HocaAdi = a.Trainer.FullName,
                    Hizmet = a.Service.Name,
                    Durum = a.Status
                })
                .ToList();

            return Ok(reportData); // Veriyi JSON olarak döndürür
        }
    }
}