# Changelog

## 2.0.0

### Added / Improved

- Stabilized repository behavior and soft-delete/restore handling.
- Safer `UpdateOnly` behavior and raw SQL parameter handling.
- Correct scoped `DbContext` DI usage.
- EF Core 9 compatible bulk update behavior.
- Deterministic pagination and count tests.
- Cancellation propagation, package consumer validation, GitHub Actions CI, SourceLink, and symbol package.

### Fixed

- Detached soft-delete data corruption risk.
- Same-key tracking conflicts.
- `UpdateFromQuery` EF Core 9 reflection issue.
- Date-range test flakiness and soft-delete test assumptions.

### Changed

- Target framework changed from .NET 8 to .NET 9.

### Breaking compatibility

- Package 2.0.0 requires .NET 9 and EF Core 9.

### Known limitations

- SQL Server provider validation pending.
- Concurrent `Upsert` is not atomic.
- Bulk audit timestamp uses two statements.
- SQL Server `ExecutionStrategy` validation pending.
