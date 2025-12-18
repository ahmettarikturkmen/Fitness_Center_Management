using FitnessCenterManagement.Models;
using FitnessCenterManagement.Models.ViewModels; // Login/Register ViewModel'leri burada
using FitnessCenterManagement.Models; // UserProfileViewModel burada olabilir (Namespace kontrolü yap)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ==========================================
        // MEVCUT KODLARIN (LOGIN / REGISTER / LOGOUT)
        // ==========================================

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded) return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email veya şifre hatalı.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                BirthDate = model.BirthDate,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // ==========================================
        // !!! YENİ EKLENEN KISIMLAR (PROFİL & ŞİFRE) !!!
        // ==========================================

        // [Authorize]: Sadece giriş yapmış kullanıcılar burayı görebilir
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Model validasyonu (Sadece şifre alanlarını kontrol etsek yeterli)
            if (!string.IsNullOrEmpty(model.CurrentPassword) &&
                !string.IsNullOrEmpty(model.NewPassword) &&
                !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                // Identity kütüphanesinin şifre değiştirme fonksiyonu
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    // Şifre değişince oturumun düşmemesi için tazeleyelim
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi!";

                    // Başarılı olursa sayfayı yeniden yükle (Mesajı görsün)
                    return RedirectToAction("Profile");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            // Hata varsa bilgileri tekrar doldurup sayfayı geri gönder
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.BirthDate = user.BirthDate;

            return View("Profile", model);
        }

        // ==========================================
        // YENİ EKLENEN: KİŞİSEL BİLGİ GÜNCELLEME
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateInfo(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Şifre alanları boş geleceği için Validation hatası verebilir.
            // Bu formda şifre değiştirmiyoruz, o yüzden o hataları temizliyoruz.
            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            if (ModelState.IsValid)
            {
                // Bilgileri güncelle
                user.FullName = model.FullName;
                if (model.BirthDate.HasValue)
                {
                    user.BirthDate = model.BirthDate.Value;
                }

                // Veritabanına kaydet
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // İsim değiştiği için üst menüdeki "Merhaba [İsim]" yazısının da 
                    // hemen güncellenmesi için oturumu tazelememiz lazım.
                    await _signInManager.RefreshSignInAsync(user);

                    TempData["SuccessMessage"] = "Profil bilgileriniz güncellendi.";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Hata varsa sayfayı tekrar göster (Email kaybolmasın diye tekrar dolduruyoruz)
            model.Email = user.Email;
            return View("Profile", model);
        }
    }
}