using System;
using System.Web.Mvc;
using SecretSanta.Models;

namespace SecretSanta.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/LogIn
        public ActionResult LogIn(Guid? id, string ReturnUrl)
        {
            if (id.HasValue)
            {
                return Redirect(LogInModel.GuidSignIn(id.Value, ReturnUrl));
            }

            var model = new LogInModel();
            return View(model);
        }

        //
        // POST: /Account/LogIn
        [HttpPost]
        public ActionResult LogIn(LogInModel model, string ReturnUrl)
        {
            if (!string.IsNullOrWhiteSpace(model.Email) && !model.IsValid())
            {
                ModelState.AddModelError("Email", "The E-Mail Address entered was not recognized.");
            }

            if (ModelState.IsValid)
            {
                ReturnUrl = model.SignIn(ReturnUrl);
                return Redirect(ReturnUrl);
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