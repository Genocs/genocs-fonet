using System.Collections;

namespace Genocs.Fonet.Pdf;

public enum Keyword
{
    Obj,
    EndObj,
    R,
    DictionaryBegin,
    DictionaryEnd,
    ArrayBegin,
    ArrayEnd,
    Stream,
    EndStream,
    True,
    False,
    Null,
    XRef,
    Trailer,
    StartXRef,
    Eof,
    BT,
    ET,
    Tf,
    Td,
    Tr,
    Tj
}

public sealed class KeywordEntries
{
    private static readonly IDictionary Entries;

    static KeywordEntries()
    {
        Entries = new Hashtable
        {
            { Keyword.Obj, "obj"u8.ToArray() },
            { Keyword.EndObj, "endobj"u8.ToArray() },
            { Keyword.R, "R"u8.ToArray() },
            { Keyword.DictionaryBegin, "<<"u8.ToArray() },
            { Keyword.DictionaryEnd, ">>"u8.ToArray() },
            { Keyword.ArrayBegin, "["u8.ToArray() },
            { Keyword.ArrayEnd, "]"u8.ToArray() },
            { Keyword.Stream, "stream"u8.ToArray() },
            { Keyword.EndStream, "endstream"u8.ToArray() },
            { Keyword.True, "true"u8.ToArray() },
            { Keyword.False, "false"u8.ToArray() },
            { Keyword.Null, "null"u8.ToArray() },
            { Keyword.XRef, "xref"u8.ToArray() },
            { Keyword.Trailer, "trailer"u8.ToArray() },
            { Keyword.StartXRef, "startxref"u8.ToArray() },
            { Keyword.Eof, "%%EOF"u8.ToArray() },
            { Keyword.BT, "BT"u8.ToArray() },
            { Keyword.ET, "ET"u8.ToArray() },
            { Keyword.Tf, "Tf"u8.ToArray() },
            { Keyword.Td, "Td"u8.ToArray() },
            { Keyword.Tr, "Tr"u8.ToArray() },
            { Keyword.Tj, "Tj"u8.ToArray() }
        };
    }

    private KeywordEntries() { }

    public static byte[]? GetKeyword(Keyword keyword)
    {
        return (byte[]?)Entries[keyword];
    }
}