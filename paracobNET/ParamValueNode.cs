namespace paracobNET;

public abstract class ParamValueNode : ParamNode
{
    // possible shared functions can go here
}

public sealed class ParamBoolNode : ParamValueNode
{
    public override ParamType Type => ParamType.Bool;

    public bool Value { get; init; }

    public ParamBoolNode(bool value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamBoolNode(Value);
}

public sealed class ParamI8Node : ParamValueNode
{
    public override ParamType Type => ParamType.I8;

    public sbyte Value { get; init; }

    public ParamI8Node(sbyte value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamI8Node(Value);
}

public sealed class ParamU8Node : ParamValueNode
{
    public override ParamType Type => ParamType.U8;

    public byte Value { get; init; }

    public ParamU8Node(byte value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamU8Node(Value);
}

public sealed class ParamI16Node : ParamValueNode
{
    public override ParamType Type => ParamType.I16;

    public short Value { get; init; }

    public ParamI16Node(short value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamI16Node(Value);
}

public sealed class ParamU16Node : ParamValueNode
{
    public override ParamType Type => ParamType.U16;

    public ushort Value { get; init; }

    public ParamU16Node(ushort value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamU16Node(Value);
}

public sealed class ParamI32Node : ParamValueNode
{
    public override ParamType Type => ParamType.I32;

    public int Value { get; init; }

    public ParamI32Node(int value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamI32Node(Value);
}

public sealed class ParamU32Node : ParamValueNode
{
    public override ParamType Type => ParamType.U32;

    public uint Value { get; init; }
    public ParamU32Node(uint value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamU32Node(Value);
}

public sealed class ParamFloatNode : ParamValueNode
{
    public override ParamType Type => ParamType.Float;

    public float Value { get; init; }

    public ParamFloatNode(float value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamFloatNode(Value);
}

public sealed class ParamHash40Node : ParamValueNode
{
    public override ParamType Type => ParamType.Hash40;

    public Hash40 Value { get; init; }

    public ParamHash40Node(Hash40 value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamHash40Node(Value);
}

public sealed class ParamStringNode : ParamValueNode
{
    public override ParamType Type => ParamType.String;

    public string Value { get; init; }

    public ParamStringNode(string value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamStringNode(Value);
}
