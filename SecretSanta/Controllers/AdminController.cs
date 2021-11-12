using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Users()
    {
        var model = new EditUsersModel();
        return View(model);
    }

    [HttpGet]
    public IActionResult Reset()
    {
        EditUsersModel.Reset();
        this.SetResultMessage("All user wishlists and picks have been reset.");
        return RedirectToAction("Users");
    }

    [HttpGet]
    public IActionResult Invite()
    {
        EditUsersModel.SendInvitationMessages(Url);
        this.SetResultMessage("Invitations successfully sent to all users.");
        return RedirectToAction("Users");
    }

    [HttpGet]
    public IActionResult AllPicked()
    {
        EditUsersModel.SendAllPickedMessages(Url);
        this.SetResultMessage("Reminders successfully sent to all users.");
        return RedirectToAction("Users");
    }

    [HttpGet]
    public IActionResult ReceivedGift()
    {
        ReceivedGiftEditModel.SendReminders(Url);
        this.SetResultMessage("Reminders successfully sent to all users who have not entered their received gift info.");
        return RedirectToAction("Users");
    }

    [HttpPost]
    public IActionResult AddUser(AddUserModel model)
    {
        if (model.EmailConflict())
        {
            ModelState.AddModelError("NewUser.Email", "A user already exists with the specified E-Mail Address.");
        }

        if (ModelState.IsValid)
        {
            model.CreateAccount();
            this.SetResultMessage($"<strong>Successfully added</strong> {model.DisplayName}.");
        }

        return RedirectToAction("Users");
    }

    [HttpPost]
    public IActionResult EditUser(EditUserModel model)
    {
        if (ModelState.IsValid)
        {
            model.Save();
            this.SetResultMessage($"<strong>Successfully updated</strong> {model.DisplayName}.");
        }

        return RedirectToAction("Users");
    }

    [HttpPost]
    public IActionResult DeleteUser(EditUserModel model)
    {
        model.Delete();
        this.SetResultMessage($"<strong>Successfully deleted</strong> {model.DisplayName}.");
        return RedirectToAction("Users");
    }
}
