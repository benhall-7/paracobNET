namespace paracobNET;

public abstract class ParamNode : ICloneable
{
    public abstract ParamType Type { get; }

    public abstract object Clone();
}
