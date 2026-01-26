using System.ComponentModel.DataAnnotations;

namespace MvcApp.Models.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please select at least 1 category")]
        [MaxLength(3, ErrorMessage = "You can select up to 3 categories only")]
        public List<int> SelectedCategoryIds { get; set; } = new();

        public List<Category> AvailableCategories { get; set; } = new();
    }

}
