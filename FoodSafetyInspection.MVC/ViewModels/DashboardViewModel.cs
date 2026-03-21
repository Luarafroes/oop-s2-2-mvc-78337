namespace FoodSafetyInspection.MVC.ViewModels
{
    public class DashboardViewModel
    {
        // Counts
        public int InspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueFollowUps { get; set; }

        // Filter values
        public string? FilterTown { get; set; }
        public string? FilterRiskRating { get; set; }

        // Dropdown options
        public List<string> Towns { get; set; } = new();
        public List<string> RiskRatings { get; set; } = new List<string> { "Low", "Medium", "High" };
    }
}