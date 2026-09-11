using Microsoft.AspNetCore.Mvc;

namespace EFCore.Repository.Demo.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Features()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }
}
