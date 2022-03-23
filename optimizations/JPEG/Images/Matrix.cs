using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace JPEG.Images;

public class Matrix : IDisposable
{
    private readonly Bitmap _bmp;
    public readonly int Height;
    public readonly int Width;
    private readonly BitmapData _bmd;
    private readonly int _depth;

    public Matrix(Bitmap bmp)
    {
        _bmp = bmp;
        Width = _bmp.Width - _bmp.Width % 8;
        Height = _bmp.Height - _bmp.Height % 8;
        var bounds = new Rectangle(Point.Empty, new Size(Width, Height));
        _bmd = _bmp.LockBits(bounds, ImageLockMode.WriteOnly, _bmp.PixelFormat);
        _depth = Image.GetPixelFormatSize(_bmd.PixelFormat) / 8;
    }

    public void SetPixels(PixelYCbCr[] pixels, int x, int y, int width, int height)
    {
        unsafe
        {
            var scan0 = (byte*)_bmd.Scan0;
            var stride = _bmd.Stride;
            for (var v = 0; v < height; v++)
            {
                var shift = scan0 + (v + y) * stride;
                var offset = v * width;
                for (var u = 0; u < width; u++)
                {
                    var pixel = pixels[offset + u];
                    var p = shift + (x + u) * _depth;
                    p[0] = (byte)ToByte((int)(pixel.R));
                    p[1] = (byte)ToByte((int)(pixel.G));
                    p[2] = (byte)ToByte((int)(pixel.B));
                }
            }
        }
    }
        
    public void PutPixels(PixelRgb[] pixelMap, int x, int y, int width, int height)
    {
        unsafe
        {
            var scan0 = (byte*)_bmd.Scan0;
            var stride = _bmd.Stride;
            for (var v = 0; v < height; v++)
            {
                var shift = scan0 + (y + v) * stride;
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
        var val = (int) d;
        return val switch
        {
            > byte.MaxValue => byte.MaxValue,
            < byte.MinValue => byte.MinValue,
            _               => val
        };
    }

    public void Dispose()
    {
        _bmp.UnlockBits(_bmd);
    }
}