using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SmartFitGym.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<MemberProfile> MemberProfiles { get; set; }
		public DbSet<Trainer> Trainers { get; set; }
		public DbSet<MembershipPlan> MembershipPlans { get; set; }
		public DbSet<Subscription> Subscriptions { get; set; }
		public DbSet<WorkoutLog> WorkoutLogs { get; set; }
		public DbSet<Attendance> Attendances { get; set; }
		public DbSet<ContactMessage> ContactMessages { get; set; }
	}
}