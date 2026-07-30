namespace Genocs.Fonet.Pdf.Gdi.Font.Tables;

/// <summary>
/// Instantiates a font table from a table tag.
/// </summary>
internal sealed class FontTableFactory
{
    /// <summary>
    /// Prevent instantiation since this is a factory class.
    /// </summary>
    private FontTableFactory()
    {
    }

    /// <summary>
    /// Creates an instance of a class that implements the FontTable interface.
    /// </summary>
    /// <param name="tableName">
    /// One of the pre-defined TrueType tables from the <see cref="TableNames"/> class.
    /// </param>
    /// <returns>
    /// A subclass of <see cref="FontTable"/> that is capable of parsing 
    /// a TrueType table.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If a class capable of parsing <i>tableName</i> is not available.
    /// </exception>
    public static FontTable Make(string tableName, FontFileReader reader)
    {
        DirectoryEntry entry = reader.GetDictionaryEntry(tableName);
        return tableName switch
        {
            TableNames.Head => new HeaderTable(entry),
            TableNames.Hhea => new HorizontalHeaderTable(entry),
            TableNames.Hmtx => new HorizontalMetricsTable(entry),
            TableNames.Maxp => new MaximumProfileTable(entry),
            TableNames.Loca => new IndexToLocationTable(entry),
            TableNames.Glyf => new GlyfDataTable(entry),
            TableNames.Cvt => new ControlValueTable(entry),
            TableNames.Prep => new ControlValueProgramTable(entry),
            TableNames.Fpgm => new FontProgramTable(entry),
            TableNames.Post => new PostTable(entry),
            TableNames.Os2 => new OS2Table(entry),
            TableNames.Name => new NameTable(entry),
            TableNames.Kern => new KerningTable(entry),
            _ => throw new ArgumentException($"Unrecognised table name", nameof(tableName)),
        };
    }
}