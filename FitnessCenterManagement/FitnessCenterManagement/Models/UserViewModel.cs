using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-Posta zorunludur")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        // Kullanıcının şu anki rolü
        [Display(Name = "Kullanıcı Rolü")]
        public string SelectedRole { get; set; }
    }
}