using Microsoft.AspNetCore.Mvc;
using MimeKit;
using SecretSanta.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SecretSanta.Models
{
    public class WishlistItem
    {
        #region Variables

        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [DisplayName("Link"), DataType(DataType.Url)]
        public string Url { get; set; }

        public byte[] PreviewImage { get; set; }

        #endregion
    }

    public class WishlistEditModel
    {
        #region Variables

        public Guid AccountId { get; set; }

        public string DisplayName { get; set; }

        public WishlistItem NewItem { get; set; }

        public IList<WishlistItem> Items { get; set; }

        #endregion

        #region Public Methods

        public WishlistEditModel()
        {
            NewItem = new WishlistItem();
            Items = new List<WishlistItem>();
        }

        public WishlistEditModel(Guid id)
        {
            var account = DataRepository.Get<Account>(id);
            AccountId = account.Id.Value;
            DisplayName = account.DisplayName;
            Items = (account?.Wishlist?.ContainsKey(DateHelper.Year) ?? false)
                ? account.Wishlist[DateHelper.Year]
                : new List<WishlistItem>();
        }

        #endregion
    }

    public static class WishlistManager
    {
        #region Public Methods

        public static async void AddItem(Account account, WishlistItem item)
        {
            if (!account.Wishlist.ContainsKey(DateHelper.Year))
            {
                account.Wishlist.Add(DateHelper.Year, new List<WishlistItem>());
            }
            item.Id = Guid.NewGuid();
            item.PreviewImage = await PreviewGenerator.GetFeaturedImage(item.Url);
            account.Wishlist[DateHelper.Year].Add(item);
            DataRepository.Save(account);
        }

        public static async void EditItem(Account account, WishlistItem item)
        {
            WishlistItem remove = account.Wishlist[DateHelper.Year].SingleOrDefault(i => i.Id.Equals(item.Id));
            account.Wishlist[DateHelper.Year].Remove(remove);
            item.PreviewImage = await PreviewGenerator.GetFeaturedImage(item.Url);
            account.Wishlist[DateHelper.Year].Add(item);
            DataRepository.Save(account);
        }

        public static void DeleteItem(Account account, WishlistItem item)
        {
            WishlistItem remove = account.Wishlist[DateHelper.Year].SingleOrDefault(i => i.Id.Equals(item.Id));
            account.Wishlist[DateHelper.Year].Remove(remove);
            DataRepository.Save(account);
        }

        public static void SendReminder(Guid id, IUrlHelper urlHelper)
        {
            var account = DataRepository.Get<Account>(id);
            string url = urlHelper.Action("LogIn", "Account", new { id = account.Id }, "http");
            string body = new StringBuilder()
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
            var to = new List<MailboxAddress> { new MailboxAddress(account.DisplayName, account.Email) };

            EmailMessage.Send(from, to, "Secret Santa Reminder", body);
        }

        #endregion
    }
}
