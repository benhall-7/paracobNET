using paracobNET;

namespace prcEditor.ViewModel
{
    /// <summary>
    /// Represents a generic value
    /// </summary>
    public abstract class VM_ParamValue : VM_Param
    {
        public new ParamValue Param
        {
            get => (ParamValue)base.Param;
            set => base.Param = value;
        }

        public string Value
        {
            get { return Param.ToString(MainWindow.HashToStringLabels); }
            set
            {
                try { Param.SetValue(value, MainWindow.StringToHashLabels); }
                catch (InvalidLabelException e)
                {
                    //the label is either not formatted to hexadecimal
                    //or is not present in the dictionary

                    LabelEditor editor = new LabelEditor(e.Label);
                    bool? corrected = editor.ShowDialog();
                    if (corrected == true)
                    {
                        //the user added the value to the dictionary
                        //with a corresponding hash
                        Param.SetValue(e.Label, MainWindow.StringToHashLabels);
                    }
                }
            }
        }

        public VM_ParamValue(ParamValue param)
        {
            Param = param;
        }

        public virtual void UpdateHashes()
        {
            if (Param.TypeKey == ParamType.hash40)
            {
                NotifyPropertyChanged(nameof(Value));
            }
        }
    }
}
