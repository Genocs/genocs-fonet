namespace Genocs.Fonet.DataTypes;

internal class KeepValue(string type, int val)
{
    public string KeepType { get; } = type;
    public int Value { get; } = val;

    public const string KEEP_WITH_ALWAYS = "KEEP_WITH_ALWAYS";
    public const string KEEP_WITH_AUTO = "KEEP_WITH_AUTO";
    public const string KEEP_WITH_VALUE = "KEEP_WITH_VALUE";

    public override string ToString()
        => KeepType;
}