using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SecretSanta.Utilities;

namespace SecretSanta.Models;

public class WishlistItem
{
    public Guid? Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [DisplayName("Link"), DataType(DataType.Url)]
    public string? Url { get; set; }

    public byte[]? PreviewImage { get; set; }
}

public class WishlistEditModel
{
    [Required]
    public Guid? AccountId { get; set; }

    [Required]
    public string? DisplayName { get; set; }

    public WishlistItem NewItem { get; set; }

    public IList<WishlistItem> Items { get; set; }

    public WishlistEditModel()
    {
        NewItem = new WishlistItem();
        Items = new List<WishlistItem>();
    }

    public WishlistEditModel(Guid id)
    {
        var account = AccountRepository.Get(id);
        AccountId = account?.Id;
        DisplayName = account?.DisplayName;
        NewItem = new WishlistItem();
        Items = (account?.Wishlist?.ContainsKey(DateHelper.Year) ?? false)
            ? account.Wishlist[DateHelper.Year]
            : new List<WishlistItem>();
    }
}

public static class WishlistManager
{
    public static async void AddItem(Account account, WishlistItem item, CancellationToken token)
    {
        if (!account.Wishlist.ContainsKey(DateHelper.Year))
        {
            account.Wishlist.Add(DateHelper.Year, new List<WishlistItem>());
        }

        item.Id = Guid.NewGuid();
        item.PreviewImage = await PreviewGenerator.GetFeaturedImage(item.Url, token);
        account.Wishlist[DateHelper.Year].Add(item);
        AccountRepository.Save(account);
    }

    public static async void EditItem(Account account, WishlistItem item, CancellationToken token)
    {
        var remove = account.Wishlist[DateHelper.Year].SingleOrDefault(i => i.Id.Equals(item.Id));

        if (remove is not null && item is not null)
        {
            account.Wishlist[DateHelper.Year].Remove(remove);
            item.PreviewImage = await PreviewGenerator.GetFeaturedImage(item.Url, token);
            account.Wishlist[DateHelper.Year].Add(item);
            AccountRepository.Save(account);
        }
    }

    public static void DeleteItem(Account account, WishlistItem item)
    {
        var remove = account.Wishlist[DateHelper.Year].SingleOrDefault(i => i.Id.Equals(item.Id));

        if (remove is not null)
        {
            account.Wishlist[DateHelper.Year].Remove(remove);
            AccountRepository.Save(account);
        }
    }

    public static void SendReminder(Guid id, IUrlHelper urlHelper)
    {
        var account = AccountRepository.Get(id);
        var url = urlHelper.Action("LogIn", "Account", new { id = account.Id }, "http");
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

        var from = new MailboxAddress("Santa Claus", AppSettings.SmtpFrom);
        var to = new[]
        {
            new MailboxAddress(account.DisplayName, account.Email)
        };

        EmailMessage.Send(from, to, "Secret Santa Reminder", body);
    }
}
