using System;
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

        public void SetPixels(int x, int y, PixelYCbCr[,] pixels)
        {
            var width = pixels.GetLength(1);
            var height = pixels.GetLength(0);
            if (x + width > Width || y + height > Height)
            {
                return;
            }
            var bounds = new Rectangle(x, y, width, height);
            unsafe
            {
                var bmd = _bmp.LockBits(bounds, ImageLockMode.ReadOnly, _bmp.PixelFormat);
                var scan0 = (byte*)bmd.Scan0;
                var stride = bmd.Stride;
                var depth = Image.GetPixelFormatSize(bmd.PixelFormat) / 8; // TODO why?
                for (var v = 0; v < height; v++)
                {
                    var shift = scan0 + v * stride;
                    for (var u = 0; u < width; u++)
                    {
                        var pixel = pixels[v, u];
                        var p = shift + u * depth;
                        p[0] = (byte)ToByte((int)(pixel.R));
                        p[1] = (byte)ToByte((int)(pixel.G));
                        p[2] = (byte)ToByte((int)(pixel.B));
                    }
                }
                _bmp.UnlockBits(bmd);
            }
        }
        
        public void PutPixels(PixelRgb[,] pixelMap, int x, int y)
        {
            var width = pixelMap.GetLength(1);
            var height = pixelMap.GetLength(0);
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
                    for (var u = 0; u < bmd.Width; u++)
                    {
                        var p = shift + u * depth;
                        pixelMap[v, u].R = p[0];
                        pixelMap[v, u].G = p[1];
                        pixelMap[v, u].B = p[2];
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