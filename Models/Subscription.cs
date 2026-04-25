namespace SmartFitGym.Models
{
	public class Subscription
	{
		public int SubscriptionId { get; set; }
		public string UserId { get; set; } = "";
		public int MembershipPlanId { get; set; }
		public DateTime StartDate { get; set; } = DateTime.Now;
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = "Active";
		public decimal AmountPaid { get; set; }
		public string PaymentMethod { get; set; } = "";
		public ApplicationUser? User { get; set; }
		public MembershipPlan? MembershipPlan { get; set; }
	}
}
