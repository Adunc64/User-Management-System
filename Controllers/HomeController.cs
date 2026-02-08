using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using task.Models;

namespace task.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View("Index", "Users");
    }
}
