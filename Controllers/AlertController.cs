using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FEENALOoFINALE.Controllers
{
    [Authorize]
    public class AlertController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlertController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alert
        public async Task<IActionResult> Index()
        {
            return View(await _context.Alerts
                .Include(a => a.Equipment) // Changed from a.EquipmentName
                .Include(a => a.AssignedTo)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync());
        }

        // GET: Alert/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts
                .Include(a => a.Equipment) // Changed from a.EquipmentName
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync(m => m.AlertId == id);

            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // GET: Alert/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts
                .Include(a => a.Equipment) // Changed from a.EquipmentName
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync(m => m.AlertId == id);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // POST: Alert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AlertExists(int id)
        {
            return _context.Alerts.Any(e => e.AlertId == id);
        }
    }
}