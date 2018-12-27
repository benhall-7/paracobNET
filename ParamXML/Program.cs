using paracobNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace ParamXML
{
    class Program
    {
        static string HelpText = 
            "ParamXML: Convert Ultimate param (.prc) files to XML format or (TODO) back.\n" +
            "required: [input] ; -d / -a (disassemble/assemble)\n" +
            "optional: -h ; -help ; -o [output] ; -l [label file]";
        static BuildMode mode = BuildMode.Invalid;
        static ParamFile file;
        static XmlDocument xml;
        static Dictionary<uint, string> labels = new Dictionary<uint, string>();
        static Stopwatch stopwatch { get; set; }

        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
            bool helpPrinted = false;
            string input = null;
            string output = null;
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
                            labels = LabelIO.Read(args[++i]);
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
                    if (output == null)
                        output = Path.GetFileNameWithoutExtension(input) + ".xml";
                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    file = new ParamFile(input);
                    Console.WriteLine("Converting...");
                    xml = new XmlDocument();
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
                    xml.AppendChild(ParamStruct2Node(file.Root));
                    xml.Save(output);
                    stopwatch.Stop();
                    Console.WriteLine("Finished in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    if (output == null)
                        output = Path.GetFileNameWithoutExtension(input) + ".prc";
                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    //stuff
                    xml = new XmlDocument();
                    xml.Load(input);
                    file = new ParamFile(Node2ParamStruct(xml.DocumentElement));
                    stopwatch.Stop();
                    Console.WriteLine("Finished in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                if (!helpPrinted)
                    Console.WriteLine(HelpText);
            }
        }

        static XmlNode Param2Node(IParam param)
        {
            switch (param.TypeKey)
            {
                case ParamType.@struct:
                    return ParamStruct2Node(param as ParamStruct);
                case ParamType.array:
                    return ParamArray2Node(param as ParamArray);
                default:
                    return ParamValue2Node(param as ParamValue);
            }
        }

        static XmlNode ParamStruct2Node(ParamStruct structure)
        {
            XmlNode xmlNode = xml.CreateElement(ParamType.@struct.ToString());
            XmlAttribute mainAttr = xml.CreateAttribute("type");
            mainAttr.Value = structure.ID.ToString();
            xmlNode.Attributes.Append(mainAttr);
            foreach (var node in structure.Nodes)
            {
                XmlNode childNode = Param2Node(node.Value);
                XmlAttribute attr = xml.CreateAttribute("hash");
                attr.Value = node.Key.ToString(labels);
                childNode.Attributes.Append(attr);
                xmlNode.AppendChild(childNode);
            }
            return xmlNode;
        }

        static XmlNode ParamArray2Node(ParamArray array)
        {
            XmlNode xmlNode = xml.CreateElement(ParamType.array.ToString());
            XmlAttribute mainAttr = xml.CreateAttribute("size");
            mainAttr.Value = array.Nodes.Length.ToString();
            xmlNode.Attributes.Append(mainAttr);
            for (int i = 0; i < array.Nodes.Length; i++)
            {
                XmlNode childNode = Param2Node(array.Nodes[i]);
                XmlAttribute attr = xml.CreateAttribute("index");
                attr.Value = i.ToString();
                childNode.Attributes.Append(attr);
                xmlNode.AppendChild(childNode);
            }
            return xmlNode;
        }

        static XmlNode ParamValue2Node(ParamValue value)
        {
            XmlNode xmlNode = xml.CreateElement(value.TypeKey.ToString());
            XmlText text = xml.CreateTextNode(value.ToString(labels));
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static ParamStruct Node2ParamStruct(XmlNode node)
        {

        }

        enum BuildMode
        {
            Disassemble,
            Assemble,
            Invalid
        }
    }
}
