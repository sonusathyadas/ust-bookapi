using System.Collections.Generic;
using System.Threading.Tasks;
using BookWebApi.Models;

namespace BookWebApi.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(int id);
        Task<Book> CreateBookAsync(Book book);
        Task<Book> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(int id);
        Task<IEnumerable<Book>> SearchBooksAsync(string query);
    }
}