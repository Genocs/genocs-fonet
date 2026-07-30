using System.Globalization;
using System.Text;
using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Render.Pdf;

internal sealed class PdfColor
{
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public PdfColor(ColorType color)
    {
        Red = color.Red;
        Green = color.Green;
        Blue = color.Blue;
    }

    public PdfColor(double red, double green, double blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    // components from 0 to 255
    public PdfColor(int red, int green, int blue) : this(red / 255d, green / 255d, blue / 255d)
    {
    }



    public string getColorSpaceOut(bool fillNotStroke)
    {
        StringBuilder p = new();

        // according to pdfspec 12.1 p.399
        // if the colors are the same then just use the g or G operator
        bool same = false;
        if (Red == Green && Red == Blue)
        {
            same = true;
        }

        // output RGB
        if (fillNotStroke)
        {
            if (same)
            {
                p.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0####} g\n", Red);
            }
            else
            {
                p.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0####} {1:0.0####} {2:0.0####} rg\n", Red, Green, Blue);
            }
        }
        else
        {
            if (same)
            {
                p.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0####} G\n", Red);
            }
            else
            {
                p.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0####} {1:0.0####} {2:0.0####} RG\n", Red, Green, Blue);
            }
        }

        return p.ToString();
    }
}