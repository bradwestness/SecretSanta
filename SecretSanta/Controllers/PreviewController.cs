using System.Web;
using System.Web.Mvc;
using SecretSanta.Utilities;

namespace SecretSanta.Controllers
{
    public class PreviewController : Controller
    {
        //
        // GET: /Preview/FeaturedImage
        public ActionResult FeaturedImage(string url)
        {
            url = HttpUtility.UrlDecode(url);
            byte[] fileContents = PreviewGenerator.GetFeaturedImage(url);

            Response.Cache.SetValidUntilExpires(true);
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.VaryByHeaders["Cookie"] = true;
            Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            Response.Cache.VaryByParams["url"] = true;

            return File(fileContents, "image/jpg");
        }
    }
}