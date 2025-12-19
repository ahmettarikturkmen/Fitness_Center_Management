using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class UserProfileViewModel
    {
        // --- GÖRÜNTÜLENECEK BİLGİLER ---
        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "Ad Soyad boş bırakılamaz.")]
        public string FullName { get; set; }

        [Display(Name = "E-Posta")]
        public string Email { get; set; }

        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        // --- ŞİFRE DEĞİŞTİRME ALANLARI ---

        // NOT: View sayfasındaki tasarıma uyması için isimleri değiştirdik.
        // Ayrıca [Required] kaldırdık ki sadece isim güncellerken şifre sormasın.

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; } // Soru işareti (?) boş olabilir demek

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor!")]
        public string? ConfirmPassword { get; set; } // İsmi düzelttik: ConfirmNewPassword -> ConfirmPassword
    }
}