using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SecretSanta.Utilities;

namespace SecretSanta.Models
{
    public class Account
    {
        #region Variables

        public Guid? Id { get; set; }

        [DisplayName("E-Mail Address"), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DisplayName("Display Name"), Required]
        public string DisplayName { get; set; }

        [DisplayName("Do Not Pick")]
        public IList<Guid> DoNotPick { get; set; }

        public IDictionary<int, Guid?> Picked { get; set; }

        [DisplayName("Wish List")]
        public IDictionary<int, IList<WishlistItem>> Wishlist { get; set; }

        [DisplayName("Gift Received")]
        public IDictionary<int, ReceivedGift> ReceivedGift { get; set; }

        #endregion

        #region Public Methods

        public Account()
        {
            DoNotPick = new List<Guid>();
            Picked = new Dictionary<int, Guid?>();
            Wishlist = new Dictionary<int, IList<WishlistItem>>();
            ReceivedGift = new Dictionary<int, ReceivedGift>();
        }

        public bool HasPicked()
        {
            return GetPicked() != null;
        }

        public bool HasBeenPicked()
        {
            return GetPickedBy() != null;
        }

        public Account GetPicked()
        {
            if (Picked != null && Picked.ContainsKey(DateTime.Now.Year))
            {
                return DataRepository.Get<Account>(Picked[DateTime.Now.Year]);
            }

            return null;
        }

        public Account GetPickedBy()
        {
            var pickedBy = DataRepository.GetAll<Account>()
                .FirstOrDefault(x => 
                    x.Picked != null && x.Picked.Any(y => 
                        y.Key == DateTime.Now.Year && 
                        y.Value == Id
                    )
                );
            return pickedBy;
        }

        public void Pick()
        {
            if (!Id.HasValue || HasPicked())
            {
                return;
            }

            Account[] candidates = DataRepository.GetAll<Account>()
                .Where(a =>
                    a.Id != Id 
                    && a.HasBeenPicked() == false
                    && !DoNotPick.Contains(a.Id.Value)
                    && !a.DoNotPick.Contains(Id.Value)
                    && !Picked.Any(y => y.Key == (DateTime.Now.Year - 1) && y.Value == a.Id)
                ).ToArray();

            int rand = new Random().Next(0, candidates.Length);
            Picked.Add(DateTime.Now.Year, candidates[rand].Id);
            DataRepository.Save(this);
        }

        #endregion
    }

    public class LogInModel
    {
        #region Variables

        [DisplayName("E-Mail Address"), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        #endregion

        #region Public Methods

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return false;

            if (Email.Equals(AppSettings.AdminEmail, StringComparison.CurrentCultureIgnoreCase))
                return true;

            IList<Account> accounts = DataRepository.GetAll<Account>();
            return accounts.Any(a => a.Email.Equals(Email, StringComparison.CurrentCultureIgnoreCase));
        }

        public string SignIn(string returnUrl)
        {
            FormsAuthentication.SetAuthCookie(Email, true);
            return string.IsNullOrWhiteSpace(returnUrl)
                ? FormsAuthentication.GetRedirectUrl(Email, true)
                : returnUrl;
        }

        public static string GuidSignIn(Guid guid, string returnUrl)
        {
            var account = DataRepository.Get<Account>(guid);
            var model = new LogInModel {Email = account.Email};
            return model.SignIn(returnUrl);
        }

        #endregion
    }

    public class LogOutModel
    {
        #region Public Methods

        public static string SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
            return FormsAuthentication.DefaultUrl;
        }

        #endregion
    }

    public class EditUsersModel
    {
        #region Variables

        public AddUserModel NewUser { get; set; }

        public IList<EditUserModel> Users { get; set; }

        public bool AllPicked
        {
            get
            {
                if (Users == null)
                    return false;

                return Users.All(u => u.Account.HasPicked() && u.Account.HasBeenPicked());
            }
        }

        #endregion

        #region Public Methods

        public EditUsersModel()
        {
            NewUser = new AddUserModel();
            Users = new List<EditUserModel>();
        }

        public static EditUsersModel Load()
        {
            var model = new EditUsersModel
            {
                NewUser = new AddUserModel(),
                Users = DataRepository.GetAll<Account>()
                    .OrderBy(a => a.DisplayName)
                    .Select(a => new EditUserModel {Account = a}).ToList()
            };

            return model;
        }

        public static void SendInvitationMessages(UrlHelper urlHelper)
        {
            IList<Account> accounts = DataRepository.GetAll<Account>();

            foreach (Account account in accounts)
            {
                string url = urlHelper.Action("LogIn", "Account", new {id = account.Id}, "http");

                StringBuilder body = new StringBuilder()
                    .AppendFormat("Hey {0}! ", account.DisplayName).AppendLine()
                    .AppendLine()
                    .AppendFormat("Santa here. Just wanted to let you know that the ")
                    .AppendFormat("Secret Santa website is ready! ").AppendLine()
                    .AppendLine()
                    .AppendFormat("Please visit the address below to pick your recipient and ")
                    .AppendFormat("create your wish list. ").AppendLine()
                    .AppendLine()
                    .AppendFormat("<a href=\"{0}\">Secret Santa Website</a> ", url).AppendLine()
                    .AppendLine()
                    .AppendFormat("Ho ho ho, ").AppendLine()
                    .AppendLine()
                    .AppendFormat("Santa ").AppendLine();

                var from = new MailAddress("santa@thenorthpole.com", "Santa Claus");
                var to = new MailAddressCollection {new MailAddress(account.Email, account.DisplayName)};

                EmailMessage.Send(from, to, "Secret Santa Reminder", body.ToString());
            }
        }

        public static void SendAllPickedMessages(UrlHelper urlHelper)
        {
            IList<Account> accounts = DataRepository.GetAll<Account>();

            foreach (Account account in accounts)
            {
                string url = urlHelper.Action("LogIn", "Account", new {id = account.Id}, "http");
                Account recipient = account.GetPicked();

                StringBuilder body = new StringBuilder()
                    .AppendFormat("Hey {0}! ", account.DisplayName).AppendLine()
                    .AppendLine()
                    .AppendFormat("Santa here. Just wanted to let you know that everyone ")
                    .AppendFormat("has now picked a person using the Secret Santa website. ").AppendLine()
                    .AppendLine()
                    .AppendFormat("Thought I'd send a frindly reminder that you picked {0}! ", recipient.DisplayName)
                    .AppendLine()
                    .AppendLine()
                    .AppendFormat("Here's their wish list as it stands right now: ").AppendLine()
                    .AppendLine();

                if (recipient.Wishlist.ContainsKey(DateTime.Now.Year))
                {
                    foreach (WishlistItem item in recipient.Wishlist[DateTime.Now.Year])
                    {
                        body.AppendFormat("Item: {0}", item.Name).AppendLine()
                            .AppendFormat("Description: {0}", item.Description).AppendLine()
                            .AppendFormat("Link: {0}", item.Url).AppendLine()
                            .AppendLine();
                    }
                }

                body.AppendFormat(
                    "Remember that you can always visit the address below to update your wish list and view ")
                    .AppendFormat("any changes made by {0} too! ", recipient.DisplayName).AppendLine()
                    .AppendLine()
                    .AppendFormat("<a href=\"{0}\">Secret Santa Website</a> ", url).AppendLine()
                    .AppendLine()
                    .AppendFormat("Ho ho ho, ").AppendLine()
                    .AppendLine()
                    .AppendFormat("Santa ").AppendLine();

                var from = new MailAddress("santa@thenorthpole.com", "Santa Claus");
                var to = new MailAddressCollection {new MailAddress(account.Email, account.DisplayName)};

                EmailMessage.Send(from, to, "Secret Santa Reminder", body.ToString());
            }
        }

        public static void Reset()
        {
            var users = DataRepository.GetAll<Account>();

            foreach(var user in users)
            {
                user.Picked.Remove(DateTime.Now.Year);
                user.ReceivedGift.Remove(DateTime.Now.Year);
                user.Wishlist.Remove(DateTime.Now.Year);

                DataRepository.Save(user);
            }
        }

        #endregion
    }

    public class AddUserModel
    {
        #region Variables

        [DisplayName("E-Mail Address"), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DisplayName("Display Name"), Required]
        public string DisplayName { get; set; }

        #endregion

        #region Public Methods

        public Account CreateAccount()
        {
            var account = new Account
            {
                Email = Email,
                DisplayName = DisplayName
            };

            DataRepository.Save(account);
            return account;
        }

        public bool EmailConflict()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return false;
            }

            IList<Account> accounts = DataRepository.GetAll<Account>();

            return accounts.Any(a => a.Email.Equals(Email, StringComparison.CurrentCultureIgnoreCase));
        }

        #endregion
    }

    public class EditUserModel
    {
        #region Variables

        public Account Account { get; set; }

        public Guid? Picked { get; set; }

        #endregion

        #region Public Methods

        public static EditUserModel Load(string id)
        {
            return new EditUserModel
            {
                Account = DataRepository.Get<Account>(new Guid(id))
            };
        }

        public void Save()
        {
            if (Picked.HasValue)
            {
                Account.Picked[DateTime.Now.Year] = Picked.Value;
            }

            DataRepository.Save(Account);
        }

        public void Delete()
        {
            DataRepository.Delete<Account>(Account.Id);
        }

        public IEnumerable<SelectListItem> GetPickOptions()
        {
            return DataRepository.GetAll<Account>()
                .Where(a => a.Id != Account.Id && !Account.DoNotPick.Contains(a.Id.Value))
                .Select(a => new SelectListItem
                {
                    Text = a.DisplayName,
                    Value = a.Id.ToString()
                });
        }

        public IEnumerable<SelectListItem> GetDoNotPickOptions()
        {
            return DataRepository.GetAll<Account>()
                .Where(a => a.Id.HasValue && !a.Email.Equals(Account.Email, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(a => a.DisplayName)
                .Select(a => new SelectListItem
                {
                    Text = a.DisplayName,
                    Value = a.Id.ToString()
                });
        }

        #endregion
    }
}