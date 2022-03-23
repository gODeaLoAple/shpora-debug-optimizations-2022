using System;
using System.Numerics;
using JPEG.Images;

namespace JPEG;

public class DctFftCompressor : BaseCompressor
{
    private readonly Complex[] _subMatrix;
    private readonly Complex[] _buffer;

    public DctFftCompressor(int[] quantizationMatrix) : base(quantizationMatrix)
    {
        _buffer = new Complex[Size];
        _subMatrix = new Complex[SquaredSize];
    }

    protected override void Compress(PixelRgb[] pixelMap, double[] output, Func<PixelRgb, double> selector)
    {
        PutSubMatrix(pixelMap, _subMatrix, selector);
        FourierTransform.FFT2(_subMatrix, output, _buffer, DirectionFft.Forward);
    }

    private static void PutSubMatrix(PixelRgb[] pixelMap, Complex[] subMatrix, Func<PixelRgb, double> selector)
    {
        for (var n = 0; n < DiscretCosineTransform.SquareSize; ++n)
        {
            subMatrix[n] = selector(pixelMap[n]);
        }
    }
}