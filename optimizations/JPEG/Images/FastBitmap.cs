using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace JPEG.Images
{
    public class FastBitmap : IDisposable
    {
        public readonly int Height;
        public readonly int Width;
        private readonly Bitmap _bmp;
        private BitmapData _bmpData;
        private bool _isDisposed;
        private unsafe byte* _bmpPointer;
        private readonly int _depth;

        public FastBitmap(Bitmap bmp)
        {
            _bmp = bmp;
            Width = _bmp.Width - _bmp.Width % 8;
            Height = _bmp.Height - _bmp.Height % 8;
            var bounds = new Rectangle(Point.Empty, _bmp.Size);
            _bmpData = _bmp.LockBits(bounds, ImageLockMode.ReadOnly, _bmp.PixelFormat);
            _depth = Image.GetPixelFormatSize(_bmpData.PixelFormat) / 8;
            unsafe
            {
                _bmpPointer = (byte*)_bmpData.Scan0.ToPointer();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetPixels(PixelYCbCr[] pixels, int x, int y, int width, int height)
        {
            unsafe
            {
                var stride = _bmpData.Stride;
                for (var v = 0; v < height; v++)
                {
                    var shift = _bmpPointer +  (y + v) * stride;
                    var offset = v * width;
                    for (var u = 0; u < width; u++)
                    {
                        var pixel = pixels[offset + u];
                        var p = shift + (x + u) * _depth;
                        p[0] = (byte)ToByte((int)pixel.R);
                        p[1] = (byte)ToByte((int)pixel.G);
                        p[2] = (byte)ToByte((int)pixel.B);
                    }
                }
            }
        }

        public void PutPixels(PixelRgb[] pixelMap, int x, int y, int width, int height)
        {
            unsafe
            {
                var stride = _bmpData.Stride;
                for (var v = 0; v < height; v++)
                {
                    var shift = _bmpPointer + (y + v) * stride;
                    var offset = v * width;
                    for (var u = 0; u < width; u++)
                    {
                        var p = shift + (x + u) * _depth;
                        var pixelRgb = pixelMap[offset + u];
                        pixelRgb.R = p[0];
                        pixelRgb.G = p[1];
                        pixelRgb.B = p[2];
                    }
                }
            }
        }

        public static int ToByte(double d)
        {
            var val = (int)d;
            return val switch
            {
                > byte.MaxValue => byte.MaxValue,
                < byte.MinValue => byte.MinValue,
                _               => val
            };
        }

        public void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                UnlockBitmap();
            }

            _isDisposed = true;
        }

        private void UnlockBitmap()
        {
            _bmp.UnlockBits(_bmpData);
            _bmpData = null;
            unsafe
            {
                _bmpPointer = null;
            }
        }
    }
}