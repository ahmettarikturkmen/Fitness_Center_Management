using FitnessCenterManagement.Models;
using FitnessCenterManagement.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers
{
    public class AccountController : Controller
    {
        // Identity servislerini tanımlıyoruz
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Constructor (Yapıcı Metot): Servisleri içeri alıyoruz
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- LOGIN (GİRİŞ YAP) ---
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer kullanıcı zaten giriş yapmışsa direkt anasayfaya atalım
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Kullanıcıyı emailden bul
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // 2. Şifreyi kontrol et
                // (son parametre 'false': şifre yanlışsa hesabı kilitleme demek)
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Email veya şifre hatalı.");
            return View(model);
        }

        // --- REGISTER (KAYIT OL) ---
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Yeni kullanıcı nesnesi oluşturuyoruz
            var user = new ApplicationUser
            {
                UserName = model.Email, // Kullanıcı adı olarak Email kullanıyoruz
                Email = model.Email,
                FullName = model.FullName,
                BirthDate = model.BirthDate,
                EmailConfirmed = true // Şimdilik email onayı istemiyoruz
            };

            // Şifreyi hashleyip kaydediyoruz
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Yeni kayıt olanlara varsayılan olarak "Member" rolü verelim
                await _userManager.AddToRoleAsync(user, "Member");

                // Kayıt bittikten sonra otomatik giriş yapsın
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Hata varsa (örn: şifre çok basitse) ekrana yazdır
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // --- LOGOUT (ÇIKIŞ YAP) ---
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Yetkisiz giriş sayfası
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}