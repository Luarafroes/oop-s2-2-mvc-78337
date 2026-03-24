using FoodSafetyInspection.MVC.Data;
using FoodSafetyInspection.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? filterTown, string? filterRiskRating)
        {
            var now = DateTime.Today;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Base query for inspections
            var inspectionsQuery = _context.Inspections
                .Include(i => i.Premises)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterTown))
                inspectionsQuery = inspectionsQuery
                    .Where(i => i.Premises.Town == filterTown);

            if (!string.IsNullOrEmpty(filterRiskRating))
                inspectionsQuery = inspectionsQuery
                    .Where(i => i.Premises.RiskRating == filterRiskRating);

            // Count inspections this month
            var inspectionsThisMonth = await inspectionsQuery
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            // Count failed inspections this month
            var failedThisMonth = await inspectionsQuery
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail")
                .CountAsync();

            // Overdue follow-ups
            var overdueFollowUpsQuery = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .Where(f => f.Status == "Open" && f.DueDate < now)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterTown))
                overdueFollowUpsQuery = overdueFollowUpsQuery
                    .Where(f => f.Inspection.Premises.Town == filterTown);

            if (!string.IsNullOrEmpty(filterRiskRating))
                overdueFollowUpsQuery = overdueFollowUpsQuery
                    .Where(f => f.Inspection.Premises.RiskRating == filterRiskRating);

            var overdueCount = await overdueFollowUpsQuery.CountAsync();

            // Get distinct towns for dropdown
            var towns = await _context.Premises
                .Select(p => p.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            // Build last 6 months chart data
            var chartLabels = new List<string>();
            var chartPass = new List<int>();
            var chartFail = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);
                var monthName = monthStart.ToString("MMM yyyy");

                var monthQuery = _context.Inspections
                    .Include(x => x.Premises)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filterTown))
                    monthQuery = monthQuery.Where(x => x.Premises.Town == filterTown);

                if (!string.IsNullOrEmpty(filterRiskRating))
                    monthQuery = monthQuery.Where(x => x.Premises.RiskRating == filterRiskRating);

                var passCount = await monthQuery
                    .Where(x => x.InspectionDate >= monthStart && x.InspectionDate < monthEnd && x.Outcome == "Pass")
                    .CountAsync();

                var failCount = await monthQuery
                    .Where(x => x.InspectionDate >= monthStart && x.InspectionDate < monthEnd && x.Outcome == "Fail")
                    .CountAsync();

                chartLabels.Add(monthName);
                chartPass.Add(passCount);
                chartFail.Add(failCount);
            }

            // Build view model
            var viewModel = new DashboardViewModel
            {
                InspectionsThisMonth = inspectionsThisMonth,
                FailedInspectionsThisMonth = failedThisMonth,
                OverdueFollowUps = overdueCount,
                FilterTown = filterTown,
                FilterRiskRating = filterRiskRating,
                Towns = towns,
                ChartLabels = chartLabels,
                ChartPass = chartPass,
                ChartFail = chartFail
            };

            return View(viewModel);
        }
    }
}
