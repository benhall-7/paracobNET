using paracobNET;
using prcEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace prcEditor
{
    public class ValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate StandardTemplate { get; set; }
        public DataTemplate BlankTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            IParam param;

            if (item is IStructChild sc)
                param = sc.Param;
            else if (item is IListChild lc)
                param = lc.Param;
            else
                return base.SelectTemplate(item, container);

            switch (param.TypeKey)
            {
                case ParamType.Bool:
                    return BooleanTemplate;
                case ParamType.I8:
                case ParamType.U8:
                case ParamType.I16:
                case ParamType.U16:
                case ParamType.I32:
                case ParamType.U32:
                case ParamType.F32:
                case ParamType.Hash40:
                case ParamType.String:
                    return StandardTemplate;
                default:
                    return BlankTemplate;
            }
        }
    }
}
