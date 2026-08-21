using System.Runtime.InteropServices;

namespace Genocs.Fonet.Pdf.Gdi.Structures;

/// <summary>
/// The ABC structure contains the width of a character in a TrueType font. 
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Abc
{
    public int abcA;
    public uint abcB;
    public int abcC;
}
