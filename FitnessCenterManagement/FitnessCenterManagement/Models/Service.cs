using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Süre (Dakika)")]
        public int Duration { get; set; } 

        [Display(Name = "Ücret")]
        public decimal Price { get; set; }

        // Hangi salona ait?
        public int GymId { get; set; }
        public Gym? Gym { get; set; }
    }
}
