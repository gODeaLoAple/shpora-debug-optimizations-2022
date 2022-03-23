using System.Collections.Generic;

namespace JPEG.Encoding;

internal class BitsBuffer
{
    private readonly List<byte> buffer = new();
    private readonly BitsWithLength _unfinishedBits = new();

    public void Add(BitsWithLength bitsWithLength)
    {
        var bitsCount = bitsWithLength.BitsCount;
        var bits = bitsWithLength.Bits;

        var neededBits = 8 - _unfinishedBits.BitsCount;
        while(bitsCount >= neededBits)
        {
            bitsCount -= neededBits;
            buffer.Add((byte) ((_unfinishedBits.Bits << neededBits) + (bits >> bitsCount)));

            bits &= ((1 << bitsCount) - 1);

            _unfinishedBits.Bits = 0;
            _unfinishedBits.BitsCount = 0;

            neededBits = 8;
        }
        _unfinishedBits.BitsCount +=  bitsCount;
        _unfinishedBits.Bits = (_unfinishedBits.Bits << bitsCount) + bits;
    }

    public byte[] ToArray(out long bitsCount)
    {
        bitsCount = buffer.Count * 8L + _unfinishedBits.BitsCount;
        var count = bitsCount / 8 + (bitsCount % 8 > 0 ? 1 : 0);
        var result = new byte[count];
        buffer.CopyTo(result);
        if (_unfinishedBits.BitsCount > 0)
            result[buffer.Count] = (byte) (_unfinishedBits.Bits << (8 - _unfinishedBits.BitsCount));
        return result;
    }
}