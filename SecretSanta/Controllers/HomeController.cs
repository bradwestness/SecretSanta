﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        var model = new SendLogInLinkModel();
        return View(model);
    }

    [Authorize, HttpGet]
    public IActionResult Dashboard()
    {
        Account model = User.GetAccount();
        return View(model);
    }

    [Authorize, HttpGet]
    public IActionResult Pick()
    {
        Account model = User.GetAccount();
        model.Pick();
        return View(model);
    }
}
