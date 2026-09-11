# AhmedOumezzine.EFCore.Repository

> A lightweight generic repository for Entity Framework Core 9 with specifications, pagination, soft delete, bulk operations, raw SQL, and transaction helpers.

[![Build](https://github.com/ahmedOumezzine/EFCore.Repository/actions/workflows/build.yml/badge.svg)](https://github.com/ahmedOumezzine/EFCore.Repository/actions/workflows/build.yml) [![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0) [![NuGet](https://img.shields.io/nuget/v/AhmedOumezzine.EFCore.Repository)](https://www.nuget.org/packages/AhmedOumezzine.EFCore.Repository) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

## Overview

This library provides a generic repository abstraction over an application-owned Entity Framework Core `DbContext`. It centralizes asynchronous CRUD and query operations while leaving the `DbContext` and business rules in the consuming application.

The public API includes specifications, database-side pagination and projection, soft-delete-aware queries, bulk updates, parameterized raw SQL, and transaction helpers. The repository pattern is an architectural choice, not a requirement for every application.

## Requirements

- .NET 9
- Entity Framework Core 9
- A provider compatible with EF Core 9

The functional suite validates SQLite. SQL Server-specific behavior remains pending provider validation.

## Installation

Version `2.0.0` is being prepared and is not presented as a public NuGet release yet. Build a local package with:

```bash
dotnet pack src/AhmedOumezzine.EFCore.Repository -c Release
```

After the release is published, install it with:

```bash
dotnet add package AhmedOumezzine.EFCore.Repository
```

The next major release targets .NET 9 and EF Core 9.

## Quick start

### 1. Define an entity

```csharp
public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 2. Configure the DbContext

```csharp
public sealed class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

### 3. Register services

`AddGenericRepository<TDbContext>` resolves the existing scoped context from dependency injection:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddGenericRepository<AppDbContext>();
```

### 4. Inject `IRepository`

```csharp
public sealed class ProductsController(IRepository repository) : ControllerBase
{
    [HttpGet]
    public Task<List<Product>> List(CancellationToken cancellationToken)
    {
        return repository.GetListAsync<Product>(cancellationToken);
    }
}
```

## Basic CRUD

```csharp
var product = new Product { Name = "Keyboard", Price = 49.99m };
await repository.InsertAsync(product, cancellationToken);

var stored = await repository.GetByIdAsync<Product>(product.Id, cancellationToken);
var products = await repository.GetListAsync<Product>(cancellationToken);

product.Price = 44.99m;
await repository.UpdateAsync(product, cancellationToken);

await repository.DeleteAsync(product, cancellationToken);
```

All asynchronous methods accept an optional `CancellationToken` where supported.

## Soft delete

Repository delete methods update `IsDeleted` and `DeletedOnUtc`. Restore methods are available through `RestoreAsync`, `RestoreRangeAsync`, and `RestoreByIdAsync`. Hard-delete and purge APIs are separate operations.

The repository does not automatically install a global EF Core query filter. Configure one in the consuming application when direct `DbContext` queries should hide deleted rows:

```csharp
modelBuilder.Entity<Product>()
    .HasQueryFilter(product => !product.IsDeleted);
```

Standard repository reads already exclude soft-deleted entities; deleted-list APIs are provided for explicit access.

## Partial updates with `UpdateOnly`

```csharp
await repository.UpdateOnlyAsync(
    product,
    new[] { nameof(Product.Price) },
    cancellationToken);
```

Only the selected properties are marked for update. Other scalar properties remain unchanged.

## Pagination

```csharp
var specification = new PaginationSpecification<Product>(pageIndex: 1, pageSize: 20)
{
    OrderBy = query => query
        .OrderBy(product => product.Name)
        .ThenBy(product => product.Id)
};

var page = await repository.GetListAsync(specification, cancellationToken);
```

The result is `PaginatedList<Product>` with `Items`, `PageIndex`, `PageSize`, `TotalItems`, and `TotalPages`. Provide an explicit deterministic ordering when page contents must be stable.

## Specifications

Specifications combine conditions, includes, and ordering:

```csharp
var specification = new Specification<Product>
{
    Includes = query => query.Include(product => product.Category),
    OrderBy = query => query.OrderBy(product => product.Name)
};

specification.Conditions.Add(product => product.IsActive);
var products = await repository.GetListAsync(specification, cancellationToken);
```

## Projections

```csharp
var results = await repository.GetListAsync<Product, object>(
    product => new { product.Id, product.Name, product.Price },
    cancellationToken);
```

Projection selects only the requested columns and avoids materializing unused entity data.

## Exists and count

```csharp
var exists = await repository.ExistsAsync<Product>(
    product => product.Name == "Keyboard",
    cancellationToken);

var count = await repository.CountAsync<Product>(cancellationToken);
```

The API also provides ID existence checks, long counts, date-range counts, and status counts.

## Bulk operations

```csharp
await repository.UpdateFromQueryAsync<Product>(
    product => product.Stock == 0,
    update => update.SetProperty(
        product => product.IsActive,
        false));
```

Bulk operations execute directly in the database and may bypass normal `ChangeTracker` synchronization. Audit timestamp updates may currently require multiple statements.

## Parameterized raw SQL

```csharp
var count = await repository.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM Products WHERE CategoryId = @p0",
    new object[] { categoryId },
    cancellationToken);
```

Always parameterize values. Do not concatenate untrusted input into SQL structure.

## Transactions

```csharp
await repository.ExecuteInTransactionAsync(
    "UPDATE Products SET IsActive = 1 WHERE IsDeleted = 0",
    parameters: null,
    ct: cancellationToken);
```

Repository-created transaction ownership is handled by the repository. A caller-created transaction remains owned by the caller.

## Cancellation

Async repository APIs expose optional `CancellationToken` parameters and propagate cancellation to EF Core operations. Cancellation is reported as `OperationCanceledException`.

## Feature matrix

| Feature | Supported |
| --- | --- |
| Async CRUD | Yes |
| Soft delete | Yes |
| Restore | Yes |
| `UpdateOnly` | Yes |
| Specifications | Yes |
| Pagination | Yes |
| Projections | Yes |
| Exists / Count | Yes |
| Bulk update/delete | Yes |
| Raw SQL | Yes |
| Transactions | Yes |
| SQLite validation | Yes |
| SQL Server validation | Pending |

## Tests

- 173 functional tests passing in the validated SQLite suite.
- Performance tests are separated by category.
- GitHub Actions restores, builds, tests the functional suite, and packs the library.

```bash
dotnet test EFCore.Repository.sln -c Release --filter "TestCategory!=Performance&TestCategory!=SqlServer"
```

## Known limitations

- SQL Server provider validation is pending.
- Concurrent `UpsertAsync` atomicity is not guaranteed across providers.
- Bulk audit timestamp updates may use two statements.
- SQL Server `ExecutionStrategy` validation is pending.

## Roadmap

- SQL Server provider validation
- Evaluation of EF Core 10 and .NET 10
- Optional specialized interfaces
- Keyset pagination
- Streaming APIs

## Contributing

Issues and pull requests are welcome on [GitHub](https://github.com/ahmedOumezzine/EFCore.Repository). Please include a focused description and relevant test coverage.

## License

MIT License. See [LICENSE.txt](LICENSE.txt).
