using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace paracobNET
{
    public static class LabelIO
    {
        public static Dictionary<uint, string> Read(string filepath)
        {
            Dictionary<uint, string> labels = new Dictionary<uint, string>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(uint.Parse(splits[0].Substring(2), System.Globalization.NumberStyles.HexNumber),
                            splits[1]);
                    else throw new InvalidDataException();
                }
                catch { Console.WriteLine($"Parse error in {filepath}, \"{line}\""); }
            }
            return labels;
        }
    }
}
