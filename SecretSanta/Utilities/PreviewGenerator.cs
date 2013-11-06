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
        public static byte[] GetFeaturedImage(string url)
        {
            byte[] output;

            // download the page contents as a string
            using (var client = new WebClient())
            {
                string pageContents = client.DownloadString(url);
                MatchCollection matches = Regex.Matches(pageContents, @"<img ([^>]+)>");

                // build a list of all img tags
                IList<ImageTag> imageTags = new List<ImageTag>();
                for (var i = 0; i < matches.Count; i++)
                {
                    imageTags.Add(new ImageTag(matches[i].Value, url));
                }

                // download the image data from the largest image
                int maxSize = imageTags.Max(t => t.Width * t.Height);
                var imageTag = imageTags.FirstOrDefault(t => t.Width * t.Height == maxSize);
                byte[] data = client.DownloadData(imageTag.Source);

                // convert the data to an image
                using (var inStream = new MemoryStream(data))
                {
                    Image image = Image.FromStream(inStream);

                    // convert the data to a compressed jpeg
                    using (var outStream = new MemoryStream())
                    {
                        image.Save(outStream, ImageFormat.Jpeg);
                        output = outStream.ToArray();
                    }
                }
            }

            return output;
        }

        private class ImageTag
        {
            #region Variables

            public string Source { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            #endregion

            #region Public Methods

            public ImageTag(string tag, string url)
            {
                tag = tag.Replace('\'', '"');
                Source = ExtractSource(tag, url);
                Width = ExtractWidth(tag);
                Height = ExtractHeight(tag);
            }

            #endregion

            #region Private Methods

            private string ExtractSource(string tag, string url)
            {
                var match = Regex.Match(tag, "src=\"([^\"]+)");
                string source = string.Empty;
                if (match.Groups.Count > 0)
                {
                    source = match.Groups[1].Value;
                    if (!source.StartsWith("http"))
                    {
                        var tokens = url.Split('/').ToList();
                        tokens.RemoveAt(tokens.Count - 1);
                        tokens.Add(source.TrimStart('/'));
                        source = string.Join("/", tokens);
                    }
                }
                return source;
            }

            private int ExtractHeight(string tag)
            {
                var match = Regex.Match(tag, "height=\"([^\"]+)");
                int height = 0;
                if (match.Groups.Count > 0)
                {
                    var value = Regex.Replace(match.Groups[1].Value, "\\D", "");
                    int.TryParse(value, out height);
                }
                return height;
            }

            private int ExtractWidth(string tag)
            {
                var match = Regex.Match(tag, "width=\"([^\"]+)");
                int width = 0;
                if (match.Groups.Count > 0)
                {
                    var value = Regex.Replace(match.Groups[1].Value, "\\D", "");
                    int.TryParse(value, out width);
                }
                return width;
            }

            #endregion
        }
    }
}
