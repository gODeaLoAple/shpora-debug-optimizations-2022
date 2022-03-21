namespace JPEG.Images
{
    public class PixelRgb
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        
        public double Y => 16.0 + (65.738 * R + 129.057 * G + 24.064 * B) / 256.0;
        public double Cb => 128.0 + (-37.945 * R - 74.494 * G + 112.439 * B) / 256.0;
        public double Cr => 128.0 + (112.439 * R - 94.154 * G - 18.285 * B) / 256.0;
    }

    public class PixelYCbCr
    {
        public double Y { get; set; } = 123;
        public double Cb { get; set; } = 123;
        public double Cr { get; set; } = 123;
        
        public double R => (298.082 * Y + 408.583 * Cr) / 256.0 - 222.921;
        public double G => (298.082 * Y - 100.291 * Cb - 208.120 * Cr) / 256.0 + 135.576;
        public double B => (298.082 * Y + 516.412 * Cb) / 256.0 - 276.836;
    }
}