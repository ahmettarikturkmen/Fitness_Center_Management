using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Doðum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doðum Tarihi")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Þifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Þifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Þifre Tekrar")]
        [Compare("Password", ErrorMessage = "Þifreler uyuþmuyor.")]
        public string ConfirmPassword { get; set; }
    }
}