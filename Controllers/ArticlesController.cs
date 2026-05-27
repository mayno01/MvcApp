using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcApp.Data;
using MvcApp.Models;
using MvcApp.Models.News;
using MvcApp.Models.ViewModels;
using MvcApp.Services;

namespace MvcApp.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NewsService _newsService;

        public ArticlesController(ApplicationDbContext context, NewsService newsService)
        {
            _context = context;
            _newsService = newsService;
        }

        [HttpGet]
        public IActionResult AddArticle()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddArticle(AddArticleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = User.Identity!.Name!;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            var article = new UserAddedArticle
            {
                UserId = user.Id,
                Title = model.Title,
                Description = model.Description,
                Category = model.Category,
                ArticleUrl = model.ArticleUrl,
                PublishedAt = DateTime.UtcNow
            };

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.ImageFile.CopyToAsync(ms);
                article.ImageData = ms.ToArray();
            }

            _context.UserAddedArticles.Add(article);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category)
        {
            var email = User.Identity!.Name!;
            var user = await _context.Users.FirstAsync(u => u.Email == email);

            ViewBag.Categories = await _context.Categories.ToListAsync();

            List<NewsArticle> news = new();

            if (!string.IsNullOrEmpty(category))
            {
                news = await _newsService.GetTopHeadlinesAsync(category) ?? new List<NewsArticle>();
                ViewBag.ActiveCategory = category;

                var userArticles = await _context.UserAddedArticles
                    .Include(u => u.User)
                    .Where(ua => ua.Category.ToLower() == category.ToLower())
                    .OrderByDescending(ua => ua.PublishedAt)
                    .ToListAsync();

                var userArticleNews = userArticles.Select(ua => new NewsArticle
                {
                    Title = ua.Title,
                    Description = ua.Description,
                    Url = ua.ArticleUrl ?? $"#user-article-{ua.Id}",
                    UrlToImage = ua.ImageData != null
                        ? $"data:image/jpeg;base64,{Convert.ToBase64String(ua.ImageData)}"
                        : string.Empty, 
                    Source = $"Added by {ua.User.Username}",
                    PublishedAt = ua.PublishedAt,
                    Category = ua.Category
                }).ToList();

                news = userArticleNews.Concat(news).ToList();
            }
            else
            {
                ViewBag.ActiveCategory = "For You";

                var likedCategories = await _context.UserArticleInteractions
                    .Where(a => a.UserId == user.Id && a.IsLiked == true)
                    .GroupBy(a => a.Category)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(3)
                    .ToListAsync();

                var categories = likedCategories.Any()
                    ? likedCategories
                    : user.UserCategories.Select(uc => uc.Category.Name).Distinct().ToList();

                if (!categories.Any()) categories.Add("general");

                foreach (var cat in categories)
                {
                    try
                    {
                        var categoryNews = await _newsService.GetTopHeadlinesAsync(cat) ?? new List<NewsArticle>();
                        news.AddRange(categoryNews.Take(6));

                        var userArticles = await _context.UserAddedArticles
                            .Include(u => u.User)
                            .Where(ua => ua.Category.ToLower() == cat.ToLower())
                            .ToListAsync();

                        news.AddRange(userArticles.Select(ua => new NewsArticle
                        {
                            Title = ua.Title,
                            Description = ua.Description,
                            Url = ua.ArticleUrl ?? $"#user-article-{ua.Id}",
                            UrlToImage = ua.ImageData != null
                                ? $"data:image/jpeg;base64,{Convert.ToBase64String(ua.ImageData)}"
                                : string.Empty,
                            Source = $"Added by {ua.User.Username}",
                            PublishedAt = ua.PublishedAt,
                            Category = ua.Category
                        }));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching {cat}: {ex.Message}");
                    }
                }

                news = news.OrderBy(_ => Guid.NewGuid()).Take(12).ToList();
            }

            return View(news);
        }
    }
}
