using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Data;
using MvcApp.Models;
using MvcApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MvcApp.Services;

namespace MvcApp.Controllers
{
  
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Models.User> _hasher = new();
        private readonly NewsService _newsService;

        public HomeController(ApplicationDbContext context , NewsService newsService)
        {
            _context = context;
            _newsService = newsService;
        }

        [Authorize]
        public async Task<IActionResult> Index(string category = "general")
        {
            ViewBag.Category = category;
            var news = await _newsService.GetTopHeadlinesAsync(category);
            return View(news);
        }


        // PROFILE (GET)

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();

            var vm = new ProfileViewModel
            {
                Username = user.Username,
                ProfileImageBase64 = user.ProfilePicture != null
                    ? Convert.ToBase64String(user.ProfilePicture)
                    : null
            };

            return View(vm);
        }


        // PROFILE (POST)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();

            user.Username = model.Username;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
            }

            // 🔹 Image upload
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.ProfileImage.CopyToAsync(ms);
                user.ProfilePicture = ms.ToArray();
            }

            await _context.SaveChangesAsync();

            ViewBag.Success = "Profile updated successfully";
            return RedirectToAction(nameof(Profile));
        }

    }
}
