# Bank Account API — Backend Architecture

## Overview

The backend is a RESTful API built with **ASP.NET Core** (.NET) following a layered **MVC (Model-View-Controller)** architecture. It exposes HTTP endpoints for managing bank accounts and runs as a self-contained web service that the `BankAccountUI` front-end consumes.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Runtime | .NET (ASP.NET Core) |
| API Style | RESTful (JSON) |
| Dependency Injection | Built-in ASP.NET Core DI container |
| Data Storage | In-memory `List<BankAccount>` (no persistent database) |
| Testing Framework | NUnit + Moq |

---

## Project Structure

```
BankAccountAPI/
├── Program.cs          # Application entry point
├── Startup.cs          # DI registration and middleware pipeline
├── Controllers/
│   ├── BankAccountController.cs   # Bank account CRUD endpoints
│   └── PrimeController.cs         # Prime number utility endpoint
├── Models/
│   └── BankAccount.cs             # Domain model
└── Services/
    ├── IBankAccountService.cs     # Service interface
    ├── BankAccountService.cs      # Business logic & in-memory data store
    └── PrimeService.cs            # Prime number utility service
```

---

## Layers

### 1. Entry Point – `Program.cs`

`Program.cs` is the application entry point. It calls `CreateHostBuilder`, which configures the ASP.NET Core generic host and delegates further setup to `Startup`.

```
Main() → CreateHostBuilder() → UseStartup<Startup>() → Build().Run()
```

---

### 2. Application Configuration – `Startup.cs`

`Startup` is split into two responsibilities:

#### `ConfigureServices` (Dependency Injection)

Registers all services with the DI container:

| Registration | Lifetime | Purpose |
|---|---|---|
| `IBankAccountService` → `BankAccountService` | Scoped | Bank account business logic |
| `PrimeService` | Scoped | Prime number calculation |
| CORS policy `AllowBankAccountUI` | — | Permits requests from `http://localhost:5074` |

#### `Configure` (Middleware Pipeline)

Defines the HTTP request pipeline in order:

1. **Developer Exception Page** – detailed errors in Development environment
2. **Routing** – maps incoming requests to controller actions
3. **CORS** – applies the `AllowBankAccountUI` policy
4. **Endpoints** – maps all controller routes

After the pipeline is configured, `PopulateAccountData` is called once at startup to seed 20 randomly generated bank accounts with 100 simulated transactions each, plus a round of inter-account transfers.

---

### 3. Controllers

Controllers receive HTTP requests, delegate to a service, and return HTTP responses. They contain **no business logic**.

#### `BankAccountController` – `/api/BankAccount`

| Method | Route | Action | Response |
|---|---|---|---|
| GET | `/api/BankAccount` | `GetAllAccounts` | `200 OK` – list of all accounts |
| GET | `/api/BankAccount/{id}` | `GetAccountById` | `200 OK` or `404 Not Found` |
| POST | `/api/BankAccount` | `CreateAccount` | `201 Created` with `Location` header |
| PUT | `/api/BankAccount/{id}` | `UpdateAccount` | `204 No Content` or `400 Bad Request` |
| DELETE | `/api/BankAccount/{id}` | `DeleteAccount` | `204 No Content` |

The controller depends on `IBankAccountService` (injected via constructor), keeping it decoupled from the concrete implementation.

#### `PrimeController` – `/api/prime`

| Method | Route | Action | Response |
|---|---|---|---|
| GET | `/api/prime/{number}` | `IsPrime` | `200 OK` – `true` or `false` |

A lightweight utility endpoint backed by `PrimeService`.

---

### 4. Models

#### `BankAccount`

The core domain object representing a single bank account.

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Unique identifier |
| `AccountNumber` | `string?` | Human-readable account label |
| `AccountHolderName` | `string?` | Name of the account owner |
| `Balance` | `decimal` | Current account balance (defaults to `0.0m`) |

The model also encapsulates the following domain behaviours:

- **`Deposit(amount, transactionType)`** – adds funds; requires a `"Credit"` transaction type and a positive amount.
- **`Withdraw(amount, transactionType)`** – removes funds; requires a `"Debit"` transaction type, a positive amount, and sufficient funds.
- **`Transfer(toAccount, amount)`** – moves funds between two accounts atomically by calling the domain logic directly on both objects.

---

### 5. Services

Services contain all business logic and own the in-memory data store.

#### `IBankAccountService` (interface)

Defines the contract for bank account operations:

```csharp
void   InitializeAccounts(List<BankAccount> accounts);
IEnumerable<BankAccount> GetAllAccounts();
BankAccount              GetAccountById(int id);
void   AddAccount(BankAccount account);
void   CreateAccount(BankAccount account);
void   UpdateAccount(BankAccount account);
void   DeleteAccount(int id);
```

#### `BankAccountService` (implementation)

Backed by a static `List<BankAccount>`. This means all application instances share the same in-memory list for the lifetime of the process (no external database). Key implementation details:

- `GetAccountById` throws `InvalidOperationException` if the account is not found.
- `DeleteAccount` and `UpdateAccount` throw `KeyNotFoundException` if the target account does not exist.
- `InitializeAccounts` replaces the entire list, used by `Startup` to seed data.

#### `PrimeService`

A standalone utility service that checks whether a given integer is prime using trial division up to `√n`.

---

## Request / Response Flow

```
HTTP Client
    │
    ▼
[ Startup Middleware Pipeline ]
    │  1. Routing
    │  2. CORS
    │  3. Endpoint mapping
    ▼
[ Controller ]          (validates HTTP input, delegates to service)
    │
    ▼
[ Service ]             (business logic, in-memory data access)
    │
    ▼
[ Model ]               (domain object with encapsulated behaviour)
    │
    ▼
[ Controller ]          (maps result to HTTP response)
    │
    ▼
HTTP Response (JSON)
```

---

## CORS Configuration

The API allows cross-origin requests from the `BankAccountUI` front-end only:

| Setting | Value |
|---|---|
| Allowed Origin | `http://localhost:5074` |
| Allowed Methods | Any |
| Allowed Headers | Any |

---

## Data Storage

The API uses an **in-memory list** (`static List<BankAccount>`) as its data store. There is no external database or ORM. All data is lost when the process restarts. Seed data is generated on startup by `Startup.PopulateAccountData`.

---

## Testing Architecture

Tests are in the `BankAccountAPI.Tests` project, structured to mirror the main project:

```
BankAccountAPI.Tests/
├── Controllers/    # Unit tests for BankAccountController (Moq + NUnit)
├── Services/       # Unit tests for BankAccountService (NUnit)
├── Models/         # Unit tests for BankAccount domain logic (NUnit)
└── EndToEndTests/  # End-to-end tests verifying the full request pipeline
```

- **Unit Tests** use `Moq` to mock `IBankAccountService`, isolating controller behaviour.
- **End-to-End Tests** exercise the full HTTP pipeline (controller → service → model) against an in-process test server.

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Interface-based service (`IBankAccountService`) | Enables mocking in unit tests and future swappable implementations |
| In-memory data store | Simplifies deployment; suitable for demonstrations and local development |
| Domain methods on the model (`Deposit`, `Withdraw`, `Transfer`) | Keeps business rules close to the data they operate on |
| Scoped DI lifetime for services | Ensures a fresh service instance per HTTP request |
| Seed data generated at startup | Ensures the API is immediately usable without manual data entry |
