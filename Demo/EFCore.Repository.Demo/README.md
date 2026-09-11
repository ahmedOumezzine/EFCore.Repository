# EFCore.Repository.Demo

An ASP.NET Core MVC and Web API application that demonstrates the public API of [`AhmedOumezzine.EFCore.Repository`](https://github.com/ahmedOumezzine/EFCore.Repository).

The demo consumes package version `2.0.0` through a local NuGet feed while that release is being prepared. It does not use a project reference to the library.

## Screenshots

Screenshots coming soon.

## Features demonstrated

- MVC pages for products and categories
- Product search in the MVC page, with paging, sorting, and category/status query examples exposed by the API controllers
- Specifications, projections, existence/count queries, and cancellation tokens
- `UpdateOnly`, soft delete, restore, hard delete, and upsert operations
- Bulk updates, parameterized raw SQL, and transactions through the API endpoints
- Dashboard counts and deterministic seed data

The API controllers expose the advanced scenarios; the MVC pages provide a small human-readable entry point for the same SQLite database.

## Tech stack

- .NET 9
- ASP.NET Core MVC and Web API
- Entity Framework Core 9
- SQLite
- Bootstrap
- `AhmedOumezzine.EFCore.Repository` `2.0.0`

## Demo dataset

On first run the application creates 12 categories and 240 deterministic products. Products have stable SKUs (`DEMO-001` through `DEMO-240`), fixed dates, different prices and stock levels, active/inactive states, and three soft-deleted records. This makes the examples repeatable across runs.

## Local package setup

Version `2.0.0` is not published on NuGet.org yet. `NuGet.Config` adds the local Release package folder at `../../src/AhmedOumezzine.EFCore.Repository/bin/Release`. Build the library package before restoring the demo:

```bash
dotnet pack src/AhmedOumezzine.EFCore.Repository -c Release
dotnet restore Demo/EFCore.Repository.Demo
```

When the package is published, the local source can be removed from `NuGet.Config`; no source code change is required.

## Run locally

From `Demo/EFCore.Repository.Demo`:

```bash
dotnet restore
dotnet build -c Release
dotnet run
```

Useful routes:

- `/` — dashboard
- `/Products` — searchable product list (for example, `/Products?search=laptop`)
- `/Categories` — category list
- `/swagger` — API documentation in the Development environment
- `/api/products`, `/api/categories`, and `/api/products/advanced` — API examples

## Database

The demo uses SQLite in `demo.db`. The application calls EF Core `EnsureCreated()` on startup, so the database is created automatically and no migration command is required for this sample. The database file, WAL files, and local Data Protection keys are ignored by Git.

## Soft delete behavior

The repository updates soft-delete state (`IsDeleted` and `DeletedOnUtc`). The consuming application owns the global query filters; this demo configures those filters in `DemoDbContext` for products and categories.

## Known limitations

- SQL Server-specific validation is pending; this sample uses SQLite.
- Concurrent upsert atomicity is not guaranteed across providers.
- Bulk audit timestamps currently use multiple statements.
- The demo is an example application, not a production deployment template.

## Related links

- [Repository source](https://github.com/ahmedOumezzine/EFCore.Repository)
- [NuGet package page](https://www.nuget.org/packages/AhmedOumezzine.EFCore.Repository) — the `2.0.0` release is still being prepared.

## License

This project is released under the [MIT License](../../LICENSE.txt).
