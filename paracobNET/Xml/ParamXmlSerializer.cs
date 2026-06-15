using System.Globalization;
using System.Xml;
using paracobNET.Extensions;

namespace paracobNET.Xml;

public static class ParamXmlSerializer
{
    public static XmlDocument ToDocument(
        ParamNode node,
        IReadOnlyDictionary<Hash40, string>? labels = null)
    {
        var document = new XmlDocument();
        document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));
        document.AppendChild(ToXmlNode(document, node, labels));
        return document;
    }

    public static XmlElement ToXmlNode(
        XmlDocument document,
        ParamNode node,
        IReadOnlyDictionary<Hash40, string>? labels = null)
    {
        return node switch
        {
            ParamMapNode map => MapToXmlNode(document, map, labels),
            ParamArrayNode array => ArrayToXmlNode(document, array, labels),
            ParamBoolNode boolNode => ScalarToXmlNode(document, boolNode.Type, boolNode.Value.ToString()),
            ParamI8Node i8Node => ScalarToXmlNode(document, i8Node.Type, i8Node.Value.ToString(CultureInfo.InvariantCulture)),
            ParamU8Node u8Node => ScalarToXmlNode(document, u8Node.Type, u8Node.Value.ToString(CultureInfo.InvariantCulture)),
            ParamI16Node i16Node => ScalarToXmlNode(document, i16Node.Type, i16Node.Value.ToString(CultureInfo.InvariantCulture)),
            ParamU16Node u16Node => ScalarToXmlNode(document, u16Node.Type, u16Node.Value.ToString(CultureInfo.InvariantCulture)),
            ParamI32Node i32Node => ScalarToXmlNode(document, i32Node.Type, i32Node.Value.ToString(CultureInfo.InvariantCulture)),
            ParamU32Node u32Node => ScalarToXmlNode(document, u32Node.Type, u32Node.Value.ToString(CultureInfo.InvariantCulture)),
            ParamFloatNode floatNode => ScalarToXmlNode(document, floatNode.Type, floatNode.Value.ToString(CultureInfo.InvariantCulture)),
            ParamHash40Node hash40Node => ScalarToXmlNode(document, hash40Node.Type, hash40Node.Value.ToLabelOrHex(labels)),
            ParamStringNode stringNode => ScalarToXmlNode(document, stringNode.Type, stringNode.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(node), node, "Unknown param node type.")
        };
    }

    public static ParamNode FromDocument(
        XmlDocument document,
        IReadOnlyDictionary<string, Hash40>? labels = null)
    {
        return FromXmlNode(document.DocumentElement ?? throw new InvalidDataException("XML document has no root element."), labels);
    }

    public static ParamNode FromXmlNode(
        XmlNode node,
        IReadOnlyDictionary<string, Hash40>? labels = null)
    {
        try
        {
            var type = GetParamType(node.Name);
            return type switch
            {
                ParamType.Map => XmlNodeToMap(node, labels),
                ParamType.Array => XmlNodeToArray(node, labels),
                ParamType.Bool => new ParamBoolNode(bool.Parse(node.InnerText)),
                ParamType.I8 => new ParamI8Node(sbyte.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.U8 => new ParamU8Node(byte.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.I16 => new ParamI16Node(short.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.U16 => new ParamU16Node(ushort.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.I32 => new ParamI32Node(int.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.U32 => new ParamU32Node(uint.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.Float => new ParamFloatNode(float.Parse(node.InnerText, CultureInfo.InvariantCulture)),
                ParamType.Hash40 => new ParamHash40Node(global::paracobNET.Extensions.Extensions.FromLabelOrHex(node.InnerText, labels)),
                ParamType.String => new ParamStringNode(node.InnerText),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown param type.")
            };
        }
        catch (Exception e)
        {
            throw CreateNodeContextException(node, e);
        }
    }

    public static void Save(
        ParamNode node,
        string path,
        IReadOnlyDictionary<Hash40, string>? labels = null,
        XmlWriterSettings? settings = null)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var writer = XmlWriter.Create(path, settings ?? new XmlWriterSettings { Indent = true });
        ToDocument(node, labels).Save(writer);
    }

    public static ParamNode Load(
        string path,
        IReadOnlyDictionary<string, Hash40>? labels = null)
    {
        var document = new XmlDocument();
        document.Load(path);
        return FromDocument(document, labels);
    }

    private static XmlElement MapToXmlNode(
        XmlDocument document,
        ParamMapNode map,
        IReadOnlyDictionary<Hash40, string>? labels)
    {
        var xmlNode = document.CreateElement(ParamType.Map.ToStandardName());
        foreach (var entry in map.Entries)
        {
            var childNode = ToXmlNode(document, entry.Value, labels);
            var attr = document.CreateAttribute("hash");
            attr.Value = entry.Key.ToLabelOrHex(labels);
            childNode.Attributes.Append(attr);
            xmlNode.AppendChild(childNode);
        }

        return xmlNode;
    }

    private static XmlElement ArrayToXmlNode(
        XmlDocument document,
        ParamArrayNode array,
        IReadOnlyDictionary<Hash40, string>? labels)
    {
        var xmlNode = document.CreateElement(ParamType.Array.ToStandardName());
        var sizeAttr = document.CreateAttribute("size");
        sizeAttr.Value = array.Items.Count.ToString(CultureInfo.InvariantCulture);
        xmlNode.Attributes.Append(sizeAttr);

        for (int i = 0; i < array.Items.Count; i++)
        {
            var childNode = ToXmlNode(document, array.Items[i], labels);
            var attr = document.CreateAttribute("index");
            attr.Value = i.ToString(CultureInfo.InvariantCulture);
            childNode.Attributes.Append(attr);
            xmlNode.AppendChild(childNode);
        }

        return xmlNode;
    }

    private static XmlElement ScalarToXmlNode(
        XmlDocument document,
        ParamType type,
        string value)
    {
        var xmlNode = document.CreateElement(type.ToStandardName());
        xmlNode.AppendChild(document.CreateTextNode(value));
        return xmlNode;
    }

    private static ParamMapNode XmlNodeToMap(
        XmlNode node,
        IReadOnlyDictionary<string, Hash40>? labels)
    {
        var children = GetElementChildren(node);
        var entries = new List<KeyValuePair<Hash40, ParamNode>>(children.Count);
        foreach (var child in children)
        {
            var hash = child.Attributes?["hash"]?.Value;
            if (string.IsNullOrEmpty(hash))
                throw new InvalidDataException("Map child is missing required hash attribute.");

            entries.Add(new KeyValuePair<Hash40, ParamNode>(
                global::paracobNET.Extensions.Extensions.FromLabelOrHex(hash, labels),
                FromXmlNode(child, labels)));
        }

        return new ParamMapNode(entries);
    }

    private static ParamArrayNode XmlNodeToArray(
        XmlNode node,
        IReadOnlyDictionary<string, Hash40>? labels)
    {
        var children = GetElementChildren(node);
        return new ParamArrayNode(children.Select(child => FromXmlNode(child, labels)));
    }

    private static List<XmlNode> GetElementChildren(XmlNode node)
    {
        var children = new List<XmlNode>(node.ChildNodes.Count);
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
                children.Add(child);
        }

        return children;
    }

    private static ParamType GetParamType(string name)
    {
        try
        {
            return ParamTypeExtensions.FromStandardName(name);
        }
        catch (ArgumentOutOfRangeException)
        {
            if (Enum.TryParse<ParamType>(name, ignoreCase: true, out var type))
                return type;

            throw;
        }
    }

    private static Exception CreateNodeContextException(XmlNode node, Exception e)
    {
        string trace = "Trace: " + node.Name;
        if (node.Attributes is not null)
        {
            foreach (XmlAttribute attr in node.Attributes)
                trace += $" ({attr.Name}=\"{attr.Value}\")";
        }

        string message = trace + Environment.NewLine + e.Message;
        return e.InnerException is null
            ? new Exception(message, e)
            : new Exception(message, e.InnerException);
    }
}