using paracobNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Param2Txt
{
    class Program
    {
        static ParamFile file;

        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            string input = null;
            string output = "output.txt";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o")
                    output = args[++i];
                else if (File.Exists(args[i]))
                    input = args[i];
                else
                {
                    HelpText();
                    return;
                }
            }
            if (input == null)
            {
                Console.WriteLine("No target file specified");
                HelpText();
                return;
            }
            Console.WriteLine("Initializing...");
            timer.Start();
            file = new ParamFile(input);
            Console.WriteLine("Writing...");
            File.WriteAllLines(output, RepresentParam(file.Root as ParamStruct).ToArray());
            timer.Stop();
            Console.WriteLine("Operation finished in {0} seconds", timer.Elapsed.TotalSeconds);
        }
        static void HelpText()
        {
            Console.WriteLine("usage: Param2Txt.exe [input]");
            Console.WriteLine("  optional: -o [output]");
        }

        static List<string> RepresentStruct(ParamStruct param)
        {
            List<string> list = new List<string>();
            list.Add("(" + param.TypeKey.ToString() + ")[" + param.Nodes.Count + "]");
            foreach (var node in param.Nodes)
            {
                List<string> nodeRep = RepresentParam(node.Value);
                nodeRep[0] = "<0x" + node.Key.ToString() + ">" + nodeRep[0];
                nodeRep[nodeRep.Count - 1] += ",";
                foreach (var line in nodeRep)
                    list.Add(line);
            }
            return list;
        }

        static List<string> RepresentArray(ParamArray param)
        {
            List<string> list = new List<string>();
            list.Add("(" + param.TypeKey.ToString() + ")[" + param.Nodes.Length + "]");
            foreach (var node in param.Nodes)
            {
                List<string> nodeRep = RepresentParam(node);
                nodeRep[nodeRep.Count - 1] += ",";
                foreach (var line in nodeRep)
                    list.Add(line);
            }
            return list;
        }

        static List<string> RepresentValue(ParamValue param)
        {
            string str = "(" + param.TypeKey.ToString() + ")";
            switch (param.TypeKey)
            {
                case ParamType.boolean:
                    str += (bool)param.Value;
                    break;
                case ParamType.int8:
                    str += (sbyte)param.Value;
                    break;
                case ParamType.uint8:
                    str += (byte)param.Value;
                    break;
                case ParamType.int16:
                    str += (short)param.Value;
                    break;
                case ParamType.uint16:
                    str += (ushort)param.Value;
                    break;
                case ParamType.int32:
                    str += (int)param.Value;
                    break;
                case ParamType.uint32:
                    str += (uint)param.Value;
                    break;
                case ParamType.float32:
                    str += (float)param.Value;
                    break;
                case ParamType.hash40:
                    str += param.Value.ToString();
                    break;
                case ParamType.str:
                    str += (string)param.Value;
                    break;
            }
            return new List<string> { str };
        }

        static List<string> RepresentParam(IParam param)
        {
            if (param is ParamValue)
                return RepresentValue(param as ParamValue);

            List<string> list;
            if (param is ParamArray)
                list = RepresentArray(param as ParamArray);
            else
                list = RepresentStruct(param as ParamStruct);

            if (list.Count > 0)
                list[0] = list[0] + " {";
            for (int i = 1; i < list.Count; i++)
                list[i] = "\t" + list[i];
            list.Add("}");
            return list;
        }
    }
}
