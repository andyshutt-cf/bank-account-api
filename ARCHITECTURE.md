# Bank Account API Architecture

This document describes the backend architecture of the BankAccountAPI project.

## 1. Overview

The **BankAccountAPI** is a simple ASP.NET Core Web API for banking account management. It provides:

- **CRUD operations** for bank accounts (create, read, update, delete)
- **Prime number checking** utility endpoint
- **Domain operations** on accounts (deposit, withdraw, transfer)

### Relationship to BankAccountUI

The BankAccountAPI serves as the backend for the **BankAccountUI** Razor Pages front-end application. The UI fetches account data via HTTP calls to the API endpoints. CORS is configured to allow requests from the UI running on `http://localhost:5074`.

---

## 2. High-Level Structure

### Solution Layout

```
BankAccountSolution.sln
├── BankAccountAPI/           # REST API backend
│   ├── Controllers/          # HTTP endpoint handlers
│   ├── Models/               # Domain entities
│   ├── Services/             # Business logic layer
│   ├── Transactions/         # Future micro-feature (placeholder)
│   ├── Program.cs            # Application entry point
│   └── Startup.cs            # DI and middleware configuration
├── BankAccountUI/            # Razor Pages front-end
│   ├── Pages/                # UI pages
│   └── Models/               # UI-specific models
├── BankAccountAPI.Tests/     # API unit and E2E tests
│   ├── Controllers/          # Controller tests
│   ├── Services/             # Service tests
│   ├── EndToEndTests/        # Integration tests
│   └── Models/               # Model tests
└── BankAccountUI.Tests/      # Selenium UI tests
```

### Execution Flow

```
Program.cs
    └── CreateHostBuilder()
            └── Startup.cs
                    ├── ConfigureServices()  → Register DI services
                    └── Configure()          → Build middleware pipeline
                            └── Controllers
                                    └── Services
                                            └── Models
```

---

## 3. Hosting & Middleware

### ASP.NET Core Generic Host

The application uses the generic host pattern:

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });
```

### Middleware Pipeline

The `Configure` method sets up the middleware in this order:

1. **Developer Exception Page** (Development only)
2. **Routing** (`UseRouting`)
3. **CORS** (`UseCors("AllowBankAccountUI")`)
4. **Endpoints** (`MapControllers`)

### CORS Policy

A named policy `AllowBankAccountUI` permits cross-origin requests from the UI:

```csharp
options.AddPolicy("AllowBankAccountUI",
    builder => builder
        .WithOrigins("http://localhost:5074")
        .AllowAnyMethod()
        .AllowAnyHeader());
```

---

## 4. Dependency Injection

### Service Registrations

| Service Interface | Implementation | Lifetime |
|-------------------|----------------|----------|
| `IBankAccountService` | `BankAccountService` | Scoped |
| `PrimeService` | `PrimeService` | Scoped |

### Stateful Service Concern

**Important**: `BankAccountService` uses a **static** in-memory list (`static List<BankAccount> _accounts`) for data storage. Although registered as **Scoped**, the static nature means:

- All service instances share the same data
- Data persists across requests within the application lifetime
- The scoped registration has no effect on data isolation
- Consider this a "pseudo-singleton" pattern for data storage

This design is suitable for demos but problematic for production (see [State & Concurrency](#9-state--concurrency)).

---

## 5. Data Model

### BankAccount Entity

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `int` | Unique identifier |
| `AccountNumber` | `string?` | Account number (e.g., "Account 1") |
| `AccountHolderName` | `string?` | Name of account holder |
| `Balance` | `decimal` | Current balance (default: 0.0) |

### Domain Operations

#### Deposit

```csharp
void Deposit(decimal amount, string transactionType)
```

- **Validation**: `transactionType` must end with "Credit"
- **Validation**: `amount` must be positive
- **Behavior**: Adds amount to balance
- **Throws**: `ArgumentException` on invalid input

#### Withdraw

```csharp
void Withdraw(decimal amount, string transactionType)
```

- **Validation**: `transactionType` must end with "Debit"
- **Validation**: `amount` must be positive
- **Validation**: `amount` must not exceed balance
- **Special Case**: If `amount == Balance`, returns without modifying balance
- **Throws**: `ArgumentException` or `InvalidOperationException`

#### Transfer

```csharp
void Transfer(BankAccount toAccount, decimal amount)
```

- **Validation**: `amount` must be positive
- **Validation**: `amount` must not exceed balance
- **Behavior**: Deducts from source, adds to destination
- **Throws**: `ArgumentException` or `InvalidOperationException`

---

## 6. Services Layer

### BankAccountService

Implements `IBankAccountService` and provides:

| Method | Description |
|--------|-------------|
| `GetAllAccounts()` | Returns all accounts |
| `GetAccountById(int id)` | Returns account or throws `InvalidOperationException` |
| `AddAccount(BankAccount)` | Adds account to list |
| `CreateAccount(BankAccount)` | Same as AddAccount |
| `UpdateAccount(BankAccount)` | Updates existing account or throws `KeyNotFoundException` |
| `DeleteAccount(int id)` | Removes account or throws `KeyNotFoundException` |
| `InitializeAccounts(List<BankAccount>)` | Replaces entire account list (used for seeding) |

#### Error Handling Approach

The service uses **exceptions** for error conditions:

- `InvalidOperationException` - Account not found (GetAccountById)
- `KeyNotFoundException` - Account not found (Update/Delete)

This approach requires callers to handle exceptions, though not all controller methods currently do so.

### PrimeService

A **stateless utility service** with a single method:

```csharp
bool IsPrime(int number)
```

**Algorithm**: Trial division up to `sqrt(n)`

- Returns `false` for numbers ≤ 1
- Tests divisibility for all integers from 2 to √n
- Time complexity: O(√n)

---

## 7. Controllers

### BankAccountController

Route: `/api/BankAccount`

| Method | Route | Description | Success Code | Error Code |
|--------|-------|-------------|--------------|------------|
| GET | `/` | Get all accounts | 200 OK | - |
| GET | `/{id}` | Get account by ID | 200 OK | 404 Not Found |
| POST | `/` | Create account | 201 Created | - |
| PUT | `/{id}` | Update account | 204 No Content | 400 Bad Request |
| DELETE | `/{id}` | Delete account | 204 No Content | - |

**Note**: The DELETE endpoint doesn't return 404 if the account doesn't exist (exception propagates).

### PrimeController

Route: `/api/prime`

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/{number}` | Check if number is prime | 200 OK with `true`/`false` |

---

## 8. Initialization/Seeding

### PopulateAccountData Method

During application startup, `Startup.PopulateAccountData()` seeds the database with test data:

1. **Creates 20 accounts** with random:
   - Balance (10-50000)
   - Account holder name (from predefined list)
   - Account type (Savings, Checking, etc.)

2. **Executes 100 random transactions per account**:
   - Deposits (positive amounts)
   - Withdrawals (negative amounts)

3. **Performs random transfers** between all account pairs

### Side Effects and Concerns

| Concern | Description |
|---------|-------------|
| **Console Output** | Extensive `Console.WriteLine` calls create noise during startup |
| **Synchronous Execution** | Blocks application startup until seeding completes |
| **Non-deterministic** | `Random` seeded with `DateTime.Now.Millisecond` produces different data each run |
| **Performance** | Creates 20 accounts × 100 transactions = 2000 transactions, then 20×19 = 380 transfers |
| **Exception Swallowing** | Catches and logs exceptions without re-throwing |

---

## 9. State & Concurrency

### Current Implementation

```csharp
private static List<BankAccount> _accounts = new List<BankAccount>();
```

### Thread-Safety Concerns

The static `List<BankAccount>` is **not thread-safe**:

| Operation | Risk |
|-----------|------|
| Concurrent reads during write | Collection modified exception |
| Concurrent writes | Data corruption |
| Read-modify-write (e.g., Transfer) | Race conditions, lost updates |
| Find + Remove patterns | Item may be removed between find and remove |

### Potential Race Conditions

1. **Account lookup + operation**: Account could be deleted between lookup and operation
2. **Balance updates**: Concurrent deposits/withdrawals could result in incorrect balances
3. **Transfer operations**: Non-atomic withdrawal + deposit could leave inconsistent state

### Mitigation (Not Currently Implemented)

- Use `ConcurrentDictionary<int, BankAccount>` or
- Add explicit locking (`lock` statements) or
- Replace with proper persistence layer with transaction support

---

## 10. Error Handling & Validation

### Current Approach

The codebase uses a **mixed approach**:

| Layer | Approach |
|-------|----------|
| **Model** | Throws exceptions for validation failures |
| **Service** | Throws exceptions for not-found conditions |
| **Controller** | Some methods check for null and return appropriate status codes |

### Missing Components

- **Global Exception Handling Middleware**: Unhandled exceptions may result in 500 errors without proper logging
- **Validation Attributes**: No `[Required]`, `[Range]`, etc. on model properties
- **Model State Validation**: Controllers don't validate `ModelState.IsValid`
- **Consistent Error Response Format**: No standardized error response structure

---

## 11. Cross-Cutting Concerns (Currently Absent)

| Concern | Status | Recommendation |
|---------|--------|----------------|
| **Logging** | ❌ Uses `Console.WriteLine` | Use `ILogger<T>` abstraction |
| **Persistence** | ❌ In-memory static list | Implement repository pattern with EF Core |
| **DTOs** | ❌ Exposes domain models directly | Create separate request/response DTOs |
| **Mapping** | ❌ Manual property assignment | Use AutoMapper or Mapperly |
| **Authentication** | ❌ No auth implemented | Add JWT or Identity authentication |
| **Authorization** | ❌ No authorization | Implement role-based access control |
| **API Documentation** | ❌ No Swagger/OpenAPI | Add Swashbuckle |
| **Health Checks** | ❌ None configured | Add ASP.NET Core health checks |

---

## 12. Testing Architecture

### Test Projects

#### BankAccountAPI.Tests

```
BankAccountAPI.Tests/
├── Controllers/
│   └── BankAccountControllerTest.cs    # Controller unit tests
├── Services/
│   ├── BankAccountServiceTest.cs       # Service unit tests
│   └── PrimeServiceTest.cs             # Prime service tests
├── Models/                              # Model tests
└── EndToEndTests/
    └── BankAccountApiTests.cs          # Integration tests using WebApplicationFactory
```

#### BankAccountUI.Tests

- **Selenium WebDriver** for browser automation
- **ChromeDriver** for Chrome testing
- **NUnit** test framework

### Test Characteristics

| Type | Framework | Approach |
|------|-----------|----------|
| Unit Tests | NUnit + Moq + FluentAssertions | Isolated component testing |
| Integration Tests | NUnit + WebApplicationFactory | Full HTTP pipeline testing |
| UI Tests | NUnit + Selenium | Browser automation |

### Testing Gaps

- ❌ No concurrency/thread-safety tests
- ❌ No load/performance tests
- ❌ No negative test cases for some endpoints
- ❌ No tests for seeding determinism
- ❌ PrimeController integration tests not present

---

## 13. Suggested Improvements

### Data & Persistence

1. **Replace static storage with repository pattern**
   ```csharp
   public interface IBankAccountRepository
   {
       Task<IEnumerable<BankAccount>> GetAllAsync();
       Task<BankAccount?> GetByIdAsync(int id);
       Task AddAsync(BankAccount account);
       Task UpdateAsync(BankAccount account);
       Task DeleteAsync(int id);
   }
   ```

2. **Implement Entity Framework Core**
   - Add `DbContext` with proper entity configuration
   - Use migrations for schema management
   - Enable proper transaction support

### Validation

3. **Add domain validation layer**
   - Consider FluentValidation for complex rules
   - Add data annotations to models
   - Implement `IValidatableObject` for cross-property validation

### Error Handling

4. **Implement global exception handling middleware**
   ```csharp
   app.UseExceptionHandler(errorApp =>
   {
       errorApp.Run(async context =>
       {
           // Return standardized error response
       });
   });
   ```

### API Design

5. **Introduce DTOs/ViewModels**
   - `CreateAccountRequest`, `UpdateAccountRequest`
   - `AccountResponse`, `AccountListResponse`
   - Separation of internal/external representations

6. **Use async patterns throughout**
   - `async/await` in services and controllers
   - `CancellationToken` support for request cancellation

### Observability

7. **Replace Console.WriteLine with ILogger**
   ```csharp
   private readonly ILogger<BankAccountService> _logger;
   
   _logger.LogInformation("Account {Id} created", account.Id);
   ```

8. **Add structured logging**
   - Use Serilog or similar
   - Log to appropriate sinks (file, seq, application insights)

### Thread Safety

9. **Address concurrency concerns**
   - Option A: Use `lock` statements around list operations
   - Option B: Use `ConcurrentDictionary`
   - Option C: Implement persistence layer (recommended)

### Testing

10. **Expand test coverage**
    - Add concurrency tests
    - Add PrimeController integration tests
    - Add seeding determinism tests (fixed seed option)
    - Add negative test cases

---

## 14. Sequence Diagram

### Typical Request Flow

```
┌──────────┐     ┌────────────────────┐     ┌────────────────────┐     ┌──────────┐
│  Client  │     │    Controller      │     │      Service       │     │  Model   │
└────┬─────┘     └─────────┬──────────┘     └──────────┬─────────┘     └────┬─────┘
     │                     │                           │                    │
     │  HTTP GET /api/BankAccount/1                    │                    │
     │────────────────────>│                           │                    │
     │                     │                           │                    │
     │                     │  GetAccountById(1)        │                    │
     │                     │──────────────────────────>│                    │
     │                     │                           │                    │
     │                     │                           │  _accounts.Find()  │
     │                     │                           │───────────────────>│
     │                     │                           │                    │
     │                     │                           │  BankAccount       │
     │                     │                           │<───────────────────│
     │                     │                           │                    │
     │                     │  BankAccount              │                    │
     │                     │<──────────────────────────│                    │
     │                     │                           │                    │
     │  200 OK + JSON      │                           │                    │
     │<────────────────────│                           │                    │
     │                     │                           │                    │
```

### Transfer Operation Flow

```
┌──────────┐     ┌────────────────────┐     ┌──────────────────────────┐
│  Client  │     │  BankAccount (src) │     │   BankAccount (dest)     │
└────┬─────┘     └─────────┬──────────┘     └────────────┬─────────────┘
     │                     │                             │
     │  Transfer(dest, 100)│                             │
     │────────────────────>│                             │
     │                     │                             │
     │                     │  Validate amount > 0        │
     │                     │  Validate amount <= Balance │
     │                     │                             │
     │                     │  Balance -= 100             │
     │                     │──────────────────────────────
     │                     │                             │
     │                     │  dest.Balance += 100        │
     │                     │────────────────────────────>│
     │                     │                             │
     │  Success            │                             │
     │<────────────────────│                             │
```

---

## 15. Future Extension Points

### Transactions Micro-Feature

A `Transactions/` folder exists with placeholder structure:

```
Transactions/
├── Controllers/
│   └── TransactionController.cs  (empty)
├── Models/
│   └── TransactionModel.cs
└── Services/
    ├── ITransactionService.cs
    └── TransactionService.cs
```

This suggests planned transaction history tracking functionality.

### Potential Bounded Contexts

The codebase could be segregated into:

1. **Account Management Context** - CRUD operations
2. **Transaction Context** - Deposits, withdrawals, transfers, history
3. **Utility Context** - Prime service (potentially removable or separate)

### API Versioning

Consider implementing API versioning for future compatibility:

```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
```

Route patterns could evolve to:
- `/api/v1/bankaccount`
- `/api/v2/bankaccount`

---

## Component Interaction Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BankAccountAPI                                  │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                         ASP.NET Core Host                              │  │
│  │  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                │  │
│  │  │   Routing   │───>│    CORS     │───>│  Endpoints  │                │  │
│  │  └─────────────┘    └─────────────┘    └──────┬──────┘                │  │
│  └───────────────────────────────────────────────┼───────────────────────┘  │
│                                                  │                          │
│  ┌───────────────────────────────────────────────▼───────────────────────┐  │
│  │                           Controllers                                  │  │
│  │  ┌─────────────────────────┐    ┌─────────────────────────┐           │  │
│  │  │  BankAccountController  │    │     PrimeController     │           │  │
│  │  │  /api/BankAccount/*     │    │     /api/prime/{n}      │           │  │
│  │  └───────────┬─────────────┘    └───────────┬─────────────┘           │  │
│  └──────────────┼──────────────────────────────┼─────────────────────────┘  │
│                 │                              │                            │
│  ┌──────────────▼──────────────────────────────▼─────────────────────────┐  │
│  │                            Services                                    │  │
│  │  ┌─────────────────────────┐    ┌─────────────────────────┐           │  │
│  │  │   BankAccountService    │    │      PrimeService       │           │  │
│  │  │   (IBankAccountService) │    │      (Stateless)        │           │  │
│  │  └───────────┬─────────────┘    └─────────────────────────┘           │  │
│  └──────────────┼────────────────────────────────────────────────────────┘  │
│                 │                                                           │
│  ┌──────────────▼────────────────────────────────────────────────────────┐  │
│  │                             Models                                     │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                        BankAccount                               │  │  │
│  │  │  - Id, AccountNumber, AccountHolderName, Balance                │  │  │
│  │  │  - Deposit(), Withdraw(), Transfer()                            │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                          Data Storage                                  │  │
│  │               static List<BankAccount> _accounts                       │  │
│  │                    (In-Memory, Not Thread-Safe)                        │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │ HTTP (CORS: AllowBankAccountUI)
                                    │
┌───────────────────────────────────┴─────────────────────────────────────────┐
│                             BankAccountUI                                    │
│                      (Razor Pages on localhost:5074)                         │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Summary

The BankAccountAPI is a straightforward demonstration project showcasing:

- ASP.NET Core Web API patterns
- Basic CRUD operations
- Domain model with validation
- Service layer abstraction
- Simple dependency injection

However, it lacks several production-ready features including proper persistence, logging, authentication, and thread safety. The architecture documentation above identifies these gaps and provides a roadmap for improvement.
