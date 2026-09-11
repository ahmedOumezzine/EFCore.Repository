using AhmedOumezzine.EFCore.Repository.Interface;
using EFCore.Repository.Demo.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EFCore.Repository.Demo.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<Product>> List(CancellationToken cancellationToken)
    {
        return await repository.GetListAsync<Product>(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync<Product>(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Product product,
        CancellationToken cancellationToken)
    {
        await repository.InsertAsync(product, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        Product input,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync<Product>(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Price = input.Price;
        product.Stock = input.Stock;

        await repository.UpdateAsync(product, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/name")]
    public async Task<IActionResult> UpdateName(
        Guid id,
        [FromBody] string name,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync<Product>(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        product.Name = name;

        await repository.UpdateOnlyAsync(
            product,
            new[] { nameof(Product.Name) },
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteByIdAsync<Product>(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        var restored = await repository.RestoreByIdAsync<Product>(id, cancellationToken);

        if (!restored)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("count")]
    public async Task<int> Count(CancellationToken cancellationToken)
    {
        return await repository.CountAsync<Product>(cancellationToken);
    }
}
