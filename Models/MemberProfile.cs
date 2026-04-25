namespace SmartFitGym.Models
{
	public class MemberProfile
	{
		public int MemberProfileId { get; set; }
		public string UserId { get; set; } = "";
		public string FullName { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Gender { get; set; } = "";
		public DateTime DateOfBirth { get; set; }
		public double Weight { get; set; }
		public double Height { get; set; }
		public string BloodGroup { get; set; } = "";
		public string EmergencyContact { get; set; } = "";
		public DateTime JoinedAt { get; set; } = DateTime.Now;
		public ApplicationUser? User { get; set; }
	}
}
