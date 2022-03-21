namespace JPEG.Images;

public class PixelRgb
{
    public byte R;
    public byte G;
    public byte B;
        
    public double Y => 16.0 + (65.738 * R + 129.057 * G + 24.064 * B) / 256.0;
    public double Cb => 128.0 + (-37.945 * R - 74.494 * G + 112.439 * B) / 256.0;
    public double Cr => 128.0 + (112.439 * R - 94.154 * G - 18.285 * B) / 256.0;
}