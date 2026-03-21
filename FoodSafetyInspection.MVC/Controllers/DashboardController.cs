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

            // Base query for inspections - apply filters if provided
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

            // Overdue follow-ups (DueDate past + still Open)
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

            var viewModel = new DashboardViewModel
            {
                InspectionsThisMonth = inspectionsThisMonth,
                FailedInspectionsThisMonth = failedThisMonth,
                OverdueFollowUps = overdueCount,
                FilterTown = filterTown,
                FilterRiskRating = filterRiskRating,
                Towns = towns
            };

            return View(viewModel);
        }
    }
}