using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Services;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class HomeController : Controller
{
    private readonly IAccountRepository _accountRepository;

    public HomeController(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        var model = new SendLogInLinkModel();
        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken token = default)
    {
        if (await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            return View();
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Pick(CancellationToken token = default)
    {
        if (await this.GetAccountAsync(_accountRepository, token) is Account account)
        {
            await PickRecipient(account, token);
            return View();
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task PickRecipient(Account account, CancellationToken token)
    {
        if (account is null
            || account.Id is null
            || (account.Picked.ContainsKey(DateHelper.Year)
            && account.Picked[DateHelper.Year] is not null))
        {
            return;
        }

        var accounts = await _accountRepository.GetAllAsync(token);
        var candidates = accounts
            .Where(a =>
                a.Id.HasValue
                && a.Id != account.Id
                && a.GetPickedByAccountId(accounts) is null
                && !account.DoNotPick.Contains(a.Id.Value)
                && !a.DoNotPick.Contains(account.Id.Value)
            );

        // Special-case it when only 2 options are left, and choosing
        // one of these options means that there'd be dangling candidate
        // that nobody chose.
        //
        // Example 1:
        //   Item     Receive   Give
        //      x          no     no
        //      y          no    yes
        // In this case, choosing 'y' will leave 'x' dangling.
        //
        // Example 2:
        //   Item     Receive   Give
        //      x          no    yes
        //      y          no    yes
        // In this case there's no possibility of a dangling candidate.
        //
        // Example 3:
        //   Item     Receive   Give
        //      x          no     no
        //      y          no     no
        // In this case there's no possibility of a dangling candidate.
        if (candidates.Count() == 2 
            && !candidates.All(a => a.Picked.ContainsKey(DateHelper.Year) && a.Picked[DateHelper.Year] is not null))
        {
            candidates = candidates.Where(a => !a.Picked.ContainsKey(DateHelper.Year) || a.Picked[DateHelper.Year] is null);
        }

        if (candidates.Count() > 1)
        {
            // if there's more than one potential candidate,
            // make sure not to pick the same person as the previous year
            candidates = candidates.Where(a =>
                !account.Picked.Any(y => y.Key == (DateHelper.Year - 1) && y.Value == a.Id)
            );
        }

        int rand = new Random().Next(0, candidates.Count());

        if (!account.Picked.ContainsKey(DateHelper.Year))
        {
            account.Picked.Add(DateHelper.Year, null);
        }

        account.Picked[DateHelper.Year] = candidates.ElementAt(rand).Id;

        await _accountRepository.SaveAsync(account, token);
    }
}
