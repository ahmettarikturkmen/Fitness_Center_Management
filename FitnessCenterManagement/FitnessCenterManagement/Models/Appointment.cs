using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        // Hangi Üye? (Identity tablosuyla ilişki)
        [Required]
        public string MemberId { get; set; } = string.Empty;
        public ApplicationUser? Member { get; set; }

        // Hangi Hoca?
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        // Hangi Ders?
        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [Display(Name = "Randevu Tarihi")]
        public DateTime AppointmentDate { get; set; } // Tarih ve Saat bir arada tutulabilir

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Onaylandı, İptal
    }
}
