using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Data;
using MvcApp.Models;
using MvcApp.Services;
using Microsoft.EntityFrameworkCore;
using MvcApp.Models.ViewModels;

namespace MvcApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // REGISTER
        public async Task<IActionResult> Register()
        {
            var vm = new RegisterViewModel
            {
                AvailableCategories = await _context.Categories.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.SelectedCategoryIds.Count > 3)
            {
                ModelState.AddModelError(
                    nameof(model.SelectedCategoryIds),
                    "You can select a maximum of 3 categories."
                );
            }

            if (!ModelState.IsValid)
            {
                model.AvailableCategories = await _context.Categories.ToListAsync();
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already exists");
                model.AvailableCategories = await _context.Categories.ToListAsync();
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Role = "User",
                PasswordHash = _hasher.HashPassword(null!, model.Password)
            };

            foreach (var catId in model.SelectedCategoryIds)
            {
                user.UserCategories.Add(new UserCategory
                {
                    CategoryId = catId
                });
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }




        // LOGIN
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt");
                return View();
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("", "Your account is blocked by the administrator.");
                return View();
            }

            if (_hasher.VerifyHashedPassword(user, user.PasswordHash, password)
                == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid login attempt");
                return View();
            }



            var token = _jwtService.GenerateToken(user);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });

            return RedirectToAction("Index", "Home");
        }


        // LOGOUT
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }
    }
}
