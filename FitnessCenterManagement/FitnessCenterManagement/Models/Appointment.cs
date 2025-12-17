using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public int GymId { get; set; }
        public Gym? Gym { get; set; }

        // Hangi Üye? (Identity tablosuyla ilişki)....
        [Required]
        public string MemberId { get; set; } = string.Empty;
        public ApplicationUser? Member { get; set; }

        [Required(ErrorMessage = "Lütfen bir eğitmen seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir eğitmen seçiniz.")]
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        [Required(ErrorMessage = "Lütfen bir hizmet seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen geçerli bir hizmet seçiniz.")]
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [Display(Name = "Randevu Tarihi")]
        public DateTime AppointmentDate { get; set; } // Tarih ve Saat bir arada tutulabilir

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Onaylandı, İptal
    }
}
