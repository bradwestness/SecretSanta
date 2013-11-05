using System.Web.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Account model = User.GetAccount();
            return View(model);
        }

        public ActionResult Pick()
        {
            Account model = User.GetAccount();
            model.Pick();
            return View(model);
        }
    }
}