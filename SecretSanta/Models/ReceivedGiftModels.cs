using Newtonsoft.Json;
using SecretSanta.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SecretSanta.Models
{
    public class ReceivedGift
    {
        #region Variables

        [Required]
        public Guid? Id { get; set; }

        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        [DisplayName("Note"), Required]
        public string Description { get; set; }

        public byte[] Image { get; set; }

        [DisplayName("Photo with your gift"), Required, JsonIgnore]
        public HttpPostedFileBase ImageUpload { get; set; }

        #endregion

        #region Public Methods

        public void Save()
        {
            if (ImageUpload != null && ImageUpload.ContentLength > 0)
            {
                var tempImage = System.Drawing.Image.FromStream(ImageUpload.InputStream);

                using (var outStream = new MemoryStream())
                {
                    tempImage.Save(outStream, ImageFormat.Jpeg);
                    Image = outStream.ToArray();
                }
            }

            Account account = HttpContext.Current.User.GetAccount();
            Id = account.Id;
            account.ReceivedGift[DateTime.Now.Year] = this;
            DataRepository.Save(account);
        }

        #endregion
    }

    public class ReceivedGiftEditModel
    {
        #region Variables

        public ReceivedGift Item { get; set; }

        public IEnumerable<ReceivedGift> Gifts { get; set; }

        #endregion

        #region Public Methods

        public ReceivedGiftEditModel()
        {
            Gifts = DataRepository.GetAll<Account>().Where(x =>
                        x.ReceivedGift.ContainsKey(DateTime.Now.Year) &&
                        x.ReceivedGift[DateTime.Now.Year] != null &&
                        !string.IsNullOrWhiteSpace(x.ReceivedGift[DateTime.Now.Year].Description)
                    )
                    .Select(x => x.ReceivedGift[DateTime.Now.Year])
                    .ToList();
        }

        public ReceivedGiftEditModel(Account account) : this()
        {
            Item = account.ReceivedGift.ContainsKey(DateTime.Now.Year)
                ? account.ReceivedGift[DateTime.Now.Year]
                : new ReceivedGift();
            Item.Id = account.Id;

            if (string.IsNullOrWhiteSpace(Item.From))
            {
                var pickedBy = account.GetPickedBy();
                if (pickedBy != null)
                {
                    Item.From = pickedBy.DisplayName;
                    Item.To = account.DisplayName;
                }
            }
        }

        public static void SendReminders(UrlHelper urlHelper)
        {
            var accounts = DataRepository.GetAll<Account>().Where(x => 
                x.ReceivedGift == null || 
                !x.ReceivedGift.ContainsKey(DateTime.Now.Year) ||
                string.IsNullOrWhiteSpace(x.ReceivedGift[DateTime.Now.Year].Description)
            );

            foreach (var account in accounts)
            {
                string url = urlHelper.Action("LogIn", "Account", new { id = account.Id }, "http");
                string body = new StringBuilder()
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

                var from = new MailAddress("santa@thenorthpole.com", "Santa Claus");
                var to = new MailAddressCollection { new MailAddress(account.Email, account.DisplayName) };

                EmailMessage.Send(from, to, "Secret Santa Reminder", body);
            }
        }

        #endregion
    }
}