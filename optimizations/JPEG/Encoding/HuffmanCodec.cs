using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JPEG.Encoding;

internal static class HuffmanCodec
{
    public static byte[] Encode(ICollection<byte> data, out Dictionary<BitsWithLength, byte> decodeTable, out long bitsCount)
    {
        var frequences = CalcFrequences(data);

        var root = BuildHuffmanTree(frequences);

        var encodeTable = new BitsWithLength[byte.MaxValue + 1];
        FillEncodeTable(root, encodeTable);

        var bitsBuffer = new BitsBuffer();
        foreach(var b in data)
            bitsBuffer.Add(encodeTable[b]);

        decodeTable = CreateDecodeTable(encodeTable);

        return bitsBuffer.ToArray(out bitsCount);
    }

    public static byte[] Decode(byte[] encodedData, Dictionary<BitsWithLength, byte> decodeTable, long bitsCount)
    {
        var result = new List<byte>();
            
        byte decodedByte;
        var sample = new BitsWithLength { Bits = 0, BitsCount = 0 };
        var length = encodedData.Length;
        for(var byteNum = 0; byteNum < length; byteNum++)
        {
            var b = encodedData[byteNum];
            for(var bitNum = 0; bitNum < 8 && byteNum * 8 + bitNum < bitsCount; bitNum++)
            {
                sample.Bits = (sample.Bits << 1) + ((b & (1 << (8 - bitNum - 1))) != 0 ? 1 : 0);
                sample.BitsCount++;

                if(decodeTable.TryGetValue(sample, out decodedByte))
                {
                    result.Add(decodedByte);

                    sample.BitsCount = 0;
                    sample.Bits = 0;
                }
            }
        }
        return result.ToArray();
    }

    private static Dictionary<BitsWithLength, byte> CreateDecodeTable(BitsWithLength[] encodeTable)
    {
        var result = new Dictionary<BitsWithLength, byte>(new BitsWithLength.Comparer());
        for(var b = 0; b < encodeTable.Length; b++)
        {
            var bitsWithLength = encodeTable[b];
            if(bitsWithLength == null)
                continue;

            result[bitsWithLength] = (byte) b;
        }
        return result;
    }

    private static void FillEncodeTable(HuffmanNode node, BitsWithLength[] encodeSubstitutionTable, int bitvector = 0, int depth = 0)
    {
        while (true)
        {
            if (node.LeafLabel != null)
            {
                encodeSubstitutionTable[node.LeafLabel.Value] = new BitsWithLength { Bits = bitvector, BitsCount = depth };
            }
            else
            {
                if (node.Left != null)
                {
                    FillEncodeTable(node.Left, encodeSubstitutionTable, (bitvector << 1) + 1, depth + 1);
                    node = node.Right;
                    bitvector = (bitvector << 1) + 0;
                    depth = depth + 1;
                    continue;
                }
            }

            break;
        }
    }

    private static HuffmanNode BuildHuffmanTree(int[] frequences)
    {
        var huffmanNodes = GetNodes(frequences);
        var nodes = huffmanNodes.ToHashSet();
        while(nodes.Count > 1)
        {
            HuffmanNode firstMin = null;
            HuffmanNode secondMin = null;

            foreach (var node in nodes)
            {
                if (firstMin is null)
                {
                    firstMin = node;
                }
                else if (secondMin is null)
                {
                    secondMin = node;
                }
                else
                {
                    if (node.Frequency < firstMin.Frequency)
                    {
                        secondMin = firstMin;
                        firstMin = node;
                    }
                    else if (node.Frequency < secondMin.Frequency)
                    {
                        secondMin = node;
                    }
                }
            }

            nodes.Remove(firstMin);
            nodes.Remove(secondMin);
            
            var root = new HuffmanNode(secondMin, firstMin);
            nodes.Add(root);
        }

        return nodes.First();
    }

    private static IEnumerable<HuffmanNode> GetNodes(int[] frequences)
    {
        for (var i = 0; i <= byte.MaxValue; i++)
        {
            if (frequences[i] > 0)
            {
                yield return new HuffmanNode((byte)i, frequences[i]);
            }
        }
    }

    private static int[] CalcFrequences(IEnumerable<byte> data)
    {
        var result = new int[byte.MaxValue + 1];
        foreach (var b in data)
        {
            result[b]++;
        }
        return result;
    }
}