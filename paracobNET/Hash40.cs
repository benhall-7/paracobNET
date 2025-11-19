using System.Globalization;

namespace paracobNET;

public readonly record struct Hash40(ulong Value)
{
    // Length in characters (upper 8 bits)
    public byte Length => (byte)(Value >> 32);

    // Raw CRC32 (lower 32 bits)
    public uint Crc32 => (uint)(Value & 0xFFFF_FFFF);

    public Hash40(byte length, uint crc32) : this(((ulong)length << 32) | crc32)
    {
    }

    /// <summary>
    /// Create a Hash40 from a UTF-8 string:
    /// value = (length << 32) | CRC32(string)
    /// </summary>
    public static Hash40 FromString(string s)
    {
        if (s is null) throw new ArgumentNullException(nameof(s));
        if (s.Length > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(s), "Hash40 length cannot exceed 255 characters.");

        uint crc = Crc.Crc32(s);

        return new Hash40((byte)s.Length, crc);
    }

    public override string ToString() => $"0x{Value:X10}";

    // public static string FormatToString(IDictionary<ulong, string> labels)
    // {
    //     if (labels.TryGetValue(Value, out string val))
    //         return val;
    //     else return Value.ToString();
    // }
    // public static Hash40 LabelToHash40(string hash40, IDictionary<string, ulong> labels)
    // {
    //     ulong val;
    //     if (hash40.StartsWith("0x") &&
    //         ulong.TryParse(hash40.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
    //         return val;
    //     if (labels.TryGetValue(hash40, out val))
    //         return val;
    //     throw new InvalidLabelException($"The provided string is not valid hexadecimal and has no matching label", hash40);
    // }
}
