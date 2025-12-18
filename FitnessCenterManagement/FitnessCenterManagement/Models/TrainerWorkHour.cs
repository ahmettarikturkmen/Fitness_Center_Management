using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class TrainerWorkHour
    {
        public int Id { get; set; }

        public int TrainerId { get; set; }
        // Navigation Property (İlişki kurmak için)
        public Trainer? Trainer { get; set; }

        // Gün Bilgisi (1: Pazartesi, 2: Salı ... 7: Pazar veya 0: Pazar)
        // C# DayOfWeek enum yapısını kullanacağız (0=Pazar, 1=Pazartesi...)
        public int DayOfWeek { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }
    }
}