using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        var model = new SendLogInLinkModel();
        return View(model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Dashboard()
    {
        if (User.GetAccount() is Account account)
        {
            return View(account);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Pick()
    {
        if (User.GetAccount() is Account account)
        {
            account.Pick();
            return View(account);
        }

        return RedirectToAction("Index", "Home");
    }
}
