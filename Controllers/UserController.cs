using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FEENALOoFINALE.Data;
using FEENALOoFINALE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace FEENALOoFINALE.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User (Protected)
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // GET: Profile for current logged-in user (Protected)
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            if (User?.Identity?.Name == null)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Logout (Protected)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Identity.Application"); // Correct scheme for Identity
            return RedirectToAction("Login", "User");
        }

        // GET: Login (Public)
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // GET: User/Details/5 (Protected)
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: User/Create (Public)
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create (Public)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Email,FullName,ContactNumber")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Id = Guid.NewGuid().ToString();
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: User/Edit/5 (Protected)
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/Edit/5 (Protected)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,FullName,ContactNumber")] User user)
        {
            if (id != user.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                        return NotFound();

                    throw;
                }
            }

            return View(user);
        }

        // GET: User/Delete/5 (Protected)
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/DeleteConfirmed/5 (Protected)
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
