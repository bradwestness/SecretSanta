using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SecretSanta.Utilities;

public static class ImageResizer
{
    private const double MaxWidth = 1024;

    private const double MaxHeight = 768;

    public static byte[] ResizeJpg(byte[] imageBytes)
    {
        using var inputStream = new MemoryStream(imageBytes, writable: false);
        return ResizeImageInternal(inputStream, ImageFormat.Jpeg);
    }

    public static byte[] ResizeJpg(Stream inputStream) =>
        ResizeImageInternal(inputStream, ImageFormat.Jpeg);

    private static byte[] ResizeImageInternal(Stream inputStream, ImageFormat imageFormat)
    {
        using var inputBitmap = new Bitmap(inputStream);

        if (inputBitmap.Width < MaxWidth
            && inputBitmap.Height < MaxHeight)
        {
            return ConvertToFormat(inputBitmap, imageFormat);
        }

        var scaleWidth = MaxWidth / inputBitmap.Width;
        var scaleHeight = MaxHeight / inputBitmap.Height;
        var scaleFactor = Math.Min(scaleWidth, scaleHeight);

        var rect = new Rectangle(
            0,
            0,
            (int)Math.Round(scaleFactor * inputBitmap.Width),
            (int)Math.Round(scaleFactor * inputBitmap.Height));

        using var imageAttributes = new ImageAttributes();
        imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

        using var outputBitmap = new Bitmap(rect.Width, rect.Height);
        using var gfx = Graphics.FromImage(outputBitmap);
        gfx.CompositingMode = CompositingMode.SourceCopy;
        gfx.CompositingQuality = CompositingQuality.HighQuality;
        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gfx.SmoothingMode = SmoothingMode.HighQuality;
        gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

        gfx.DrawImage(
            inputBitmap,
            rect,
            0,
            0,
            inputBitmap.Width,
            inputBitmap.Height,
            GraphicsUnit.Pixel,
            imageAttributes);

        return ConvertToFormat(outputBitmap, imageFormat);
    }

    private static byte[] ConvertToFormat(Bitmap bitmap, ImageFormat imageFormat)
    {
        using var outputStream = new MemoryStream();
        bitmap.Save(outputStream, imageFormat);
        return outputStream.ToArray();
    }
}
