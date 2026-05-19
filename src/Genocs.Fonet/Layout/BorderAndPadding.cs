using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Layout;

internal class BorderAndPadding : ICloneable
{
    public const int TOP = 0;
    public const int RIGHT = 1;
    public const int BOTTOM = 2;
    public const int LEFT = 3;

    internal class ResolvedCondLength : ICloneable
    {
        internal int Length { get; set; }
        internal bool Discard { get; set; }

        private ResolvedCondLength(int length, bool discard)
        {
            Length = length;
            Discard = discard;
        }

        internal ResolvedCondLength(CondLength length)
        {
            Discard = length.IsDiscard();
            Length = length.MValue();
        }

        public object Clone()
            => new ResolvedCondLength(Length, Discard);
    }

    public object Clone()
    {
        BorderAndPadding bp = new()
        {
            padding = (ResolvedCondLength[])padding.Clone(),
            borderInfo = (BorderInfo[])borderInfo.Clone()
        };

        for (int i = 0; i < padding.Length; i++)
        {
            if (padding[i] != null)
            {
                bp.padding[i] = (ResolvedCondLength)padding[i].Clone();
            }
            if (borderInfo[i] != null)
            {
                bp.borderInfo[i] = (BorderInfo)borderInfo[i].Clone();
            }
        }
        return bp;
    }

    internal class BorderInfo : ICloneable
    {
        internal int Style { get; private set; }
        internal ColorType Color { get; private set; }
        internal ResolvedCondLength Width { get; private set; }

        internal BorderInfo(int style, CondLength width, ColorType color)
        {
            Style = style;
            Width = new ResolvedCondLength(width);
            Color = color;
        }

        private BorderInfo(int style, ResolvedCondLength width, ColorType color)
        {
            Style = style;
            Width = width;
            Color = color;
        }

        public object Clone()
            => new BorderInfo(Style, (ResolvedCondLength)Width.Clone(), (ColorType)Color.Clone());
    }

    private BorderInfo[] borderInfo = new BorderInfo[4];
    private ResolvedCondLength[] padding = new ResolvedCondLength[4];

    public BorderAndPadding()
    {
    }

    public void SetBorder(int side, int style, CondLength width, ColorType color)
    {
        borderInfo[side] = new BorderInfo(style, width, color);
    }

    public void SetPadding(int side, CondLength width)
    {
        padding[side] = new ResolvedCondLength(width);
    }

    public void SetPaddingLength(int side, int iLength)
    {
        padding[side].Length = iLength;
    }

    public void SetBorderLength(int side, int iLength)
    {
        borderInfo[side].Width.Length = iLength;
    }

    public int GetBorderLeftWidth(bool discard)
    {
        return GetBorderWidth(LEFT, discard);
    }

    public int GetBorderRightWidth(bool discard)
    {
        return GetBorderWidth(RIGHT, discard);
    }

    public int GetBorderTopWidth(bool discard)
    {
        return GetBorderWidth(TOP, discard);
    }

    public int GetBorderBottomWidth(bool discard)
    {
        return GetBorderWidth(BOTTOM, discard);
    }

    public int GetPaddingLeft(bool discard)
    {
        return GetPadding(LEFT, discard);
    }

    public int GetPaddingRight(bool discard)
    {
        return GetPadding(RIGHT, discard);
    }

    public int GetPaddingBottom(bool discard)
    {
        return GetPadding(BOTTOM, discard);
    }

    public int GetPaddingTop(bool discard)
        => GetPadding(TOP, discard);


    private int GetBorderWidth(int side, bool discard)
    {
        if ((borderInfo[side] == null) || (discard && borderInfo[side].Width.Discard))
        {
            return 0;
        }
        else
        {
            return borderInfo[side].Width.Length;
        }
    }

    public ColorType? GetBorderColor(int side)
    {
        if (borderInfo[side] != null)
        {
            return borderInfo[side].Color;
        }
        else
        {
            return null;
        }
    }

    public int getBorderStyle(int side)
    {
        if (borderInfo[side] != null)
        {
            return borderInfo[side].Style;
        }
        else
        {
            return 0;
        }
    }

    private int GetPadding(int side, bool discard)
    {
        if ((padding[side] == null) || (discard && padding[side].Discard))
        {
            return 0;
        }
        else
        {
            return padding[side].Length;
        }
    }
}