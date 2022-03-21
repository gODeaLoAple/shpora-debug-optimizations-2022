namespace JPEG.Images
{
    public class PixelFormat
    {
        private string Format;

        private PixelFormat(string format)
        {
            Format = format;
        }

        public static readonly PixelFormat RGB = new PixelFormat(nameof(RGB));
        public static readonly PixelFormat YCbCr = new PixelFormat(nameof(YCbCr));

        public override string ToString() => Format;

        ~PixelFormat()
        {
            Format = null;
        }
    }
}