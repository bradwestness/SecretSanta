using Microsoft.AspNetCore.Mvc.Rendering;
using SecretSanta.Models;

namespace SecretSanta.Utilities;

public static class AccountExtensions
{
    public static Guid? GetPickedAccountId(this Account account) =>
        account.Picked.ContainsKey(DateHelper.Year)
            ? account.Picked[DateHelper.Year]
            : null;

    public static Guid? GetPickedByAccountId(this Account account, IEnumerable<Account> accounts) =>
        accounts.FirstOrDefault(
            a => a.Picked.ContainsKey(DateHelper.Year)
            && a.Picked[DateHelper.Year] == account?.Id)?.Id;

    public static string? GetPickedByDisplayName(this Account account, IEnumerable<Account> accounts) =>
        accounts.FirstOrDefault(
            a => a.Picked.ContainsKey(DateHelper.Year)
            && a.Picked[DateHelper.Year] == account?.Id)?.DisplayName;

    public static IList<SelectListItem> GetPickOptions(this Account account, IEnumerable<Account> accounts) =>
        accounts.Where(
                a => a.Id.HasValue
                && a.Id != account.Id
                && !account.DoNotPick.Contains(a.Id.Value))
            .Select(a => new SelectListItem
            {
                Text = a.DisplayName,
                Value = $"{a.Id}",
                Selected = account.Picked.ContainsKey(DateHelper.Year) && account.Picked[DateHelper.Year] == a.Id
            })
            .ToList();

    public static IList<SelectListItem> GetDoNotPickOptions(this Account account, IEnumerable<Account> accounts) =>
        accounts.Where(
                a => a.Id.HasValue
                && a.Id != account.Id)
            .Select(a => new SelectListItem
            {
                Text = a.DisplayName,
                Value = $"{a.Id}",
                Selected = account.DoNotPick.Contains(a.Id!.Value)
            })
            .ToList();
}
