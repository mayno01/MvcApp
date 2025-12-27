using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MvcApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string? ConfirmPassword { get; set; }

        // 🔹 Upload Image
        public IFormFile? ProfileImage { get; set; }

        // 🔹 Display Image
        public string? ProfileImageBase64 { get; set; }
    }
}
