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
            "ParamXML: Convert Ultimate param (.prc) files to XML format or back.\n" +
            "required: [input] ; -d / -a (disassemble/assemble)\n" +
            "optional: -h ; -help ; -o [output] ; -l [label file]";
        static BuildMode mode = BuildMode.Invalid;
        static ParamContainer file { get; set; }
        static XmlDocument xml { get; set; }
        static string labelName { get; set; }
        static OrderedDictionary<ulong, string> hashToStringLabels { get; set; }
        static OrderedDictionary<string, ulong> stringToHashLabels { get; set; }
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
                    hashToStringLabels = new OrderedDictionary<ulong, string>();
                    if (!string.IsNullOrEmpty(labelName))
                        hashToStringLabels = LabelIO.GetHashStringDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    file = new ParamContainer();
                    file.OpenFile(input);

                    Console.WriteLine("Disassembling...");
                    xml = new XmlDocument();
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
                    xml.AppendChild(ParamStruct2Node(file.Root));
                    XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                    XmlWriter writer = XmlWriter.Create(output, settings);
                    var dirname = Path.GetDirectoryName(output);
                    if (!string.IsNullOrEmpty(dirname))
                        Directory.CreateDirectory(dirname);
                    xml.Save(writer);

                    stopwatch.Stop();
                    Console.WriteLine("Finished in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    if (string.IsNullOrEmpty(output))
                        output = Path.GetFileNameWithoutExtension(input) + ".prc";
                    stringToHashLabels = new OrderedDictionary<string, ulong>();
                    if (!string.IsNullOrEmpty(labelName))
                        stringToHashLabels = LabelIO.GetStringHashDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    xml = new XmlDocument();
                    xml.Load(input);
                    file = new ParamContainer(Node2ParamStruct(xml.DocumentElement));

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

        static XmlNode Param2Node(IParam param)
        {
            switch (param.Type)
            {
                case ParamType.Map:
                    return ParamStruct2Node(param as ParamMapNode);
                case ParamType.Array:
                    return ParamArray2Node(param as ParamArrayNode);
                default:
                    return ParamValue2Node(param as ParamValueNode);
            }
        }

        static XmlNode ParamStruct2Node(ParamMapNode structure)
        {
            XmlNode xmlNode = xml.CreateElement(ParamType.Map.ToString());
            foreach (var node in structure.Nodes)
            {
                XmlNode childNode = Param2Node(node.Value);
                XmlAttribute attr = xml.CreateAttribute("hash");
                attr.Value = Hash40Util.FormatToString(node.Key, hashToStringLabels);
                childNode.Attributes.Append(attr);
                xmlNode.AppendChild(childNode);
            }
            return xmlNode;
        }

        static XmlNode ParamArray2Node(ParamArrayNode array)
        {
            XmlNode xmlNode = xml.CreateElement(ParamType.Array.ToString());
            XmlAttribute mainAttr = xml.CreateAttribute("size");
            mainAttr.Value = array.Entries.Count.ToString();
            xmlNode.Attributes.Append(mainAttr);
            for (int i = 0; i < array.Entries.Count; i++)
            {
                XmlNode childNode = Param2Node(array.Entries[i]);
                XmlAttribute attr = xml.CreateAttribute("index");
                attr.Value = i.ToString();
                childNode.Attributes.Append(attr);
                xmlNode.AppendChild(childNode);
            }
            return xmlNode;
        }

        static XmlNode ParamValue2Node(ParamValueNode value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.ToString(hashToStringLabels));
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static IParam Node2Param(XmlNode node)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ParamType), node.Name))
                    throw new FormatException($"\"{node.Name}\" is not a valid param type");
                ParamType type = (ParamType)Enum.Parse(typeof(ParamType), node.Name);
                switch (type)
                {
                    case ParamType.Map:
                        return Node2ParamStruct(node);
                    case ParamType.Array:
                        return Node2ParamArray(node);
                    default:
                        return Node2ParamValue(node, type);
                }
            }
            catch (Exception e)
            {
                //recursively add param node context to exceptions until we exit
                string trace = "Trace: " + node.Name;
                foreach (XmlAttribute attr in node.Attributes)
                    trace += $" ({attr.Name}=\"{attr.Value}\")";
                string message = trace + Environment.NewLine + e.Message;
                if (e.InnerException == null)
                    throw new Exception(message, e);
                else
                    throw new Exception(message, e.InnerException);
            }
        }

        static ParamMapNode Node2ParamStruct(XmlNode node)
        {
            Hash40Pairs<IParam> childParams = new Hash40Pairs<IParam>();
            foreach (XmlNode child in node.ChildNodes)
                childParams.Add(child.Attributes["hash"].Value, stringToHashLabels, Node2Param(child));
            return new ParamStruct(childParams);
        }

        static ParamArrayNode Node2ParamArray(XmlNode node)
        {
            int count = node.ChildNodes.Count;
            List<IParam> children = new List<IParam>(count);
            for (int i = 0; i < count; i++)
                children.Add(Node2Param(node.ChildNodes[i]));
            return new ParamList(children);
        }

        static ParamValueNode Node2ParamValue(XmlNode node, ParamType type)
        {
            ParamValueNode param = new ParamValueNode(type);
            param.SetValue(node.InnerText, stringToHashLabels);
            return param;
        }

        enum BuildMode
        {
            Disassemble,
            Assemble,
            Invalid
        }
    }
}
