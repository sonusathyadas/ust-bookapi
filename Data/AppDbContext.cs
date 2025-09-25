using Microsoft.EntityFrameworkCore;
using BookWebApi.Models;

namespace BookWebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
    public DbSet<BookWebApi.Models.Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", PublishedDate = new DateTime(1999,10,20), Language = "English", Genre = "Software" },
                new Book { Id = 2, Title = "Clean Code", Author = "Robert C. Martin", PublishedDate = new DateTime(2008,8,1), Language = "English", Genre = "Software" },
                new Book { Id = 3, Title = "Domain-Driven Design", Author = "Eric Evans", PublishedDate = new DateTime(2003,8,30), Language = "English", Genre = "Software" },
                new Book { Id = 4, Title = "Design Patterns", Author = "Erich Gamma", PublishedDate = new DateTime(1994,10,31), Language = "English", Genre = "Software" },
                new Book { Id = 5, Title = "Refactoring", Author = "Martin Fowler", PublishedDate = new DateTime(1999,7,8), Language = "English", Genre = "Software" }
            );
        }
    }
}