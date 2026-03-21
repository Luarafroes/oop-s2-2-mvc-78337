using FoodSafetyInspection.Domain;
using FoodSafetyInspection.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.MVC.Controllers
{
    [Authorize]
    public class InspectionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InspectionsController> _logger;

        public InspectionsController(AppDbContext context, ILogger<InspectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .ToListAsync();
            return View(inspections);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .Include(i => i.FollowUps)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspection == null)
            {
                _logger.LogWarning("Inspection with ID {InspectionId} not found", id);
                return NotFound();
            }
            return View(inspection);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            ViewBag.Premises = new SelectList(_context.Premises, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create(Inspection inspection)
        {
            if (ModelState.IsValid)
            {
                _context.Inspections.Add(inspection);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection created: ID {InspectionId} for PremisesId {PremisesId} Outcome {Outcome} by {User}",
                    inspection.Id, inspection.PremisesId, inspection.Outcome, User.Identity?.Name);

                if (inspection.Outcome == "Fail")
                    _logger.LogWarning("Failed inspection recorded: ID {InspectionId} for PremisesId {PremisesId} Score {Score}",
                        inspection.Id, inspection.PremisesId, inspection.Score);

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Premises = new SelectList(_context.Premises, "Id", "Name");
            return View(inspection);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return NotFound();
            ViewBag.Premises = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, Inspection inspection)
        {
            if (id != inspection.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection updated: ID {InspectionId} by {User}",
                    inspection.Id, User.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Premises = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
            return View(inspection);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (inspection == null) return NotFound();
            return View(inspection);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inspection deleted: ID {InspectionId} by {User}",
                    id, User.Identity?.Name);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}