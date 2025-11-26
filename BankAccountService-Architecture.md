# BankAccountService Architecture

## Overview

The `BankAccountService` class is the core business logic layer of the Bank Account API, responsible for managing bank account data and operations. It implements a service-oriented architecture that separates business logic from the presentation layer (controllers) and data storage.

## Design Patterns

### 1. Repository Pattern

The `BankAccountService` class implements the **Repository Pattern**, which provides an abstraction layer between the business logic and data access. This pattern:

- Centralizes data access logic in a single place
- Makes the code more maintainable and testable
- Allows for easy swapping of data storage mechanisms

**Implementation Details:**
- The service maintains an in-memory data store using `private static List<BankAccount> _accounts`
- All CRUD operations are encapsulated within the service methods
- The controller layer interacts with data only through the service interface

### 2. Singleton Pattern (Static State)

The service uses a **static collection** (`_accounts`) to maintain state across all instances:

```csharp
private static List<BankAccount> _accounts = new List<BankAccount>();
```

**Key characteristics:**
- All instances of `BankAccountService` share the same account list
- State persists throughout the application lifetime
- Provides a centralized data store without requiring a database

**Note:** While this pattern is useful for simple applications and testing, production systems typically use a proper database with dependency injection of repository instances.

### 3. Dependency Injection Pattern

The service is registered and injected using ASP.NET Core's built-in dependency injection container:

**Registration in `Startup.cs` (line 33):**
```csharp
services.AddScoped<IBankAccountService, BankAccountService>();
```

**Injection in `BankAccountController.cs` (lines 12-16):**
```csharp
private readonly IBankAccountService _bankAccountService;

public BankAccountController(IBankAccountService bankAccountService)
{
    _bankAccountService = bankAccountService;
}
```

**Benefits:**
- Loose coupling between controller and service implementation
- Easy to mock for unit testing
- Supports interface-based programming

### 4. Interface Segregation

The service implements the `IBankAccountService` interface, which defines the contract:

```csharp
public interface IBankAccountService
{
    void InitializeAccounts(List<BankAccount> accounts);
    IEnumerable<BankAccount> GetAllAccounts();
    BankAccount GetAccountById(int id);
    void AddAccount(BankAccount account);
    void DeleteAccount(int id);
    void CreateAccount(BankAccount account);
    void UpdateAccount(BankAccount account);
}
```

This follows the **Dependency Inversion Principle** - depend on abstractions, not concretions.

## Method Responsibilities

### Data Retrieval Methods

#### `GetAllAccounts()`
**Location:** Line 11-14  
**Purpose:** Retrieves all bank accounts in the system

```csharp
public IEnumerable<BankAccount> GetAllAccounts()
{
    return _accounts;
}
```

**Responsibilities:**
- Returns the complete collection of accounts
- Used for displaying all accounts in the UI
- Returns an `IEnumerable` to provide read-only access

#### `GetAccountById(int id)`
**Location:** Line 16-20  
**Purpose:** Retrieves a specific account by its unique identifier

```csharp
public BankAccount GetAccountById(int id)
{
    var account = _accounts.Find(account => account.Id == id);
    return account ?? throw new InvalidOperationException("Account not found");
}
```

**Responsibilities:**
- Searches the accounts list using LINQ `Find` method
- Throws `InvalidOperationException` if account doesn't exist
- Ensures clients handle missing accounts appropriately

### Data Modification Methods

#### `CreateAccount(BankAccount account)` and `AddAccount(BankAccount account)`
**Location:** Lines 22-25 and 44-47  
**Purpose:** Adds a new bank account to the system

```csharp
public void CreateAccount(BankAccount account)
{
    _accounts.Add(account);
}
```

**Responsibilities:**
- Adds new account to the in-memory collection
- Note: Both methods have identical implementations (potential for refactoring)
- No validation for duplicate IDs or account numbers

#### `UpdateAccount(BankAccount account)`
**Location:** Line 49-59  
**Purpose:** Updates an existing bank account's properties

```csharp
public void UpdateAccount(BankAccount account)
{
    var existingAccount = _accounts.Find(a => a.Id == account.Id);
    if (existingAccount == null)
    {
        throw new KeyNotFoundException($"Account with ID {account.Id} not found.");
    }
    existingAccount.AccountNumber = account.AccountNumber;
    existingAccount.AccountHolderName = account.AccountHolderName;
    existingAccount.Balance = account.Balance;
}
```

**Responsibilities:**
- Finds the existing account by ID
- Throws `KeyNotFoundException` if account doesn't exist
- Updates all mutable properties (AccountNumber, AccountHolderName, Balance)
- Preserves the account's position in the list

#### `DeleteAccount(int id)`
**Location:** Line 27-42  
**Purpose:** Removes a bank account from the system

```csharp
public void DeleteAccount(int id)
{
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
}
```

**Responsibilities:**
- Finds and removes the account from the collection
- Provides comprehensive error handling
- Converts `InvalidOperationException` to `KeyNotFoundException` for consistency

#### `InitializeAccounts(List<BankAccount> accounts)`
**Location:** Line 61-64  
**Purpose:** Initializes or replaces the entire account collection

```csharp
public void InitializeAccounts(List<BankAccount> accounts)
{
    _accounts = accounts;
}
```

**Responsibilities:**
- Replaces the entire account list with a new collection
- Used during application startup to seed data (see `Startup.cs`, line 133)
- Useful for testing to reset state between test runs

## Transaction Handling

The `BankAccountService` itself **does not directly handle financial transactions**. Instead, transaction logic is implemented in the `BankAccount` model class, following the **Domain-Driven Design** principle of placing business logic close to the data it operates on.

### Transaction Methods in BankAccount Model

#### `Deposit(decimal amount, string transactionType)`
**Location:** `BankAccount.cs`, lines 17-28  
**Purpose:** Increases account balance (credit transactions)

```csharp
public void Deposit(decimal amount, string transactionType)
{
    if (!transactionType.EndsWith("Credit", StringComparison.OrdinalIgnoreCase))
    {
        throw new ArgumentException("Transaction type must be Credit.");
    }
    if (amount <= 0)
    {
        throw new ArgumentException("Deposit amount must be positive.");
    }
    Balance += amount;
}
```

**Transaction Rules:**
- Validates transaction type ends with "Credit"
- Ensures amount is positive
- Increases balance atomically

#### `Withdraw(decimal amount, string transactionType)`
**Location:** `BankAccount.cs`, lines 30-49  
**Purpose:** Decreases account balance (debit transactions)

```csharp
public void Withdraw(decimal amount, string transactionType)
{
    if (!transactionType.EndsWith("Debit", StringComparison.OrdinalIgnoreCase))
    {
        throw new ArgumentException("Transaction type must be Debit.");
    }
    if (amount <= 0)
    {
        throw new ArgumentException("Withdrawal amount must be positive.");
    }
    if (amount > Balance)
    {
        throw new InvalidOperationException("Insufficient funds.");
    }
    if (amount == Balance)
    {
        return;
    }
    Balance -= amount;
}
```

**Transaction Rules:**
- Validates transaction type ends with "Debit"
- Ensures amount is positive
- Checks for sufficient funds before withdrawal
- Special case: If withdrawing entire balance, returns without modifying balance

#### `Transfer(BankAccount toAccount, decimal amount)`
**Location:** `BankAccount.cs`, lines 51-63  
**Purpose:** Transfers funds between accounts

```csharp
public void Transfer(BankAccount toAccount, decimal amount)
{
    if (amount <= 0)
    {
        throw new ArgumentException("Transfer amount must be positive.");
    }
    if (amount > Balance)
    {
        throw new InvalidOperationException("Insufficient funds.");
    }
    Balance -= amount;
    toAccount.Balance += amount;
}
```

**Transaction Rules:**
- Validates amount is positive
- Checks for sufficient funds
- Performs atomic transfer (debit from source, credit to destination)

### Transaction Initialization

The application demonstrates transaction handling during startup in `Startup.cs` (lines 58-134):

1. **Account Creation:** Creates 20 accounts with random initial balances
2. **Random Transactions:** Performs 100 random credit/debit transactions per account
3. **Transfers:** Performs transfers between all account pairs
4. **Error Handling:** Catches and logs transaction failures

**Example from `Startup.cs` (lines 84-98):**
```csharp
if (transAmt >= 0)
{
    acc.Deposit(transAmt, "Credit");
    Console.WriteLine("Credit: " + transAmt + ", Balance: " + acc.Balance + ...);
}
else
{
    acc.Withdraw(-transAmt, "Debit");
    Console.WriteLine("Debit: " + -transAmt + ", Balance: " + acc.Balance + ...);
}
```

## Architecture Benefits

1. **Separation of Concerns:** Business logic is separated from controllers and models
2. **Testability:** Easy to unit test with mock data (see `BankAccountServiceTest.cs`)
3. **Maintainability:** Changes to data access don't affect controllers
4. **Scalability:** Can easily replace in-memory storage with database repositories
5. **Type Safety:** Strong typing through interfaces and models

## Architecture Limitations

1. **No Persistence:** Data is stored in memory and lost on application restart
2. **Thread Safety:** No synchronization for concurrent access to `_accounts`
3. **No Validation:** Duplicate IDs or account numbers are not prevented
4. **Duplicate Methods:** `AddAccount` and `CreateAccount` have identical implementations
5. **Transaction Atomicity:** No distributed transaction support for complex operations

## Usage Example

Here's how the service is used in the controller layer:

```csharp
[HttpPost]
public IActionResult CreateAccount(BankAccount account)
{
    _bankAccountService.CreateAccount(account);
    return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
}

[HttpGet("{id}")]
public ActionResult<BankAccount> GetAccountById(int id)
{
    var account = _bankAccountService.GetAccountById(id);
    if (account == null)
    {
        return NotFound();
    }
    return Ok(account);
}
```

## Conclusion

The `BankAccountService` follows established design patterns and SOLID principles to provide a clean, maintainable service layer. While suitable for demonstration and learning purposes, production deployments would benefit from:

- Persistent data storage (database)
- Thread-safe concurrent access
- Transaction logging and audit trails
- More sophisticated validation and error handling
- Asynchronous operations for scalability
