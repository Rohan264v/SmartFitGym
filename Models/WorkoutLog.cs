namespace SmartFitGym.Models
{
	public class WorkoutLog
	{
		public int WorkoutLogId { get; set; }
		public string UserId { get; set; } = "";
		public string WorkoutType { get; set; } = "";
		public int DurationMinutes { get; set; }
		public int CaloriesBurned { get; set; }
		public string Notes { get; set; } = "";
		public DateTime LoggedAt { get; set; } = DateTime.Now;
		public ApplicationUser? User { get; set; }
	}
}
