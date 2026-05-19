using Genocs.Fonet.Fo;

namespace Genocs.Fonet.Fo;

internal class ColorProfile : ToBeImplementedElement
{
    new internal class Maker : FObj.Maker
    {
        public override FObj Make(FObj parent, PropertyList propertyList)
        {
            return new ColorProfile(parent, propertyList);
        }
    }

    new public static FObj.Maker GetMaker()
    {
        return new ColorProfile.Maker();
    }

    protected ColorProfile(FObj parent, PropertyList propertyList)
        : base(parent, propertyList)
    {
        this._name = "fo:color-profile";
    }

}