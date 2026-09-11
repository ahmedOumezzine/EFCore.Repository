using AhmedOumezzine.EFCore.Repository.Interface;
using EFCore.Repository.Demo.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EFCore.Repository.Demo.Controllers;

[Route("Categories")]
public sealed class CategoriesPageController(IRepository repo) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await repo.GetListAsync<Category>(ct));
}
