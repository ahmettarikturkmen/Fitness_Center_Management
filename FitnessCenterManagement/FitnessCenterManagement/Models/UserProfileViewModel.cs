using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class UserProfileViewModel
    {
        // --- GÖRÜNTÜLENECEK BİLGİLER ---
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Display(Name = "E-Posta")]
        public string Email { get; set; }

        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        // --- ŞİFRE DEĞİŞTİRME ALANLARI ---

        // Güvenlik gereği eski şifreyi de istemeliyiz
        [Required(ErrorMessage = "Mevcut şifrenizi girmelisiniz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifreyi tekrar giriniz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor!")]
        public string ConfirmNewPassword { get; set; }
    }
}