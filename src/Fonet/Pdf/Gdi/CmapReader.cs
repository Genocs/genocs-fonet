using SkiaSharp;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
/// Gets the CmapReader parses TrueType cmap tables for glyph mapping and Unicode coverage.
/// </summary>
internal sealed class CmapReader
{
    private readonly Func<int, ushort> glyphMapper;
    private readonly List<(ushort Start, ushort End)> ranges;

    private CmapReader(Func<int, ushort> glyphMapper, List<(ushort Start, ushort End)> ranges)
    {
        this.glyphMapper = glyphMapper;
        this.ranges = ranges;
    }

    public static CmapReader? FromFontData(byte[] fontData)
    {
        var cmapTable = FontTableAccess.ReadTableFromBytes(fontData, "cmap");
        return cmapTable == null ? null : FromTable(cmapTable);
    }

    public static CmapReader? FromTypeface(SKTypeface typeface)
    {
        var fontData = FontManager.Instance.GetFontData(typeface);
        return fontData.Length == 0 ? null : FromFontData(fontData);
    }

    public static CmapReader? FromTable(byte[] cmapTable)
    {
        if (cmapTable.Length < 4)
        {
            return null;
        }

        int numTables = ReadUInt16BE(cmapTable, 2);
        int bestOffset = -1;
        int bestScore = int.MinValue;

        for (int i = 0; i < numTables; i++)
        {
            int recordOffset = 4 + i * 8;
            if (recordOffset + 8 > cmapTable.Length)
            {
                break;
            }

            int platformId = ReadUInt16BE(cmapTable, recordOffset);
            int encodingId = ReadUInt16BE(cmapTable, recordOffset + 2);
            int offset = ReadInt32BE(cmapTable, recordOffset + 4);
            int score = ScoreEncoding(platformId, encodingId);

            if (score > bestScore && offset > 0 && offset < cmapTable.Length)
            {
                bestScore = score;
                bestOffset = offset;
            }
        }

        if (bestOffset < 0)
        {
            return null;
        }

        int format = ReadUInt16BE(cmapTable, bestOffset);
        return format switch
        {
            0 => ParseFormat0(cmapTable, bestOffset),
            4 => ParseFormat4(cmapTable, bestOffset),
            12 => ParseFormat12(cmapTable, bestOffset),
            _ => null
        };
    }

    private static int ScoreEncoding(int platformId, int encodingId) => (platformId, encodingId) switch
    {
        (3, 1) => 100,
        (3, 10) => 95,
        (3, 4) => 90,
        (3, 0) => 80,
        (0, 3) => 70,
        (0, 0) => 60,
        (1, 0) => 50,
        _ => platformId == 3 ? 40 : 10
    };

    private static CmapReader? ParseFormat0(byte[] cmapTable, int offset)
    {
        if (offset + 262 > cmapTable.Length)
        {
            return null;
        }

        var coverage = new List<(ushort Start, ushort End)> { (0x0000, 0x00FF) };
        return new CmapReader(codePoint =>
        {
            if (codePoint is < 0 or > 255)
            {
                return 0;
            }

            return cmapTable[offset + 6 + codePoint];
        }, coverage);
    }

    public ushort MapCharacter(int codePoint) => glyphMapper(codePoint);

    public IReadOnlyList<(ushort Start, ushort End)> Ranges => ranges;

    private static CmapReader ParseFormat4(byte[] cmapTable, int offset)
    {
        int segCount = ReadUInt16BE(cmapTable, offset + 6) / 2;
        int endCodesOffset = offset + 14;
        int startCodesOffset = endCodesOffset + segCount * 2 + 2;
        int idDeltaOffset = startCodesOffset + segCount * 2;
        int idRangeOffset = idDeltaOffset + segCount * 2;

        var segments = new List<(ushort End, ushort Start, short Delta, ushort RangeOffset)>(segCount);
        var coverage = new List<(ushort Start, ushort End)>();

        for (int i = 0; i < segCount; i++)
        {
            ushort endCode = ReadUInt16BE(cmapTable, endCodesOffset + i * 2);
            ushort startCode = ReadUInt16BE(cmapTable, startCodesOffset + i * 2);
            short idDelta = ReadInt16BE(cmapTable, idDeltaOffset + i * 2);
            ushort idRangeOffsetValue = ReadUInt16BE(cmapTable, idRangeOffset + i * 2);

            if (endCode == 0xFFFF && startCode == 0xFFFF)
            {
                continue;
            }

            segments.Add((endCode, startCode, idDelta, idRangeOffsetValue));
            coverage.Add((startCode, endCode));
        }

        ushort Map(int codePoint)
        {
            if (codePoint < 0 || codePoint > 0xFFFF)
            {
                return 0;
            }

            ushort charCode = (ushort)codePoint;
            for (int i = 0; i < segments.Count; i++)
            {
                var (End, Start, Delta, RangeOffset) = segments[i];
                if (charCode < Start || charCode > End)
                {
                    continue;
                }

                if (RangeOffset == 0)
                {
                    return (ushort)((charCode + Delta) & 0xFFFF);
                }

                int glyphIndexAddress = RangeOffset + (charCode - Start) * 2 + (idRangeOffset + i * 2);
                if (glyphIndexAddress + 2 > cmapTable.Length)
                {
                    return 0;
                }

                ushort glyphIndex = ReadUInt16BE(cmapTable, glyphIndexAddress);
                return glyphIndex == 0 ? (ushort)0 : (ushort)((glyphIndex + Delta) & 0xFFFF);
            }

            return 0;
        }

        return new CmapReader(Map, coverage);
    }

    private static CmapReader ParseFormat12(byte[] cmapTable, int offset)
    {
        long numGroups = ReadUInt32BE(cmapTable, offset + 12);
        int groupsOffset = offset + 16;
        var coverage = new List<(ushort Start, ushort End)>();
        var groups = new List<(uint Start, uint End, uint StartGlyph)>();

        for (int i = 0; i < numGroups; i++)
        {
            int groupOffset = groupsOffset + i * 12;
            if (groupOffset + 12 > cmapTable.Length)
            {
                break;
            }

            uint startChar = ReadUInt32BE(cmapTable, groupOffset);
            uint endChar = ReadUInt32BE(cmapTable, groupOffset + 4);
            uint startGlyph = ReadUInt32BE(cmapTable, groupOffset + 8);
            groups.Add((startChar, endChar, startGlyph));

            if (startChar <= 0xFFFF && endChar <= 0xFFFF)
            {
                coverage.Add(((ushort)startChar, (ushort)endChar));
            }
        }

        ushort Map(int codePoint)
        {
            if (codePoint < 0)
            {
                return 0;
            }

            uint charCode = (uint)codePoint;
            foreach (var group in groups)
            {
                if (charCode < group.Start || charCode > group.End)
                {
                    continue;
                }

                return (ushort)(group.StartGlyph + (charCode - group.Start));
            }

            return 0;
        }

        return new CmapReader(Map, coverage);
    }

    private static ushort ReadUInt16BE(byte[] data, int offset)
        => (ushort)((data[offset] << 8) | data[offset + 1]);

    private static short ReadInt16BE(byte[] data, int offset)
        => (short)ReadUInt16BE(data, offset);

    private static int ReadInt32BE(byte[] data, int offset)
        => (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];

    private static uint ReadUInt32BE(byte[] data, int offset)
        => (uint)ReadInt32BE(data, offset);
}
