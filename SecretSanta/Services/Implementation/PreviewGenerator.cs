using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using SecretSanta.Utilities;

namespace SecretSanta.Services.Implementation;

public class PreviewGenerator : IPreviewGenerator
{
    private readonly IAppSettings _appSettings;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly IWebHostEnvironment _webHostEnvironment;

    public PreviewGenerator(
        IAppSettings appSettings,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _webHostEnvironment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task<byte[]> GeneratePreviewAsync(string? url, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                if (await GetPreviewImageUri(client, url, token) is Uri previewImageUri)
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));

                    var imageBytes = await client.GetByteArrayAsync(previewImageUri, token);

                    return ImageResizer.ResizeJpg(imageBytes);
                }
            }
            catch
            {
            }
        }

        return await GetDefaultPreviewImageBytes(token);
    }

    private static async Task<Uri?> GetPreviewImageUri(HttpClient client, string url, CancellationToken token)
    {
        var result = await client.GetAsync(url, token);

        var pageContent = await result.Content.ReadAsStringAsync(token);

        if (GetOgImageMetaTag(pageContent) is string metaTag
            && GetContentAttributeValue(metaTag) is Uri contentUri)
        {
            return contentUri;
        }

        return null;
    }

    private static string? GetOgImageMetaTag(string pageContent)
    {
        foreach (Match match in Regex.Matches(pageContent, @"<meta[^>]*property=""og:image""[^>]*>", RegexOptions.IgnoreCase))
        {
            if (match.Success)
            {
                return match.Value;
            }
        }

        return null;
    }

    private static Uri? GetContentAttributeValue(string metaTag)
    {
        foreach (Match match in Regex.Matches(metaTag, @"content=""([^""]+)""", RegexOptions.IgnoreCase))
        {
            if (match is not null
                && match.Groups.Count > 0
                && Uri.TryCreate(match.Groups[1].Value, UriKind.Absolute, out var contentUri))
            {
                return contentUri;
            }
        }

        return null;
    }

    private Task<byte[]> GetDefaultPreviewImageBytes(CancellationToken token)
    {
        var defaultPreviewImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, _appSettings.DefaultPreviewImage);
        return File.ReadAllBytesAsync(defaultPreviewImagePath, token);
    }
}
