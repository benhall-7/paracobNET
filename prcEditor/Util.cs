using paracobNET;
using System;

namespace prcEditor
{
    static class Util
    {
        public static string GetStructChildName(IStructChild sc)
        {
            if (sc.Param is ParamStruct || sc.Param is ParamList)
                return sc.Key;
            return $"{sc.Key} ({sc.Param.TypeKey})";
        }

        public static string GetListChildName(IListChild lc)
        {
            if (lc.Param is ParamStruct || lc.Param is ParamList)
                return lc.Index.ToString();
            return $"{lc.Index} ({lc.Param.TypeKey})";
        }

        public static void ChangeStructChildIndex(IStructChild node, int value)
        {
            var parent = node.Parent;
            //ViewModel:
            parent.Children.Move(node.Index, value);
            //underlying class:
            var parentParam = parent.Param;
            parentParam.Nodes.RemoveAt(node.Index);
            parentParam.Nodes.Insert(value, new HashValuePair<IParam>(node.Hash40, node.Param));
            parent.UpdateChildrenIndeces();
        }

        public static void ChangeListChildIndex(IListChild node, int value)
        {
            var parent = node.Parent;
            //ViewModel:
            parent.Children.Move(node.Index, value);
            //underlying class:
            var parentParam = parent.Param;
            parentParam.Nodes.RemoveAt(node.Index);
            parentParam.Nodes.Insert(value, node.Param);
            parent.UpdateChildrenIndeces();
        }
    }
}
