using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class WishlistController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new WishlistEditModel(User.GetAccount().Id.Value);
        return View(model);
    }

    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var model = new WishlistEditModel(id);
        return View(model);
    }

    [HttpPost]
    public IActionResult AddItem(WishlistItem model)
    {
        if (ModelState.IsValid)
        {
            WishlistManager.AddItem(User.GetAccount(), model);
            this.SetResultMessage($"<strong>Successfully added</strong> {model.Name}.");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult EditItem(WishlistItem model)
    {
        if (ModelState.IsValid)
        {
            WishlistManager.EditItem(User.GetAccount(), model);
            this.SetResultMessage($"<strong>Successfully updated</strong> {model.Name}.");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteItem(WishlistItem model)
    {
        if (ModelState.IsValid)
        {
            WishlistManager.DeleteItem(User.GetAccount(), model);
            this.SetResultMessage($"<strong>Successfully deleted</strong> {model.Name}.");
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Remind(Guid id)
    {
        WishlistManager.SendReminder(id, Url);
        this.SetResultMessage("<strong>Reminder sent</strong> successfully.");
        return RedirectToAction("Details", new { id });
    }
}