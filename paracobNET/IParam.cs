namespace paracobNET
{
    public interface IParam
    {
        ParamType TypeKey { get; }
        IParam Clone();
    }
}
