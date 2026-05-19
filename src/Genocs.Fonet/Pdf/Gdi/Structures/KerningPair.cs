using System.Runtime.InteropServices;

namespace Genocs.Fonet.Pdf.Gdi.Structures;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct KerningPair
{
    public ushort wFirst;
    public ushort wSecond;
    public int iKernAmount;
}
