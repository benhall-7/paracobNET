using paracobNET;

namespace prcEditor.ViewModel
{
    /// <summary>
    /// A ParamStruct view model serving as the base of all other Param view models.
    /// </summary>
    public class VM_ParamRoot : VM_ParamStruct
    {
        public override string Name => "Root";

        public VM_ParamRoot(ParamStruct struc) : base(struc) { }

        public new void UpdateHashes()
        {
            base.UpdateHashes();
        }
    }
}
