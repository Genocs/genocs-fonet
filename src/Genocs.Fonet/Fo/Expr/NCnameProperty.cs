using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Expr;

internal class NCnameProperty : Property
{
    private string ncName;

    public NCnameProperty(string ncName)
    {
        this.ncName = ncName;
    }

    public ColorType getColor()
    {
        throw new PropertyException("Not a Color");
    }

    public override string GetString()
    {
        return this.ncName;
    }

    public override string GetNCname()
    {
        return this.ncName;
    }
}