using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace SecretSanta.Utilities;

public static class PreviewGenerator
{
    private static string _webRootPath = string.Empty;
    private static IHttpClientFactory? _httpClientFactory = null;

    public static void Initialize(string webRootPath, IHttpClientFactory httpClientFactory)
    {
        _webRootPath = webRootPath;
        _httpClientFactory = httpClientFactory;
    }

    public static async Task<byte[]> GetFeaturedImage(string? url, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Array.Empty<byte>();
        }

        byte[] output;

        try
        {
            // download the page contents as a string
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            var result = await client.GetAsync(url, token);

            var pageContents = await result.Content.ReadAsStringAsync(token);
            var matches = Regex.Matches(pageContents, @"<img ([^>]+)>");

            // build a list of all img tags
            var imageTags = new List<ImageTag>();
            for (int i = 0; i < matches.Count && i < AppSettings.MaxImagesToLoad; i++)
            {
                imageTags.Add(await ImageTag.GetFromUrl(matches[i].Value, url, token));
            }

            var featured = imageTags.OrderByDescending(t => t.Width * t.Height).First();
            output = featured.ImageBytes;
        }
        catch
        {
            var fileName = Path.Combine(_webRootPath, AppSettings.DefaultPreviewImage);
            output = File.ReadAllBytes(fileName);
        }

        return output;
    }

    private class ImageTag
    {
        public byte[] ImageBytes { get; private set; } = Array.Empty<byte>();

        public int Height { get; private set; }

        public int Width { get; private set; }

        public static async Task<ImageTag> GetFromUrl(string tag, string url, CancellationToken token)
        {
            tag = tag.Replace('\'', '"');
            var source = ExtractSource(tag, url);

            return await DownloadImage(source, token);
        }

        private static string ExtractSource(string tag, string url)
        {
            var match = Regex.Match(tag, "src=\"([^\"]+)");
            var source = string.Empty;

            if (match.Groups.Count > 0 && !match.Groups[1].Value.StartsWith("http"))
            {
                List<string> tokens = url.Split('/').ToList();
                tokens.RemoveAt(tokens.Count - 1);
                tokens.Add(match.Groups[1].Value.TrimStart('/'));
                source = string.Join("/", tokens);
            }

            return source;
        }

        private static async Task<ImageTag> DownloadImage(string source, CancellationToken token)
        {
            ImageTag imageTag;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));

                var response = await client.GetAsync(source, token);
                
                using var inStream = await response.Content.ReadAsStreamAsync(token);

                var tempImage = Image.FromStream(inStream);
                var thumbnail = tempImage.GetThumbnailImage(200, 200, new Image.GetThumbnailImageAbort(() => false), IntPtr.Zero);

                using var outStream = new MemoryStream();

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
