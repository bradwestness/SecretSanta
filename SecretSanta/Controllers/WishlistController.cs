using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Services;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailSender _emailSender;
    private readonly IPreviewGenerator _previewGenerator;

    public WishlistController(
        IAccountRepository accountRepository,
        IEmailSender emailSender,
        IPreviewGenerator previewGenerator)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _previewGenerator = previewGenerator ?? throw new ArgumentNullException(nameof(previewGenerator));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken token = default)
    {
        if (await this.GetAccountAsync(_accountRepository, token) is Account account
            && account.Id.HasValue)
        {
            var model = new WishlistEditModel
            {
                AccountId = account.Id,
                DisplayName = account.DisplayName
            };

            if (account.Wishlist.ContainsKey(DateHelper.Year))
            {
                model.Items = account.Wishlist[DateHelper.Year];
            }

            return View(model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken token = default)
    {
        if (await _accountRepository.GetAsync(id, token) is Account account)
        {
            var model = new WishlistEditModel
            {
                AccountId = account.Id,
                DisplayName = account.DisplayName
            };

            if (account.Wishlist.ContainsKey(DateHelper.Year))
            {
                model.Items = account.Wishlist[DateHelper.Year];
            }

            return View(model);

        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> AddItem(WishlistItem model, CancellationToken token = default)
    {
        if (ModelState.IsValid && await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            if (!account.Wishlist.ContainsKey(DateHelper.Year))
            {
                account.Wishlist.Add(DateHelper.Year, new List<WishlistItem>());
            }

            model.Id = Guid.NewGuid();
            model.PreviewImage = await _previewGenerator.GeneratePreviewAsync(model.Url, token);

            account.Wishlist[DateHelper.Year].Add(model);

            await _accountRepository.SaveAsync(account, token);
            this.SetResultMessage($"<strong>Successfully added</strong> {model.Name}.");
        }

        return RedirectToAction("Index", new { t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }

    [HttpPost]
    public async Task<IActionResult> EditItem(WishlistItem model, CancellationToken token = default)
    {
        if (ModelState.IsValid && await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            var remove = account.Wishlist[DateHelper.Year].SingleOrDefault(i => i.Id.Equals(model.Id));

            if (remove is not null)
            {
                account.Wishlist[DateHelper.Year].Remove(remove);
            }

            model.PreviewImage = await _previewGenerator.GeneratePreviewAsync(model.Url, token);

            account.Wishlist[DateHelper.Year].Add(model);

            await _accountRepository.SaveAsync(account, token);
            this.SetResultMessage($"<strong>Successfully updated</strong> {model.Name}.");
        }

        return RedirectToAction("Index", new { t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(WishlistItem model, CancellationToken token = default)
    {
        if (ModelState.IsValid && await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            var remove = account.Wishlist[DateHelper.Year].SingleOrDefault(i => i.Id.Equals(model.Id));

            if (remove is not null)
            {
                account.Wishlist[DateHelper.Year].Remove(remove);
            }

            await _accountRepository.SaveAsync(account, token);
            this.SetResultMessage($"<strong>Successfully deleted</strong> {model.Name}.");
        }

        return RedirectToAction("Index", new { t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
    }

    [HttpGet]
    public async Task<IActionResult> Remind(Guid id, CancellationToken token = default)
    {
        if (await _accountRepository.GetAsync(id, token) is Account account)
        {
            await SendWishListReminder(account, token);
            this.SetResultMessage("<strong>Reminder sent</strong> successfully.");
        }

        return RedirectToAction("Details", new { id });
    }

    private async Task SendWishListReminder(Account account, CancellationToken token)
    {
        if (account?.Id is null)
        {
            return;
        }

        var guid = GuidEncoder.Encode(account.Id.Value);
        var url = Url.Action("LogIn", "Account", new { token = guid }, "http");
        var body = new StringBuilder()
            .AppendFormat("Hey {0}, ", account.DisplayName).AppendLine()
            .AppendLine()
            .AppendFormat("Santa here. A little birdie told me you haven't added any items to your ")
            .AppendFormat("wish list yet. Maybe you should increase your chances ")
            .AppendFormat("of getting something you actually want by ")
            .AppendFormat("visiting the address below! ").AppendLine()
            .AppendLine()
            .AppendFormat("<a href=\"{0}\">Secret Santa Website</a> ", url).AppendLine()
            .AppendLine()
            .AppendFormat("Ho ho ho, ").AppendLine()
            .AppendLine()
            .AppendFormat("Santa ").AppendLine()
            .ToString();

        var to = new[]
        {
            (account.DisplayName, account.Email)
        };

        await _emailSender.SendAsync(to, "Secret Santa Reminder", body, token);
    }
}