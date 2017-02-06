using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace SecretSanta.Utilities
{
    public static class PreviewGenerator
    {
        private static string _webRootPath;

        public static void Initialize(string webRootPath)
        {
            _webRootPath = webRootPath;
        }

        public static byte[] GetFeaturedImage(string url)
        {
            byte[] output;

            try
            {
                // download the page contents as a string
                var request = HttpWebRequest.CreateHttp(url);
                request.Accept = "text/html";

                var result = request.GetResponseAsync().Result;

                using (var reader = new StreamReader(result.GetResponseStream()))
                {
                    string pageContents = reader.ReadToEnd();
                    MatchCollection matches = Regex.Matches(pageContents, @"<img ([^>]+)>");

                    // build a list of all img tags
                    IList<ImageTag> imageTags = new List<ImageTag>();
                    for (int i = 0; i < matches.Count && i < AppSettings.MaxImagesToLoad; i++)
                    {
                        imageTags.Add(new ImageTag(matches[i].Value, url));
                    }

                    var featured = imageTags.OrderByDescending(t => t.Width * t.Height).First();
                    output = featured.ImageBytes;
                }
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
            #region Variables

            public byte[] ImageBytes { get; private set; }

            public int Height { get; private set; }

            public int Width { get; private set; }

            #endregion

            #region Public Methods

            public ImageTag(string tag, string url)
            {
                tag = tag.Replace('\'', '"');
                string source = ExtractSource(tag, url);
                ImageBytes = DownloadImage(source, out int height, out int width);
                Height = height;
                Width = width;
            }

            #endregion

            #region Private Methods

            private string ExtractSource(string tag, string url)
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

            private byte[] DownloadImage(string source, out int height, out int width)
            {
                byte[] output;

                try
                {
                    var request = HttpWebRequest.CreateHttp(source);
                    request.Accept = "image/*";

                    var response = request.GetResponseAsync().Result;

                    using (var inStream = response.GetResponseStream())
                    using (var outStream = new MemoryStream())
                    {
                        var tempImage = Image.FromStream(inStream);

                        height = tempImage.Height;
                        width = tempImage.Width;

                        var thumbnail = tempImage.GetThumbnailImage(200, 200, new Image.GetThumbnailImageAbort(() => false), IntPtr.Zero);
                        thumbnail.Save(outStream, ImageFormat.Jpeg);
                        output = outStream.ToArray();
                    }
                }
                catch
                {
                    height = 0;
                    width = 0;
                    output = new byte[] { };
                }

                return output;
            }

            #endregion
        }
    }
}
