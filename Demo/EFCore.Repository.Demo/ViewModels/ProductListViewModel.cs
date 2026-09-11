using EFCore.Repository.Demo.Entities;

namespace EFCore.Repository.Demo.ViewModels;

public sealed class ProductListViewModel
{
    public IReadOnlyList<Product> Items { get; init; } = Array.Empty<Product>();

    public string? Search { get; init; }

    public int PageIndex { get; init; }

    public int PageSize { get; init; }

    public long TotalItems { get; init; }

    public int TotalPages { get; init; }
}
