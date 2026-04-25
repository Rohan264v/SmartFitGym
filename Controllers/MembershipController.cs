using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFitGym.Models;

namespace SmartFitGym.Controllers
{
    public class MembershipController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembershipController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var plans = await _context.MembershipPlans.Where(p => p.IsActive).ToListAsync();
            
            // Seed default plans if Free plan doesn't exist
            if (!plans.Any(p => p.PlanName == "Free"))
            {
                // Remove old plans to keep it clean
                _context.MembershipPlans.RemoveRange(plans);
                await _context.SaveChangesAsync();

                _context.MembershipPlans.AddRange(
                    new MembershipPlan { PlanName = "Free", Price = 0m, DurationMonths = 1, Features = "Gym Access (Off-Peak)", IsActive = true, Description = "Get a feel of the gym." },
                    new MembershipPlan { PlanName = "Basic", Price = 999m, DurationMonths = 1, Features = "Full Gym Access,Locker Room", IsActive = true, Description = "Perfect for beginners." },
                    new MembershipPlan { PlanName = "Pro", Price = 1999m, DurationMonths = 1, Features = "Full Gym Access,Locker Room,Group Classes,AI Workouts", IsActive = true, Description = "Most popular choice." },
                    new MembershipPlan { PlanName = "Elite", Price = 3999m, DurationMonths = 1, Features = "All Pro Features,Personal Trainer,Spa Access,Nutrition Plan", IsActive = true, Description = "The ultimate fitness experience." }
                );
                await _context.SaveChangesAsync();
                plans = await _context.MembershipPlans.Where(p => p.IsActive).ToListAsync();
            }

            return View(plans);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Subscribe(int id)
        {
            var plan = await _context.MembershipPlans.FindAsync(id);
            if (plan == null) return NotFound();

            if (plan.Price == 0)
            {
                var user = await _userManager.GetUserAsync(User);

                // Deactivate any existing active subscriptions
                var existingSubs = await _context.Subscriptions
                    .Where(s => s.UserId == user!.Id && s.Status == "Active")
                    .ToListAsync();
                
                foreach(var sub in existingSubs)
                {
                    sub.Status = "Cancelled";
                }

                var subscription = new Subscription
                {
                    UserId = user!.Id,
                    MembershipPlanId = plan.MembershipPlanId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(plan.DurationMonths),
                    Status = "Active",
                    AmountPaid = 0,
                    PaymentMethod = "Free"
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully subscribed to the {plan.PlanName} plan!";
                return RedirectToAction("Dashboard", "Member");
            }

            return View(plan);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmSubscription(int planId, string paymentMethod)
        {
            var plan = await _context.MembershipPlans.FindAsync(planId);
            if (plan == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // Deactivate any existing active subscriptions
            var existingSubs = await _context.Subscriptions
                .Where(s => s.UserId == user!.Id && s.Status == "Active")
                .ToListAsync();
            
            foreach(var sub in existingSubs)
            {
                sub.Status = "Cancelled";
            }

            var subscription = new Subscription
            {
                UserId = user!.Id,
                MembershipPlanId = plan.MembershipPlanId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(plan.DurationMonths),
                Status = "Active",
                AmountPaid = plan.Price,
                PaymentMethod = paymentMethod
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully subscribed to the {plan.PlanName} plan!";
            return RedirectToAction("Dashboard", "Member");
        }
    }
}
