using Microsoft.EntityFrameworkCore;
using LinqRetrievalLab.Models.Domain;

namespace LinqRetrievalLab.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Product => Set<Product>();
        public DbSet<Category> Category => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "School Supplies" },
                new Category { Id = 3, Name = "Accessories" }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Price = 45000,
                    Stock = 10,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Smartphone",
                    Price = 25000,
                    Stock = 15,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 3,
                    Name = "Notebook",
                    Price = 100,
                    Stock = 50,
                    CategoryId = 2
                },
                new Product
                {
                    Id = 4,
                    Name = "Ballpen",
                    Price = 25,
                    Stock = 100,
                    CategoryId = 2
                },
                new Product
                {
                    Id = 5,
                    Name = "Keyboard",
                    Price = 1500,
                    Stock = 20,
                    CategoryId = 3
                }
            );
        }
    }
}