namespace SmartFitGym.Models
{
	public class Attendance
	{
		public int AttendanceId { get; set; }
		public string UserId { get; set; } = "";
		public DateTime CheckIn { get; set; } = DateTime.Now;
		public DateTime? CheckOut { get; set; }
		public ApplicationUser? User { get; set; }
	}
}
