using FoodSafetyInspection.Domain;
using FoodSafetyInspection.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.MVC.Controllers
{
    [Authorize]
    public class FollowUpsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(AppDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? searchStatus, bool overdueOnly = false)
        {
            var query = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchStatus))
                query = query.Where(f => f.Status == searchStatus);

            if (overdueOnly)
                query = query.Where(f => f.Status == "Open" && f.DueDate < DateTime.Today);

            ViewBag.SearchStatus = searchStatus;
            ViewBag.OverdueOnly = overdueOnly;

            var followUps = await query.ToListAsync();
            return View(followUps);
        }

        public async Task<IActionResult> Details(int id)
        {
            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (followUp == null)
            {
                _logger.LogWarning("FollowUp with ID {FollowUpId} not found", id);
                return NotFound();
            }
            return View(followUp);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create()
        {
            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .ToListAsync();

            ViewBag.Inspections = new SelectList(
                inspections.Select(i => new {
                    i.Id,
                    Display = $"{i.Premises?.Name} — {i.InspectionDate.ToShortDateString()}"
                }),
                "Id",
                "Display"
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create(FollowUp followUp)
        {
            ModelState.Remove("Inspection");

            // Business rule: due date must be after inspection date
            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
            if (inspection != null && followUp.DueDate < inspection.InspectionDate)
            {
                _logger.LogWarning("FollowUp creation rejected: DueDate {DueDate} is before InspectionDate {InspectionDate} for InspectionId {InspectionId}",
                    followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);
                ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    followUp.Status = "Open";
                    followUp.ClosedDate = null;
                    _context.FollowUps.Add(followUp);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("FollowUp created: ID {FollowUpId} for InspectionId {InspectionId} DueDate {DueDate} by {User}",
                        followUp.Id, followUp.InspectionId, followUp.DueDate, User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FollowUp for InspectionId {InspectionId} by {User}",
                    followUp.InspectionId, User.Identity?.Name);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .ToListAsync();

            ViewBag.Inspections = new SelectList(
                inspections.Select(i => new {
                    i.Id,
                    Display = $"{i.Premises?.Name} — {i.InspectionDate.ToShortDateString()}"
                }),
                "Id",
                "Display"
            );
            return View(followUp);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return NotFound();

            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .ToListAsync();

            ViewBag.Inspections = new SelectList(
                inspections.Select(i => new {
                    i.Id,
                    Display = $"{i.Premises?.Name} — {i.InspectionDate.ToShortDateString()}"
                }),
                "Id",
                "Display",
                followUp.InspectionId
            );
            return View(followUp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, FollowUp followUp)
        {
            ModelState.Remove("Inspection");

            if (id != followUp.Id) return NotFound();

            if (followUp.Status == "Closed" && followUp.ClosedDate == null)
            {
                _logger.LogWarning("FollowUp close rejected: No ClosedDate provided for FollowUpId {FollowUpId}",
                    followUp.Id);
                ModelState.AddModelError("ClosedDate", "A closed date is required when closing a follow-up.");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("FollowUp updated: ID {FollowUpId} Status {Status} by {User}",
                        followUp.Id, followUp.Status, User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FollowUp ID {FollowUpId} by {User}",
                    followUp.Id, User.Identity?.Name);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .ToListAsync();

            ViewBag.Inspections = new SelectList(
                inspections.Select(i => new {
                    i.Id,
                    Display = $"{i.Premises?.Name} — {i.InspectionDate.ToShortDateString()}"
                }),
                "Id",
                "Display",
                followUp.InspectionId
            );
            return View(followUp);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (followUp == null) return NotFound();
            return View(followUp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var followUp = await _context.FollowUps.FindAsync(id);
                if (followUp != null)
                {
                    _context.FollowUps.Remove(followUp);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("FollowUp deleted: ID {FollowUpId} by {User}",
                        id, User.Identity?.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FollowUp ID {FollowUpId} by {User}",
                    id, User.Identity?.Name);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
