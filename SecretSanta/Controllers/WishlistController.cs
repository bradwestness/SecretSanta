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
                this.SetResultMessage(string.Format("<strong>Successfully added</strong> {0}.", model.Name));
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
                this.SetResultMessage(string.Format("<strong>Successfully updated</strong> {0}.", model.Name));
            }

            return RedirectToAction("Index");
        }

        //
        // POST: /Wishlist/DeleteItem
        [HttpPost]
        public ActionResult DeleteItem(WishlistItem model)
        {
            WishlistManager.DeleteItem(model);
            this.SetResultMessage(string.Format("<strong>Successfully deleted</strong> {0}.", model.Name));
            return RedirectToAction("Index");
        }

        //
        // GET: /Wishlist/Remind
        public ActionResult Remind(string id)
        {
            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
            WishlistManager.SendReminder(new Guid(id), urlHelper);
            this.SetResultMessage("<strong>Reminder sent</strong> successfully.");
            return RedirectToAction("Details", new {id});
        }
    }
}