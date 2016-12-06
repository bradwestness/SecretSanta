using SecretSanta.Models;
using SecretSanta.Utilities;
using System.Web.Mvc;

namespace SecretSanta.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/LogIn
        public ActionResult LogIn(string token, string returnUrl)
        {
            Response.Write(token);
            Response.Flush();
            Response.End();

            var redirect = Url.Action("Index", "Home");

            if (!string.IsNullOrWhiteSpace(token))
            {
                redirect = LogInModel.TokenSignIn(token, returnUrl);
            }

            return Redirect(redirect);
        }

        //
        // GET: /Account/LogOut
        [Authorize]
        public ActionResult LogOut()
        {
            return Redirect(LogOutModel.SignOut());
        }

        //
        // POST: /Account/SendLogInLink
        public ActionResult SendLogInLink(SendLogInLinkModel model)
        {
            if (ModelState.IsValid)
            {
                model.Send(Url);
                this.SetResultMessage($"A log in link will be sent to {model.Email}.");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}