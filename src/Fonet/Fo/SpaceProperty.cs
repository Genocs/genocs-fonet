using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo;

internal class SpaceProperty(Space space) : Property
{
    private readonly Space _space = space;

    internal class Maker : LengthRangeProperty.Maker
    {
        protected Maker(string name) : base(name) { }
    }


    public override Space GetSpace()
        => _space;

    public override LengthRange GetLengthRange()
        => _space;

    public override object GetObject()
        => _space;
}