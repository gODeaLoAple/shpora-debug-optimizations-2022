using System;
using System.Collections.Generic;
using System.Linq;

namespace JPEG;

internal class HuffmanNode : IEquatable<HuffmanNode>, IComparable<HuffmanNode>, IComparable
{
    public int CompareTo(HuffmanNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var frequencyComparison = Frequency.CompareTo(other.Frequency);
        if (frequencyComparison != 0) return frequencyComparison;
        return _id.CompareTo(other._id);
    }

    public int CompareTo(object obj)
    {
        if (ReferenceEquals(null, obj)) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is HuffmanNode other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(HuffmanNode)}");
    }

    public bool Equals(HuffmanNode other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _id == other._id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((HuffmanNode)obj);
    }

    public static bool operator ==(HuffmanNode left, HuffmanNode right) => Equals(left, right);

    public static bool operator !=(HuffmanNode left, HuffmanNode right) => !Equals(left, right);

    private static int Id;


    public readonly byte? LeafLabel = null;
    public readonly int Frequency;
    public readonly HuffmanNode Left;
    public readonly HuffmanNode Right;
    private readonly int _id;

    public HuffmanNode(byte leafLabel, int frequency)
    {
        LeafLabel = leafLabel;
        Frequency = frequency;
        _id = ++Id;
    }

    public HuffmanNode(HuffmanNode left, HuffmanNode right)
    {
        Frequency = left.Frequency + right.Frequency;
        Left = left;
        Right = right;
        _id = ++Id;
    }

    public override string ToString()
    {
        return $"[{_id}] => " + string.Join(" | ", Leafs.OrderBy(x => x));
    }

    public IEnumerable<byte> Leafs
    {
        get
        {
            if (LeafLabel != null)
            {
                return Enumerable.Empty<byte>().Append(LeafLabel.Value);
            }

            return Left.Leafs.Concat(Right.Leafs);
        }
    }
    public override int GetHashCode()
    {
        return _id;
    }
}