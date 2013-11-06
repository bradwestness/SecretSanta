using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SecretSanta.Utilities;

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

        #endregion
    }

    public class WishlistEditModel
    {
        #region Variables

        public Account Account { get; set; }

        public WishlistItem NewItem { get; set; }

        #endregion

        #region Public Methods

        public WishlistEditModel()
        {
            NewItem = new WishlistItem();
        }

        public static WishlistEditModel Load(string id)
        {
            var model = new WishlistEditModel
            {
                Account = DataRepository.Load<Account>(new Guid(id))
            };
            model.Account.Wishlist = model.Account.Wishlist.OrderBy(x => x.Name).ToList();
            return model;
        }

        #endregion
    }

    public static class WishlistManager
    {
        #region Public Methods

        public static void AddItem(WishlistItem item)
        {
            Account account = HttpContext.Current.User.GetAccount();
            if (account.Wishlist == null)
            {
                account.Wishlist = new List<WishlistItem>();
            }
            item.Id = Guid.NewGuid();
            account.Wishlist.Add(item);
            DataRepository.Save(account);
        }

        public static void EditItem(WishlistItem item)
        {
            Account account = HttpContext.Current.User.GetAccount();
            WishlistItem remove = account.Wishlist.SingleOrDefault(i => i.Id.Equals(item.Id));
            account.Wishlist.Remove(remove);
            account.Wishlist.Add(item);
            DataRepository.Save(account);
        }

        public static void DeleteItem(WishlistItem item)
        {
            Account account = HttpContext.Current.User.GetAccount();
            WishlistItem remove = account.Wishlist.SingleOrDefault(i => i.Id.Equals(item.Id));
            account.Wishlist.Remove(remove);
            DataRepository.Save(account);
        }

        public static void SendReminder(Guid id, UrlHelper urlHelper)
        {
            var account = DataRepository.Load<Account>(id);
            var url = urlHelper.Action("LogIn", "Account", new {id = account.Id}, "http");
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

            var from = new MailAddress("santa@thenorthpole.com", "Santa Claus");
            var to = new MailAddressCollection { new MailAddress(account.Email, account.DisplayName) };

            EmailMessage.Send(from, to, "Secret Santa Reminder", body);
        }

        #endregion
    }
}