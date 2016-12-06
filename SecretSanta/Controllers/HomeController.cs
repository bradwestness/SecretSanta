using SecretSanta.Models;
using SecretSanta.Utilities;
using System.Web.Mvc;

namespace SecretSanta.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var model = new SendLogInLinkModel();
            return View(model);
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            Account model = User.GetAccount();
            return View(model);
        }

        [Authorize]
        public ActionResult Pick()
        {
            Account model = User.GetAccount();
            model.Pick();
            return View(model);
        }
    }
}