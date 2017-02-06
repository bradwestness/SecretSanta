using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretSanta.Controllers
{
    public class PreviewController : Controller
    {
        //
        // GET: /Preview/FeaturedImage
        [Route("Preview/{accountId:Guid}/{itemId:Guid}"), ResponseCache(Location = ResponseCacheLocation.Any, Duration = int.MaxValue, VaryByHeader = "Cookie", VaryByQueryKeys = new[] { "accountId", "itemId" })]
        public IActionResult FeaturedImage(Guid accountId, Guid itemId)
        {
            var account = DataRepository.Get<Account>(accountId);
            var item = new KeyValuePair<int, WishlistItem>();
            foreach (var year in account.Wishlist.Keys)
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

            return File(item.Value.PreviewImage, "image/jpg");
        }
    }
}
