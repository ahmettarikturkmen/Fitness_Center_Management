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

            // Sadece şifre değiştirdiğimiz için Ad ve Email kontrollerini devre dışı bırakıyoruz
            ModelState.Remove("FullName");
            ModelState.Remove("Email");

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Lütfen yeni şifrenizi girin.");
            }

            if (ModelState.IsValid)
            {
                // ESKİ KODDA: ChangePasswordAsync vardı (Eski şifre istiyordu).
                // YENİ KODDA: RemovePassword + AddPassword yapıyoruz (Eski şifreye gerek yok).

                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (removeResult.Succeeded)
                {
                    var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (addResult.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        foreach (var error in addResult.Errors) ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    foreach (var error in removeResult.Errors) ModelState.AddModelError("", error.Description);
                }
            }

            // Hata varsa formu tekrar doldur
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.BirthDate = user.BirthDate;

            return View("Profile", model);
        }

        // ==========================================
        // YENİ EKLENEN: KİŞİSEL BİLGİ GÜNCELLEME
        // ==========================================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Profil güncellerken şifre alanlarını kontrol etme (Hata vermemesi için siliyoruz)
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword"); // Senin kodda ConfirmNewPassword yazıyordu, doğrusu bu.

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                // Eğer tarih girilmişse güncelle, girilmemişse (null ise) dokunma
                if (model.BirthDate.HasValue)
                {
                    user.BirthDate = model.BirthDate.Value;
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Profil bilgileriniz güncellendi.";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            model.Email = user.Email;
            return View("Profile", model);
        }
    }
}