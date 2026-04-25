using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFitGym.Models;

namespace SmartFitGym.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Dashboard Statistics
            ViewBag.TotalMembers = await _userManager.GetUsersInRoleAsync("Member");
            ViewBag.ActiveSubscriptions = await _context.Subscriptions.CountAsync(s => s.Status == "Active");
            ViewBag.TotalRevenue = await _context.Subscriptions.SumAsync(s => s.AmountPaid);
            ViewBag.RecentWorkouts = await _context.WorkoutLogs.CountAsync(w => w.LoggedAt >= DateTime.Now.AddDays(-7));

            var recentSignups = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentSignups);
        }

        public async Task<IActionResult> ManageMembers()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");
            
            var memberList = new List<dynamic>();
            foreach(var user in members)
            {
                var profile = await _context.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                var sub = await _context.Subscriptions
                    .Include(s => s.MembershipPlan)
                    .Where(s => s.UserId == user.Id && s.Status == "Active")
                    .FirstOrDefaultAsync();

                memberList.Add(new {
                    User = user,
                    Profile = profile,
                    Subscription = sub
                });
            }

            return View(memberList);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "Member deleted successfully.";
            }
            return RedirectToAction("ManageMembers");
        }
    }
}
