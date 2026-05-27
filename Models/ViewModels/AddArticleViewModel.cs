using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MvcApp.Models.ViewModels
{
    public class AddArticleViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Url]
        public string? ArticleUrl { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
