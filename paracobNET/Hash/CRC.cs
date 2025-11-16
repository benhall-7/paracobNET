namespace paracobNET;

internal static class Crc
{
    private static readonly uint[] Crc32Table = new Crc32Table().Table;

    internal static uint Crc32(string word)
    {
        uint hash = 0xffffffff;
        for (int i = 0; i < word.Length; i++)
            hash = (hash >> 8) ^ Crc32Table[(hash ^ word[i]) & 0xff];
        return ~hash;
    }
}

internal class Crc32Table
{
    public uint[] Table { get; }

    internal Crc32Table()
    {
        var table = new uint[256];
        const uint poly = 0xEDB88320u;
        for (uint i = 0; i < 256; i++)
        {
            uint v = i;
            for (int j = 0; j < 8; j++)
            {
                if ((v & 1) != 0) v = (v >> 1) ^ poly;
                else v >>= 1;
            }
            table[i] = v;
        }
        Table = table;
    }
}
