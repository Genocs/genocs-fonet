using System.Text;
using Genocs.Fonet.Pdf.Gdi.Font.Tables;

namespace Genocs.Fonet.Pdf.Gdi.Font;

/// <summary>
/// Represents an entry in the directory table.
/// </summary>
internal class DirectoryEntry
{
    /// <summary>
    /// Gets or sets a value that represents a <see cref="FontTable"/>
    /// offset, i.e. the number of bytes from the beginning of the file.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// Gets or sets a value representing the number of bytes
    /// a <see cref="FontTable"/> object occupies in a stream.
    /// </summary>
    public uint Length { get; set; }

    /// <summary>
    /// Gets or sets value that represents a checksum of a <see cref="FontTable"/>.
    /// </summary>
    public uint CheckSum { get; set; }

    /// <summary>
    /// Gets the table tag encoded as an unsigned 32-bite integer.
    /// </summary>
    public uint Tag { get; }

    /// <summary>
    /// Returns the table tag as a string.
    /// </summary>
    /// <returns>The table name.</returns>
    public string TableName { get; }

    public DirectoryEntry(string tagName)
    {
        Tag = (uint)(((byte)tagName[0] << 24) | ((byte)tagName[1] << 16) | ((byte)tagName[2] << 8) | ((byte)tagName[3]));
        TableName = tagName;
    }

    public DirectoryEntry(byte[] tag, uint checkSum, uint offset, uint length)
    {
        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag), "tag cannot be null");
        }

        if (tag.Length != 4)
        {
            throw new ArgumentException("tag array must be 4 bytes in size", nameof(tag));
        }

        Tag = (uint)((tag[0] << 24) | (tag[1] << 16) | (tag[2] << 8) | tag[3]);
        TableName = Encoding.ASCII.GetString(tag);
        CheckSum = checkSum;
        Offset = offset;
        Length = length;
    }

    /// <summary>
    /// Gets an instance of an <see cref="FontTable"/> implementation that is
    /// capable of parsing the table identified by <b>tab</b>.
    /// </summary>
    /// <returns>The font name.</returns>
    internal FontTable MakeTable(FontFileReader reader)
        => FontTableFactory.Make(TableName, reader);
}