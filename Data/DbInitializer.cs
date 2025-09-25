using System;
using System.Linq;
using BookWebApi.Models;

namespace BookWebApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Look for any books.
            if (context.Books.Any())
            {
                return;   // DB has been seeded
            }

            var books = new Book[]
            {
                new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", PublishedDate = new DateTime(1999,10,20), Language = "English", Genre = "Software" },
                new Book { Title = "Clean Code", Author = "Robert C. Martin", PublishedDate = new DateTime(2008,8,1), Language = "English", Genre = "Software" },
                new Book { Title = "Domain-Driven Design", Author = "Eric Evans", PublishedDate = new DateTime(2003,8,30), Language = "English", Genre = "Software" },
                new Book { Title = "Design Patterns", Author = "Erich Gamma", PublishedDate = new DateTime(1994,10,31), Language = "English", Genre = "Software" },
                new Book { Title = "Refactoring", Author = "Martin Fowler", PublishedDate = new DateTime(1999,7,8), Language = "English", Genre = "Software" }
            };

            context.Books.AddRange(books);
            context.SaveChanges();

            // Add a default customer for testing (username: testuser, password: Password123!)
            if (!context.Set<BookWebApi.Models.Customer>().Any())
            {
                var password = "Password123!";
                // simple hash for seeding (not recommended for production)
                var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));

                var customer = new BookWebApi.Models.Customer
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Mobile = "1234567890",
                    Address = "Test Address",
                    UserName = "testuser",
                    PasswordHash = passwordHash
                };

                context.Add(customer);
                context.SaveChanges();
            }
        }
    }
}
