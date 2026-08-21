using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties;

internal class AlignmentBaselineMaker : ToBeImplementedProperty.Maker
{
    new public static PropertyMaker Maker(string propName)
    {
        return new AlignmentBaselineMaker(propName);
    }

    protected AlignmentBaselineMaker(string name)
        : base(name)
    {
    }


    public override bool IsInherited()
        => false;

    private Property? _defaultProperty;

    public override Property Make(PropertyList propertyList)
        => _defaultProperty ??= Make(propertyList, "auto", propertyList.GetParentFObj());
}