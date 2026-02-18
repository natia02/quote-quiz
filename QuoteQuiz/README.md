# QuoteQuiz — Backend API

A RESTful API for the Famous Quote Quiz application built with **.NET 8** and **ASP.NET Core**. The backend handles authentication, quiz logic, game history tracking, and admin management. This document covers not just how to run the project but the reasoning behind every architectural and technical decision made.

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Core Business Logic](#core-business-logic)
- [Architecture: Why Clean Architecture](#architecture-why-clean-architecture)
- [Design Patterns](#design-patterns)
- [Authentication & Security](#authentication--security)
- [Validation Strategy](#validation-strategy)
- [Error Handling](#error-handling)
- [Logging](#logging)
- [Database & Migrations](#database--migrations)
- [Testing](#testing)
- [API Reference](#api-reference)

---

## Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET / ASP.NET Core | 8.0 | Web framework |
| Entity Framework Core | 8.0 | ORM / database access |
| SQL Server | — | Relational database |
| BCrypt.Net | 4.0 | Password hashing |
| JWT Bearer | 8.0 | Authentication tokens |
| FluentValidation | 11.x | Input validation |
| AutoMapper | 12.x | DTO ↔ Entity mapping |
| xUnit | 2.x | Unit testing |
| Moq | 4.x | Mocking in tests |
| Swashbuckle (Swagger) | 6.4 | API documentation |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (local or remote)

### 1. Clone the repository

```bash
git clone https://github.com/natia02/quote-quiz.git
cd quote-quiz/QuoteQuiz
```

### 2. Configure the database

Open `QuoteQuiz.API/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QuoteQuizDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters-long",
    "Issuer": "QuoteQuizAPI",
    "Audience": "QuoteQuizClient"
  }
}
```

### 3. Run the API

```bash
dotnet run --project QuoteQuiz.API
```

The API starts at `https://localhost:5294`. Migrations and database seeding run automatically on startup.

> **Default admin account** seeded automatically:
> - Username: `admin`
> - Password: `Admin123!`

### 4. Open Swagger

Navigate to `http://localhost:5294/swagger` to explore and test all endpoints interactively.

---

## Project Structure

```
QuoteQuiz/
├── QuoteQuiz.Domain/           # Core business entities, enums, exceptions
│   ├── Entities/               # User, Quote, GameHistory, ShownQuote
│   ├── Enums/                  # UserRole, QuizMode
│   ├── Exceptions/             # Domain-specific exception types
│   └── Common/                 # BaseEntity (Id, CreatedAt)
│
├── QuoteQuiz.Application/      # Business logic, interfaces, DTOs
│   ├── DTOs/                   # Data Transfer Objects (Auth, Quiz, Admin)
│   ├── Interfaces/             # Service and repository contracts
│   ├── Services/               # Business logic implementations
│   ├── Validators/             # FluentValidation validators
│   └── Mappings/               # AutoMapper profiles
│
├── QuoteQuiz.Infrastructure/   # Data access, EF Core, repositories
│   ├── Data/                   # DbContext, Migrations, DbInitializer
│   ├── Configurations/         # Entity type configurations
│   └── Repositories/           # Repository and UnitOfWork implementations
│
├── QuoteQuiz.API/              # Presentation layer
│   ├── Controllers/            # HTTP endpoints
│   ├── Middleware/             # Logging, exception handling
│   ├── Filters/                # Validation action filter
│   └── Extensions/             # ClaimsPrincipal helpers
│
└── QuoteQuiz.Tests/            # Unit tests
    └── Services/               # Service-level tests
```

---

## Core Business Logic

Understanding what the system does makes the architectural decisions easier to follow.

### Quiz Modes

The application supports two modes, selected per session and persisted in the frontend:

**Binary mode (Yes / No)**
The API picks an unseen quote and randomly assigns a displayed author — 50% chance it's the real author, 50% chance it's a random wrong one from the pool. The user answers Yes ("this is the real author") or No ("this isn't the real author").

Answer evaluation:
```
userAgreed       = selectedAnswer == displayedAuthor
displayedCorrect = displayedAuthor == quote.AuthorName
isCorrect        = userAgreed == displayedCorrect
```

This means both "Yes to the right author" and "No to the wrong author" are correct. The `DisplayedAuthor` field is sent back in the submit request so the backend can evaluate this correctly regardless of what was shown on screen.

**Multiple choice mode**
The API picks an unseen quote and builds a 3-option list: 1 correct author + 2 random wrong authors shuffled together. The user picks one. The answer is correct if `selectedAnswer == quote.AuthorName` (case-insensitive).

---

### Quote Rotation (No Repeats)

Each user has a `ShownQuotes` table tracking which quotes they have seen. The flow on every `GET /quiz/question`:

1. Load all `ShownQuoteId`s for this user
2. Filter the full quote pool to only unseen quotes
3. If the unseen list is empty → **delete all ShownQuotes for this user** (reset) and start over
4. Pick a random quote from the unseen list

A `ShownQuote` record is written only when the user submits an answer, not when they receive the question. This ensures a quote is only marked seen once it has actually been answered.

The composite unique index on `(UserId, QuoteId)` at the database level guarantees no duplicate tracking records even under concurrent requests.

---

### Services and Their Responsibilities

| Service | Responsibility |
|---|---|
| `AuthService` | Register, login, password hashing, JWT generation |
| `TokenService` | Builds and signs JWT tokens with user claims |
| `QuizService` | Question generation (both modes), answer evaluation, ShownQuote tracking |
| `QuoteService` | Quote CRUD, author list, delete-guard (can't delete a quote used in game history) |
| `UserService` | User CRUD, disable (soft), delete, password hashing for admin-created users |
| `GameHistoryService` | Fetch personal history, compute statistics, fetch all history for admin |

---

## Architecture: Why Clean Architecture

The project follows **Clean Architecture** (also known as Onion Architecture). The dependency rule is strict: **inner layers never depend on outer layers**.

```
Domain  ←  Application  ←  Infrastructure
                        ←  API
```

### Why not a simpler 3-layer (Controllers → Services → DB)?

A traditional 3-layer architecture works fine for small apps but it couples business logic to infrastructure concerns. With Clean Architecture:

**1. The Domain layer has zero dependencies.**
The `User`, `Quote`, `GameHistory` entities know nothing about EF Core, SQL Server, or HTTP. This means the business rules can be tested in complete isolation without spinning up a database.

**2. The Application layer owns the interfaces.**
`IRepository<T>`, `IUnitOfWork`, `IAuthService` are all defined in the Application layer — not in Infrastructure. This is the **Dependency Inversion Principle** in practice: high-level policy (business logic) defines the contracts; low-level detail (EF Core, SQL Server) implements them. Swapping SQL Server for PostgreSQL means only changing the Infrastructure layer — Application and Domain stay untouched.

**3. DTOs keep domain entities off the wire.**
No domain entity is ever returned directly from a controller. Every response goes through a DTO. This prevents accidentally exposing sensitive fields (like `PasswordHash`) and decouples the API contract from the internal data model. Adding a field to `User` doesn't automatically change the API surface.

**4. Separation of concerns is explicit by layer, not by convention.**
Each project has one job: Domain defines what things are, Application defines what can be done, Infrastructure defines how data is stored, API defines how the outside world communicates.

---

## Design Patterns

### Repository Pattern

**What:** Abstracts data access behind a generic interface.

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
```

**Why:** Services like `AuthService` and `QuizService` work against `IRepository<User>` and `IRepository<Quote>` — not against `DbContext` directly. In tests, these are replaced with in-memory mocks. No real database is needed to test business logic.

**Why not use EF Core directly in services?** EF Core's `DbContext` is already an abstraction, but it's a leaky one — it exposes LINQ, change tracking, and migration APIs. The repository gives services a clean, minimal interface that focuses on what the service actually needs.

---

### Unit of Work Pattern

**What:** Groups multiple repository operations into a single transaction.

```csharp
public class UnitOfWork : IUnitOfWork
{
    public IRepository<User> Users { get; }
    public IRepository<Quote> Quotes { get; }
    public IRepository<GameHistory> GameHistories { get; }
    public IRepository<ShownQuote> ShownQuotes { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
```

**Why:** When `SubmitAnswerAsync` creates a `GameHistory` record and a `ShownQuote` record in the same operation, both are staged through their repositories and committed with a single `SaveChangesAsync()` call. If either fails, neither is persisted. This is transactional consistency without explicit `BeginTransaction()` boilerplate — EF Core's change tracker handles it.

**Without UnitOfWork:** Each repository would hold its own `DbContext`, so `GameHistories.AddAsync()` and `ShownQuotes.AddAsync()` would be separate transactions. A failure between them would leave the database in an inconsistent state.

---

### Service Layer

**What:** All business logic lives in dedicated service classes (`AuthService`, `QuizService`, `QuoteService`, `UserService`, `GameHistoryService`, `TokenService`).

**Why:** Controllers are kept thin — they receive a request, call a service, return a result. No business logic in controllers. This means:
- Services are fully testable without HTTP context
- The same service can be called from different endpoints or background jobs in the future
- Each service has a single responsibility (SRP)

---

### Custom Exception Hierarchy

```csharp
DomainException (base)
├── ValidationException    → 400
├── UnauthorizedException  → 401
├── NotFoundException      → 404
└── ConflictException      → 409
```

**Why typed exceptions instead of returning booleans or result objects?**

Services throw meaningful domain exceptions (`ConflictException("email already taken")`). The middleware catches them and maps to the correct HTTP status code. This keeps service code clean — no `if (!result.Success) return BadRequest(result.Error)` scattered through every controller. The error flows naturally up the call stack and is handled in one place.

---

## Authentication & Security

### JWT (JSON Web Tokens)

**Choice:** Stateless JWT over session-based authentication.

**Why:**
- **Stateless** — the server does not store session state. Every request carries a self-contained, signed token. This scales horizontally without a shared session store.
- **Role claims embedded** — the token contains `ClaimTypes.Role`, so role-based authorization (`[Authorize(Roles = "Admin")]`) works without a database round-trip on every request.
- **Standard** — JWT is an industry standard (RFC 7519), supported natively by ASP.NET Core's `JwtBearer` middleware.

**Token configuration:**
- Algorithm: **HMAC-SHA256** — fast and sufficient for a symmetric key setup
- Expiry: **24 hours** — balances security (short-lived) with usability (doesn't expire mid-session)
- `ClockSkew = TimeSpan.Zero` — no tolerance for expired tokens; a token expires exactly when it should

**Claims embedded in every token:**
- `NameIdentifier` — user ID (used by controllers to identify the caller without a DB query)
- `Name` — username
- `Email` — user email
- `Role` — User or Admin

---

### Password Hashing with BCrypt

**Choice:** BCrypt over MD5, SHA-256, or PBKDF2.

**Why BCrypt:**
- **Adaptive cost factor** — BCrypt is intentionally slow. As hardware improves, the work factor can be increased. A brute-force attack that takes 1 second per hash attempt on today's hardware is not viable at scale.
- **Salt built in** — each hash includes a random salt automatically. Two users with the same password get different hashes. Rainbow table attacks are ineffective.
- **Industry standard** — BCrypt is the most widely recommended choice for password hashing in application-level code.

**Security decisions around passwords:**
- Passwords are hashed immediately on registration — the plaintext never touches the database
- Login uses `BCrypt.Verify()` which compares in constant time, preventing timing attacks
- The logging middleware sanitizes request bodies, ensuring passwords never appear in logs
- Wrong password and non-existent user both return `"Invalid credentials"` — the error message does not reveal whether the username exists (prevents account enumeration)

---

### Role-Based Authorization

Two roles exist: `User` and `Admin`.

- `[Authorize]` — any authenticated user
- `[Authorize(Roles = "Admin")]` — admin only

Admin-only endpoints: all user management, quote write operations, full game history access. Regular users can only read their own history and play the quiz.

---

## Validation Strategy

**Choice:** FluentValidation with a global `ValidationFilter`.

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDto>();
```

**Why FluentValidation over Data Annotations:**
- Validation rules are separate from the DTO class — the DTO is a pure data container, the validator is the rules engine
- Rules are composable, reusable, and testable in isolation
- Much richer rule expressions: `.Must()`, `.WithMessage()`, cascading conditions
- Validators are discovered automatically from the Application assembly

**The ValidationFilter** runs before any controller action. If any DTO fails validation, a `400 Bad Request` with grouped error messages is returned immediately — no service code is ever reached with invalid data.

Validators implemented:
- `RegisterDtoValidator` — username length, email format, password minimum length
- `LoginDtoValidator` — required fields
- `SubmitAnswerDtoValidator` — valid quoteId, valid quizMode enum value
- `CreateQuoteDtoValidator` — quote text and author required, max lengths
- `CreateUserDtoValidator` — same rules as register plus role must be `User` or `Admin`

---

## Error Handling

All exception handling is centralised in `ExceptionHandlingMiddleware`.

```
Request → LoggingMiddleware → ExceptionHandlingMiddleware → Controllers
```

**Why middleware instead of try/catch in every controller?**

Without centralised handling, each controller action would need:
```csharp
try { ... }
catch (NotFoundException ex) { return NotFound(ex.Message); }
catch (ConflictException ex) { return Conflict(ex.Message); }
catch (Exception ex) { return StatusCode(500, "..."); }
```

That's repeated in every action. The middleware handles this once:

| Exception Type | HTTP Status |
|---|---|
| `ValidationException` | 400 Bad Request |
| `UnauthorizedException` | 401 Unauthorized |
| `NotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| Anything else | 500 Internal Server Error |

Unhandled exceptions return a generic `"An unexpected error occurred"` message to the client. The real error is logged server-side. This prevents stack traces and internal details from leaking to the client.

---

## Logging

`LoggingMiddleware` logs every HTTP request and response with method, path, status code, and duration.

**Key design decisions:**

**Sensitive data sanitization** — before logging a request body, the middleware redacts any field whose key contains: `password`, `token`, `secret`, `key`, `authorization`, `confirmPassword`. This ensures credentials never appear in log files.

```
// What you see in the log:
POST /api/Auth/login {"emailOrUsername":"admin","password":"****"}
```

**Log level by status code:**
- `5xx` → `LogError` — needs immediate attention
- `4xx` → `LogWarning` — client errors, worth monitoring
- Others → `LogInformation` — normal traffic

**Body size limit** — request bodies larger than 10 KB are not logged to prevent log flooding from large payloads.

---

## Database & Migrations

**EF Core Code-First** with explicit entity configurations in `IEntityTypeConfiguration<T>` classes.

**Why separate configuration classes instead of Data Annotations on entities?**

Data Annotations (`[MaxLength(100)]`, `[Required]`) couple the domain entity to EF Core. The Domain layer would then have a dependency on an infrastructure library. Separate `IEntityTypeConfiguration` classes in the Infrastructure layer keep the Domain layer clean.

**Configurations applied:**
- `UserConfiguration` — unique indexes on Email and Username, max lengths, role stored as string
- `QuoteConfiguration` — max lengths, cascade delete rules
- `ShownQuoteConfiguration` — composite unique index on `(UserId, QuoteId)` prevents duplicate tracking
- `GameHistoryConfiguration` — FK relationships with restrict/cascade rules

**Auto-migration on startup:**
```csharp
await context.Database.MigrateAsync();
await DbInitializer.SeedAsync(context);
```

The database is created and seeded automatically when the application starts. No manual migration steps needed.

**Seeded data:**
- Admin user (`admin` / `Admin123!`)
- 11 famous quotes across 11 different authors

---

## Testing

Tests are in `QuoteQuiz.Tests` using **xUnit** and **Moq**.

```bash
dotnet test
```

**What is tested:**

`AuthServiceTests` — 9 tests covering:
- Successful registration
- Duplicate email/username rejection (ConflictException)
- Password is hashed, never stored as plaintext
- Login by email
- Login by username
- Non-existent user returns generic error (not "user not found" — prevents enumeration)
- Wrong password returns same generic error as above
- Disabled account returns specific message

`QuizServiceTests` — 17 tests covering:
- Binary mode question format
- Multiple choice returns exactly 3 options (1 correct, 2 wrong)
- Unseen quotes are prioritized
- When all quotes are seen, progress resets and all quotes become available again
- Correct/wrong answer evaluation for both modes
- Case-insensitive answer comparison
- `ShownQuote` record created on first answer, not duplicated on repeat

**Why these services are tested first:**

`AuthService` handles security — password hashing, credential verification, and preventing user enumeration are critical correctness requirements. `QuizService` contains the core game logic — the seen/unseen progress tracking is the most complex algorithmic piece in the application.

**Test approach — pure unit tests:**

All services are tested with mocked repositories (`Moq`). No database, no HTTP layer, no EF Core. Tests run in milliseconds and can be run anywhere without infrastructure. This is only possible because of the Repository pattern — swapping `IRepository<User>` for a mock is trivial.

---

## API Reference

### Auth — no token required

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register new user |
| `POST` | `/api/auth/login` | Login, returns JWT token |

### Quiz — authenticated users

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/quiz/question?mode=Binary` | Get next unseen question |
| `GET` | `/api/quiz/question?mode=MultipleChoice` | Get next unseen question |
| `POST` | `/api/quiz/answer` | Submit answer, returns result |

### Quotes — authenticated users (write: Admin only)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/quotes` | Get all quotes |
| `GET` | `/api/quotes/{id}` | Get quote by ID |
| `GET` | `/api/quotes/authors` | Get all unique author names |
| `POST` | `/api/quotes` | Create quote (Admin) |
| `PUT` | `/api/quotes/{id}` | Update quote (Admin) |
| `DELETE` | `/api/quotes/{id}` | Delete quote (Admin) |

### Game History — authenticated users

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/gamehistory/my-history` | Current user's game history |
| `GET` | `/api/gamehistory/my-statistics` | Current user's statistics |
| `GET` | `/api/gamehistory` | All users' history (Admin) |
| `GET` | `/api/gamehistory/user/{id}/statistics` | Specific user's stats (Admin) |

### User Management — Admin only

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/user` | Get all users |
| `GET` | `/api/user/{id}` | Get user by ID |
| `POST` | `/api/user` | Create user |
| `PUT` | `/api/user/{id}` | Update user |
| `PATCH` | `/api/user/{id}/disable` | Disable user account |
| `DELETE` | `/api/user/{id}` | Delete user |
