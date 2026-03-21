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

        public async Task<IActionResult> Index()
        {
            var followUps = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i.Premises)
                .ToListAsync();
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
        public IActionResult Create()
        {
            ViewBag.Inspections = new SelectList(_context.Inspections, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create(FollowUp followUp)
        {
            // Business rule: due date must be after inspection date
            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
            if (inspection != null && followUp.DueDate < inspection.InspectionDate)
            {
                _logger.LogWarning("FollowUp creation rejected: DueDate {DueDate} is before InspectionDate {InspectionDate} for InspectionId {InspectionId}",
                    followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);
                ModelState.AddModelError("DueDate", "Due date cannot be before the inspection date.");
            }

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
            ViewBag.Inspections = new SelectList(_context.Inspections, "Id", "Id");
            return View(followUp);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return NotFound();
            ViewBag.Inspections = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            if (followUp.Status == "Closed" && followUp.ClosedDate == null)
            {
                _logger.LogWarning("FollowUp close rejected: No ClosedDate provided for FollowUpId {FollowUpId}",
                    followUp.Id);
                ModelState.AddModelError("ClosedDate", "A closed date is required when closing a follow-up.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(followUp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("FollowUp updated: ID {FollowUpId} Status {Status} by {User}",
                    followUp.Id, followUp.Status, User.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Inspections = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (followUp == null) return NotFound();
            return View(followUp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("FollowUp deleted: ID {FollowUpId} by {User}",
                    id, User.Identity?.Name);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}