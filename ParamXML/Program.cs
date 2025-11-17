using paracobNET;
using paracobNET.Hash40FormattingExtensions;
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
        static paracobNET.OrderedDictionary<Hash40, string> hashToStringLabels { get; set; }
        static paracobNET.OrderedDictionary<string, Hash40> stringToHashLabels { get; set; }
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
                    hashToStringLabels = new paracobNET.OrderedDictionary<Hash40, string>();
                    if (!string.IsNullOrEmpty(labelName))
                        hashToStringLabels = LabelIO.GetHashStringDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    file = new ParamContainer(input);

                    Console.WriteLine("Disassembling...");
                    xml = new XmlDocument();
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "UTF-8", null));
                    xml.AppendChild(ParamMapToNode(file.Root));
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
                    stringToHashLabels = new paracobNET.OrderedDictionary<string, Hash40>();
                    if (!string.IsNullOrEmpty(labelName))
                        stringToHashLabels = LabelIO.GetStringHashDict(labelName);

                    Console.WriteLine("Initializing...");
                    stopwatch.Start();
                    xml = new XmlDocument();
                    xml.Load(input);
                    file = new ParamContainer(NodeToParamStruct(xml.DocumentElement));

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

        static XmlNode ParamToNode(ParamNode param)
        {
            switch (param)
            {
                case ParamMapNode map:
                    return ParamMapToNode(map);
                case ParamArrayNode array:
                    return ParamArrayToNode(array);
                case ParamBoolNode boolNode:
                    return ParamBoolToNode(boolNode);
                case ParamI8Node i8Node:
                    return ParamI8ToNode(i8Node);
                case ParamU8Node u8Node:
                    return ParamU8ToNode(u8Node);
                case ParamI16Node i16Node:
                    return ParamI16ToNode(i16Node);
                case ParamU16Node u16Node:
                    return ParamU16ToNode(u16Node);
                case ParamI32Node i32Node:
                    return ParamI32ToNode(i32Node);
                case ParamU32Node u32Node:
                    return ParamU32ToNode(u32Node);
                case ParamFloatNode floatNode:
                    return ParamFloatToNode(floatNode);
                case ParamHash40Node hash40Node:
                    return ParamHash40ToNode(hash40Node);
                case ParamStringNode stringNode:
                    return ParamStringToNode(stringNode);
                default:
                    throw new Exception("Unreachable code reached in ParamToNode");
            }
        }

        static XmlNode ParamMapToNode(ParamMapNode structure)
        {
            XmlNode xmlNode = xml.CreateElement(ParamType.Map.ToStandardName());
            foreach (var node in structure.Entries)
            {
                XmlNode childNode = ParamToNode(node.Value);
                XmlAttribute attr = xml.CreateAttribute("hash");
                attr.Value = node.Key.ToLabelOrHex(hashToStringLabels);
                childNode.Attributes.Append(attr);
                xmlNode.AppendChild(childNode);
            }
            return xmlNode;
        }

        static XmlNode ParamArrayToNode(ParamArrayNode array)
        {
            XmlNode xmlNode = xml.CreateElement(ParamType.Array.ToStandardName());
            XmlAttribute mainAttr = xml.CreateAttribute("size");
            mainAttr.Value = array.Items.Count.ToString();
            xmlNode.Attributes.Append(mainAttr);
            for (int i = 0; i < array.Items.Count; i++)
            {
                XmlNode childNode = ParamToNode(array.Items[i]);
                XmlAttribute attr = xml.CreateAttribute("index");
                attr.Value = i.ToString();
                childNode.Attributes.Append(attr);
                xmlNode.AppendChild(childNode);
            }
            return xmlNode;
        }

        static XmlNode ParamBoolToNode(ParamBoolNode value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamI8ToNode(ParamI8Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamU8ToNode(ParamU8Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamI16ToNode(ParamI16Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamU16ToNode(ParamU16Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamI32ToNode(ParamI32Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamU32ToNode(ParamU32Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamFloatToNode(ParamFloatNode value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamHash40ToNode(ParamHash40Node value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToLabelOrHex(hashToStringLabels));
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static XmlNode ParamStringToNode(ParamStringNode value)
        {
            XmlNode xmlNode = xml.CreateElement(value.Type.ToString());
            XmlText text = xml.CreateTextNode(value.Value.ToString());
            xmlNode.AppendChild(text);
            return xmlNode;
        }

        static ParamNode NodeToParam(XmlNode node)
        {
            try
            {
                var type = ParamTypeExtensions.FromStandardName(node.Name);
                switch (type)
                {
                    case ParamType.Map:
                        return NodeToParamStruct(node);
                    case ParamType.Array:
                        return NodeToParamArray(node);
                    case ParamType.Bool:
                        return NodeToParamBool(node);
                    case ParamType.I8:
                        return NodeToParamI8(node);
                    case ParamType.U8:
                        return NodeToParamU8(node);
                    case ParamType.I16:
                        return NodeToParamI16(node);
                    case ParamType.U16:
                        return NodeToParamU16(node);
                    case ParamType.I32:
                        return NodeToParamI32(node);
                    case ParamType.U32:
                        return NodeToParamU32(node);
                    case ParamType.Float:
                        return NodeToParamFloat(node);
                    case ParamType.Hash40:
                        return NodeToParamHash40(node);
                    case ParamType.String:
                        return NodeToParamString(node);
                    default:
                        throw new Exception("Unreachable code reached in NodeToParam");
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

        static ParamMapNode NodeToParamStruct(XmlNode node)
        {
            var entries = new List<KeyValuePair<Hash40, ParamNode>>(node.ChildNodes.Count);
            foreach (XmlNode child in node.ChildNodes)
                entries.Add(new KeyValuePair<Hash40, ParamNode>(
                    Hash40FormattingExtensions.FromLabelOrHex(child.Attributes["hash"].Value, stringToHashLabels),
                    NodeToParam(child)));
            return new ParamMapNode(entries);
        }

        static ParamArrayNode NodeToParamArray(XmlNode node)
        {
            List<ParamNode> children = new List<ParamNode>(node.ChildNodes.Count);
            foreach (XmlNode child in node.ChildNodes)
                children.Add(NodeToParam(child));
            return new ParamArrayNode(children);
        }

        static ParamBoolNode NodeToParamBool(XmlNode node)
        {
            return new ParamBoolNode(node.InnerText.ToLower() == "true");
        }

        static ParamI8Node NodeToParamI8(XmlNode node)
        {
            return new ParamI8Node(sbyte.Parse(node.InnerText));
        }

        static ParamU8Node NodeToParamU8(XmlNode node)
        {
            return new ParamU8Node(byte.Parse(node.InnerText));
        }

        static ParamI16Node NodeToParamI16(XmlNode node)
        {
            return new ParamI16Node(short.Parse(node.InnerText));
        }

        static ParamU16Node NodeToParamU16(XmlNode node)
        {
            return new ParamU16Node(ushort.Parse(node.InnerText));
        }

        static ParamI32Node NodeToParamI32(XmlNode node)
        {
            return new ParamI32Node(int.Parse(node.InnerText));
        }

        static ParamU32Node NodeToParamU32(XmlNode node)
        {
            return new ParamU32Node(uint.Parse(node.InnerText));
        }

        static ParamFloatNode NodeToParamFloat(XmlNode node)
        {
            return new ParamFloatNode(float.Parse(node.InnerText));
        }

        static ParamHash40Node NodeToParamHash40(XmlNode node)
        {
            return new ParamHash40Node(Hash40FormattingExtensions.FromLabelOrHex(node.InnerText, stringToHashLabels));
        }

        static ParamStringNode NodeToParamString(XmlNode node)
        {
            return new ParamStringNode(node.InnerText);
        }

        enum BuildMode
        {
            Disassemble,
            Assemble,
            Invalid
        }
    }
}
