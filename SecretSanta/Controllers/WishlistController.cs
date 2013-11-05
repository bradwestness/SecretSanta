using System;
using System.Web.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        //
        // GET: /Wishlist/
        public ActionResult Index()
        {
            string id = User.GetAccount().Id.ToString();
            WishlistEditModel model = WishlistEditModel.Load(id);
            return View(model);
        }

        //
        // GET: /Wishlist/Details
        public ActionResult Details(string id)
        {
            WishlistEditModel model = WishlistEditModel.Load(id);
            return View(model);
        }

        //
        // POST: /Wishlist/AddItem
        [HttpPost]
        public ActionResult AddItem(WishlistItem model)
        {
            if (ModelState.IsValid)
            {
                WishlistManager.AddItem(model);
            }

            return RedirectToAction("Index");
        }

        //
        // POST: /Wishlist/EditItem
        [HttpPost]
        public ActionResult EditItem(WishlistItem model)
        {
            if (ModelState.IsValid)
            {
                WishlistManager.EditItem(model);
            }

            return RedirectToAction("Index");
        }

        //
        // POST: /Wishlist/DeleteItem
        [HttpPost]
        public ActionResult DeleteItem(WishlistItem model)
        {
            WishlistManager.DeleteItem(model);
            return RedirectToAction("Index");
        }

        //
        // GET: /Wishlist/Remind
        public ActionResult Remind(string id)
        {
            string url = Url.Action("Index", "Home", null, "http");
            WishlistManager.SendReminder(new Guid(id), url);
            return RedirectToAction("Details", new {id});
        }
    }
}