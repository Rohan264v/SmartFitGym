using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFitGym.Models;

namespace SmartFitGym.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            
            // If profile is missing, redirect to create it
            if (profile == null)
            {
                return RedirectToAction("Profile");
            }

            var workouts = await _context.WorkoutLogs
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.LoggedAt)
                .Take(5)
                .ToListAsync();

            var subscription = await _context.Subscriptions
                .Include(s => s.MembershipPlan)
                .Where(s => s.UserId == user.Id && s.Status == "Active")
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            ViewBag.Profile = profile;
            ViewBag.Subscription = subscription;
            ViewBag.Workouts = workouts;

            // Calculate BMI
            if (profile.Height > 0)
            {
                double heightInMeters = profile.Height / 100.0;
                ViewBag.BMI = Math.Round(profile.Weight / (heightInMeters * heightInMeters), 1);
            }
            else
            {
                ViewBag.BMI = 0;
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user!.Id);

            if (profile == null)
            {
                profile = new MemberProfile { UserId = user!.Id, FullName = user.FullName };
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(MemberProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.UserId = user!.Id;

            if (ModelState.IsValid)
            {
                var existingProfile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                
                if (existingProfile == null)
                {
                    _context.MemberProfiles.Add(model);
                }
                else
                {
                    existingProfile.FullName = model.FullName;
                    existingProfile.Phone = model.Phone;
                    existingProfile.Gender = model.Gender;
                    existingProfile.DateOfBirth = model.DateOfBirth;
                    existingProfile.Weight = model.Weight;
                    existingProfile.Height = model.Height;
                    existingProfile.BloodGroup = model.BloodGroup;
                    existingProfile.EmergencyContact = model.EmergencyContact;
                    _context.MemberProfiles.Update(existingProfile);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult LogWorkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogWorkout(WorkoutLog model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.UserId = user!.Id;
            model.LoggedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.WorkoutLogs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Workout logged successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }
    }
}
