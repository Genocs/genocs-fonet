using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo;

internal class CondLengthProperty : Property
{
    internal class Maker(string name)
        : PropertyMaker(name)
    {

    }

    private readonly CondLength? _condLength;

    public CondLengthProperty(CondLength condLength)
    {
        _condLength = condLength;
    }

    public override CondLength? GetCondLength()
    {
        return _condLength;
    }

    public override Length? GetLength()
    {
        return _condLength?.GetLength()?.GetLength();
    }

    public override object? GetObject()
    {
        return _condLength;
    }
}