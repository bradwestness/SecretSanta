using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text.RegularExpressions;

namespace SecretSanta.Utilities;

public static class PreviewGenerator
{
    private static string _webRootPath;

    public static void Initialize(string webRootPath)
    {
        _webRootPath = webRootPath;
    }

    public static async Task<byte[]> GetFeaturedImage(string url)
    {
        byte[] output;

        try
        {
            // download the page contents as a string
            var request = WebRequest.CreateHttp(url);
            request.Accept = "text/html";

            var result = await request.GetResponseAsync();

            using var reader = new StreamReader(result.GetResponseStream());

            string pageContents = reader.ReadToEnd();
            MatchCollection matches = Regex.Matches(pageContents, @"<img ([^>]+)>");

            // build a list of all img tags
            IList<ImageTag> imageTags = new List<ImageTag>();
            for (int i = 0; i < matches.Count && i < AppSettings.MaxImagesToLoad; i++)
            {
                imageTags.Add(await ImageTag.GetFromUrl(matches[i].Value, url));
            }

            var featured = imageTags.OrderByDescending(t => t.Width * t.Height).First();
            output = featured.ImageBytes;
        }
        catch
        {
            string fileName = Path.Combine(_webRootPath, AppSettings.DefaultPreviewImage);
            output = File.ReadAllBytes(fileName);
        }

        return output;
    }

    private class ImageTag
    {
        public byte[] ImageBytes { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public static async Task<ImageTag> GetFromUrl(string tag, string url)
        {
            tag = tag.Replace('\'', '"');
            string source = ExtractSource(tag, url);

            return await DownloadImage(source);
        }

        private static string ExtractSource(string tag, string url)
        {
            Match match = Regex.Match(tag, "src=\"([^\"]+)");
            string source = string.Empty;
            if (match.Groups.Count > 0)
            {
                source = match.Groups[1].Value;
                if (!source.StartsWith("http"))
                {
                    List<string> tokens = url.Split('/').ToList();
                    tokens.RemoveAt(tokens.Count - 1);
                    tokens.Add(source.TrimStart('/'));
                    source = string.Join("/", tokens);
                }
            }
            return source;
        }

        private static async Task<ImageTag> DownloadImage(string source)
        {
            ImageTag imageTag;

            try
            {
                var request = WebRequest.CreateHttp(source);
                request.Accept = "image/*";

                var response = await request.GetResponseAsync();

                using var inStream = response.GetResponseStream();
                using var outStream = new MemoryStream();

                var tempImage = Image.FromStream(inStream);
                var thumbnail = tempImage.GetThumbnailImage(200, 200, new Image.GetThumbnailImageAbort(() => false), IntPtr.Zero);
                thumbnail.Save(outStream, ImageFormat.Jpeg);

                imageTag = new ImageTag
                {
                    Height = tempImage.Height,
                    Width = tempImage.Width,
                    ImageBytes = outStream.ToArray()
                };
            }
            catch
            {
                imageTag = new ImageTag();
            }

            return imageTag;
        }
    }
}
