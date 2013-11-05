using System.Web.Mvc;
using SecretSanta.Models;

namespace SecretSanta.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/LogIn
        public ActionResult LogIn(string returnUrl)
        {
            var model = new LogInModel();
            return View(model);
        }

        //
        // POST: /Account/LogIn
        [HttpPost]
        public ActionResult LogIn(LogInModel model, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && !model.Authenticate())
            {
                ModelState.AddModelError("Email", "The E-Mail Address entered was not recognized.");
            }

            if (ModelState.IsValid)
            {
                returnUrl = model.SignIn(returnUrl);
                return Redirect(returnUrl);
            }

            return View(model);
        }

        //
        // GET: /Account/LogOut
        [Authorize]
        public ActionResult LogOut()
        {
            return Redirect(LogOutModel.SignOut());
        }
    }
}