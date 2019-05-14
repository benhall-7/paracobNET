using paracobNET;
using System;

namespace prcEditor
{
    static class Util
    {
        public static string GetStructChildName(IParam param, ulong hash40)
        {
            return $"{param.TypeKey} ({Hash40Util.FormatToString(hash40, MainWindow.HashToStringLabels)})";
        }

        public static string GetListChildName(IParam param, int index)
        {
            return $"{param.TypeKey} ({index})";
        }

        public static void ChangeStructChildIndex(IStructChild node, ref int index, int value)
        {
            var parent = node.Parent;
            //ViewModel:
            parent.Children.Move(index, value);
            //underlying class:
            var parentParam = parent.Param;
            parentParam.Nodes.RemoveAt(index);
            parentParam.Nodes.Insert(value, node.Hash40, node.Param);

            index = value;
        }

        public static void ChangeStructChildHash40(IStructChild node, ref ulong hash40, ulong value)
        {
            ParamStruct parent = node.Parent.Param;
            parent.Nodes.ChangeKey(hash40, value);
            hash40 = value;
        }

        public static void ChangeListChildIndex(IListChild node, ref int index, int value)
        {
            //ParamList parent = node.Parent.Param;
            //IParam param = parent.Nodes[index];
            //parent.Nodes.RemoveAt(index);
            //parent.Nodes.Insert(value, param);
        }
    }
}
