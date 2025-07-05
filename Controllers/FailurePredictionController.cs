using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class FailurePredictionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FailurePredictionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FailurePrediction
        public async Task<IActionResult> Index()
        {
            return View(await _context.FailurePredictions
                .Include(f => f.Equipment)
                .OrderByDescending(f => f.PredictedFailureDate)
                .ToListAsync());
        }

        // GET: FailurePrediction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .FirstOrDefaultAsync(m => m.PredictionId == id);

            if (prediction == null)
            {
                return NotFound();
            }

            return View(prediction);
        }

        // GET: FailurePrediction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prediction = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .FirstOrDefaultAsync(m => m.PredictionId == id);
            if (prediction == null)
            {
                return NotFound();
            }

            return View(prediction);
        }

        // POST: FailurePrediction/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var prediction = await _context.FailurePredictions.FindAsync(id);
            if (prediction != null)
            {
                _context.FailurePredictions.Remove(prediction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: FailurePrediction/ByEquipment/5
        public async Task<IActionResult> ByEquipment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predictions = await _context.FailurePredictions
                .Include(f => f.Equipment)
                .Where(f => f.EquipmentId == id)
                .OrderByDescending(f => f.PredictedFailureDate)
                .ToListAsync();

            ViewBag.Equipment = await _context.Equipment.FindAsync(id);
            return View(predictions);
        }

        private bool FailurePredictionExists(int id)
        {
            return _context.FailurePredictions.Any(e => e.PredictionId == id);
        }
    }
}