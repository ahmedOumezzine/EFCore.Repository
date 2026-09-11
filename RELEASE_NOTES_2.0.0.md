# AhmedOumezzine.EFCore.Repository 2.0.0

## Highlights

- .NET 9 and EF Core 9.
- 173/173 functional tests passing.
- Soft-delete, tracking, SQL, and transaction stabilization.
- Improved package metadata and SourceLink.
- External package-consumer validation and CI validation.

## Installation

```bash
dotnet add package AhmedOumezzine.EFCore.Repository --version 2.0.0
```

## Breaking compatibility

.NET 9 is now required.

## Known limitations

SQL Server validation is pending, concurrent Upsert atomicity is not guaranteed, and bulk audit timestamps use two statements.
