Here is a class diagram that represents a high-level "software map" of your `SimpleLauncher` application.

![Screenshot](https://github.com/drpetersonfernandes/SimpleLauncher/PlantUML.png)

### How to Interpret the Diagram (Software Map)

This diagram provides a high-level architectural overview of your `SimpleLauncher` application. It's structured into logical packages to make it easier to understand the roles and responsibilities of different parts of the codebase.

**1. Packages (Colored Boxes):**

*   **Application Core (Blue):** This is the entry point. The `App` class is responsible for initializing the application, setting up dependency injection (`ServiceProvider`), loading configuration, and managing global state like `SettingsManager`.
*   **UI (Windows) (LightSkyBlue):** This package contains all the user-facing windows. `MainWindow` is the central hub from which most other windows are launched. These classes are primarily responsible for displaying data and capturing user input.
*   **Managers (Green):** This is your core logic layer. These classes (`SettingsManager`, `SystemManager`, `FavoritesManager`, etc.) are responsible for loading, saving, and managing the application's data and state from files like `system.xml`, `favorites.dat`, and `settings.xml`. They act as the bridge between the UI and the raw data.
*   **Services (Yellow):** These are utility classes that perform specific, often complex, tasks. They are the "workers" of the application.
    *   `GameLauncher` is a critical service that orchestrates the entire process of launching a game, including file extraction and mounting.
    *   `LogErrors`, `MessageBoxLibrary`, and `UpdateChecker` are cross-cutting concerns that provide essential services to many other parts of the application.
*   **UI Helpers (Coral):** These classes (`GameButtonFactory`, `ContextMenuFunctions`) are specialized services that are tightly coupled with the UI. They are responsible for dynamically creating UI elements like game buttons and context menus.
*   **Models (Wheat):** This package contains your data transfer objects (DTOs) or Plain Old C# Objects (POCOs). These classes (`Favorite`, `PlayHistoryItem`, `SystemManager`, etc.) simply hold data and are passed between the different layers.
*   **ViewModels (Thistle):** This contains classes that support the UI in an MVVM-like pattern, such as `GameButtonViewModel`, which holds the state for a single game button (e.g., whether it's a favorite).

**2. Relationships (Arrows):**

*   **Dependency (`..>`):** The most common relationship. A dotted arrow from Class A to Class B means A "uses" or "depends on" B. This could be creating a new instance of B, calling a static method on B, or receiving B as a parameter. For example, `MainWindow ..> GameLauncher` means the main window calls the game launcher service to start a game.
*   **Aggregation (`o--`):** A hollow diamond arrow from A to B means A "has a" B. This represents a relationship where `MainWindow` holds a long-term reference to an instance of a manager, like `MainWindow o-- SettingsManager`.
*   **Composition (`*--`):** A filled diamond arrow from A to B means A "owns" or is "composed of" B. This is used to show that a manager holds a collection of model objects, like `FavoritesManager *-- "many" Favorite`.

There is a good separation of concerns between the UI, business logic (Managers), and utility services. The `App` class acts as the central setup point, and `MainWindow` is the primary orchestrator of the user experience, delegating tasks to the appropriate managers and services.