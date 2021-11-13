using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class ReceivedController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.GetAccount() is Account account)
        {
            var model = new ReceivedGiftEditModel(account);
            return View(model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Save(ReceivedGift model)
    {
        if (ModelState.IsValid && User.GetAccount() is Account account)
        {
            model.Save(account);
            this.SetResultMessage("Successfully updated your recieved gift info.");
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("Received/Image/{accountId:Guid}")]
    //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = int.MaxValue, VaryByHeader = "Cookie", VaryByQueryKeys = new[] { "accountId" })]
    public IActionResult Image(Guid accountId)
    {
        var account = AccountRepository.Get(accountId);
        ReceivedGift gift = account.ReceivedGift[DateHelper.Year];

        if (gift.Image == null || gift.Image.Length == 0)
        {
            return NoContent();
        }

        return File(gift.Image, "image/jpg");
    }
}
