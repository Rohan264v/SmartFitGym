using Microsoft.AspNetCore.Mvc;
using SmartFitGym.Models;
using System.Diagnostics;

namespace SmartFitGym.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                model.SentAt = DateTime.Now;
                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your message has been sent successfully. We will get back to you soon!";
                return RedirectToAction("Contact");
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
