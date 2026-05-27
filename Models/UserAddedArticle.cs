using System.ComponentModel.DataAnnotations;

namespace MvcApp.Models
{
    public class UserAddedArticle
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public string? ArticleUrl { get; set; }
        
        // 🔹 Changed from ImageUrl string to byte array for file upload
        public byte[]? ImageData { get; set; }
        
        [Required]
        public string Category { get; set; }
        
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    }
}