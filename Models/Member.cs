using Microsoft.AspNetCore.Identity;

namespace SmartFitGym.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string FullName { get; set; } = "";
		public string? ProfilePhoto { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
