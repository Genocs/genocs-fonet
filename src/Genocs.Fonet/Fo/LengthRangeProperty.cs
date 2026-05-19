using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo;

internal class LengthRangeProperty(LengthRange lengthRange) : Property
{
    private readonly LengthRange _lengthRange = lengthRange;

    internal class Maker : LengthProperty.Maker
    {
        protected Maker(string name) : base(name) { }
    }

    public override LengthRange GetLengthRange() 
        => _lengthRange;

    public override object GetObject() 
        => _lengthRange;
}