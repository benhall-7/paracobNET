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
        static ParamFile file { get; set; }
        static XmlDocument xml { get; set; }
        static string labelName { get; set; }
        static Dictionary<ulong, string> hashToStringLabels { get; set; }
        static Dictionary<string, ulong> stringToHashLabels { get; set; }
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
                    if (output == null)
                        output = Path.GetFileNameWithoutExtension(input) + ".xml";
                    hashToStringLabels = new Dictionary<ulong, string>();
                    if (!string.IsNullOrEmpty(labelName))
                        hashToStringLabels = LabelIO.GetHashStringDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    file = new ParamFile(input);
                    Console.WriteLine("Disassembling...");
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
                    stringToHashLabels = new Dictionary<string, ulong>();
                    if (!string.IsNullOrEmpty(labelName))
                        stringToHashLabels = LabelIO.GetStringHashDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    xml = new XmlDocument();
                    xml.Load(input);
                    file = new ParamFile(Node2ParamStruct(xml.DocumentElement));
                    Console.WriteLine("Assembling...");
                    file.Save(output);
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
                attr.Value = Hash40Operator.FormatToString(node.Key, hashToStringLabels);
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
            XmlText text = xml.CreateTextNode(value.ToString(hashToStringLabels));
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static IParam Node2Param(XmlNode node)
        {
            if (!Enum.IsDefined(typeof(ParamType), node.Name))
                throw new FormatException(node.Name + " is not a valid param type");
            ParamType type = (ParamType)Enum.Parse(typeof(ParamType), node.Name);
            switch (type)
            {
                case ParamType.@struct:
                    return Node2ParamStruct(node);
                case ParamType.array:
                    return Node2ParamArray(node);
                default:
                    return Node2ParamValue(node, type);
            }
        }

        static ParamStruct Node2ParamStruct(XmlNode node)
        {
            Hash40Dictionary<IParam> childParams = new Hash40Dictionary<IParam>();
            foreach (XmlNode child in node.ChildNodes)
                childParams.Add(Hash40Operator.LabelToHash40(child.Attributes["hash"].Value, stringToHashLabels), Node2Param(child));
            return new ParamStruct(childParams);
        }

        static ParamArray Node2ParamArray(XmlNode node)
        {
            IParam[] children = new IParam[node.ChildNodes.Count];
            for (int i = 0; i < children.Length; i++)
                children[i] = Node2Param(node.ChildNodes[i]);
            return new ParamArray(children);
        }

        static ParamValue Node2ParamValue(XmlNode node, ParamType type)
        {
            object value;
            switch (type)
            {
                case ParamType.@bool:
                    value = bool.Parse(node.InnerText);
                    break;
                case ParamType.@sbyte:
                    value = sbyte.Parse(node.InnerText);
                    break;
                case ParamType.@byte:
                    value = byte.Parse(node.InnerText);
                    break;
                case ParamType.@short:
                    value = short.Parse(node.InnerText);
                    break;
                case ParamType.@ushort:
                    value = ushort.Parse(node.InnerText);
                    break;
                case ParamType.@int:
                    value = int.Parse(node.InnerText);
                    break;
                case ParamType.@uint:
                    value = uint.Parse(node.InnerText);
                    break;
                case ParamType.@float:
                    value = float.Parse(node.InnerText);
                    break;
                case ParamType.hash40:
                    value = Hash40Operator.LabelToHash40(node.InnerText, stringToHashLabels);
                    break;
                case ParamType.@string:
                    value = node.InnerText;
                    break;
                default:
                    throw new Exception("This exception isn't designed to be possible");
            }
            return new ParamValue(type, value);
        }

        enum BuildMode
        {
            Disassemble,
            Assemble,
            Invalid
        }
    }
}
