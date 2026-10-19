[Home](Home.md) / Developer Setup <!-- wikidown:breadcrumb -->

# Developer Setup

How to get the Little Help Book running on your machine, from an empty database to the providers page.

## Prerequisites

- .NET 10 SDK.
- Visual Studio 2026 with the ASP.NET and **Data storage and processing** workloads (SQL Server Data Tools and LocalDB).
- SQL Server Management Studio, or SqlPackage, for importing the database.

## 1. Create the database from the bacpac

Import the data backup first. A bacpac can only be imported into a **new** database, and Oqtane adds its own tables to that database later. Oqtane's table names don't overlap with the book's.

The backup is `Sql/backup/little-help-book-data.bacpac`. It holds the book's tables and data: providers, addresses, phone numbers, categories and subcategories.

- **SSMS:** connect to `(localdb)\MSSQLLocalDB`, right-click **Databases** → **Import Data-tier Application**, choose the bacpac, and name the database `little-help-book`.
- **Command line:**

  ```bash
  SqlPackage /Action:Import /SourceFile:"Sql/backup/little-help-book-data.bacpac" /TargetServerName:"(localdb)\MSSQLLocalDB" /TargetDatabaseName:little-help-book
  ```

If you already have a `little-help-book` database, drop it or choose another name, and update the connection string in step 2 to match.

## 2. Create your appsettings

Copy `Server/appsettings.default.json` to `Server/appsettings.json`. The copy is git-ignored, because Oqtane writes your machine's installation details into it.

The default file already:

- points at the `little-help-book` database on LocalDB;
- sets the render mode to **Interactive**;
- makes **OpenEugene LittleHelpBook** the default theme and container for new sites.

## 3. Run the installer

1. Start the `Server` project from Visual Studio, or run `dotnet run --project Server`.
2. The Oqtane installer opens. Keep **LocalDB** and the `little-help-book` database, then create the host account.
3. Log in as host.

## 4. Check the site settings

Go to **Admin → Site Settings** and confirm:

- **Theme** is **OpenEugene LittleHelpBook**, with its default container. The theme hosts MudBlazor's providers, styles and script; on another theme, the MudBlazor dropdowns and dialogs won't work.
- **Render Mode** is **Interactive**. On Static, the theme is rendered once as plain HTML and only modules are interactive, so MudBlazor dropdowns open empty.

A fresh install gets both from the defaults. An existing site must be changed by hand. Save, then reload the page.

## 5. Find the pages

The modules create their own pages:

- **providers** shows the provider list for everyone.
- **categories** is for managing categories and subcategories, and is visible to Administrators only.

## Troubleshooting

- **Dropdowns open but show no options:** the render mode is Static, or the site is not using the LittleHelpBook theme.
- **The build can't copy a DLL:** the app or Visual Studio is still running and has the file locked. Stop it and build again.
- **A page title shows something like `providers:1`:** that page was created by an older build. Rename the module title, or delete the page and restart so it is recreated.
