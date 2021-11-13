using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult LogIn(string token, string returnUrl) =>
        Redirect(string.IsNullOrWhiteSpace(token)
            ? Url.Action("Index", "Home") ?? string.Empty
            : LogInModel.TokenSignIn(Request.HttpContext, token, returnUrl));

    [Authorize]
    [HttpGet]
    public IActionResult LogOut() =>
        Redirect(LogOutModel.SignOut(Request.HttpContext));

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
