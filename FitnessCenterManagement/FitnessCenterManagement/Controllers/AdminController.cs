using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    // Sadece 'Admin' rolü olanlar buraya girebilir!
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //Admin Ana Sayfası (Dashboard)
        public IActionResult Index()
        {
            return View();
        }

        // GYM (SPOR SALONU) YÖNETİMİ 

        // Salonları Listele
        public async Task<IActionResult> GymList()
        {
            var gyms = await _context.Gyms.ToListAsync();
            return View(gyms);
        }

        // Yeni Salon Ekleme Sayfası (GET)
        [HttpGet]
        public IActionResult CreateGym()
        {
            return View();
        }

        // Yeni Salon Kaydetme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> CreateGym(Gym gym)
        {
            if (ModelState.IsValid)
            {
                _context.Gyms.Add(gym);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GymList));
            }
            return View(gym);
        }

        // --- TRAINER (ANTRENÖR) YÖNETİMİ ---

        //  Antrenörleri Listele
        public async Task<IActionResult> TrainerList()
        {
            // Include(t => t.Gym) diyerek antrenörün salon bilgisini de çekiyoruz
            var trainers = await _context.Trainers.Include(t => t.Gym).ToListAsync();
            return View(trainers);
        }

        //  Yeni Antrenör Ekleme Sayfası (GET)
        [HttpGet]
        public IActionResult CreateTrainer()
        {
            // Dropdown için salonları ViewBag'e yüklüyoruz
            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name");
            return View();
        }

        // Yeni Antrenör Kaydetme (POST)
        [HttpPost]
        public async Task<IActionResult> CreateTrainer(Trainer trainer)
        {

            ModelState.Remove("Gym");

            if (ModelState.IsValid)
            {
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(TrainerList));
            }

            // Hata olursa dropdown boş kalmasın diye tekrar dolduruyoruz
            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name");
            return View(trainer);
        }
        // SERVICE (HİZMET/DERS) YÖNETİMİ 

        // Hizmetleri Listele
        public async Task<IActionResult> ServiceList()
        {
            var services = await _context.Services.Include(s => s.Gym).ToListAsync();
            return View(services);
        }

        //  Yeni Hizmet Ekleme (GET)
        [HttpGet]
        public IActionResult CreateService()
        {
            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name");
            return View();
        }

        //  Yeni Hizmet Kaydetme (POST)
        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
            ModelState.Remove("Gym"); // Validation hatasını önlemek için

            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ServiceList));
            }

            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name");
            return View(service);
        }

        //  GYM (SALON) İŞLEMLERİ

        // GYM DÜZENLEME (GET) - Sayfayı açar ve verileri doldurur
        [HttpGet]
        public async Task<IActionResult> EditGym(int id)
        {
            var gym = await _context.Gyms.FindAsync(id);
            if (gym == null) return NotFound();
            return View(gym);
        }

        // GYM DÜZENLEME (POST) - Değişiklikleri kaydeder
        [HttpPost]
        public async Task<IActionResult> EditGym(Gym gym)
        {
            if (ModelState.IsValid)
            {
                _context.Gyms.Update(gym);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GymList));
            }
            return View(gym);
        }

        // GYM SİLME (POST)
        [HttpPost] // Güvenlik için silme işlemi POST olmalı
        public async Task<IActionResult> DeleteGym(int id)
        {
            var gym = await _context.Gyms.FindAsync(id);
            if (gym != null)
            {
                _context.Gyms.Remove(gym);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(GymList));
        }


        // TRAINER (ANTRENÖR) İŞLEMLERİ 

        // TRAINER DÜZENLEME (GET)
        [HttpGet]
        public async Task<IActionResult> EditTrainer(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return NotFound();

            // Dropdown için salonları tekrar yüklüyoruz
            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            return View(trainer);
        }

        // TRAINER DÜZENLEME (POST)
        [HttpPost]
        public async Task<IActionResult> EditTrainer(Trainer trainer)
        {
            ModelState.Remove("Gym");

            if (ModelState.IsValid)
            {
                _context.Trainers.Update(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(TrainerList));
            }
            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            return View(trainer);
        }

        // TRAINER SİLME
        [HttpPost]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(TrainerList));
        }


        // SERVICE (HİZMET) İŞLEMLERİ 

        // SERVICE DÜZENLEME (GET)
        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name", service.GymId);
            return View(service);
        }

        // SERVICE DÜZENLEME (POST)
        [HttpPost]
        public async Task<IActionResult> EditService(Service service)
        {
            ModelState.Remove("Gym");

            if (ModelState.IsValid)
            {
                _context.Services.Update(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ServiceList));
            }
            ViewBag.Gyms = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Gyms, "Id", "Name", service.GymId);
            return View(service);
        }

        // SERVICE SİLME
        [HttpPost]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ServiceList));
        }

        // Kullanıcı Listesi
        public async Task<IActionResult> UserList()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    BirthDate = user.BirthDate,
                    SelectedRole = roles.FirstOrDefault() ?? "Rol Yok" // İlk rolü al
                });
            }

            return View(userViewModels);
        }

        //  Kullanıcı Düzenle (GET)
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                SelectedRole = roles.FirstOrDefault()
            };

            // Sistemdeki tüm rolleri ViewBag ile gönderelim (Dropdown için)
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

            return View(model);
        }

        //  Kullanıcı Düzenle (POST)
        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            //  Temel Bilgileri Güncelle
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email; 
            if (model.BirthDate.HasValue)
            {
                user.BirthDate = model.BirthDate.Value;
            }

            // Veritabanını güncelle
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Rol Güncelleme İşlemi
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRole = currentRoles.FirstOrDefault();

                // Eğer seçilen rol, mevcut rolden farklıysa değiştir
                if (model.SelectedRole != currentRole)
                {
                    // Eski rolleri sil
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    // Yeni rolü ekle
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }
                }

                return RedirectToAction("UserList");
            }

            // Hata varsa rolleri tekrar yükle ve sayfayı göster
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // Kullanıcı Sil (Opsiyonel ama gerekli olabilir)
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("UserList");
        }

        // RAPORLAMA SAYFASI (VIEW)
        public IActionResult Reports()
        {
            return View();
        }

    }
}