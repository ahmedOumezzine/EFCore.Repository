using AhmedOumezzine.EFCore.Repository.Interface;
using EFCore.Repository.Demo.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EFCore.Repository.Demo.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(IRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<Category>> List(CancellationToken cancellationToken)
    {
        return await repository.GetListAsync<Category>(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync<Category>(id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Category category,
        CancellationToken cancellationToken)
    {
        await repository.InsertAsync(category, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        Category input,
        CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync<Category>(id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        category.Name = input.Name;
        category.Description = input.Description;

        await repository.UpdateAsync(category, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteByIdAsync<Category>(id, cancellationToken);

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
        var restored = await repository.RestoreByIdAsync<Category>(id, cancellationToken);

        if (!restored)
        {
            return NotFound();
        }

        return NoContent();
    }
}
