using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Services;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class AccountController : Controller
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAppSettings _appSettings;
    private readonly IEmailSender _emailSender;

    public AccountController(
        IAccountRepository accountRepository,
        IAppSettings appSettings,
        IEmailSender emailSender)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    [HttpGet]
    public async Task<IActionResult> LogIn(string token, string returnUrl, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(token))
        {
            var guid = GuidEncoder.Decode(token);

            if (guid.HasValue && await _accountRepository.GetAsync(guid.Value, cancellationToken) is Account account)
            {
                var principal = new ClaimsPrincipal(new Identity(account.Email ?? string.Empty));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
        }

        return Redirect(
            string.IsNullOrWhiteSpace(returnUrl)
                ? "/Home/Index"
                : returnUrl);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();

        return RedirectToAction("LogIn", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> SendLogInLink(SendLogInLinkModel model, CancellationToken token = default)
    {
        if (ModelState.IsValid)
        {
            var accounts = await _accountRepository.GetAllAsync(token);
            var account = accounts.SingleOrDefault(a => a.Email?.Equals(model.Email, StringComparison.OrdinalIgnoreCase) ?? false);

            if (account is null && _appSettings.AdminEmail.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
            {
                account = await CreateAdminUser(token);
            }

            await SendLogInEmail(account, token);
        }

        this.SetResultMessage($"A log in link will be sent to {model.Email}.");
        return RedirectToAction("Index", "Home");
    }

    private async Task<Account> CreateAdminUser(CancellationToken token)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Email = _appSettings.AdminEmail,
            DisplayName = "Admin"
        };

        await _accountRepository.SaveAsync(account, token);

        return account;
    }

    private Task SendLogInEmail(Account? account, CancellationToken token)
    {
        if (account?.Id is null)
        {
            return Task.CompletedTask;
        }

        var guid = GuidEncoder.Encode(account.Id.Value);
        var url = Url.Action("LogIn", "Account", new { guid }, "http");
        var body = new StringBuilder()
            .AppendLine($"Hey {account.DisplayName}!")
            .AppendLine()
            .Append("Santa here. Just sending you the log-in link ")
            .AppendLine("you requested for the Secret Santa website. ")
            .AppendLine()
            .Append("Please click the link below to access the website ")
            .AppendLine("and manage your wish list.")
            .AppendLine()
            .AppendLine($"<a href=\"{url}\">Secret Santa</a>")
            .AppendLine()
            .AppendLine("Ho ho ho, ")
            .AppendLine()
            .AppendLine("Santa")
            .AppendLine();

        var to = new[]
        {
            (account.DisplayName, account.Email)
        };

        return _emailSender.SendAsync(to, "Secret Santa Log-In Link", body.ToString(), token);
    }

    protected class Identity : IIdentity
    {
        public string AuthenticationType => CookieAuthenticationDefaults.AuthenticationScheme;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Name);

        public string Name { get; private set; }

        public Identity()
        {
            Name = string.Empty;
        }

        public Identity(string name)
        {
            Name = name;
        }
    }
}
