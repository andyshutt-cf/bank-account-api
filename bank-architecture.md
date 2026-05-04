# Bank Account API – Backend Architecture

## Overview

The Bank Account solution is an ASP.NET Core application split into four projects inside a single solution (`BankAccountSolution.sln`). The backend REST API (`BankAccountAPI`) is the focus of this document, but each project's role is described so the full picture is clear.

```
BankAccountSolution
├── BankAccountAPI          # REST API (backend)
├── BankAccountAPI.Tests    # Unit & end-to-end tests for the API
├── BankAccountUI           # Razor Pages front-end (consumes the API)
└── BankAccountUI.Tests     # Selenium UI tests for the front-end
```

---

## Technology Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 8 |
| API framework | ASP.NET Core Web API |
| Front-end framework | ASP.NET Core Razor Pages |
| Data storage | In-memory `List<BankAccount>` (no external database) |
| ORM / data library | Microsoft.EntityFrameworkCore.InMemory (package referenced but not used) |
| JSON serialisation | `Microsoft.AspNetCore.Mvc.NewtonsoftJson` |
| API exploration | Swashbuckle / Swagger (`Swashbuckle.AspNetCore`) |
| Unit testing | NUnit + Moq |
| UI testing | Selenium WebDriver + ChromeDriver |
| CI pipeline | Azure Pipelines (`azure-pipelines.yml`) |

---

## Project: BankAccountAPI

### Folder Structure

```
BankAccountAPI/
├── Controllers/
│   ├── BankAccountController.cs   # CRUD endpoints for bank accounts
│   └── PrimeController.cs         # Utility endpoint – prime-number check
├── Models/
│   └── BankAccount.cs             # Domain model
├── Services/
│   ├── IBankAccountService.cs     # Service abstraction
│   ├── BankAccountService.cs      # In-memory service implementation
│   └── PrimeService.cs            # Prime-number business logic
├── Program.cs                     # Host builder entry point
└── Startup.cs                     # DI registration, middleware, seed data
```

### Architectural Layers

```
┌──────────────────────────────────────────────────┐
│                   HTTP Client                    │  (BankAccountUI / external callers)
└──────────────────────┬───────────────────────────┘
                       │ HTTP (JSON)
┌──────────────────────▼───────────────────────────┐
│              Controllers (API Layer)             │
│  BankAccountController   PrimeController         │
└──────────────────────┬───────────────────────────┘
                       │ Interface (IBankAccountService)
┌──────────────────────▼───────────────────────────┐
│               Service Layer                      │
│  BankAccountService      PrimeService            │
└──────────────────────┬───────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────┐
│             In-Memory Data Store                 │
│         static List<BankAccount>                 │
└──────────────────────────────────────────────────┘
```

---

### Domain Model – `BankAccount`

**Namespace:** `BankAccountAPI.Models`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Unique identifier |
| `AccountNumber` | `string?` | Human-readable account label |
| `AccountHolderName` | `string?` | Name of the account owner |
| `Balance` | `decimal` | Current balance (initialised to `0.0`) |

**Domain methods on `BankAccount`:**

| Method | Description |
|---|---|
| `Deposit(amount, transactionType)` | Adds `amount` to `Balance`. Requires `transactionType` to end with `"Credit"`. Throws `ArgumentException` for invalid type or non-positive amount. |
| `Withdraw(amount, transactionType)` | Subtracts `amount` from `Balance`. Requires `transactionType` to end with `"Debit"`. Throws `ArgumentException` for invalid type or non-positive amount; `InvalidOperationException` for insufficient funds. |
| `Transfer(toAccount, amount)` | Moves `amount` from this account's balance to `toAccount.Balance`. Throws for non-positive amount or insufficient funds. |

---

### Service Layer

#### `IBankAccountService` (interface)

**Namespace:** `BankAccountAPI.Services`

```
InitializeAccounts(List<BankAccount>)   - Replace the entire account list (used for seeding)
GetAllAccounts()                        - Return all accounts
GetAccountById(int id)                  - Return a single account or throw InvalidOperationException
AddAccount(BankAccount)                 - Append an account to the list
CreateAccount(BankAccount)              - Alias for AddAccount (used by the controller)
UpdateAccount(BankAccount)              - Update an existing account by Id; throws KeyNotFoundException if not found
DeleteAccount(int id)                   - Remove an account by Id; throws KeyNotFoundException if not found
```

#### `BankAccountService` (implementation)

**Namespace:** `BankAccountAPI.Services`

- Backed by a `static List<BankAccount> _accounts` field.  
- Because the field is `static`, all DI-scoped instances share the same in-memory state for the lifetime of the process.
- No persistence; data is lost on application restart (seed data is re-generated on each startup).

#### `PrimeService`

**Namespace:** `BankAccountAPI.Services`

Provides a single method `IsPrime(int number) → bool` using a trial-division algorithm up to `√number`.

---

### Controllers

#### `BankAccountController`

**Route:** `api/bankaccount`  
**Namespace:** `BankAccountAPI.Controllers`

Injects `IBankAccountService` via constructor injection.

| HTTP Method | Route | Action | Success Response |
|---|---|---|---|
| `GET` | `api/bankaccount` | `GetAllAccounts` | `200 OK` – array of `BankAccount` |
| `GET` | `api/bankaccount/{id}` | `GetAccountById` | `200 OK` – single `BankAccount`; `404 Not Found` if missing |
| `POST` | `api/bankaccount` | `CreateAccount` | `201 Created` with `Location` header |
| `PUT` | `api/bankaccount/{id}` | `UpdateAccount` | `204 No Content`; `400 Bad Request` if route id ≠ body id |
| `DELETE` | `api/bankaccount/{id}` | `DeleteAccount` | `204 No Content`; `404 Not Found` if the account does not exist |

#### `PrimeController`

**Route:** `api/prime`  
**Namespace:** `BankAccountAPI.Controllers`

Injects `PrimeService` via constructor injection.

| HTTP Method | Route | Action | Success Response |
|---|---|---|---|
| `GET` | `api/prime/{number}` | `IsPrime` | `200 OK` – `true` or `false` |

---

### Startup & Dependency Injection

**File:** `BankAccountAPI/Startup.cs`

Service registrations (`ConfigureServices`):

```csharp
services.AddCors(...)                                   // CORS policy "AllowBankAccountUI"
services.AddControllers();
services.AddScoped<IBankAccountService, BankAccountService>();
services.AddScoped<PrimeService>();
```

Middleware pipeline (`Configure`):

```
UseRouting → UseCors("AllowBankAccountUI") → UseEndpoints (MapControllers)
```

**CORS policy:** Allows requests from `http://localhost:5074` (the default BankAccountUI address) with any method and header.

**Seed data (`PopulateAccountData`):** On every startup, 20 `BankAccount` instances are created with random balances, names, and account types. Each account then receives 100 randomised deposit/withdraw transactions, followed by cross-account transfers between all account pairs. The fully initialised list is handed to `IBankAccountService.InitializeAccounts(...)`.

---

## Project: BankAccountUI

A companion Razor Pages front-end that displays accounts fetched from the API.

- **Framework:** ASP.NET Core Razor Pages (.NET 8)
- **API communication:** `IHttpClientFactory` is used to create an `HttpClient` named `"BankAccountAPI"`. The `BankAccountsModel` page model calls `GET api/bankaccount` on `OnGetAsync` and binds the result to a `List<BankAccount>` for rendering.
- **API base URL:** Configured via `appsettings.json`; defaults to the API address so the UI and API can be run independently.

---

## Project: BankAccountAPI.Tests

**Test framework:** NUnit  
**Mocking:** Moq

| Test class | Location | What it tests |
|---|---|---|
| `BankAccountControllerTest` | `Controllers/` | HTTP response codes and return values from `BankAccountController` using a mocked `IBankAccountService` |
| `BankAccountServiceTest` | `Services/` | CRUD operations on the real `BankAccountService` with an isolated in-memory list |
| `PrimeServiceTest` | `Services/` | `PrimeService.IsPrime` correctness for various inputs |
| `BankAccountApiTests` | `EndToEndTests/` | Full end-to-end HTTP round-trips against a running API instance |

---

## Project: BankAccountUI.Tests

Selenium WebDriver tests that automate a Chrome browser, navigate to the BankAccountUI, and assert that account data is visible on screen.

---

## Key Design Decisions

1. **In-memory storage** – `BankAccountService` uses a `static List<T>` rather than a database. This simplifies development and testing but means data does not persist across restarts and is shared across all DI-scoped instances.

2. **Seed data on startup** – `Startup.PopulateAccountData` populates accounts with realistic-looking random transactions so the UI is immediately useful without a separate data-loading step.

3. **Scoped service, static backing field** – `BankAccountService` is registered as `Scoped` but its `_accounts` list is `static`. In a production system this should be replaced with a proper database-backed repository.

4. **Interface-driven services** – `IBankAccountService` decouples the controller from the implementation, enabling straightforward unit testing with Moq.

5. **CORS** – A named CORS policy restricts cross-origin calls to the known UI origin, preventing unintended browser-side access from other domains.

6. **Separate front-end project** – `BankAccountUI` is a distinct ASP.NET Core project that communicates with the API over HTTP. This keeps the API stateless and allows the UI and API to be deployed and scaled independently.
