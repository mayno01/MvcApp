namespace MvcApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<UserCategory> UserCategories { get; set; } = new List<UserCategory>();
    }
}
