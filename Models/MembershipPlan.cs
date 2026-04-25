namespace SmartFitGym.Models
{
	public class MembershipPlan
	{
		public int MembershipPlanId { get; set; }
		public string PlanName { get; set; } = "";
		public string Description { get; set; } = "";
		public decimal Price { get; set; }
		public int DurationMonths { get; set; }
		public string Features { get; set; } = "";
		public bool IsActive { get; set; } = true;
	}
}