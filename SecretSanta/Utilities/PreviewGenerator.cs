using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace SecretSanta.Utilities
{
    public static class PreviewGenerator
    {
        public static byte[] GetFeaturedImage(string url)
        {
            byte[] output;

            try
            {
                // download the page contents as a string
                using (var client = new WebClient())
                {
                    string pageContents = client.DownloadString(url);
                    MatchCollection matches = Regex.Matches(pageContents, @"<img ([^>]+)>");

                    // build a list of all img tags
                    IList<ImageTag> imageTags = new List<ImageTag>();
                    for (int i = 0; i < matches.Count && i < AppSettings.MaxImagesToLoad; i++)
                    {
                        imageTags.Add(new ImageTag(matches[i].Value, url));
                    }

                    int maxSize = imageTags.Max(t => t.Width*t.Height);
                    ImageTag featured = imageTags.First((t => t.Width*t.Height == maxSize));
                    output = featured.ImageBytes;
                }
            }
            catch
            {
                string fileName = HttpContext.Current.Server.MapPath(AppSettings.DefaultPreviewImage);
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
                int height;
                int width;
                string source = ExtractSource(tag, url);
                ImageBytes = DownloadImage(source, out height, out width);
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
                    using (var client = new WebClient())
                    {
                        byte[] data = client.DownloadData(source);
                        using (var inStream = new MemoryStream(data))
                        {
                            Image tempImage = Image.FromStream(inStream);

                            using (var outStream = new MemoryStream())
                            {
                                tempImage.Save(outStream, ImageFormat.Jpeg);
                                height = tempImage.Height;
                                width = tempImage.Width;
                                output = outStream.ToArray();
                            }
                        }
                    }
                }
                catch
                {
                    height = -1;
                    width = -1;
                    output = new byte[] {};
                }

                return output;
            }

            #endregion
        }
    }
}