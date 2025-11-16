using System.Globalization;

namespace paracobNET
{
    public static class LabelIO
    {
        public static OrderedDictionary<ulong, string> GetHashStringDict(string filepath)
        {
            OrderedDictionary<ulong, string> labels = new OrderedDictionary<ulong, string>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(ulong.Parse(splits[0].Substring(2), NumberStyles.HexNumber), splits[1]);
                    else throw new InvalidDataException();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Parse error in {filepath}: \"{line}\"");
#if DEBUG
                    Console.WriteLine(e.Message);
#endif
                }
            }
            return labels;
        }

        public static OrderedDictionary<string, ulong> GetStringHashDict(string filepath)
        {
            OrderedDictionary<string, ulong> labels = new OrderedDictionary<string, ulong>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(splits[1], ulong.Parse(splits[0].Substring(2), NumberStyles.HexNumber));
                    else throw new InvalidDataException();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Parse error in {filepath}: \"{line}\"");
#if DEBUG
                    Console.WriteLine(e.Message);
#endif
                }
            }
            return labels;
        }

        public static void WriteLabels(string filepath, OrderedDictionary<ulong, string> labels)
        {
            List<string> lines = new List<string>();
            foreach (var label in labels)
                lines.Add($"0x{label.Key.ToString("x10")},{label.Value}");
            File.WriteAllLines(filepath, lines);
        }
    }
}
