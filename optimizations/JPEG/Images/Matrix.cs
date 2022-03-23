using System.Drawing;
using System.Drawing.Imaging;

namespace JPEG.Images
{
    public class Matrix
    {
        private readonly Bitmap _bmp;
        public readonly int Height;
        public readonly int Width;

        public Matrix(Bitmap bmp)
        {
            _bmp = bmp;
            Width = _bmp.Width - _bmp.Width % 8;
            Height = _bmp.Height - _bmp.Height % 8;
        }

        public void SetPixels(PixelYCbCr[] pixels, int x, int y, int width, int height)
        {
            var bounds = new Rectangle(x, y, width, height);
            unsafe
            {
                var bmd = _bmp.LockBits(bounds, ImageLockMode.WriteOnly, _bmp.PixelFormat);
                var scan0 = (byte*)bmd.Scan0;
                var stride = bmd.Stride;
                var depth = Image.GetPixelFormatSize(bmd.PixelFormat) / 8; // TODO why?
                for (var v = 0; v < height; v++)
                {
                    var shift = scan0 + v * stride;
                    var offset = v * width;
                    for (var u = 0; u < width; u++)
                    {
                        var pixel = pixels[offset + u];
                        var p = shift + u * depth;
                        p[0] = (byte)ToByte((int)(pixel.R));
                        p[1] = (byte)ToByte((int)(pixel.G));
                        p[2] = (byte)ToByte((int)(pixel.B));
                    }
                }
                _bmp.UnlockBits(bmd);
            }
        }
        
        public void PutPixels(PixelRgb[] pixelMap, int x, int y, int width, int height)
        {
            var bounds = new Rectangle(x, y, width, height);
            unsafe
            {
                var bmd = _bmp.LockBits(bounds, ImageLockMode.ReadOnly, _bmp.PixelFormat);
                var scan0 = (byte*)bmd.Scan0;
                var stride = bmd.Stride;
                var depth = Image.GetPixelFormatSize(bmd.PixelFormat) / 8;
                for (var v = 0; v < bmd.Height; v++)
                {
                    var shift = scan0 + v * stride;
                    var offset = v * width;
                    for (var u = 0; u < bmd.Width; u++)
                    {
                        var p = shift + u * depth;
                        var pixelRgb = pixelMap[offset + u];
                        pixelRgb.R = p[0];
                        pixelRgb.G = p[1];
                        pixelRgb.B = p[2];
                    }
                }
                _bmp.UnlockBits(bmd);
            }
        }
        
        public static int ToByte(double d)
        {
            var val = (int) d;
            return val switch
            {
                > byte.MaxValue => byte.MaxValue,
                < byte.MinValue => byte.MinValue,
                _               => val
            };
        }
    }
}