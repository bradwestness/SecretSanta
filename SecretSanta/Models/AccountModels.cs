using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public Guid? Picked { get; set; }

        [DisplayName("Do Not Pick")]
        public IList<Guid> DoNotPick { get; set; }

        [DisplayName("Wish List")]
        public IList<WishlistItem> Wishlist { get; set; }

        #endregion

        #region Public Methods

        public Account()
        {
            DoNotPick = new List<Guid>();
            Wishlist = new List<WishlistItem>();
        }

        public bool HasPicked()
        {
            return Picked.HasValue && Picked.Value != new Guid();
        }

        public bool HasBeenPicked()
        {
            return GetPickedBy() != null;
        }

        public Account GetPicked()
        {
            return DataRepository.Load<Account>(Picked);
        }

        public Account GetPickedBy()
        {
            return DataRepository.GetAll<Account>().SingleOrDefault(a => a.Picked == Id);
        }

        public void Pick()
        {
            if (!Id.HasValue || HasPicked())
                return;

            Account[] candidates = DataRepository.GetAll<Account>()
                .Where(a =>
                    a.HasBeenPicked() == false
                    && !DoNotPick.Contains(a.Id.Value)
                    && !a.DoNotPick.Contains(Id.Value)
                ).ToArray();

            int rand = new Random().Next(0, candidates.Length);
            Picked = candidates[rand].Id;
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

        public bool Authenticate()
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

        #endregion

        #region Public Methods

        public static EditUserModel Load(string id)
        {
            return new EditUserModel
            {
                Account = DataRepository.Load<Account>(new Guid(id))
            };
        }

        public void Save()
        {
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
                .Where(a => !a.Email.Equals(Account.Email, StringComparison.CurrentCultureIgnoreCase))
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