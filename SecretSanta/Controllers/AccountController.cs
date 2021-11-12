using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult LogIn(string token, string returnUrl)
    {
        var redirect = Url.Action("Index", "Home");

        if (!string.IsNullOrWhiteSpace(token))
        {
            redirect = LogInModel.TokenSignIn(Request.HttpContext, token, returnUrl);
        }

        return Redirect(redirect);
    }

    [Authorize, HttpGet]
    public IActionResult LogOut()
    {
        return Redirect(LogOutModel.SignOut(Request.HttpContext));
    }

    [HttpPost]
    public IActionResult SendLogInLink(SendLogInLinkModel model)
    {
        if (ModelState.IsValid)
        {
            model.Send(Url);
            this.SetResultMessage($"A log in link will be sent to {model.Email}.");
        }

        return RedirectToAction("Index", "Home");
    }
}
