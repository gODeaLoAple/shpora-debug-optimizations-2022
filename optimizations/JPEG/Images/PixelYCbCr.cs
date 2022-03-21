namespace JPEG.Images
{
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