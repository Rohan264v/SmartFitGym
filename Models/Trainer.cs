namespace SmartFitGym.Models
{
	public class Trainer
	{
		public int TrainerId { get; set; }
		public string FullName { get; set; } = "";
		public string Specialization { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Email { get; set; } = "";
		public string? PhotoUrl { get; set; }
		public int ExperienceYears { get; set; }
		public double Rating { get; set; }
		public bool IsAvailable { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
