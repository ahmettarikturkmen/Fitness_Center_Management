using FitnessCenterManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace FitnessCenterManagement.Controllers
{
    [Authorize] // Sadece üye olanlar girebilsin
    public class AIController : Controller
    {
        // Google Gemini API Ayarları
        private readonly string _apiKey = "API KEY"; // <-- SENİN API KEY'İN BURAYA!
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        [HttpGet]
        public IActionResult Index()
        {
            // Sayfa ilk açıldığında boş bir model gönderiyoruz
            return View(new AIRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlan(AIRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // 1. Yapay Zeka'ya gidecek mesajı (Prompt) hazırlayalım
                string prompt = $"Ben {model.Age} yaşında, {model.Gender} cinsiyetinde, {model.Height} cm boyunda ve {model.Weight} kg ağırlığında biriyim. " +
                                $"Günlük aktivite düzeyim: {model.ActivityLevel}. " +
                                $"Fitness hedefim: {model.Goal}. " +
                                $"Bana ÇOK KISA ve ÖZET 1 günlük diyet ve antrenman listesi ver. " +
                                $"Yemekleri ve hareketleri tek satırda özetle. Uzun açıklama yapma.Cevabı Türkçe ver."; ;


                // 2. Google Gemini için JSON verisini hazırla
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 3. API'ye İsteği Gönder
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        // Gelen JSON cevabını parçala
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

                        // Gemini cevabı şurada saklıyor: candidates[0].content.parts[0].text
                        string advice = jsonResponse.candidates[0].content.parts[0].text;

                        // Cevabı modele ekle
                        model.AIResponse = advice;
                    }
                    else
                    {
                        // Hatayı detaylı görelim ki sorunu çözelim
                        var errorContent = await response.Content.ReadAsStringAsync();
                        model.AIResponse = $"HATA OLUŞTU! Hata Kodu: {response.StatusCode}. Detay: {errorContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                model.AIResponse = "Bir hata oluştu: " + ex.Message;
            }

            // Sonuçlarla birlikte sayfayı tekrar göster
            return View("Index", model);
        }
    }
}