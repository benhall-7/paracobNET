namespace paracobNET;

public sealed class ParamBoolNode : ParamNode
{
    public override ParamType Type => ParamType.Bool;

    public bool Value { get; set; }

    public ParamBoolNode(bool value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamBoolNode(Value);
}

public sealed class ParamI8Node : ParamNode
{
    public override ParamType Type => ParamType.I8;

    public sbyte Value { get; set; }

    public ParamI8Node(sbyte value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamI8Node(Value);
}

public sealed class ParamU8Node : ParamNode
{
    public override ParamType Type => ParamType.U8;

    public byte Value { get; set; }

    public ParamU8Node(byte value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamU8Node(Value);
}

public sealed class ParamI16Node : ParamNode
{
    public override ParamType Type => ParamType.I16;

    public short Value { get; set; }

    public ParamI16Node(short value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamI16Node(Value);
}

public sealed class ParamU16Node : ParamNode
{
    public override ParamType Type => ParamType.U16;

    public ushort Value { get; set; }

    public ParamU16Node(ushort value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamU16Node(Value);
}

public sealed class ParamI32Node : ParamNode
{
    public override ParamType Type => ParamType.I32;

    public int Value { get; set; }

    public ParamI32Node(int value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamI32Node(Value);
}

public sealed class ParamU32Node : ParamNode
{
    public override ParamType Type => ParamType.U32;

    public uint Value { get; set; }
    public ParamU32Node(uint value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamU32Node(Value);
}

public sealed class ParamFloatNode : ParamNode
{
    public override ParamType Type => ParamType.Float;

    public float Value { get; set; }

    public ParamFloatNode(float value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamFloatNode(Value);
}

public sealed class ParamHash40Node : ParamNode
{
    public override ParamType Type => ParamType.Hash40;

    public Hash40 Value { get; set; }

    public ParamHash40Node(Hash40 value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamHash40Node(Value);
}

public sealed class ParamStringNode : ParamNode
{
    public override ParamType Type => ParamType.String;

    public string Value { get; set; }

    public ParamStringNode(string value)
    {
        Value = value;
    }

    public override ParamNode Clone() => new ParamStringNode(Value);
}
