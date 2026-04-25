namespace SmartFitGym.Models
{
	public class ContactMessage
	{
		public int ContactMessageId { get; set; }
		public string FullName { get; set; } = "";
		public string Email { get; set; } = "";
		public string Subject { get; set; } = "";
		public string Message { get; set; } = "";
		public DateTime SentAt { get; set; } = DateTime.Now;
		public bool IsRead { get; set; } = false;
	}
}