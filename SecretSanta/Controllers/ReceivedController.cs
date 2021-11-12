﻿using Microsoft.AspNetCore.Authorization;
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
        var account = User.GetAccount();
        ReceivedGiftEditModel model = new ReceivedGiftEditModel(account);
        return View(model);
    }

    [HttpPost]
    public IActionResult Save(ReceivedGift model)
    {
        if (ModelState.IsValid)
        {
            model.Save(User.GetAccount());
            this.SetResultMessage("Successfully updated your recieved gift info.");
        }

        return RedirectToAction("Index");
    }

    [HttpGet, Route("Received/Image/{accountId:Guid}")]
    //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = int.MaxValue, VaryByHeader = "Cookie", VaryByQueryKeys = new[] { "accountId" })]
    public IActionResult Image(Guid accountId)
    {
        var account = DataRepository.Get<Account>(accountId);
        ReceivedGift gift = account.ReceivedGift[DateHelper.Year];

        if (gift.Image == null || gift.Image.Length == 0)
        {
            return null;
        }

        return File(gift.Image, "image/jpg");
    }
}
