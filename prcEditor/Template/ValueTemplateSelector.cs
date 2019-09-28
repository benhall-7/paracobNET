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
                case ParamType.@bool:
                    return BooleanTemplate;
                case ParamType.@sbyte:
                case ParamType.@byte:
                case ParamType.@short:
                case ParamType.@ushort:
                case ParamType.@int:
                case ParamType.@uint:
                case ParamType.@float:
                case ParamType.hash40:
                case ParamType.@string:
                    return StandardTemplate;
                default:
                    return BlankTemplate;
            }
        }
    }
}
