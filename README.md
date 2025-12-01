# Bank Account Solution

This solution contains a simple Bank Account MVC project with a REST API, a front-end built with Razor Pages, and accompanying unit tests.

## Project Overview

### BankAccountAPI
- **Controllers**: Contains the `BankAccountController` which handles HTTP requests related to bank accounts, and the `PrimeController` for prime number checking.
- **Models**: Defines the `BankAccount` class representing a bank account with properties like `Id`, `AccountNumber`, `AccountHolderName`, and `Balance`. Includes methods for `Deposit`, `Withdraw`, and `Transfer`.
- **Services**: Implements the `BankAccountService` class that provides business logic for managing bank accounts, and `PrimeService` for prime number operations.
- **Transactions**: Placeholder directory structure for future transaction-related functionality.

### BankAccountUI (Front-End)
- **Razor Pages**: Implements a simple front-end for viewing bank accounts.
- **Integration**: Fetches data from the `BankAccountAPI` using HTTP client services.

### BankAccountAPI.Tests
- **Controllers**: Contains unit tests for the `BankAccountController` to ensure correct handling of HTTP requests.
- **Services**: Contains unit tests for the `BankAccountService` to verify business logic and data manipulation.
- **End-to-End Tests**: Contains end-to-end tests to verify the complete workflow of the application.

### BankAccountUI.Tests (UI Testing)
- **Selenium WebDriver**: Automates UI interactions to validate front-end functionality.
- **ChromeDriver**: Enables automated testing in Google Chrome.
- **NUnit Framework**: Provides test structure and assertions.

## Setup Instructions

1. Clone the repository:
   ```sh
   git clone <repository-url>
   ```

2. Navigate to the repository directory (where the solution file is located):
   ```sh
   cd bank-account-api  # This is the repository root, not a subdirectory
   ```

3. Restore the dependencies:
   ```sh
   dotnet restore
   ```

4. Run the API:
   ```sh
   dotnet run --project BankAccountAPI
   ```
   The API will be available at `http://localhost:5000`

5. Run the front-end (in a separate terminal):
   ```sh
   dotnet run --project BankAccountUI
   ```

6. Open the browser and navigate to:
   ```
   http://localhost:5074/
   ```
   This will display the Bank Account UI.
   ![Bank Account UI](images/bank-account-ui.png)

7. Run all tests using the provided script:
   ```sh
   ./run-tests.sh
   ```

   Or run tests individually:
   ```sh
   dotnet test BankAccountAPI.Tests
   ```

8. Run the UI tests:
   ```sh
   dotnet test BankAccountUI.Tests
   ```

## API Endpoints

### Bank Account Controller (`/api/BankAccount`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/BankAccount` | Get all bank accounts |
| GET | `/api/BankAccount/{id}` | Get a specific bank account by ID |
| POST | `/api/BankAccount` | Create a new bank account |
| PUT | `/api/BankAccount/{id}` | Update an existing bank account |
| DELETE | `/api/BankAccount/{id}` | Delete a bank account |

### Prime Controller (`/api/prime`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/prime/{number}` | Check if a number is prime |

## Docker Support

Build and run the API using Docker:

```sh
# Build the Docker image
docker build -t bank-account-api .

# Run the container
docker run -p 8080:8080 bank-account-api
```

The API will be available at `http://localhost:8080`

## Running Tests Separately

### API Tests
1. Open a terminal and navigate to the `BankAccountAPI.Tests` directory.
2. Run the following command to execute the tests:
   ```sh
   dotnet test
   ```

### UI Tests with Selenium:
1. Ensure **Google Chrome** is installed on your system.
2. Open a terminal and navigate to `BankAccountUI.Tests`.
3. Run:
   ```sh
   dotnet test
   ```
   This will launch **Chrome**, navigate to the Bank Accounts page, and verify the UI.

## Utility Scripts

- **run-tests.sh**: Builds the solution and runs all tests
  ```sh
  ./run-tests.sh
  ```

- **free-port.sh**: Check if a port is in use and optionally free it
  ```sh
  ./free-port.sh <port_number>
  ```

## Dependencies

This project may require the following NuGet packages for testing:

- `NUnit`: A popular testing framework for .NET.
- `Moq`: A library for creating mock objects in unit tests.
- `Selenium.WebDriver`: Provides automation for web UI testing.
- `Selenium.WebDriver.ChromeDriver`: Enables Chrome browser automation.

Make sure to restore the packages by running:
```sh
   dotnet restore
```

## Technologies Used
- .NET 8 (or later)
- ASP.NET Core MVC
- ASP.NET Core Razor Pages
- Entity Framework Core 
- NUnit (for testing)
- Moq (for mocking dependencies in tests)
- Selenium WebDriver (for UI testing)

## Contributing
Feel free to submit issues or pull requests for improvements or bug fixes.

