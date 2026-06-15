using paracobNET;
using System.Diagnostics;
using System.Xml;
using paracobNET.Xml;

namespace ParamXML
{
    class Program
    {
        static string HelpText =
            "ParamXML: Convert Ultimate param (.prc) files to XML format or back.\n" +
            "required: [input] ; -d / -a (disassemble/assemble)\n" +
            "optional: -h ; -help ; -o [output] ; -l [label file]";
        static BuildMode mode = BuildMode.Invalid;
        static string? labelName { get; set; }
        static Stopwatch stopwatch { get; set; } = new();

        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
            bool helpPrinted = false;
            string? input = null;
            string? output = null;
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-d":
                            mode = BuildMode.Disassemble;
                            break;
                        case "-a":
                            mode = BuildMode.Assemble;
                            break;
                        case "-h":
                        case "-help":
                            Console.WriteLine(HelpText);
                            helpPrinted = true;
                            break;
                        case "-o":
                            output = args[++i];
                            break;
                        case "-l":
                            labelName = args[++i];
                            break;
                        default:
                            input = args[i];
                            break;
                    }
                }
                if (input == null || mode == BuildMode.Invalid)
                {
                    if (!helpPrinted)
                        Console.WriteLine(HelpText);
                    return;
                }
                if (mode == BuildMode.Disassemble)
                {
                    if (string.IsNullOrEmpty(output))
                        output = Path.GetFileNameWithoutExtension(input) + ".xml";
                    var hashToStringLabels = new paracobNET.OrderedDictionary<Hash40, string>();
                    if (!string.IsNullOrEmpty(labelName))
                        hashToStringLabels = LabelIO.GetHashStringDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    var file = new ParamContainer(input);

                    Console.WriteLine("Disassembling...");
                    var settings = new XmlWriterSettings() { Indent = true };
                    ParamXmlSerializer.Save(file.Root, output, hashToStringLabels, settings);

                    stopwatch.Stop();
                    Console.WriteLine("Finished in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    if (string.IsNullOrEmpty(output))
                        output = Path.GetFileNameWithoutExtension(input) + ".prc";
                    var stringToHashLabels = new paracobNET.OrderedDictionary<string, Hash40>();
                    if (!string.IsNullOrEmpty(labelName))
                        stringToHashLabels = LabelIO.GetStringHashDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    var root = ParamXmlSerializer.Load(input, stringToHashLabels);
                    if (root is not ParamMapNode mapRoot)
                        throw new InvalidDataException("XML root must be a struct element.");
                    var file = new ParamContainer(mapRoot);

                    Console.WriteLine("Assembling...");
                    var dirname = Path.GetDirectoryName(output);
                    if (!string.IsNullOrEmpty(dirname))
                        Directory.CreateDirectory(dirname);
                    file.SaveFile(output);

                    stopwatch.Stop();
                    Console.WriteLine("Finished in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException == null)
                    Console.WriteLine(e.StackTrace);
                else
                    Console.WriteLine(e.InnerException.StackTrace);
            }
        }

        enum BuildMode
        {
            Disassemble,
            Assemble,
            Invalid
        }
    }
}
