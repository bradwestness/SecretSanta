using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Services;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailSender _emailSender;
    private readonly IPreviewGenerator _previewGenerator;

    public AdminController(
        IAccountRepository accountRepository,
        IEmailSender emailSender,
        IPreviewGenerator previewGenerator)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _previewGenerator = previewGenerator ?? throw new ArgumentNullException(nameof(previewGenerator));
    }

    [HttpGet]
    public async Task<IActionResult> Users(CancellationToken token = default)
    {
        var accounts = await _accountRepository.GetAllAsync(token);
        var users = accounts
            .Select(a => new EditUserModel
            {
                AccountId = a.Id,
                DisplayName = a.DisplayName,
                DoNotPick = a.DoNotPick,
                Email = a.Email,
                Picked = a.GetPickedAccountId(),
                PickedBy = a.GetPickedByDisplayName(accounts),
                PickOptions = a.GetPickOptions(accounts),
                DoNotPickOptions = a.GetDoNotPickOptions(accounts)
            })
            .ToList();

        var model = new EditUsersModel
        {
            Users = users,
            AllPicked = users.All(u => u.Picked.HasValue && !string.IsNullOrEmpty(u.PickedBy))
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Reset(CancellationToken token = default)
    {
        var accounts = await _accountRepository.GetAllAsync(token);

        for (var i = 0; i < accounts.Count(); i++)
        {
            var account = accounts.ElementAt(i);

            account.Picked.Remove(DateHelper.Year);
            account.ReceivedGift.Remove(DateHelper.Year);
            account.Wishlist.Remove(DateHelper.Year);

            await _accountRepository.SaveAsync(account, token);
        }

        this.SetResultMessage("All user wishlists and picks have been reset.");
        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> Invite(CancellationToken token)
    {
        await SendInvitationMessages(token);
        this.SetResultMessage("Invitations successfully sent to all users.");
        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> AllPicked(CancellationToken token)
    {
        await SendAllPickedMessages(token);
        this.SetResultMessage("Reminders successfully sent to all users.");
        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> ReceivedGift(CancellationToken token)
    {
        await SendReceivedGiftReminders(token);
        this.SetResultMessage("Reminders successfully sent to all users who have not entered their received gift info.");
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(AddUserModel model, CancellationToken token = default)
    {
        var accounts = await _accountRepository.GetAllAsync(token);

        if (accounts.Any(a => model?.Email?.Equals(a.Email, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            ModelState.AddModelError("NewUser.Email", "A user already exists with the specified E-Mail Address.");
        }

        if (ModelState.IsValid)
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                DisplayName = model.DisplayName
            };

            await _accountRepository.SaveAsync(account, token);
            this.SetResultMessage($"<strong>Successfully added</strong> {model.DisplayName}.");
        }

        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserModel model, CancellationToken token = default)
    {
        if (ModelState.IsValid && model.AccountId.HasValue)
        {
            var account = await _accountRepository.GetAsync(model.AccountId.Value, token);
            account.Email = model.Email;
            account.DisplayName = model.DisplayName;
            account.Picked[DateHelper.Year] = model.Picked;
            account.DoNotPick = model.DoNotPick;

            await _accountRepository.SaveAsync(account, token);
            this.SetResultMessage($"<strong>Successfully updated</strong> {model.DisplayName}.");
        }

        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(EditUserModel model, CancellationToken token = default)
    {
        if (model.AccountId.HasValue)
        {
            await _accountRepository.DeleteAsync(model.AccountId.Value, token);
            this.SetResultMessage($"<strong>Successfully deleted</strong> {model.DisplayName}.");
        }

        return RedirectToAction("Users");
    }

    private async Task SendInvitationMessages(CancellationToken token)
    {
        var accounts = await _accountRepository.GetAllAsync(token);

        foreach (var chunk in accounts.Chunk(5))
        {
            foreach (var account in chunk)
            {
                if (account?.Id is null)
                {
                    continue;
                }

                var guid = GuidEncoder.Encode(account.Id.Value);
                var url = Url.Action("LogIn", "Account", new { guid }, "http");
                var body = new StringBuilder()
                    .AppendLine($"Hey {account.DisplayName}!")
                    .AppendLine()
                    .Append("Santa here. Just wanted to let you know that the ")
                    .AppendLine("Secret Santa website is ready!")
                    .AppendLine()
                    .Append("Please visit the address below to pick your recipient and ")
                    .AppendLine("create your wish list.")
                    .AppendLine()
                    .AppendLine($"<a href=\"{url}\">Secret Santa Website</a>")
                    .AppendLine()
                    .AppendLine("Ho ho ho, ")
                    .AppendLine()
                    .AppendLine("Santa")
                    .AppendLine();

                var to = new[]
                {
                    (account.DisplayName, account.Email)
                };

                await _emailSender.SendAsync(to, "Secret Santa Reminder", body.ToString(), token);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    private async Task SendAllPickedMessages(CancellationToken token)
    {
        var accounts = await _accountRepository.GetAllAsync(token);

        foreach (var chunk in accounts.Chunk(5))
        {
            foreach (var account in chunk)
            {
                var recipientId = account?.Picked?[DateHelper.Year];

                if (account?.Id is null || recipientId is null)
                {
                    continue;
                }

                var recipient = await _accountRepository.GetAsync(recipientId.Value, token);
                var guid = GuidEncoder.Encode(account.Id.Value);
                var url = Url.Action("LogIn", "Account", new { guid }, "http");
                var body = new StringBuilder()
                    .AppendLine($"Hey {account.DisplayName}!")
                    .AppendLine()
                    .AppendFormat("Santa here. Just wanted to let you know that everyone ")
                    .AppendLine("has now picked a person using the Secret Santa website.")
                    .AppendLine()
                    .AppendLine($"Thought I'd send a frindly reminder that you picked {recipient.DisplayName}!")
                    .AppendLine()
                    .AppendLine()
                    .AppendLine("Here's their wish list as it stands right now:")
                    .AppendLine();

                if (recipient.Wishlist.ContainsKey(DateHelper.Year))
                {
                    foreach (WishlistItem item in recipient.Wishlist[DateHelper.Year])
                    {
                        body.AppendLine($"Item: {item.Name}")
                            .AppendLine($"Description: {item.Description}")
                            .AppendLine($"Link: {item.Url}")
                            .AppendLine();
                    }
                }

                body.Append("Remember that you can always visit the address below ")
                    .AppendLine($"to update your wish list and view any changes made by {recipient.DisplayName} too!")
                    .AppendLine()
                    .AppendLine($"<a href=\"{url}\">Secret Santa Website</a>")
                    .AppendLine()
                    .AppendLine("Ho ho ho,")
                    .AppendLine()
                    .AppendLine("Santa");

                var to = new[]
                {
                    (account.DisplayName, account.Email)
                };

                await _emailSender.SendAsync(to, "Secret Santa Reminder", body.ToString(), token);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    private async Task SendReceivedGiftReminders(CancellationToken token)
    {
        var accounts = await _accountRepository.GetAllAsync(token);
        var missingReceivedGift = accounts.Where(
            x => x.ReceivedGift?.ContainsKey(DateHelper.Year) == false
            || string.IsNullOrWhiteSpace(x?.ReceivedGift?[DateHelper.Year]?.Description));

        foreach (var chunk in missingReceivedGift.Chunk(5))
        {
            foreach (var account in chunk)
            {
                var url = Url.Action("LogIn", "Account", new { id = account.Id }, "http");
                var body = new StringBuilder()
                    .AppendFormat("Hey {0}, ", account.DisplayName).AppendLine()
                    .AppendLine()
                    .AppendFormat("Santa again. Just wanted to remind you that once you've received your gift, ")
                    .AppendFormat("you can visit the address below and add a thank-you note about the  ")
                    .AppendFormat("present you received so that everyone can see the results of the ")
                    .AppendFormat("gift exchange! ").AppendLine()
                    .AppendLine()
                    .AppendFormat("<a href=\"{0}\">Secret Santa Website</a>", url).AppendLine()
                    .AppendLine()
                    .AppendFormat("Ho ho ho, ").AppendLine()
                    .AppendLine()
                    .AppendFormat("Santa ").AppendLine()
                    .ToString();

                var to = new[]
                {
                    (account.DisplayName, account.Email)
                };

                await _emailSender.SendAsync(to, "Secret Santa Reminder", body.ToString(), token);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
