using Genocs.Fonet.Pdf.Gdi.Font;
using SkiaSharp;
using System.Text;

namespace Genocs.Fonet.Pdf.Gdi;

/// <summary>
///     Reads TrueType/OpenType table data from font files without GDI or OpenStream.
/// </summary>
internal static class FontTableAccess
{
    private const uint TtcfTag = 0x66637474; // 'ttcf'

    public static byte[] ReadFullFont(SKTypeface typeface)
    {
        return FontManager.Instance.GetFontData(typeface);
    }

    public static bool IsTrueTypeCollection(byte[] fontData)
    {
        return fontData.Length >= 4 && ReadUInt32BE(fontData, 0) == TtcfTag;
    }

    public static byte[]? ReadTable(SKTypeface typeface, uint tableTag)
    {
        var fontData = ReadFullFont(typeface);
        if (fontData.Length == 0)
        {
            return null;
        }

        if (tableTag == 0)
        {
            return fontData;
        }

        return ReadTableFromBytes(fontData, tableTag);
    }

    public static byte[]? ReadTable(SKTypeface typeface, string tableName)
    {
        return ReadTable(typeface, TableNames.ToUint(tableName));
    }

    public static byte[]? ReadTableFromBytes(byte[] fontData, string tableName)
    {
        return ReadTableFromBytes(fontData, TableNames.ToUint(tableName));
    }

    public static byte[]? ReadTableFromBytes(byte[] fontData, uint tableTag)
    {
        if (fontData.Length < 12)
        {
            return null;
        }

        if (ReadUInt32BE(fontData, 0) == TtcfTag)
        {
            return ReadTableFromCollection(fontData, tableTag, fontIndex: 0);
        }

        return ReadTableFromSfnt(fontData, tableTag, tableOffset: 0);
    }

    private static byte[]? ReadTableFromCollection(byte[] fontData, uint tableTag, int fontIndex)
    {
        if (fontData.Length < 16)
        {
            return null;
        }

        int version = (int)ReadUInt32BE(fontData, 4);
        if (version is not (0x00010000 or 0x00020000))
        {
            return null;
        }

        int numFonts = ReadInt32BE(fontData, 8);
        if (fontIndex < 0 || fontIndex >= numFonts)
        {
            return null;
        }

        int offsetTableOffset = ReadInt32BE(fontData, 12 + fontIndex * 4);
        if (offsetTableOffset <= 0 || offsetTableOffset >= fontData.Length)
        {
            return null;
        }

        return ReadTableFromSfnt(fontData, tableTag, offsetTableOffset);
    }

    private static byte[]? ReadTableFromSfnt(byte[] fontData, uint tableTag, int tableOffset)
    {
        if (tableOffset + 12 > fontData.Length)
        {
            return null;
        }

        int numTables = ReadUInt16BE(fontData, tableOffset + 4);
        int directoryStart = tableOffset + 12;

        for (int i = 0; i < numTables; i++)
        {
            int entryOffset = directoryStart + i * 16;
            if (entryOffset + 16 > fontData.Length)
            {
                break;
            }

            uint tag = ReadUInt32BE(fontData, entryOffset);
            if (tag != TagToBigEndian(tableTag))
            {
                continue;
            }

            uint offset = ReadUInt32BE(fontData, entryOffset + 8);
            uint length = ReadUInt32BE(fontData, entryOffset + 12);
            if (length == 0 || offset + length > fontData.Length)
            {
                return null;
            }

            var tableData = new byte[length];
            Buffer.BlockCopy(fontData, (int)offset, tableData, 0, (int)length);
            return tableData;
        }

        return null;
    }

    public static string? ReadFamilyName(SKTypeface typeface)
    {
        if (!string.IsNullOrEmpty(typeface.FamilyName))
        {
            return typeface.FamilyName;
        }

        var nameTable = ReadTable(typeface, TableNames.Name);
        return nameTable == null ? null : NameTableParser.ReadFamilyName(nameTable);
    }

    private static uint TagToBigEndian(uint tag) =>
        ((tag & 0x000000FF) << 24) |
        ((tag & 0x0000FF00) << 8) |
        ((tag & 0x00FF0000) >> 8) |
        ((tag & 0xFF000000) >> 24);

    private static ushort ReadUInt16BE(byte[] data, int offset) =>
        (ushort)((data[offset] << 8) | data[offset + 1]);

    private static int ReadInt32BE(byte[] data, int offset) =>
        (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];

    private static uint ReadUInt32BE(byte[] data, int offset) => (uint)ReadInt32BE(data, offset);
}

internal static class NameTableParser
{
    public static string? ReadFamilyName(byte[] nameTable)
    {
        if (nameTable.Length < 6)
        {
            return null;
        }

        int recordCount = (nameTable[2] << 8) | nameTable[3];
        int storageOffset = (nameTable[4] << 8) | nameTable[5];

        for (int i = 0; i < recordCount; i++)
        {
            int recordOffset = 6 + i * 12;
            if (recordOffset + 12 > nameTable.Length)
            {
                break;
            }

            int platformId = (nameTable[recordOffset] << 8) | nameTable[recordOffset + 1];
            int encodingId = (nameTable[recordOffset + 2] << 8) | nameTable[recordOffset + 3];
            int languageId = (nameTable[recordOffset + 4] << 8) | nameTable[recordOffset + 5];
            int nameId = (nameTable[recordOffset + 6] << 8) | nameTable[recordOffset + 7];
            int length = (nameTable[recordOffset + 8] << 8) | nameTable[recordOffset + 9];
            int stringOffset = (nameTable[recordOffset + 10] << 8) | nameTable[recordOffset + 11];

            if (platformId != 3 || nameId != 1 || (encodingId != 0 && encodingId != 1))
            {
                continue;
            }

            if (languageId is not (0x0409 or 0))
            {
                continue;
            }

            int start = storageOffset + stringOffset;
            if (start < 0 || start + length > nameTable.Length)
            {
                continue;
            }

            return Encoding.BigEndianUnicode.GetString(nameTable, start, length);
        }

        return null;
    }

    public static string? ReadPostScriptName(byte[] nameTable)
    {
        if (nameTable.Length < 6)
        {
            return null;
        }

        int recordCount = (nameTable[2] << 8) | nameTable[3];
        int storageOffset = (nameTable[4] << 8) | nameTable[5];

        for (int i = 0; i < recordCount; i++)
        {
            int recordOffset = 6 + i * 12;
            if (recordOffset + 12 > nameTable.Length)
            {
                break;
            }

            int platformId = (nameTable[recordOffset] << 8) | nameTable[recordOffset + 1];
            int encodingId = (nameTable[recordOffset + 2] << 8) | nameTable[recordOffset + 3];
            int nameId = (nameTable[recordOffset + 6] << 8) | nameTable[recordOffset + 7];
            int length = (nameTable[recordOffset + 8] << 8) | nameTable[recordOffset + 9];
            int stringOffset = (nameTable[recordOffset + 10] << 8) | nameTable[recordOffset + 11];

            if (platformId != 3 || nameId != 6 || (encodingId != 0 && encodingId != 1))
            {
                continue;
            }

            int start = storageOffset + stringOffset;
            if (start < 0 || start + length > nameTable.Length)
            {
                continue;
            }

            return Encoding.BigEndianUnicode.GetString(nameTable, start, length);
        }

        return null;
    }
}
