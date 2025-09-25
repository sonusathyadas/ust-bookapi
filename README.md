## BookWebApi

A small, opinionated .NET 8 Web API that demonstrates a simple Books service backed by Entity Framework Core and SQLite. It exposes REST endpoints to create, read, update, delete, search and page through books. The project follows basic clean architecture practices (Repository pattern, DTOs, DI) suitable for learning and small demos.

## Project structure

Top-level (BookWebApi)

- `Program.cs` — application startup and DI wiring.
- `BookWebApi.csproj` — project file.
- `appsettings.json` / `appsettings.Development.json` — configuration.
- `books.db` — SQLite database file (created or seeded at runtime).
- `Controllers/BooksController.cs` — Web API controller with endpoints.
- `Data/AppDbContext.cs` — EF Core DbContext and seed data.
- `Data/DbInitializer.cs` — helper to create/seed the database.
- `Models/Book.cs` — EF entity model for a book.
- `DTOs/BookCreateDto.cs`, `DTOs/BookUpdateDto.cs` — input DTOs.
- `Repositories/IBookRepository.cs`, `Repositories/BookRepository.cs` — repository abstraction and implementation.
- `Migrations/` — EF Core migrations (if present).

## List of classes and their explanation

- `BookWebApi.Controllers.BooksController` — API controller exposing endpoints under `api/books`. Handles CRUD, search, and paged listing. Uses `IBookRepository` for data access.
- `BookWebApi.Models.Book` — Domain/EF model for the Books table (Id, Title, Author, PublishedDate, Language, Genre).
- `BookWebApi.DTOs.BookCreateDto` — DTO used when creating a book (Title, Author, PublishedDate, Language, Genre).
- `BookWebApi.DTOs.BookUpdateDto` — DTO used when updating a book; same shape as create DTO.
- `BookWebApi.Data.AppDbContext` — EF Core DbContext that defines the `Books` DbSet and provides seed data through `OnModelCreating`.
- `BookWebApi.Data.DbInitializer` — Ensures database is created and seeded on first run (calls `EnsureCreated()` and adds sample books).
- `BookWebApi.Repositories.IBookRepository` — Repository interface exposing async methods: GetAll, GetById, Create, Update, Delete, and Search.
- `BookWebApi.Repositories.BookRepository` — EF Core implementation of `IBookRepository` using `AppDbContext` and async EF methods.

## API endpoints and usage

Base route: `https://{host}:{port}/api/books`

1. GET /api/books
   - Description: Returns all books.
   - Response: 200 OK with JSON array of `Book` objects.

2. GET /api/books/{id}
   - Description: Returns the book with the specified id.
   - Response: 200 OK with the `Book` object or 404 NotFound if not present.

3. POST /api/books
   - Description: Creates a new book.
   - Request body (application/json):

```json
{
  "title": "The Pragmatic Programmer",
  "author": "Andrew Hunt",
  "publishedDate": "1999-10-20T00:00:00",
  "language": "English",
  "genre": "Software"
}
```

   - Response: 201 Created with Location header pointing to the newly created resource and the created object in the body.

4. PUT /api/books/{id}
   - Description: Updates an existing book. Supply `BookUpdateDto` in the request body (same shape as create).
   - Response: 204 NoContent on success, 404 NotFound if the book does not exist.

5. DELETE /api/books/{id}
   - Description: Deletes a book by id.
   - Response: 204 NoContent on success, 404 NotFound if the book does not exist.

6. GET /api/books/search?q={query}
   - Description: Searches books by title or author (case-insensitive contains match).
   - Query parameter: `q` (required). Returns 400 BadRequest if `q` is missing or whitespace.
   - Response: 200 OK with matching books.

7. GET /api/books/paged?page={n}&pageSize={m}
   - Description: Returns a paginated result with `Data`, `CurrentPage`, `PrevPage` and `NextPage` fields.
   - Query parameters: `page` (1-based, default 1), `pageSize` (default 10).
   - Response: 200 OK with page payload or 400 BadRequest for invalid pagination parameters.

Response shape for `Book` (example):

```json
{
  "id": 1,
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "publishedDate": "2008-08-01T00:00:00",
  "language": "English",
  "genre": "Software"
}
```

Examples (PowerShell / pwsh):

Get all books

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/books" -Method Get
```

Create a book

```powershell
$body = @{
  title = 'Practical API Design'
  author = 'Jane Doe'
  publishedDate = '2024-01-01'
  language = 'English'
  genre = 'Software'
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/books" -Method Post -Body $body -ContentType 'application/json'
```

Search

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/books/search?q=clean" -Method Get
```

Paged

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/books/paged?page=1&pageSize=5" -Method Get
```

## Patterns and Practices used

- Repository Pattern: `IBookRepository` abstracts data access and `BookRepository` implements EF Core access.
- DTOs: `BookCreateDto` / `BookUpdateDto` separate API models from domain entities.
- EF Core (Code First) with SQLite: `AppDbContext` and Migrations folder provide schema management and seeding.
- Async/Await: Repository methods and controller actions are asynchronous to avoid blocking threads.
- Dependency Injection: The repository is injected into the controller via constructor DI.
- RESTful routes and standard status codes (200, 201, 204, 400, 404, 500 where applicable).

## Steps to build, test, run and publish

Prerequisites

- .NET 8 SDK installed: https://dotnet.microsoft.com
- (Optional) `dotnet-ef` tool if you want to run EF migrations manually:

```powershell
dotnet tool install --global dotnet-ef
```

From the repository root (where `BookWebApi.sln` is located) or inside the `BookWebApi` folder, run:

Restore dependencies and build:

```powershell
cd BookWebApi
dotnet restore
dotnet build -c Debug
```

Run the application locally:

```powershell
dotnet run
# The app will start and print the listening URL (e.g. http://localhost:5000 or https://localhost:5001)
```

Apply EF migrations (if you modified migrations or want to ensure DB schema is up to date):

```powershell
# From BookWebApi folder
dotnet ef database update
```

Publish for production:

```powershell
dotnet publish -c Release -o ./publish
# The published output will be in BookWebApi/publish. You can run the published exe or containerize it.
```

Run basic manual tests using curl or PowerShell's Invoke-RestMethod (examples are above). For automated testing, consider adding an xUnit project and writing unit/integration tests that target the repository and controller behavior.

## Notes and next steps

- The repository seeds sample data via `AppDbContext.OnModelCreating` and `DbInitializer.Initialize`.
- Consider adding validation attributes to DTOs and using FluentValidation for richer server-side validation.
- Add integration tests that run against an in-memory or test SQLite database.
- Add OpenAPI/Swagger enhancements and authorization if you plan to extend the API for production.

---

Requirements coverage:

- Project Title and description: Done
- Project structure: Done
- List of classes and their explanation: Done
- API endpoints and its usage: Done (examples provided)
- Patterns and Practices used: Done
- Steps to build, test, run and publish the project: Done

If you want, I can also add a small Postman collection or example tests (xUnit) to this repo next.
