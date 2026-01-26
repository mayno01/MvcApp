using Microsoft.EntityFrameworkCore;
using MvcApp.Models;

namespace MvcApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserCategory> UserCategories { get; set; }
        public DbSet<UserArticleInteraction> UserArticleInteractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCategory>()
                .HasKey(uc => new { uc.UserId, uc.CategoryId });

            modelBuilder.Entity<Category>().HasData(
    new Category { Id = 1, Name = "general" },
    new Category { Id = 2, Name = "business" },
    new Category { Id = 3, Name = "technology" },
    new Category { Id = 4, Name = "sports" },
    new Category { Id = 5, Name = "health" },
    new Category { Id = 6, Name = "science" },
    new Category { Id = 7, Name = "entertainment" }
);


            base.OnModelCreating(modelBuilder);
        }



    }
}
