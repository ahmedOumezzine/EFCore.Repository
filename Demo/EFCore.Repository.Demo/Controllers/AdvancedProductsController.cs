using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Specification;
using EFCore.Repository.Demo.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repository.Demo.Controllers;

[ApiController]
[Route("api/products/advanced")]
public sealed class AdvancedProductsController(IRepository repository) : ControllerBase
{
    [HttpGet("page")]
    public async Task<IActionResult> Page(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest();
        }

        var specification = new PaginationSpecification<Product>(page, pageSize)
        {
            OrderBy = query => query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
        };

        var result = await repository.GetListAsync(specification, cancellationToken);

        return Ok(new
        {
            items = result.Items,
            page,
            pageSize,
            totalCount = result.TotalItems,
            totalPages = result.TotalPages
        });
    }

    [HttpGet("specification/{categoryId:guid}")]
    public async Task<IEnumerable<Product>> Specification(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var specification = new Specification<Product>
        {
            Includes = query => query.Include(product => product.Category!),
            OrderBy = query => query.OrderBy(product => product.Name)
        };

        specification.Conditions.Add(product =>
            product.CategoryId == categoryId && product.IsActive);

        return await repository.GetListAsync(specification, cancellationToken);
    }

    [HttpGet("projection")]
    public async Task<IEnumerable<object>> Projection(
        CancellationToken cancellationToken)
    {
        return await repository.GetListAsync<Product, object>(
            product => new
            {
                product.Id,
                product.Name,
                product.Price,
                CategoryName = product.Category == null
                    ? null
                    : product.Category.Name
            },
            cancellationToken);
    }

    [HttpGet("{id:guid}/exists")]
    public async Task<object> Exists(
        Guid id,
        CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsByIdAsync<Product>(id, cancellationToken);

        return new { exists };
    }

    [HttpGet("count/active")]
    public async Task<int> CountActive(CancellationToken cancellationToken)
    {
        return await repository.CountAsync<Product>(
            product => product.IsActive,
            cancellationToken);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(
        Product product,
        CancellationToken cancellationToken)
    {
        await repository.UpsertAsync<Product>(
            item => item.Name == product.Name,
            product,
            cancellationToken);

        return Ok(product);
    }

    [HttpPost("bulk/deactivate-out-of-stock")]
    public async Task<int> Bulk(CancellationToken cancellationToken)
    {
        return await repository.UpdateFromQueryAsync<Product>(
            product => product.Stock == 0,
            update => update.SetProperty(
                product => product.IsActive,
                false));
    }

    [HttpGet("raw/count-by-category/{categoryId:guid}")]
    public async Task<int?> Raw(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return await repository.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Products WHERE CategoryId = @p0 AND IsDeleted = 0",
            new object[] { categoryId },
            cancellationToken);
    }

    [HttpPost("transaction-demo")]
    public async Task<int> Transaction(CancellationToken cancellationToken)
    {
        return await repository.ExecuteInTransactionAsync(
            "UPDATE Products SET IsActive = 1 WHERE IsDeleted = 0",
            null,
            cancellationToken);
    }
}
