using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers
{
    public class PreviewController : Controller
    {
        //
        // GET: /Preview/FeaturedImage
        [Route("Preview/{accountId:Guid}/{itemId:Guid}")]
        public ActionResult FeaturedImage(Guid accountId, Guid itemId)
        {
            var account = DataRepository.Get<Account>(accountId);
            WishlistItem item = account.Wishlist.Single(x => x.Id == itemId);

            if (item.PreviewImage == null || item.PreviewImage.Length == 0)
            {
                account.Wishlist.Remove(item);
                item.PreviewImage = PreviewGenerator.GetFeaturedImage(item.Url);
                account.Wishlist.Add(item);
                DataRepository.Save(account);
            }

            Response.Cache.SetValidUntilExpires(true);
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.VaryByHeaders["Cookie"] = true;
            Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            Response.Cache.VaryByParams["accountId"] = true;
            Response.Cache.VaryByParams["itemId"] = true;

            return File(item.PreviewImage, "image/jpg");
        }
    }
}