using SkiaSharp;

namespace SecretSanta.Utilities;

public static class ImageResizer
{
    private const double MaxWidth = 1024;

    private const double MaxHeight = 768;

    private const int DefaultQuality = 75;

    public static byte[] ResizeJpg(byte[] imageBytes)
    {
        using var inputStream = new MemoryStream(imageBytes, writable: false);
        return ResizeImageInternal(inputStream, SKEncodedImageFormat.Jpeg);
    }

    public static byte[] ResizeJpg(Stream inputStream) =>
        ResizeImageInternal(inputStream, SKEncodedImageFormat.Jpeg);

    private static byte[] ResizeImageInternal(
        Stream inputStream,
        SKEncodedImageFormat imageFormat)
    {
        using var managedStream = new SKManagedStream(inputStream);
        using var inputBitmap = SKBitmap.Decode(managedStream);

        if (inputBitmap.Width < MaxWidth
            && inputBitmap.Height < MaxHeight)
        {
            return ConvertToFormat(inputBitmap, imageFormat);
        }

        var scaleWidth = MaxWidth / inputBitmap.Width;
        var scaleHeight = MaxHeight / inputBitmap.Height;
        var scaleFactor = Math.Min(scaleWidth, scaleHeight);
        var outputWidth = (int)Math.Round(scaleFactor * inputBitmap.Width);
        var outputHeight = (int)Math.Round(scaleFactor * inputBitmap.Height);

        var outputImageInfo = new SKImageInfo(outputWidth, outputHeight);

        using var outputBitmap = inputBitmap.Resize(
            outputImageInfo,
            SKFilterQuality.High);

        return ConvertToFormat(outputBitmap, imageFormat);
    }

    private static byte[] ConvertToFormat(
        SKBitmap bitmap,
        SKEncodedImageFormat imageFormat,
        int quality = DefaultQuality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        return image.Encode(imageFormat, quality).ToArray();
    }
}
