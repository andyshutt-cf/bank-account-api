# Backend Architecture Documentation

## Overview

The Bank Account API is a RESTful web service built using **ASP.NET Core 8.0** following the **Model-View-Controller (MVC)** architectural pattern with a clean separation of concerns. The backend uses **dependency injection** for loose coupling and testability, and implements an in-memory data store for managing bank account information.

## Architecture Layers

### 1. **Presentation Layer (Controllers)**
The controllers handle HTTP requests and return appropriate responses. They act as the entry point for client requests.

#### Key Components:
- **BankAccountController** (`Controllers/BankAccountController.cs`)
  - Handles CRUD operations for bank accounts
  - Endpoints:
    - `GET /api/BankAccount` - Retrieve all bank accounts
    - `GET /api/BankAccount/{id}` - Retrieve a specific account by ID
    - `POST /api/BankAccount` - Create a new bank account
    - `PUT /api/BankAccount/{id}` - Update an existing account
    - `DELETE /api/BankAccount/{id}` - Delete a bank account
  - Uses dependency injection to receive `IBankAccountService`
  - Returns HTTP status codes (200 OK, 201 Created, 204 No Content, 404 Not Found, 400 Bad Request)

- **PrimeController** (`Controllers/PrimeController.cs`)
  - Utility endpoint for checking if a number is prime
  - Endpoint: `GET /api/prime/{number}`
  - Uses `PrimeService` for business logic

### 2. **Business Logic Layer (Services)**
Services contain the core business logic and data manipulation operations. They are abstracted behind interfaces to enable testability and maintainability.

#### Key Components:
- **IBankAccountService** (Interface) - Defines the contract for bank account operations
  ```
  - InitializeAccounts(List<BankAccount>) - Populate initial data
  - GetAllAccounts() - Retrieve all accounts
  - GetAccountById(int) - Retrieve account by ID
  - CreateAccount(BankAccount) - Add new account
  - UpdateAccount(BankAccount) - Modify existing account
  - DeleteAccount(int) - Remove account by ID
  ```

- **BankAccountService** (`Services/BankAccountService.cs`)
  - Implements `IBankAccountService`
  - Manages an in-memory static list of bank accounts
  - Provides error handling for operations like account not found
  - Registered as **Scoped** service in dependency injection container

- **PrimeService** (`Services/PrimeService.cs`)
  - Utility service for prime number checking
  - Implements mathematical logic for prime validation
  - Registered as **Scoped** service

### 3. **Data Layer (Models)**
Domain models represent the core business entities.

#### Key Components:
- **BankAccount** (`Models/BankAccount.cs`)
  - Properties:
    - `Id` (int) - Unique identifier
    - `AccountNumber` (string) - Account number
    - `AccountHolderName` (string) - Name of account holder
    - `Balance` (decimal) - Current account balance
  - Methods:
    - `Deposit(decimal, string)` - Add funds to account
    - `Withdraw(decimal, string)` - Remove funds from account
    - `Transfer(BankAccount, decimal)` - Transfer funds between accounts
  - Business rules enforced:
    - Deposits must be positive and have "Credit" transaction type
    - Withdrawals must be positive, have "Debit" transaction type, and not exceed balance
    - Transfers validate sufficient funds

### 4. **Application Configuration Layer**

#### Program.cs
- Entry point for the application
- Configures the web host using `CreateHostBuilder`
- Sets up the Kestrel web server
- Delegates configuration to `Startup` class

#### Startup.cs
- **ConfigureServices Method:**
  - Registers dependency injection services:
    - `IBankAccountService` → `BankAccountService` (Scoped lifetime)
    - `PrimeService` (Scoped lifetime)
  - Adds MVC Controllers support
  - Configures CORS policy "AllowBankAccountUI" to allow requests from `http://localhost:5074`
  
- **Configure Method:**
  - Sets up middleware pipeline:
    1. Developer exception page (development environment)
    2. Routing middleware
    3. CORS middleware with "AllowBankAccountUI" policy
    4. Endpoint routing for controllers
  - Initializes sample data by calling `PopulateAccountData()` on application startup

- **PopulateAccountData Method:**
  - Seeds the application with 20 sample bank accounts
  - Generates random account holders from predefined name list
  - Creates 100 random transactions (deposits/withdrawals) per account
  - Performs random transfers between all accounts
  - Demonstrates usage of business logic methods (Deposit, Withdraw, Transfer)

## Data Storage

### In-Memory Storage
- Uses a static `List<BankAccount>` within `BankAccountService`
- Data is stored in memory and persists only during application runtime
- Data is lost when the application restarts
- Suitable for development and testing purposes

**Note:** The project references Entity Framework Core InMemory package (`Microsoft.EntityFrameworkCore.InMemory`), suggesting potential for future database integration, but currently uses a simple list-based approach.

## Dependency Injection

The application uses ASP.NET Core's built-in dependency injection container:

```
Services → Interfaces → Controllers
```

**Benefits:**
- **Loose Coupling:** Controllers depend on interfaces, not concrete implementations
- **Testability:** Easy to mock services in unit tests
- **Flexibility:** Can swap implementations without changing controllers
- **Lifetime Management:** 
  - **Scoped:** New instance per HTTP request (current implementation)

## Request Flow

1. **HTTP Request** arrives at the API (e.g., `GET /api/BankAccount`)
2. **Routing Middleware** matches the request to the appropriate controller action
3. **CORS Middleware** validates the request origin against configured policy
4. **Controller** receives the request and invokes the injected service
5. **Service** executes business logic and accesses data
6. **Model** represents and validates the data
7. **Controller** returns formatted HTTP response (JSON by default)
8. **Response** is sent back to the client

## CORS Configuration

- **Policy Name:** `AllowBankAccountUI`
- **Allowed Origin:** `http://localhost:5074` (the frontend UI)
- **Allowed Methods:** All HTTP methods (GET, POST, PUT, DELETE)
- **Allowed Headers:** All headers
- **Purpose:** Enables the frontend Razor Pages application to communicate with the API

## API Design Principles

### RESTful Design
- Uses standard HTTP verbs (GET, POST, PUT, DELETE)
- Resource-based URLs (`/api/BankAccount`, `/api/BankAccount/{id}`)
- Returns appropriate HTTP status codes
- Uses JSON for request/response payloads

### Error Handling
- Controllers return `NotFound()` for missing resources
- Controllers return `BadRequest()` for invalid requests
- Services throw exceptions for business rule violations
- Models validate data integrity

## Technology Stack

- **Framework:** ASP.NET Core 8.0
- **Pattern:** MVC (Model-View-Controller)
- **Web Server:** Kestrel
- **Serialization:** JSON (via `Microsoft.AspNetCore.Mvc.NewtonsoftJson`)
- **Testing Framework:** NUnit with Moq for mocking
- **Additional Packages:**
  - FluentAssertions (for readable test assertions)
  - Swashbuckle.AspNetCore (for API documentation - Swagger/OpenAPI)

## Extensibility Points

### Future Enhancements
1. **Persistent Storage:** Replace in-memory list with Entity Framework Core + SQL Server/PostgreSQL
2. **Authentication/Authorization:** Add JWT or OAuth2 for secure API access
3. **Validation:** Implement Data Annotations or FluentValidation for input validation
4. **Logging:** Add structured logging with Serilog or NLog
5. **API Versioning:** Support multiple API versions
6. **Caching:** Implement response caching for frequently accessed data
7. **Transaction Support:** Use the `Transactions` folder structure (currently empty placeholders exist)
8. **Rate Limiting:** Protect API from abuse
9. **Health Checks:** Add health check endpoints for monitoring

## Testing Strategy

The architecture supports comprehensive testing:

- **Unit Tests:** Test individual services and models in isolation using Moq
- **Controller Tests:** Test HTTP request handling and response formatting
- **End-to-End Tests:** Validate complete workflows through the API
- **UI Tests:** Selenium-based tests for the frontend (separate from backend)

## Conclusion

This backend architecture provides a clean, maintainable, and testable foundation for the Bank Account API. The separation of concerns between controllers, services, and models enables independent development and testing of each layer. The use of dependency injection promotes loose coupling and makes the codebase flexible for future enhancements.
