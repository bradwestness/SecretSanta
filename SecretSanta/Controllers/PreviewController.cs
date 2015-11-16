using SecretSanta.Models;
using SecretSanta.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            var item = new KeyValuePair<int, WishlistItem>();
            foreach(var year in account.Wishlist.Keys)
            {
                var match = account.Wishlist[year].FirstOrDefault(x => x.Id == itemId);
                if (match != null)
                {
                    item = new KeyValuePair<int, WishlistItem>(year, match);
                    break;
                }
            }

            if (item.Value.PreviewImage == null || item.Value.PreviewImage.Length == 0)
            {
                account.Wishlist[item.Key].Remove(item.Value);
                item.Value.PreviewImage = PreviewGenerator.GetFeaturedImage(item.Value.Url);
                account.Wishlist[item.Key].Add(item.Value);
                DataRepository.Save(account);
            }

            Response.Cache.SetValidUntilExpires(true);
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.VaryByHeaders["Cookie"] = true;
            Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            Response.Cache.VaryByParams["accountId"] = true;
            Response.Cache.VaryByParams["itemId"] = true;

            return File(item.Value.PreviewImage, "image/jpg");
        }
    }
}