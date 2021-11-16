using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class WishlistController : Controller
{
    [HttpGet]
    [ResponseCache(VaryByQueryKeys = new[] { "t" } )]
    public IActionResult Index(long? t = null)
    {
        if (User.GetAccount() is Account account
            && account.Id.HasValue)
        {
            var model = new WishlistEditModel(account.Id.Value);
            return View(model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var model = new WishlistEditModel(id);
        return View(model);
    }

    [HttpPost]
    public IActionResult AddItem(WishlistItem model, CancellationToken token = default)
    {
        if (ModelState.IsValid && User.GetAccount() is Account account)
        {
            WishlistManager.AddItem(account, model, token);
            this.SetResultMessage($"<strong>Successfully added</strong> {model.Name}.");
        }

        return RedirectToAction("Index", new { t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }

    [HttpPost]
    public IActionResult EditItem(WishlistItem model, CancellationToken token = default)
    {
        if (ModelState.IsValid && User.GetAccount() is Account account)
        {
            WishlistManager.EditItem(account, model, token);
            this.SetResultMessage($"<strong>Successfully updated</strong> {model.Name}.");
        }

        return RedirectToAction("Index", new { t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }

    [HttpPost]
    public IActionResult DeleteItem(WishlistItem model)
    {
        if (ModelState.IsValid && User.GetAccount() is Account account)
        {
            WishlistManager.DeleteItem(account, model);
            this.SetResultMessage($"<strong>Successfully deleted</strong> {model.Name}.");
        }

        return RedirectToAction("Index", new { t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }

    [HttpGet]
    public IActionResult Remind(Guid id)
    {
        WishlistManager.SendReminder(id, Url);
        this.SetResultMessage("<strong>Reminder sent</strong> successfully.");
        return RedirectToAction("Details", new { id });
    }
}