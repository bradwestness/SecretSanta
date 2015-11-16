using SecretSanta.Models;
using SecretSanta.Utilities;
using System;
using System.Web;
using System.Web.Mvc;

namespace SecretSanta.Controllers
{
    public class ReceivedController : Controller
    {
        //
        // GET: /Received/
        public ActionResult Index()
        {
            var account = User.GetAccount();
            ReceivedGiftEditModel model = new ReceivedGiftEditModel(account);
            return View(model);
        }

        //
        // POST: /Received/Save
        [HttpPost]
        public ActionResult Save(ReceivedGift model)
        {
            if (ModelState.IsValid)
            {
                model.Save();
                this.SetResultMessage("Successfully updated your recieved gift info.");
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Received/Image
        [Route("Received/Image/{accountId:Guid}")]
        public ActionResult Image(Guid accountId)
        {
            var account = DataRepository.Get<Account>(accountId);
            ReceivedGift gift = account.ReceivedGift[DateTime.Now.Year];

            if (gift.Image == null || gift.Image.Length == 0)
            {
                return null;
            }

            Response.Cache.SetValidUntilExpires(true);
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.VaryByHeaders["Cookie"] = true;
            Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            Response.Cache.VaryByParams["accountId"] = true;

            return File(gift.Image, "image/jpg");
        }
    }
}