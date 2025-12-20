using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models.ViewModels
{
    public class AIRequestViewModel
    {
        [Display(Name = "Yaşınız")]
        [Required(ErrorMessage = "Yaş girilmesi zorunludur.")]
        public int Age { get; set; }

        [Display(Name = "Boyunuz (cm)")]
        [Required(ErrorMessage = "Boy girilmesi zorunludur.")]
        public int Height { get; set; }

        [Display(Name = "Kilonuz (kg)")]
        [Required(ErrorMessage = "Kilo girilmesi zorunludur.")]
        public int Weight { get; set; }

        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; }

        [Display(Name = "Hedefiniz")]
        public string Goal { get; set; } 

        [Display(Name = "Aktivite Düzeyi")]
        public string ActivityLevel { get; set; } 

        //  Yapay Zeka'dan gelen Diyet/Antrenman metni
        public string? AIResponse { get; set; }

       
        public string? ImagePrompt { get; set; }

        // 3. Oluşturulan Resmin Linki (Gerekirse kullanılabilir)
        public string? GeneratedImageUrl { get; set; }
    }
}