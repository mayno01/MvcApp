namespace MvcApp.Models
{
    public class UserArticleInteraction
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string ArticleUrl { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public bool IsSaved { get; set; }
        public bool? IsLiked { get; set; } // true = like, false = dislike, null = no vote

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
