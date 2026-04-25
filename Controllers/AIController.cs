using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFitGym.Models;
using System.Text;

namespace SmartFitGym.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AIController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Recommend()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user!.Id);
            
            ViewBag.Profile = profile;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetRecommendation(string goal, string fitnessLevel, string equipment)
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user!.Id);

            string apiKey = _configuration["GeminiAPI:ApiKey"] ?? "";
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                // Fallback dummy response if API key is not set
                ViewBag.Recommendation = "<h2>API Key Missing</h2><p>Please configure the Gemini API key in appsettings.json to see real AI recommendations.</p><p>Based on your goal to <b>" + goal + "</b>, you should focus on consistent training 3-4 times a week using " + equipment + ".</p>";
                return View("Recommend");
            }

            string userDetails = profile != null ? $" Age: {(DateTime.Now.Year - profile.DateOfBirth.Year)}, Gender: {profile.Gender}, Weight: {profile.Weight}kg, Height: {profile.Height}cm." : "";
            
            string prompt = $"Act as a professional fitness trainer. Create a short, bulleted 3-day workout plan for a user with the following details:{userDetails} Goal: {goal}. Fitness Level: {fitnessLevel}. Equipment available: {equipment}. Format the output using HTML tags for styling (e.g., <h4>, <ul>, <li>, <strong>). Keep it concise.";

            try
            {
                using var client = new HttpClient();
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseString)!;
                    string recommendationText = jsonResponse.candidates[0].content.parts[0].text;
                    ViewBag.Recommendation = recommendationText;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Recommendation = $"<div class='alert alert-danger'><strong>AI Service Error ({response.StatusCode}):</strong><br/> <small>{errorContent}</small></div>";
                }
            }
            catch (Exception)
            {
                ViewBag.Recommendation = "<p class='text-danger'>An error occurred while generating your workout. Please try again later.</p>";
            }

            return View("Recommend");
        }
    }
}
