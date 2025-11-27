# UI Architecture Documentation

## Overview
The BankAccountUI is a web-based front-end application built using modern ASP.NET Core technologies to provide a user interface for managing and viewing bank account information.

## Programming Language and Version

### Primary Language: C#
- **Framework**: .NET 8.0 (net8.0)
- **SDK Version**: Compatible with .NET SDK 10.0.100 or later
- **Language Features**: 
  - Nullable reference types enabled
  - Implicit usings enabled for cleaner code

### Web Technologies
- **HTML5**: Markup for page structure
- **CSS3**: Styling and layout
- **JavaScript**: Client-side interactivity

## UI Framework and Architecture

### ASP.NET Core Razor Pages
The UI is built using **ASP.NET Core Razor Pages**, a page-based programming model that makes building web UI easier and more productive.

**Key Characteristics:**
- Page-focused framework built on top of ASP.NET Core MVC
- Simpler page-based scenarios compared to full MVC
- Each page is a self-contained unit with its own page model
- Convention-based routing (e.g., `/BankAccounts` maps to `Pages/BankAccounts.cshtml`)

### Project Structure
```
BankAccountUI/
├── Pages/                      # Razor Pages
│   ├── BankAccounts.cshtml     # Bank accounts listing page
│   ├── BankAccounts.cshtml.cs  # Page model with business logic
│   ├── Index.cshtml            # Home page
│   ├── Privacy.cshtml          # Privacy policy page
│   ├── Error.cshtml            # Error handling page
│   └── Shared/                 # Shared layout components
│       ├── _Layout.cshtml      # Main layout template
│       └── _ValidationScriptsPartial.cshtml
├── Models/                     # Data models
│   └── BankAccount.cs          # Bank account model
├── wwwroot/                    # Static files
│   ├── css/                    # Stylesheets
│   ├── js/                     # JavaScript files
│   └── favicon.ico
├── Program.cs                  # Application entry point
└── appsettings.json            # Configuration settings
```

## Third-Party Libraries and Dependencies

### Backend Dependencies (.NET/NuGet)

#### 1. Microsoft.Extensions.Http (v9.0.1)
- **Purpose**: Provides HTTP client factory pattern
- **Usage**: Makes HTTP requests to the BankAccountAPI backend
- **Features**:
  - HttpClient lifecycle management
  - Named and typed HTTP clients
  - Built-in support for Polly policies
  - Dependency injection integration

**Implementation in Program.cs:**
```csharp
builder.Services.AddHttpClient("BankAccountAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/api/BankAccount");
});
```

### Frontend Dependencies (Client-Side Libraries)

#### 1. Bootstrap
- **Purpose**: CSS framework for responsive design
- **Usage**: UI components, grid system, and responsive layout
- **Referenced in**: `Pages/Shared/_Layout.cshtml`
- **Components Used**:
  - Navigation bar (navbar)
  - Grid system for responsive layout
  - Utility classes for spacing and styling

#### 2. jQuery
- **Purpose**: JavaScript library for DOM manipulation
- **Usage**: Required by Bootstrap for interactive components
- **Referenced in**: `Pages/Shared/_Layout.cshtml`
- **Location**: Loaded from `~/lib/jquery/dist/jquery.min.js`

**Note**: Bootstrap and jQuery are referenced in the layout file but are expected to be provided via client-side library management (LibMan, npm, or CDN).

### Testing Dependencies

The BankAccountUI.Tests project includes:

#### 1. NUnit (v3.13.3)
- **Purpose**: Testing framework for .NET
- **Usage**: Unit and integration tests for UI components

#### 2. Selenium.WebDriver (v4.28.0)
- **Purpose**: Browser automation framework
- **Usage**: End-to-end UI testing
- **Features**:
  - Cross-browser testing support
  - Page interaction automation
  - Element location and manipulation

#### 3. Selenium.WebDriver.ChromeDriver (v133.0.6943.5300)
- **Purpose**: Chrome browser driver for Selenium
- **Usage**: Enables automated Chrome browser testing

#### 4. Microsoft.NET.Test.Sdk (v17.12.0)
- **Purpose**: .NET testing platform
- **Usage**: Test execution and reporting

#### 5. NUnit3TestAdapter (v4.6.0)
- **Purpose**: NUnit adapter for Visual Studio Test Platform
- **Usage**: Enables NUnit tests to run in Visual Studio and via dotnet test

#### 6. coverlet.collector (v6.0.0)
- **Purpose**: Code coverage collection tool
- **Usage**: Generates code coverage reports during test execution

## Architecture Patterns

### 1. Page Model Pattern
Each Razor Page consists of:
- **View** (.cshtml): HTML markup with Razor syntax
- **Page Model** (.cshtml.cs): C# class handling page logic and data

### 2. Dependency Injection
- HttpClient instances injected via constructor
- Service registration in Program.cs
- Lifetime management handled by ASP.NET Core

### 3. API Integration
- RESTful communication with BankAccountAPI
- HTTP GET requests to fetch bank account data
- Asynchronous operations using async/await pattern

### 4. Separation of Concerns
- **Presentation Layer**: Razor Pages and views
- **Business Logic**: Page models
- **Data Access**: Via HTTP calls to separate API
- **Static Resources**: CSS, JavaScript in wwwroot

## Configuration

### Application Settings (appsettings.json)
- Logging configuration
- Environment-specific settings
- API endpoint configuration

### Program.cs Configuration
- Middleware pipeline setup
- Service registration
- HTTP client configuration
- Routing configuration
- Static file serving

## Key Features

### 1. Responsive Design
- Mobile-first approach using Bootstrap
- Responsive navigation with collapsible menu
- Flexible grid system for various screen sizes

### 2. Client-Side Enhancements
- Custom CSS in `wwwroot/css/site.css`
- Custom JavaScript in `wwwroot/js/site.js`
- Bootstrap components for consistent UI

### 3. Error Handling
- Dedicated error page (Error.cshtml)
- Exception handler middleware for non-development environments
- HSTS (HTTP Strict Transport Security) for production

### 4. Routing
- Convention-based routing for Razor Pages
- Fallback routing to `/BankAccounts` page
- URL-friendly page paths

## Development Workflow

### Building the UI
```bash
dotnet build BankAccountUI
```

### Running the UI
```bash
dotnet run --project BankAccountUI
```
Default URL: `http://localhost:5074/`

### Testing the UI
```bash
dotnet test BankAccountUI.Tests
```

## Browser Compatibility
- Modern browsers supporting HTML5, CSS3, and ES6+
- Chrome (primary test browser via Selenium)
- Firefox, Safari, Edge (via Bootstrap compatibility)

## Performance Considerations
- Static file serving enabled
- HTTP client reuse via factory pattern
- Asynchronous I/O operations
- Minimal JavaScript for faster page loads

## Security Features
- HTTPS redirection (configurable)
- HSTS headers in production
- Authorization middleware ready for implementation
- Anti-forgery tokens for forms (via Razor Pages)

## Summary

The BankAccountUI is a lightweight, modern web application built with:
- **Primary Technology**: ASP.NET Core 8.0 Razor Pages with C#
- **Backend Integration**: Microsoft.Extensions.Http for API communication
- **Frontend Framework**: Bootstrap for responsive design
- **JavaScript Library**: jQuery for DOM manipulation
- **Testing**: NUnit with Selenium WebDriver for comprehensive test coverage

This architecture provides a clean separation between the UI and API layers, making the application maintainable, testable, and scalable.
