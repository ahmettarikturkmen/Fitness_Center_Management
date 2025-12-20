using FitnessCenterManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly string _apiKey = "API KEY";
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AIRequestViewModel());
        }

        // DİYET PLANI OLUŞTURMA (Gemini)
        [HttpPost]
        public async Task<IActionResult> GeneratePlan(AIRequestViewModel model)
        {
            if (!ModelState.IsValid) return View("Index", model);

            try
            {
                string prompt = $"Ben {model.Age} yaşında, {model.Gender} cinsiyetinde, {model.Height} cm boyunda ve {model.Weight} kg ağırlığında biriyim. " +
                                $"Günlük aktivite düzeyim: {model.ActivityLevel}. " +
                                $"Fitness hedefim: {model.Goal}. " +
                                $"Bana ÇOK KISA ve ÖZET 1 günlük diyet ve antrenman listesi ver. " +
                                $"Yemekleri ve hareketleri tek satırda özetle. Uzun açıklama yapma. Cevabı Türkçe ver.";

                var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                        model.AIResponse = jsonResponse.candidates[0].content.parts[0].text;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        model.AIResponse = $"HATA: {response.StatusCode} - {error}";
                    }
                }
            }
            catch (Exception ex)
            {
                model.AIResponse = "Hata: " + ex.Message;
            }
            return View("Index", model);
        }

        // RESİM OLUŞTURMA 
        [HttpPost]
        public IActionResult GenerateImage(AIRequestViewModel model)
        {
            try
            {

                string genderEn = model.Gender == "Kadın" ? "female" : "male";

                string myPrompt = $"fitness gym photo of a {genderEn}, body transformation goal: {model.Goal}, athletic body, cinematic lighting, 8k resolution, highly detailed, realistic texture";


                // Prompt'u URL uyumlu hale getir
                string encodedPrompt = Uri.EscapeDataString(myPrompt);

                // Pollinations.ai (Ücretsiz API) URL'si oluşturuluyor
                string imageUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=1024&height=1024&nologo=true&seed={new Random().Next(1000)}";

                return Json(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}