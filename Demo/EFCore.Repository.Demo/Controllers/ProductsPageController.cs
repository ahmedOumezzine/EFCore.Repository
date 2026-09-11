using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Specification;
using EFCore.Repository.Demo.Entities;
using EFCore.Repository.Demo.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EFCore.Repository.Demo.Controllers;

[Route("Products")]
public sealed class ProductsPageController(IRepository repository) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        var specification = new PaginationSpecification<Product>(page, 12)
        {
            OrderBy = query => query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            specification.Conditions.Add(product =>
                product.Name.Contains(search) ||
                product.Sku.Contains(search) ||
                product.Description.Contains(search));
        }

        var result = await repository.GetListAsync(specification, cancellationToken);

        var model = new ProductListViewModel
        {
            Items = result.Items,
            Search = search,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };

        return View(model);
    }
}
