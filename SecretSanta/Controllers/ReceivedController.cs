using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Services;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class ReceivedController : Controller
{
    private readonly IAccountRepository _accountRepository;

    public ReceivedController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken token = default)
    {
        if (await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            var model = new ReceivedGiftEditModel
            {
                Item = account.ReceivedGift.ContainsKey(DateHelper.Year)
                    ? account.ReceivedGift[DateHelper.Year]
                    : new(),
            };

            model.Item.Id = account.Id;

            if (string.IsNullOrWhiteSpace(model.Item.From))
            {
                var accounts = await _accountRepository.GetAllAsync(token);
                model.Item.From = account.GetPickedByDisplayName(accounts);
                model.Item.To = account.DisplayName;
            }

            return View(model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Save(ReceivedGift model, CancellationToken token = default)
    {
        if (ModelState.IsValid && await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            if (model.ImageUpload is not null && model.ImageUpload.Length > 0)
            {
                using var imageStream = model.ImageUpload.OpenReadStream();

                model.Image = ImageResizer.ResizeJpg(imageStream);
            }

            account.ReceivedGift[DateHelper.Year] = model;

            await _accountRepository.SaveAsync(account, token);
            this.SetResultMessage("Successfully updated your recieved gift info.");
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("Received/Image/{accountId:Guid}")]
    public async Task<IActionResult> Image(Guid accountId, CancellationToken token = default)
    {
        var account = await _accountRepository.GetAsync(accountId, token);
        var gift = account.ReceivedGift[DateHelper.Year];

        if (gift?.Image is null || gift.Image.Length == 0)
        {
            return NoContent();
        }

        return File(gift.Image, "image/jpg");
    }
}
