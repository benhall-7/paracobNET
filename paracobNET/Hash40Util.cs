using System;
using System.Collections.Generic;
using System.Text;
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
        public static string FormatToString(ulong hash40, Dictionary<ulong, string> labels)
        {
            if (labels.ContainsKey(hash40))
                return labels[hash40];
            else return FormatToString(hash40);
        }
        public static ulong LabelToHash40(string hash40, Dictionary<string, ulong> labels)
        {
            if (hash40.StartsWith("0x") &&
                ulong.TryParse(hash40.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong val))
                return val;
            if (labels.ContainsKey(hash40))
                return labels[hash40];
            throw new Exception("The string " + hash40 + " does not represent a hexadecimal value and no matching label was found");
        }
    }
}
