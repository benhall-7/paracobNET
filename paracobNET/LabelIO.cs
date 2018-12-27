using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace paracobNET
{
    public static class LabelIO
    {
        public static Dictionary<ulong, string> GetHashStringDict(string filepath)
        {
            Dictionary<ulong, string> labels = new Dictionary<ulong, string>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(ulong.Parse(splits[0].Substring(2), NumberStyles.HexNumber), splits[1]);
                    else throw new InvalidDataException();
                }
                catch { Console.WriteLine($"Parse error in {filepath}, \"{line}\""); }
            }
            return labels;
        }

        public static Dictionary<string, ulong> GetStringHashDict(string filepath)
        {
            Dictionary<string, ulong> labels = new Dictionary<string, ulong>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(splits[1], ulong.Parse(splits[0].Substring(2), NumberStyles.HexNumber));
                    else throw new InvalidDataException();
                }
                catch { Console.WriteLine($"Parse error in {filepath}, \"{line}\""); }
            }
            return labels;
        }

        public static void WriteLabels(string filepath, Dictionary<ulong, string> labels)
        {
            List<string> lines = new List<string>();
            foreach (var label in labels)
                lines.Add($"0x{label.Key.ToString("x8")},{label.Value}");
            File.WriteAllLines(filepath, lines);
        }
    }
}
