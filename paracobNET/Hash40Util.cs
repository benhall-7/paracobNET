using System.Collections.Generic;
using System.Globalization;

namespace paracobNET
{
    public static class Hash40Util
    {
        public static uint GetCRC32(ulong hash40)
        {
            return (uint)hash40;
        }
        public static byte GetStrLength(ulong hash40)
        {
            return (byte)(hash40 >> 32);
        }
        public static ulong StringToHash40(string word)
        {
            return (ulong)word.Length << 32 | Util.CRC32(word);
        }
        public static string FormatToString(ulong hash40)
        {
            return "0x" + hash40.ToString("x10");
        }
        public static string FormatToString(ulong hash40, IDictionary<ulong, string> labels)
        {
            if (labels.TryGetValue(hash40, out string val))
                return val;
            else return FormatToString(hash40);
        }
        public static ulong LabelToHash40(string hash40, IDictionary<string, ulong> labels)
        {
            ulong val;
            if (hash40.StartsWith("0x") &&
                ulong.TryParse(hash40.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
                return val;
            if (labels.TryGetValue(hash40, out val))
                return val;
            throw new InvalidLabelException($"The provided string is not valid hexadecimal and has no matching label", hash40);
        }
    }
}
