using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Data;
using MvcApp.Models;
using MvcApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MvcApp.Services;
using MvcApp.Models.News;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace MvcApp.Controllers
{
  
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Models.User> _hasher = new();
        private readonly NewsService _newsService;
        private readonly IMemoryCache _cache;

        public HomeController(ApplicationDbContext context, NewsService newsService, IMemoryCache cache)
        {
            _context = context;
            _newsService = newsService;
            _cache = cache;
        }

   
        [Authorize]
        public async Task<IActionResult> Index(string? category, int page = 1)
        {
            const int pageSize = 6;

            var email = User.Identity!.Name!;
            var user = await _context.Users
                .Include(u => u.UserCategories)
                .ThenInclude(uc => uc.Category)
                .FirstAsync(u => u.Email == email);

            List<NewsArticle> news;

            if (!string.IsNullOrEmpty(category))
            {
                // If category is specified, no caching, fetch fresh
                news = await _newsService.GetTopHeadlinesAsync(category);
                ViewBag.ActiveCategory = category;
            }
            else
            {
                ViewBag.ActiveCategory = "For You";

                string cacheKey = $"ForYouNews_{user.Id}";

                if (!_cache.TryGetValue(cacheKey, out news))
                {
                    Console.WriteLine($"Cache miss for {cacheKey}, fetching news...");
                    // Cache miss: fetch fresh news
                    var categories = user.UserCategories
                        .Select(uc => uc.Category.Name)
                        .Distinct()
                        .ToList();

                    if (!categories.Any())
                        categories.Add("general");

                    news = new List<NewsArticle>();

                    foreach (var cat in categories)
                    {
                        try
                        {
                            var categoryNews = await _newsService.GetTopHeadlinesAsync(cat) ?? new List<NewsArticle>();
                            news.AddRange(categoryNews);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching {cat}: {ex.Message}");
                        }
                    }

                    news = news
                        .GroupBy(n => n.Url)
                        .Select(g => g.First())
                        .OrderByDescending(n => n.PublishedAt)
                        .ToList();

                    // Cache it for 10 minutes (adjust as needed)
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _cache.Set(cacheKey, news, cacheEntryOptions);
                }
                else
                {
                    Console.WriteLine($"Cache hit for {cacheKey}, news count: {news.Count}");
                }
            }

            int totalArticles = news.Count;
            int totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

            news = news
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            var articleUrls = news.Select(n => n.Url).ToList();

            var likesCount = await _context.UserArticleInteractions
                .Where(a => articleUrls.Contains(a.ArticleUrl) && a.IsLiked == true)
                .GroupBy(a => a.ArticleUrl)
                .Select(g => new { Url = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Url, x => x.Count);

            ViewData["LikesCount"] = likesCount;

            var userInteractions = await _context.UserArticleInteractions
                .Where(a => a.UserId == user.Id && articleUrls.Contains(a.ArticleUrl))
                .ToDictionaryAsync(a => a.ArticleUrl);

            ViewData["UserInteractions"] = userInteractions;

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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleSave(string url, string title, string category, string? imageUrl)
        {
            var email = User.Identity!.Name!;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var interaction = await _context.UserArticleInteractions
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.ArticleUrl == url);

            if (interaction == null)
            {
                interaction = new UserArticleInteraction
                {
                    UserId = user.Id,
                    ArticleUrl = url,
                    Title = title,
                    Category = category,
                    ImageUrl = imageUrl,
                    IsSaved = true
                };
                _context.UserArticleInteractions.Add(interaction);
            }
            else
            {
                interaction.IsSaved = !interaction.IsSaved;
                interaction.ImageUrl ??= imageUrl;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [Authorize]
        public async Task<IActionResult> Saved()
        {
            var email = User.Identity!.Name!;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var saved = await _context.UserArticleInteractions
                .Where(a => a.UserId == user.Id && a.IsSaved)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(saved);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> React(string url, string title, string category, bool like)
        {
            var email = User.Identity!.Name!;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var interaction = await _context.UserArticleInteractions
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.ArticleUrl == url);

            if (interaction == null)
            {
                interaction = new UserArticleInteraction
                {
                    UserId = user.Id,
                    ArticleUrl = url,
                    Title = title,
                    Category = category,
                    IsLiked = like
                };
                _context.UserArticleInteractions.Add(interaction);
            }
            else
            {
                interaction.IsLiked = like;
            }

            await _context.SaveChangesAsync();
            return Ok();
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
