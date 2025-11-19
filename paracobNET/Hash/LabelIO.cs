using System.Globalization;

namespace paracobNET
{
    public static class LabelIO
    {
        public static OrderedDictionary<Hash40, string> GetHashStringDict(string filepath)
        {
            OrderedDictionary<Hash40, string> labels = new OrderedDictionary<Hash40, string>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(
                            new Hash40(ulong.Parse(splits[0].Substring(2),
                            NumberStyles.HexNumber)), splits[1]);
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

        public static OrderedDictionary<string, Hash40> GetStringHashDict(string filepath)
        {
            OrderedDictionary<string, Hash40> labels = new OrderedDictionary<string, Hash40>();
            foreach (var line in File.ReadAllLines(filepath))
            {
                string[] splits = line.Split(',');
                try
                {
                    if (splits[0].Substring(0, 2) == "0x")
                        labels.Add(splits[1], new Hash40(
                            ulong.Parse(splits[0].Substring(2),
                            NumberStyles.HexNumber)));
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

        public static void WriteLabels(string filepath, OrderedDictionary<Hash40, string> labels)
        {
            List<string> lines = new List<string>();
            foreach (var label in labels)
                lines.Add($"0x{label.Key},{label.Value}");
            File.WriteAllLines(filepath, lines);
        }
    }
}
