namespace Genocs.Fonet.Fo.Expr;

internal class NCnameProperty(string ncName) : Property
{
    public override string GetString()
    {
        return ncName;
    }

    public override string GetNCname()
    {
        return ncName;
    }
}