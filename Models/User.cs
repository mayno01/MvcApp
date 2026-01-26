using System.ComponentModel.DataAnnotations;

namespace MvcApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        // 🔹 ROLE
        [Required]
        public string Role { get; set; } = "User"; // default

        // 🔹 PROFILE IMAGE (stored as bytes)
        public byte[]? ProfilePicture { get; set; }

        public bool IsBlocked { get; set; } = false;

        public ICollection<UserCategory> UserCategories { get; set; } = new List<UserCategory>();
        public ICollection<UserArticleInteraction> ArticleInteractions { get; set; } = new List<UserArticleInteraction>();


    }
}
