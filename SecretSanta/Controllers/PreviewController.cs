using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers;

public class PreviewController : Controller
{
    [HttpGet, Route("Preview/{accountId:Guid}/{itemId:Guid}")]
    //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = int.MaxValue, VaryByHeader = "Cookie", VaryByQueryKeys = new[] { "accountId", "itemId" })]
    public async Task<IActionResult> FeaturedImage(Guid accountId, Guid itemId, CancellationToken token = default)
    {
        var account = AccountRepository.Get(accountId);
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
            item.Value.PreviewImage = await PreviewGenerator.GetFeaturedImage(item.Value.Url, token);
            account.Wishlist[item.Key].Add(item.Value);
            AccountRepository.Save(account);
        }

        return File(item.Value.PreviewImage, "image/jpg");
    }
}
