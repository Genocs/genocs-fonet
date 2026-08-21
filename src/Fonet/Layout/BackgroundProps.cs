using Genocs.Fonet.Image;
using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Layout;

internal class BackgroundProps
{
    public int Attachment { get; set; }

    public ColorType? Color { get; set; }

    public FonetImage? backImage;

    public int backRepeat;

    public Length? backPosHorizontal;

    public Length? backPosVertical;
}