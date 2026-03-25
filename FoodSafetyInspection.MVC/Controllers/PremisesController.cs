using FoodSafetyInspection.Domain;
using FoodSafetyInspection.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.MVC.Controllers
{
    [Authorize]
    public class PremisesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PremisesController> _logger;

        public PremisesController(AppDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var premises = await _context.Premises.ToListAsync();
            return View(premises);
        }

        public async Task<IActionResult> Details(int id)
        {
            var premises = await _context.Premises
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises with ID {PremisesId} not found", id);
                return NotFound();
            }
            return View(premises);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create(Premises premises)
        {
            ModelState.Remove("Inspections");

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Premises.Add(premises);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Premises created: {PremisesName} in {Town} with ID {PremisesId} by {User}",
                        premises.Name, premises.Town, premises.Id, User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating premises {PremisesName} by {User}",
                    premises.Name, User.Identity?.Name);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View(premises);
        }

        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id)
        {
            var premises = await _context.Premises.FindAsync(id);
            if (premises == null) return NotFound();
            return View(premises);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, Premises premises)
        {
            ModelState.Remove("Inspections");

            if (id != premises.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(premises);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Premises updated: ID {PremisesId} by {User}",
                        premises.Id, User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating premises ID {PremisesId} by {User}",
                    premises.Id, User.Identity?.Name);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View(premises);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var premises = await _context.Premises.FindAsync(id);
            if (premises == null) return NotFound();
            return View(premises);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var premises = await _context.Premises.FindAsync(id);
                if (premises != null)
                {
                    _context.Premises.Remove(premises);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Premises deleted: ID {PremisesId} by {User}",
                        id, User.Identity?.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting premises ID {PremisesId} by {User}",
                    id, User.Identity?.Name);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}