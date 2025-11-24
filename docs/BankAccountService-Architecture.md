# BankAccountService Architecture

## Overview

The `BankAccountService` class is the core business logic layer of the Bank Account API, responsible for managing bank account data and operations. It implements the `IBankAccountService` interface and serves as a bridge between the API controllers and the data storage layer.

**Location:** `BankAccountAPI/Services/BankAccountService.cs`

**Interface:** `BankAccountAPI/Services/IBankAccountService.cs`

## Design Patterns

### 1. Repository Pattern

The `BankAccountService` implements the **Repository Pattern**, which provides an abstraction layer between the business logic and data access layers. This pattern offers several benefits:

- **Separation of Concerns**: Isolates data access logic from business logic
- **Testability**: Easier to mock and test business logic independently
- **Maintainability**: Changes to data storage mechanisms don't affect business logic
- **Centralized Data Access**: All data operations go through a single point

**Implementation Details:**
```csharp
private static List<BankAccount> _accounts = new List<BankAccount>();
```

The service maintains an in-memory collection of bank accounts, acting as a simple repository. In production systems, this would typically be replaced with a database-backed repository.

### 2. Dependency Injection (Service Layer Pattern)

The service is registered with the ASP.NET Core dependency injection container as a **scoped service**:

```csharp
services.AddScoped<IBankAccountService, BankAccountService>();
```

**Scoped Lifetime Characteristics:**
- A new instance is created once per client request (HTTP request)
- The instance is shared across all components in that request
- Disposed at the end of the request

This ensures that:
- Controllers receive the same service instance throughout a single request
- Multiple concurrent requests don't interfere with each other
- Resources are properly cleaned up after request completion

### 3. Interface Segregation Principle

By implementing the `IBankAccountService` interface, the service follows the **Interface Segregation Principle**:

- Controllers depend on abstractions (`IBankAccountService`) rather than concrete implementations
- Easy to swap implementations (e.g., for testing with mocks)
- Promotes loose coupling between layers

### 4. Static Repository Pattern (Singleton-like behavior)

The static `_accounts` field creates a **singleton-like storage mechanism**:

```csharp
private static List<BankAccount> _accounts = new List<BankAccount>();
```

**Important Characteristics:**
- Shared across all instances of `BankAccountService`
- Persists for the application lifetime
- Not thread-safe by default (potential concurrency issues)
- Acts as an in-memory database for the application

**Trade-offs:**
- ✅ Simple implementation for demonstration purposes
- ✅ Fast access without database overhead
- ❌ Data lost on application restart
- ❌ Not suitable for distributed/multi-instance deployments
- ❌ Potential thread-safety issues with concurrent requests

## Transaction Handling

### Transaction Support in BankAccount Model

While the `BankAccountService` itself doesn't handle financial transactions directly, the `BankAccount` model (located in `BankAccountAPI/Models/BankAccount.cs`) provides transaction methods:

#### 1. Deposit Operations

**Method:** `Deposit(decimal amount, string transactionType)`

**Responsibilities:**
- Validates transaction type ends with "Credit"
- Ensures deposit amount is positive
- Increases account balance

**Usage Example:**
```csharp
account.Deposit(100.00m, "Credit");
```

**Validation Rules:**
- Transaction type must end with "Credit" (case-insensitive)
- Amount must be greater than zero
- No maximum deposit limit

#### 2. Withdrawal Operations

**Method:** `Withdraw(decimal amount, string transactionType)`

**Responsibilities:**
- Validates transaction type ends with "Debit"
- Ensures withdrawal amount is positive
- Verifies sufficient funds are available
- Decreases account balance

**Usage Example:**
```csharp
account.Withdraw(50.00m, "Debit");
```

**Validation Rules:**
- Transaction type must end with "Debit" (case-insensitive)
- Amount must be greater than zero
- Amount cannot exceed current balance

**Unusual Edge Case Behavior:**
```csharp
if (amount == Balance)
{
    return; // No-op when withdrawing exact balance
}
```

**⚠️ Note:** The current implementation has an unusual behavior where withdrawing the exact account balance performs no operation. This appears to be a bug rather than intentional design, as it contradicts typical banking behavior where withdrawing the full balance should zero the account. In production, this logic should be reviewed and likely corrected to allow withdrawals that result in a zero balance.

#### 3. Transfer Operations

**Method:** `Transfer(BankAccount toAccount, decimal amount)`

**Responsibilities:**
- Validates transfer amount is positive
- Verifies sufficient funds in source account
- Atomically decreases source account balance
- Atomically increases destination account balance

**Usage Example:**
```csharp
fromAccount.Transfer(toAccount, 200.00m);
```

**Validation Rules:**
- Amount must be greater than zero
- Amount cannot exceed source account balance
- Both accounts must be valid

**Atomicity Considerations:**
- Transfer operations are NOT truly atomic in the current implementation
- No rollback mechanism if the operation fails midway
- In production, should use database transactions or compensating transactions

## Service Methods and Responsibilities

### 1. `GetAllAccounts()`

**Signature:** `IEnumerable<BankAccount> GetAllAccounts()`

**Responsibilities:**
- Returns all bank accounts in the system
- Used for listing and overview operations

**Usage:**
- Called by `BankAccountController.GetAllAccounts()` to handle GET requests to `/api/BankAccount`

**Return Value:**
- Collection of all `BankAccount` objects
- Empty collection if no accounts exist

### 2. `GetAccountById(int id)`

**Signature:** `BankAccount GetAccountById(int id)`

**Responsibilities:**
- Retrieves a specific account by its ID
- Throws exception if account not found

**Usage:**
- Called by `BankAccountController.GetAccountById(int id)` for GET requests to `/api/BankAccount/{id}`

**Error Handling:**
- Throws `InvalidOperationException` if account with specified ID doesn't exist
- Exception message: "Account not found"

**Implementation:**
```csharp
var account = _accounts.Find(account => account.Id == id);
return account ?? throw new InvalidOperationException("Account not found");
```

### 3. `AddAccount(BankAccount account)` and `CreateAccount(BankAccount account)`

**Signature:** `void AddAccount(BankAccount account)` / `void CreateAccount(BankAccount account)`

**Note:** Both methods have identical implementations - this is likely a code smell indicating duplicate functionality.

**Responsibilities:**
- Adds a new account to the repository
- Does not validate for duplicate IDs or account numbers

**Usage:**
- `CreateAccount` is called by `BankAccountController.CreateAccount()` for POST requests to `/api/BankAccount`
- `AddAccount` may be used internally or in initialization

**Current Limitations:**
- No duplicate checking (can add accounts with same ID)
- No ID auto-generation
- No validation of required fields

### 4. `UpdateAccount(BankAccount account)`

**Signature:** `void UpdateAccount(BankAccount account)`

**Responsibilities:**
- Updates an existing account's properties
- Validates account exists before updating
- Updates AccountNumber, AccountHolderName, and Balance

**Usage:**
- Called by `BankAccountController.UpdateAccount(int id, BankAccount account)` for PUT requests to `/api/BankAccount/{id}`

**Error Handling:**
- Throws `KeyNotFoundException` if account doesn't exist
- Exception message includes the account ID

**Implementation:**
```csharp
var existingAccount = _accounts.Find(a => a.Id == account.Id);
if (existingAccount == null)
{
    throw new KeyNotFoundException($"Account with ID {account.Id} not found.");
}
existingAccount.AccountNumber = account.AccountNumber;
existingAccount.AccountHolderName = account.AccountHolderName;
existingAccount.Balance = account.Balance;
```

**Design Note:**
- Updates by replacing properties rather than replacing the entire object
- Maintains object reference integrity

### 5. `DeleteAccount(int id)`

**Signature:** `void DeleteAccount(int id)`

**Responsibilities:**
- Removes an account from the repository
- Validates account exists before deletion

**Usage:**
- Called by `BankAccountController.DeleteAccount(int id)` for DELETE requests to `/api/BankAccount/{id}`

**Error Handling:**
- Throws `KeyNotFoundException` if account doesn't exist
- Exception message includes the account ID
- Catches and re-throws `InvalidOperationException` as `KeyNotFoundException`

**Implementation:**
```csharp
try
{
    var account = _accounts.Find(account => account.Id == id);
    if (account == null)
    {
        throw new KeyNotFoundException($"Account with ID {id} not found.");
    }
    _accounts.Remove(account);
}
catch (InvalidOperationException)
{
    throw new KeyNotFoundException($"Account with ID {id} not found.");
}
```

### 6. `InitializeAccounts(List<BankAccount> accounts)`

**Signature:** `void InitializeAccounts(List<BankAccount> accounts)`

**Responsibilities:**
- Replaces the entire account collection
- Used for seeding initial data or resetting state

**Usage:**
- Called by `Startup.PopulateAccountData()` during application startup
- Used in tests to reset state between test runs

**Important Note:**
- Completely replaces the static `_accounts` reference
- All previous account data is lost
- Not thread-safe

**Implementation:**
```csharp
_accounts = accounts;
```

## Service Lifecycle

### Initialization

1. **Application Startup** (`Startup.cs`):
   - `BankAccountService` is registered in the DI container with scoped lifetime
   - `Configure` method retrieves an instance of the service
   - `PopulateAccountData()` is called to seed initial data

2. **Data Seeding Process**:
   - Creates 20 bank accounts with random data
   - Performs 100 random transactions (deposits/withdrawals) per account
   - Executes transfers between all account pairs
   - Calls `InitializeAccounts()` to populate the service

### Request Processing

1. **Controller Request**:
   - Client sends HTTP request to API endpoint
   - ASP.NET Core routing matches request to controller action
   - DI container creates new `BankAccountService` instance (scoped to request)

2. **Service Invocation**:
   - Controller calls appropriate service method
   - Service accesses static `_accounts` collection
   - Service performs business logic and data operations

3. **Response**:
   - Service returns data or throws exceptions
   - Controller transforms service response to HTTP response
   - Request completes, scoped service instance is disposed

### Concurrency Considerations

**Current Implementation:**
- Static `_accounts` collection is shared across all requests
- No locking or thread-safety mechanisms
- Potential for race conditions in concurrent scenarios

**Potential Issues:**
1. **Lost Updates**: Two concurrent updates might overwrite each other
2. **Dirty Reads**: Reading account during partial update
3. **Collection Modification**: Concurrent add/remove operations could corrupt the list

**Production Recommendations:**
- Implement proper locking mechanisms (e.g., `lock` statements or `ReaderWriterLockSlim`)
- Use thread-safe collections (e.g., `ConcurrentBag<T>`)
- Replace in-memory storage with database with ACID guarantees
- Implement optimistic or pessimistic concurrency control

## Integration with Controllers

### BankAccountController

The `BankAccountController` (located in `BankAccountAPI/Controllers/BankAccountController.cs`) acts as the presentation layer:

**Constructor Injection:**
```csharp
private readonly IBankAccountService _bankAccountService;

public BankAccountController(IBankAccountService bankAccountService)
{
    _bankAccountService = bankAccountService;
}
```

**Endpoint Mappings:**
- `GET /api/BankAccount` → `GetAllAccounts()`
- `GET /api/BankAccount/{id}` → `GetAccountById(int id)`
- `POST /api/BankAccount` → `CreateAccount(BankAccount account)`
- `PUT /api/BankAccount/{id}` → `UpdateAccount(int id, BankAccount account)`
- `DELETE /api/BankAccount/{id}` → `DeleteAccount(int id)`

**Responsibilities:**
- HTTP request/response handling
- Input validation (model binding)
- Status code determination
- Exception handling (delegated to ASP.NET Core middleware)

## Testing Strategy

The service is thoroughly tested in `BankAccountAPI.Tests/Services/BankAccountServiceTest.cs`:

**Test Coverage:**
1. ✅ `GetAllAccounts_ShouldReturnAllAccounts` - Verifies retrieval of all accounts
2. ✅ `GetAccountById_ValidId_ShouldReturnAccount` - Tests single account retrieval
3. ✅ `CreateAccount_ShouldAddAccount` - Validates account creation
4. ✅ `UpdateAccount_ValidId_ShouldUpdateAccount` - Checks update functionality
5. ✅ `DeleteAccount_ValidId_ShouldRemoveAccount` - Confirms deletion
6. ✅ `InitialiseAccounts_ShouldClearExistingAccounts` - Tests initialization (note: test name has British spelling "Initialise")

**Test Setup Pattern:**
```csharp
[SetUp]
public void Setup()
{
    _service = new BankAccountService();
    _service.InitializeAccounts(new List<BankAccount>());
}
```

Each test starts with a clean slate by initializing an empty account list.

## Architecture Strengths

1. **Clear Separation of Concerns**: Service layer is distinct from presentation (controllers) and data (models)
2. **Interface-Based Design**: Enables dependency injection and testability
3. **Consistent API**: All CRUD operations follow predictable patterns
4. **Error Handling**: Appropriate exceptions for different error conditions
5. **Simple and Understandable**: Easy to comprehend for new developers

## Architecture Limitations and Improvement Opportunities

1. **Thread Safety**: No concurrency control mechanisms
2. **Data Persistence**: In-memory storage lost on restart
3. **Duplicate Methods**: `AddAccount` and `CreateAccount` are redundant
4. **No Validation**: Missing input validation (null checks, business rules)
5. **No Transaction Support**: Operations are not atomic (especially transfers)
6. **Static State**: Makes testing and parallel execution challenging
7. **No Logging**: No audit trail or diagnostic logging
8. **No Pagination**: `GetAllAccounts` could be problematic with large datasets
9. **Tight Coupling**: Direct dependency on concrete `BankAccount` model

## Recommended Enhancements

### Short-term Improvements:
1. Remove duplicate `AddAccount` method
2. Add input validation (null checks, business rule validation)
3. Add logging for audit and diagnostics
4. Implement thread-safe access patterns
5. Add pagination support to `GetAllAccounts`

### Medium-term Improvements:
1. Replace in-memory storage with Entity Framework Core and database
2. Implement Unit of Work pattern for transaction management
3. Add domain events for account operations
4. Implement proper exception hierarchy
5. Add caching layer for frequently accessed data

### Long-term Improvements:
1. Consider CQRS pattern for complex queries
2. Implement event sourcing for transaction history
3. Add distributed transaction support
4. Implement domain-driven design principles
5. Add API versioning strategy

## Conclusion

The `BankAccountService` implements a straightforward repository pattern with dependency injection, providing a clean separation between business logic and data access. While suitable for demonstration and learning purposes, production deployments would require enhancements in thread safety, data persistence, transaction management, and validation. The service's design makes it relatively easy to evolve and extend as requirements grow.
