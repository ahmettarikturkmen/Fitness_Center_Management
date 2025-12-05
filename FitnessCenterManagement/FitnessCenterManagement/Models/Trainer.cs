using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Trainer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Uzmanlık Alanı")]
        public string Specialty { get; set; } = string.Empty; // Yoga, Fitness, vb.

        // Hangi salonda çalışıyor?
        public int GymId { get; set; }
        public Gym? Gym { get; set; }

        // Randevuları
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
