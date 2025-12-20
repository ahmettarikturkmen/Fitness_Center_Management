using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FitnessCenterManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Veritabaný baðlantýsý tanýmý
        private readonly ApplicationDbContext _context;

        // Constructor'da veritabanýný içeri alýyoruz (Dependency Injection)
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanýndan Gym (Salon) tablosunu ve içindeki Hizmetleri çekiyoruz
            var gyms = await _context.Gyms.Include(g => g.Services).ToListAsync();

            // Veriyi View'a (Sayfaya) gönderiyoruz
            return View(gyms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}