using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookWebApi.Models;
using BookWebApi.DTOs;
using BookWebApi.Repositories;
using Microsoft.AspNetCore.Cors;

namespace BookWebApi.Controllers
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _bookRepository.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(BookCreateDto bookCreateDto)
        {
            var book = new Book
            {
                Title = bookCreateDto.Title,
                Author = bookCreateDto.Author,
                PublishedDate = bookCreateDto.PublishedDate,
                Language = bookCreateDto.Language,
                Genre = bookCreateDto.Genre
            };

            var created = await _bookRepository.CreateBookAsync(book);
            return CreatedAtAction(nameof(GetBook), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDto bookUpdateDto)
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            existingBook.Title = bookUpdateDto.Title;
            existingBook.Author = bookUpdateDto.Author;
            existingBook.PublishedDate = bookUpdateDto.PublishedDate;
            existingBook.Language = bookUpdateDto.Language;
            existingBook.Genre = bookUpdateDto.Genre;

            await _bookRepository.UpdateBookAsync(existingBook);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            await _bookRepository.DeleteBookAsync(id);
            return NoContent();
        }

        // New: GET /api/books/search?q=...
        /// <summary>
        /// Searches for books that match the specified query string.
        /// </summary>
        /// <param name="q">The search query supplied via the query string. Must not be null, empty, or whitespace.</param>
        /// <returns>
        /// An asynchronous ActionResult containing an IEnumerable of <see cref="Book"/> objects.
        /// Returns 400 BadRequest if the query parameter 'q' is null or whitespace; otherwise returns 200 OK with the matching books.
        /// </returns>
        /// <remarks>
        /// This action is exposed as GET /search and expects the search term in the 'q' query parameter
        /// (for example: GET /books/search?q=aspnet). The search is delegated to the repository method SearchBooksAsync(q).
        /// </remarks>
        /// <response code="200">A collection of matching books.</response>
        /// <response code="400">The query parameter 'q' was not provided or is invalid.</response>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Query parameter 'q' is required.");
            }

            var results = await _bookRepository.SearchBooksAsync(q);
            return Ok(results);
        }

        
        private const int DEFAULT_PAGE_SIZE = 10;

        /// <summary>
        /// Returns a paginated list of books along with pagination metadata.
        /// </summary>
        /// <param name="page">The 1-based page number to return. Must be greater than 0.</param>
        /// <param name="pageSize">The number of items per page. If not supplied, a default is used.</param>
        /// <returns>An object containing the page data and pagination numbers (current, previous, next).</returns>
        [HttpGet("paged")]
        public async Task<ActionResult<object>> GetBooksPaged([FromQuery] int page = 1, [FromQuery] int pageSize = DEFAULT_PAGE_SIZE)
        {
            
            try
            {
                if (page <= 0 || pageSize <= 0)
                {
                    return BadRequest("Both 'page' and 'pageSize' must be greater than zero.");
                }

                var booksEnumerable = await _bookRepository.GetAllBooksAsync();

                var allBooksList = new List<Book>();
                if (booksEnumerable != null)
                {
                    allBooksList.AddRange(booksEnumerable);
                }

                var totalCount = allBooksList.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                totalPages = Math.Max(1, totalPages);

                if (page > totalPages)
                {
                    return BadRequest($"Requested page '{page}' exceeds total pages '{totalPages}'.");
                }

                var startIndex = (page - 1) * pageSize;
                var takeCount = Math.Min(pageSize, Math.Max(0, totalCount - startIndex));
                var pageItems = startIndex < totalCount ? allBooksList.GetRange(startIndex, takeCount) : new();

                int? prevPage = page > 1 ? page - 1 : null;
                int? nextPage = page < totalPages ? page + 1 : null;

                var responsePayload = new
                {
                    Data = pageItems,
                    CurrentPage = page,
                    PrevPage = prevPage,
                    NextPage = nextPage
                };

                return Ok(responsePayload);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving paginated books.");
            }
        }
    }
}