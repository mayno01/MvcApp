using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcApp.Data;

namespace MvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBlock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // ❌ Prevent blocking another admin (optional safety)
            if (user.Role == "Admin")
                return BadRequest("Cannot block an admin");

            user.IsBlocked = !user.IsBlocked;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

    }
}
