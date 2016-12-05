using SecretSanta.Models;
using SecretSanta.Utilities;
using System;
using System.Web.Mvc;

namespace SecretSanta.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        //
        // GET: /Wishlist/
        public ActionResult Index()
        {
            var model = new WishlistEditModel(User.GetAccount().Id.Value);
            return View(model);
        }

        //
        // GET: /Wishlist/Details
        public ActionResult Details(Guid id)
        {
            var model = new WishlistEditModel(id);
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
                this.SetResultMessage($"<strong>Successfully added</strong> {model.Name}.");
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
                this.SetResultMessage($"<strong>Successfully updated</strong> {model.Name}.");
            }

            return RedirectToAction("Index");
        }

        //
        // POST: /Wishlist/DeleteItem
        [HttpPost]
        public ActionResult DeleteItem(WishlistItem model)
        {
            WishlistManager.DeleteItem(model);
            this.SetResultMessage($"<strong>Successfully deleted</strong> {model.Name}.");
            return RedirectToAction("Index");
        }

        //
        // GET: /Wishlist/Remind
        public ActionResult Remind(Guid id)
        {
            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
            WishlistManager.SendReminder(id, urlHelper);
            this.SetResultMessage("<strong>Reminder sent</strong> successfully.");
            return RedirectToAction("Details", new {id});
        }
    }
}