# Bank Account Solution

This solution contains a simple Bank Account MVC project with a REST API, a front-end built with Razor Pages, and accompanying unit tests.

## Documentation

For a detailed explanation of the backend architecture, see [BACKEND_ARCHITECTURE.md](BACKEND_ARCHITECTURE.md).

## Project Overview

### BankAccountAPI
- **Controllers**: Contains the `BankAccountController` which handles HTTP requests related to bank accounts.
- **Models**: Defines the `BankAccount` class representing a bank account with properties like `Id`, `AccountNumber`, `AccountHolderName`, and `Balance`.
- **Services**: Implements the `BankAccountService` class that provides business logic for managing bank accounts.

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

2. Navigate to the solution directory:
   ```sh
   cd BankAccountSolution
   ```

3. Restore the dependencies:
   ```sh
   dotnet restore
   ```

4. Run the API:
   ```sh
   dotnet run --project BankAccountAPI
   ```

5. Run the front-end:
   ```sh
   dotnet run --project BankAccountUI
   ```

6. Open the browser and navigate to:
   ```
   http://localhost:5074/
   ```
   This will display the Bank Account UI.
   ![Bank Account UI](images/bank-account-ui.png)

7. Run the API and unit tests from the project root directory:
   ```sh
   dotnet test BankAccountAPI.Tests
   ```

8. Run the UI tests:
   ```sh
   dotnet test BankAccountUI.Tests
   ```

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

