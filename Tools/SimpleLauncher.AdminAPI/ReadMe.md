# SimpleLauncher.AdminAPI

SimpleLauncher.AdminAPI is an ASP.NET Core 10.0 application that serves as both a RESTful API and an administrative UI for managing system and emulator configurations for the SimpleLauncher client application. It provides a robust backend for clients to fetch configuration data and a user-friendly web interface for administrators to perform CRUD operations and import configurations from XML files.

## Table of Contents

-   [Features](#features)
-   [Technologies Used](#technologies-used)
-   [Getting Started](#getting-started)
    -   [Prerequisites](#prerequisites)
    -   [Database Setup](#database-setup)
    -   [Running the Application](#running-the-application)
-   [API Endpoints](#api-endpoints)
-   [Admin UI](#admin-ui)
-   [Configuration](#configuration)
-   [Bug Reporting](#bug-reporting)
-   [Authentication and Authorization](#authentication-and-authorization)
-   [Theme Toggling](#theme-toggling)
-   [Project Structure](#project-structure)

## Features

*   **RESTful API**: Provides endpoints for the SimpleLauncher client to retrieve `SystemConfiguration` data, including associated `EmulatorConfiguration`, filtered by architecture (x64/arm64).
*   **Admin UI (Razor Pages)**: A web-based interface for managing system and emulator configurations.
*   **CRUD Operations**: Full Create, Read, Update, and Delete functionality for `SystemConfiguration` and `EmulatorConfiguration` entities.
*   **XML Import**: Ability to import multiple system configurations from a structured XML file, with options to add new or update existing entries.
*   **User Authentication**: Secure login and user management using ASP.NET Core Identity.
*   **Role-Based Authorization**: Admin pages are protected, requiring users to be in the "Admin" role.
*   **Global Exception Handling**: Centralized error handling middleware that logs exceptions and sends bug reports to an external service.
*   **Integrated Bug Reporting**: Automatically sends detailed bug reports for unhandled exceptions to a configurable external API.
*   **Dark/Light Theme**: User interface supports theme toggling (light, dark, and auto-detect based on system preferences).
*   **Database Seeding**: Initializes the database with an "Admin" role and an initial admin user for easy setup.

## Technologies Used

*   **Backend**: ASP.NET Core 10.0 (Web API, Razor Pages, Identity)
*   **Database**: Entity Framework Core 10.0 with SQLite
*   **API Documentation**: Swagger/OpenAPI
*   **Frontend**: Bootstrap 5.3, jQuery, jQuery Validation, jQuery Unobtrusive Validation
*   **Logging**: Microsoft.Extensions.Logging with custom `LoggerMessage` delegates

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

*   [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   A code editor like [Visual Studio](https://visualstudio.microsoft.com/) or [Jebrains Rider](https://www.jetbrains.com/rider/).

### Database Setup

The project uses SQLite, so no external database server is required. The database file `simplelauncher.db` will be created in the project directory.

1.  **Apply Migrations**:
    Navigate to the `SimpleLauncher.AdminAPI` directory in your terminal and run:
    ```bash
    dotnet ef database update
    ```
    This command will create the `simplelauncher.db` file and apply all necessary database schema migrations.

2.  **Initial Admin User**:
    The `DbInitializer.cs` class automatically seeds the database with an initial admin user upon application startup if one doesn't exist.
    *   **Email**: `admin@simplelauncher.com`
    *   **Password**: `Password123!`
    
    **WARNING**: This password is for initial setup and development purposes only. **It is highly recommended to change this password immediately after the first login or use a more secure method for managing credentials in a production environment (e.g., ASP.NET Core User Secrets).**

### Running the Application

1.  **Navigate to Project Directory**:
    Open your terminal or command prompt and go to the `SimpleLauncher.AdminAPI` folder:
    ```bash
    cd C:\Users\Peterson\Documents\Sincronizar\source\repos\CSharp_SimpleLauncher\Tools\SimpleLauncher.AdminAPI
    ```

2.  **Restore Dependencies**:
    ```bash
    dotnet restore
    ```

3.  **Run the Application**:
    ```bash
    dotnet run
    ```
    The application will typically launch on:
    *   `https://localhost:7181` (HTTPS)
    *   `http://localhost:5070` (HTTP)
    (Check `Properties/launchSettings.json` for exact URLs.)

4.  **Access the Admin UI**:
    Open your web browser and navigate to `https://localhost:7181/Admin/Systems`. You will be redirected to the login page. Use the seeded admin credentials to log in.

5.  **Access API Documentation (Swagger)**:
    Open your web browser and navigate to `https://localhost:7181/swagger`.

## API Endpoints

The primary API endpoint for client applications is:

*   **`GET /api/Systems/{architecture}`**
    *   **Description**: Retrieves a list of `SystemConfigurationDto` objects for a specified architecture.
    *   **Parameters**:
        *   `architecture` (string, required): The target architecture, either `"x64"` or `"arm64"`.
    *   **Example**: `GET https://localhost:7181/api/Systems/x64`
    *   **Response**: A JSON array of `SystemConfigurationDto` objects, each including its associated `EmulatorConfigDto`.

## Admin UI

The administrative interface is built using Razor Pages and provides comprehensive management capabilities for system configurations.

*   **Access**: `https://localhost:7181/Admin/Systems` (requires login with an "Admin" role).
*   **Pages**:
    *   `/Admin/Systems/Index`: Displays a list of all configured systems.
    *   `/Admin/Systems/Create`: Form to add a new system and its associated emulator.
    *   `/Admin/Systems/Edit/{id}`: Form to modify an existing system and its emulator. Includes navigation buttons for easy browsing.
    *   `/Admin/Systems/Details/{id}`: Displays detailed information about a specific system and its emulator. Includes navigation buttons.
    *   `/Admin/Systems/Delete/{id}`: Confirmation page for deleting a system. Includes navigation buttons.
    *   `/Admin/Systems/Import`: Page to upload an XML file and import system configurations.

## Configuration

The application's settings are managed via `appsettings.json` and `appsettings.Development.json`.

*   **`appsettings.json`**:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Data Source=simplelauncher.db" // SQLite database file path
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "BugReportService": {
        "Url": "https://www.purelogiccode.com/bugreport/api/send-bug-report", // URL for the external bug report service
        "ApiKey": "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e" // API Key for authentication with the bug report service
      }
    }
    ```
*   **`appsettings.Development.json`**: Overrides specific settings for the development environment, typically for logging verbosity.

## Bug Reporting

The application includes a `GlobalExceptionHandler` middleware that catches all unhandled exceptions. When an exception occurs:

1.  It is logged using the application's logger.
2.  A bug report payload containing the exception message, stack trace, application name, and version is asynchronously sent to an external bug reporting service configured in `appsettings.json`.
3.  A generic `500 Internal Server Error` response is returned to the client to prevent sensitive information leakage.

If `BugReportService:Url` or `BugReportService:ApiKey` are not configured in `appsettings.json`, the bug reporting service will log a warning and skip sending the report.

## Authentication and Authorization

*   **ASP.NET Core Identity**: Used for user management, authentication, and role management.
*   **Admin Role**: An "Admin" role is created during database seeding.
*   **Authorization**: All pages under `/Pages/Admin/` are protected by the `[Authorize(Roles = "Admin")]` attribute, ensuring only authenticated users with the "Admin" role can access them.
*   **Login**: The login page is located at `/Identity/Account/Login`.

## Theme Toggling

The admin UI incorporates a theme switcher that allows users to select between "Light", "Dark", or "Auto" (system preference) themes. The selected theme is persisted in `localStorage`.

## Project Structure

```
SimpleLauncher.AdminAPI/
├── Areas/
│   └── Identity/             # ASP.NET Core Identity UI pages (e.g., Login)
│       └── Pages/
│           └── Account/
├── Controllers/              # RESTful API controllers
│   └── SystemsController.cs
├── Data/                     # Entity Framework Core DbContext, Migrations, and DbInitializer
│   ├── ApplicationDbContext.cs
│   ├── DbInitializer.cs
│   └── Migrations/
├── Middleware/
│   └── GlobalExceptionHandler.cs # Centralized exception handling
├── Models/
│   ├── DTOs/                 # Data Transfer Objects for API responses
│   ├── BugReportPayload.cs   # Model for bug report data
│   ├── EmulatorConfiguration.cs # Entity for emulator details
│   └── SystemConfiguration.cs   # Entity for system details
├── Pages/                    # Razor Pages for the administrative UI
│   ├── Admin/
│   │   ├── Shared/           # Layout and partials specific to admin area
│   │   └── Systems/          # CRUD and Import pages for System Configurations
│   ├── Shared/               # General layout and partials
├── Services/
│   └── BugReportService.cs   # Service for sending bug reports
├── wwwroot/                  # Static files (CSS, JavaScript)
│   ├── css/
│   └── js/
├── appsettings.json          # Application configuration
├── appsettings.Development.json # Development-specific configuration
├── Log.cs                    # Custom LoggerMessage definitions
├── Program.cs                # Application entry point and configuration
└── SimpleLauncher.AdminAPI.csproj # Project file
```