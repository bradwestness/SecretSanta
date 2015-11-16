using SecretSanta.Models;
using System;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SecretSanta.Utilities
{
    public static class Helpers
    {
        public static Account GetAccount(this IPrincipal user)
        {
            if (!user.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(user.Identity.Name))
                return null;

            Account account = DataRepository.GetAll<Account>()
                .SingleOrDefault(a => a.Email.Equals(user.Identity.Name, StringComparison.CurrentCultureIgnoreCase));

            if (account == null)
            {
                var model = new AddUserModel();
                model.DisplayName = user.Identity.Name;
                model.Email = user.Identity.Name;
                account = model.CreateAccount();
            }

            return account;
        }

        public static void SetResultMessage(this Controller controller, string message)
        {
            HttpContext.Current.Session.Add("ResultMessage", message);
        }

        public static MvcHtmlString ResultMessage(this HtmlHelper helper)
        {
            var message = HttpContext.Current.Session["ResultMessage"] as string;
            string output = string.Empty;
            if (!string.IsNullOrWhiteSpace(message))
            {
                output = new StringBuilder()
                    .Append("<div class='alert alert-info alert-dismissable'>")
                    .Append(
                        "<button type='button' class='close' data-dismiss='alert' aria-hidden='true'>&times;</button>")
                    .Append(message)
                    .Append("</div>")
                    .ToString();
            }
            HttpContext.Current.Session.Remove("ResultMessage");
            return new MvcHtmlString(output);
        }
    }
}