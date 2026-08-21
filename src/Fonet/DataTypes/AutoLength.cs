namespace Genocs.Fonet.DataTypes;

internal class AutoLength : Length
{
    public override bool IsAuto()
        => true;

    public override string ToString()
        => "auto";
}