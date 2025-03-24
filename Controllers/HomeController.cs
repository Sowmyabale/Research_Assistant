using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Research_Assistant.Models;
using Microsoft.AspNetCore.Authorization;

namespace Research_Assistant.Controllers;

public class HomeController : Controller
{
    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard() 
    {
    return View();
    }

    [Authorize(Roles = "User")]
    public IActionResult UserDashboard()
    {
    return View();
    }

    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
