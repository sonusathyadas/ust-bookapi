## BookWebApi

A small, opinionated .NET 8 Web API that demonstrates a simple Books service backed by Entity Framework Core and SQLite. It exposes REST endpoints to create, read, update, delete, search and page through books with JWT authentication. The project follows basic clean architecture practices (Repository pattern, DTOs, DI) suitable for learning and small demos.

## Quick Start Guide

1. **Prerequisites:** .NET 8 SDK installed
2. **Clone and run:**
   ```bash
   git clone <repository-url>
   cd BookWebApi
   dotnet run
   ```
3. **API will be available at:** 
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001` 
   - Swagger UI: `https://localhost:5001/swagger`

4. **Basic workflow:**
   - Register: `POST /api/auth/register`
   - Login: `POST /api/auth/login` (get JWT token)
   - Use token: Include `Authorization: Bearer <token>` header for book endpoints
   - Access books: `GET /api/books` with authentication header

## Project structure

Top-level (BookWebApi)

- `Program.cs` — application startup, DI wiring, JWT configuration, and Swagger setup.
- `BookWebApi.csproj` — project file.
- `appsettings.json` / `appsettings.Development.json` — configuration including JWT settings.
- `books.db` — SQLite database file (created or seeded at runtime).
- `Controllers/BooksController.cs` — Web API controller with book management endpoints.
- `Controllers/AuthController.cs` — Web API controller with authentication endpoints.
- `Data/AppDbContext.cs` — EF Core DbContext and seed data for both Books and Customers.
- `Data/DbInitializer.cs` — helper to create/seed the database.
- `Models/Book.cs` — EF entity model for a book.
- `Models/Customer.cs` — EF entity model for user accounts.
- `DTOs/BookCreateDto.cs`, `DTOs/BookUpdateDto.cs` — book input DTOs.
- `DTOs/AuthDtos.cs` — authentication input/output DTOs.
- `Repositories/IBookRepository.cs`, `Repositories/BookRepository.cs` — repository abstraction and implementation.
- `Migrations/` — EF Core migrations (if present).

## List of classes and their explanation

### Controllers
- `BookWebApi.Controllers.BooksController` — API controller exposing endpoints under `api/books`. Handles CRUD, search, and paged listing. Uses `IBookRepository` for data access. Requires JWT authentication.
- `BookWebApi.Controllers.AuthController` — API controller exposing authentication endpoints under `api/auth`. Handles user registration, login, and password reset.

### Models
- `BookWebApi.Models.Book` — Domain/EF model for the Books table (Id, Title, Author, PublishedDate, Language, Genre).
- `BookWebApi.Models.Customer` — Domain/EF model for the Customer table (Id, Name, Email, Mobile, Address, UserName, PasswordHash).

### DTOs (Data Transfer Objects)
- `BookWebApi.DTOs.BookCreateDto` — DTO used when creating a book (Title, Author, PublishedDate, Language, Genre).
- `BookWebApi.DTOs.BookUpdateDto` — DTO used when updating a book; same shape as create DTO.
- `BookWebApi.DTOs.RegisterDto` — DTO used for user registration (UserName, Password, Email, Name).
- `BookWebApi.DTOs.LoginDto` — DTO used for user login (UserName, Password).
- `BookWebApi.DTOs.AuthResponseDto` — DTO returned after successful login (Token).
- `BookWebApi.DTOs.ForgotPasswordDto` — DTO used for password reset requests (Email).

### Data Access
- `BookWebApi.Data.AppDbContext` — EF Core DbContext that defines the `Books` and `Customers` DbSets and provides seed data through `OnModelCreating`.
- `BookWebApi.Data.DbInitializer` — Ensures database is created and seeded on first run (calls `EnsureCreated()` and adds sample books).
- `BookWebApi.Repositories.IBookRepository` — Repository interface exposing async methods: GetAll, GetById, Create, Update, Delete, and Search.
- `BookWebApi.Repositories.BookRepository` — EF Core implementation of `IBookRepository` using `AppDbContext` and async EF methods.

## API endpoints and usage

### Authentication Endpoints

Base route: `https://{host}:{port}/api/auth`

> **Note:** All book endpoints (except authentication) require a valid JWT token in the Authorization header.

#### 1. POST /api/auth/register
   - **Description:** Register a new user account.
   - **Authentication:** Not required
   - **Request body (application/json):**
```json
{
  "userName": "johndoe",
  "password": "securepassword123",
  "email": "john.doe@example.com",
  "name": "John Doe"
}
```
   - **Response:** 
     - 200 OK on successful registration
     - 400 BadRequest if username/password missing
     - 409 Conflict if username already exists

#### 2. POST /api/auth/login
   - **Description:** Authenticate user and receive JWT token.
   - **Authentication:** Not required
   - **Request body (application/json):**
```json
{
  "userName": "johndoe",
  "password": "securepassword123"
}
```
   - **Response:** 
     - 200 OK with JWT token:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```
     - 400 BadRequest if username/password missing
     - 401 Unauthorized if credentials are invalid

#### 3. POST /api/auth/forgot-password
   - **Description:** Reset password for a user account.
   - **Authentication:** Not required
   - **Request body (application/json):**
```json
{
  "email": "john.doe@example.com"
}
```
   - **Response:**
     - 200 OK with temporary password (for demo purposes):
```json
{
  "message": "Password reset successful. Check your email for the temporary password.",
  "temporaryPassword": "Abc12345"
}
```
     - 400 BadRequest if email missing
     - 404 NotFound if user with email not found

### Book Management Endpoints

Base route: `https://{host}:{port}/api/books`

> **Authentication Required:** All book endpoints require a valid JWT token in the Authorization header:
> `Authorization: Bearer <your-jwt-token>`

#### 4. GET /api/books
   - **Description:** Returns all books.
   - **Authentication:** Required
   - **Response:** 200 OK with JSON array of `Book` objects.

#### 5. GET /api/books/{id}
   - **Description:** Returns the book with the specified id.
   - **Authentication:** Required
   - **Response:** 200 OK with the `Book` object or 404 NotFound if not present.

#### 6. POST /api/books
   - **Description:** Creates a new book.
   - **Authentication:** Required
   - **Request body (application/json):**

```json
{
  "title": "The Pragmatic Programmer",
  "author": "Andrew Hunt",
  "publishedDate": "1999-10-20T00:00:00",
  "language": "English",
  "genre": "Software"
}
```

   - **Response:** 201 Created with Location header pointing to the newly created resource and the created object in the body.

#### 7. PUT /api/books/{id}
   - **Description:** Updates an existing book. Supply `BookUpdateDto` in the request body (same shape as create).
   - **Authentication:** Required
   - **Response:** 204 NoContent on success, 404 NotFound if the book does not exist.

#### 8. DELETE /api/books/{id}
   - **Description:** Deletes a book by id.
   - **Authentication:** Required
   - **Response:** 204 NoContent on success, 404 NotFound if the book does not exist.

#### 9. GET /api/books/search?q={query}
   - **Description:** Searches books by title or author (case-insensitive contains match).
   - **Authentication:** Required
   - **Query parameter:** `q` (required). Returns 400 BadRequest if `q` is missing or whitespace.
   - **Response:** 200 OK with matching books.

#### 10. GET /api/books/paged?page={n}&pageSize={m}
   - **Description:** Returns a paginated result with `Data`, `CurrentPage`, `PrevPage` and `NextPage` fields.
   - **Authentication:** Required
   - **Query parameters:** `page` (1-based, default 1), `pageSize` (default 10).
   - **Response:** 200 OK with page payload or 400 BadRequest for invalid pagination parameters.

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

### Common Error Responses

#### Authentication Errors
- **401 Unauthorized:** Missing or invalid JWT token
```json
{
  "message": "Unauthorized"
}
```

- **403 Forbidden:** Token expired or malformed
```json
{
  "message": "Forbidden"
}
```

#### Validation Errors
- **400 Bad Request:** Invalid request body or parameters
```json
{
  "message": "Username and password are required."
}
```

#### Not Found Errors
- **404 Not Found:** Resource not found
```json
{
  "message": "Book not found"
}
```

### HTTP Status Code Reference

| Status Code | Meaning | When It Occurs |
|-------------|---------|----------------|
| 200 OK | Success | GET requests that return data |
| 201 Created | Resource created | POST requests that create new resources |
| 204 No Content | Success with no body | PUT/DELETE requests that succeed |
| 400 Bad Request | Invalid request | Missing required fields, invalid parameters |
| 401 Unauthorized | Authentication required | Missing or invalid JWT token |
| 403 Forbidden | Access denied | Token expired or insufficient permissions |
| 404 Not Found | Resource not found | Requested book/user doesn't exist |
| 409 Conflict | Resource already exists | Username already taken during registration |
| 500 Internal Server Error | Server error | Unexpected server-side errors |

## Usage Examples (PowerShell / pwsh)

### Authentication Flow Examples

#### Register a new user

```powershell
$registerBody = @{
  userName = 'testuser'
  password = 'testpass123'
  email = 'test@example.com'
  name = 'Test User'
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $registerBody -ContentType 'application/json'
```

#### Login and get JWT token

```powershell
$loginBody = @{
  userName = 'testuser'
  password = 'testpass123'
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType 'application/json'
$token = $authResponse.token
Write-Host "JWT Token: $token"
```

#### Reset password

```powershell
$forgotPasswordBody = @{
  email = 'test@example.com'
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/forgot-password" -Method Post -Body $forgotPasswordBody -ContentType 'application/json'
```

### Book Management Examples

> **Note:** Remember to replace `$token` with your actual JWT token from the login response.

#### Get all books (with authentication)

```powershell
$headers = @{
  'Authorization' = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:5000/api/books" -Method Get -Headers $headers
```

#### Create a book (with authentication)

```powershell
$headers = @{
  'Authorization' = "Bearer $token"
}

$bookBody = @{
  title = 'Practical API Design'
  author = 'Jane Doe'
  publishedDate = '2024-01-01'
  language = 'English'
  genre = 'Software'
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/books" -Method Post -Body $bookBody -ContentType 'application/json' -Headers $headers
```

#### Search books (with authentication)

```powershell
$headers = @{
  'Authorization' = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:5000/api/books/search?q=clean" -Method Get -Headers $headers
```

#### Get paginated books (with authentication)

```powershell
$headers = @{
  'Authorization' = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:5000/api/books/paged?page=1&pageSize=5" -Method Get -Headers $headers
```

### Alternative: cURL Examples

#### Authentication with cURL

```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"userName":"testuser","password":"testpass123","email":"test@example.com","name":"Test User"}'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"testuser","password":"testpass123"}'

# Use the token from login response in subsequent requests
export TOKEN="your-jwt-token-here"

# Get all books
curl -X GET http://localhost:5000/api/books \
  -H "Authorization: Bearer $TOKEN"

# Create book
curl -X POST http://localhost:5000/api/books \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Clean Architecture","author":"Robert C. Martin","publishedDate":"2017-09-20T00:00:00","language":"English","genre":"Software"}'
```

## Patterns and Practices used

- **Repository Pattern:** `IBookRepository` abstracts data access and `BookRepository` implements EF Core access.
- **DTOs (Data Transfer Objects):** `BookCreateDto` / `BookUpdateDto` / `RegisterDto` / `LoginDto` etc. separate API models from domain entities.
- **EF Core (Code First) with SQLite:** `AppDbContext` and Migrations folder provide schema management and seeding.
- **JWT Authentication:** Token-based authentication using JSON Web Tokens for securing API endpoints.
- **Async/Await:** Repository methods and controller actions are asynchronous to avoid blocking threads.
- **Dependency Injection:** The repository and other services are injected into controllers via constructor DI.
- **RESTful routes and standard HTTP status codes:** (200, 201, 204, 400, 401, 403, 404, 409, 500 where applicable).
- **CORS (Cross-Origin Resource Sharing):** Configured to allow cross-origin requests for frontend integration.
- **Swagger/OpenAPI:** Integrated for API documentation and testing interface.

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

## Configuration

### JWT Settings

The API uses JWT for authentication. Configure the following settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-here-make-it-long-and-secure",
    "Issuer": "BookWebApi",
    "Audience": "BookWebApi",
    "ExpiryMinutes": 60
  }
}
```

### Database Connection

SQLite database configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=books.db"
  }
}
```

## Swagger/OpenAPI Documentation

When running in development mode, access the interactive API documentation at:
- **Swagger UI:** `http://localhost:5000/swagger` or `https://localhost:5001/swagger`

The Swagger interface includes:
- Interactive API testing
- Request/response examples
- Authentication support (click "Authorize" button and enter `Bearer <your-jwt-token>`)

## Troubleshooting

### Common Issues

#### 1. "Unauthorized" errors when accessing book endpoints
- **Solution:** Ensure you include the JWT token in the Authorization header:
  ```
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  ```

#### 2. "Username already exists" error during registration
- **Solution:** Use a different username or check existing users in the database.

#### 3. Database errors on first run
- **Solution:** Ensure the application has write permissions to create the SQLite database file.

#### 4. JWT token expired
- **Solution:** Login again to get a new token. Tokens expire after the configured time (default: 60 minutes).

### Development Tips
- Use the Swagger UI for easy API testing during development
- Check the application logs for detailed error information
- Ensure your JWT secret key is secure and not exposed in production

## Notes and next steps

### Current Implementation Notes
- The repository seeds sample book data via `AppDbContext.OnModelCreating` and `DbInitializer.Initialize`.
- Password storage uses simple Base64 encoding (suitable for demo purposes only).
- JWT tokens are configured with HMAC SHA256 signing.
- The forgot-password endpoint returns the temporary password in the response (for demo purposes).
- All book endpoints require authentication except the authentication endpoints themselves.

### Security Considerations for Production
- Replace Base64 password encoding with proper password hashing (bcrypt, Argon2, etc.).
- Implement proper email service for password reset functionality.
- Use environment variables for JWT secret keys and sensitive configuration.
- Implement rate limiting for authentication endpoints.
- Add input validation and sanitization.
- Enable HTTPS in production environments.

### Recommended Enhancements
- Add validation attributes to DTOs and use FluentValidation for richer server-side validation.
- Add integration tests that run against an in-memory or test SQLite database.
- Implement refresh token functionality for enhanced security.
- Add logging and monitoring capabilities.
- Consider implementing role-based authorization.
- Add API versioning for future extensibility.

---

Requirements coverage:

- Project Title and description: Done
- Project structure: Done
- List of classes and their explanation: Done
- API endpoints and its usage: Done (examples provided)
- Patterns and Practices used: Done
- Steps to build, test, run and publish the project: Done

If you want, I can also add a small Postman collection or example tests (xUnit) to this repo next.
