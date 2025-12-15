using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Gym
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Salon Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Açılış Saati")]
        public TimeSpan OpenHour { get; set; }

        [Display(Name = "Kapanış Saati")]
        public TimeSpan CloseHour { get; set; }

        // İliskiler
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}

