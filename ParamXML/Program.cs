using paracobNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ParamXML
{
    class Program
    {
        static string HelpText = "ParamXML: Convert Ultimate param (.prc) files to XML format or back.\n" +
            "required: [input]\n" +
            "optional: -h ; -help ; -o [output] ; -l [label file]";
        Dictionary<uint, string> labels = new Dictionary<uint, string>();

        static void Main(string[] args)
        {
            bool helpPrinted = false;
            string input = null;
            string output = null;
            ParamFile file;
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-h":
                        case "-help":
                            Console.WriteLine(HelpText);
                            helpPrinted = true;
                            break;
                        case "-o":
                            output = args[++i];
                            break;
                        default:
                            input = args[i];
                            break;
                    }
                }
                if (output == null)
                    output = Path.GetFileNameWithoutExtension(input) + ".xml";

                file = new ParamFile(input);

                XmlDocument xml = new XmlDocument();
                xml.AppendChild(ParamStruct2Node(file.Root));
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

        }

        static XmlNode ParamArray2Node(ParamArray array)
        {

        }

        static XmlNode ParamValue2Node(ParamValue value)
        {

        }
    }
}
