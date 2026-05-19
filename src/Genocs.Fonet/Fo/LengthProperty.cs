using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Expr;

namespace Genocs.Fonet.Fo;

internal class LengthProperty(Length length) : Property
{
    private readonly Length _length = length;

    internal class Maker(string name) : PropertyMaker(name)
    {
        protected virtual bool IsAutoLengthAllowed()
            => false;

        public override Property ConvertProperty(Property p, PropertyList propertyList, FObj fo)
        {
            if (IsAutoLengthAllowed())
            {
                string pval = p.GetString();
                if (pval != null && pval.Equals("auto"))
                {
                    return new LengthProperty(new AutoLength());
                }
            }

            if (p is LengthProperty)
            {
                return p;
            }

            Length val = p.GetLength();
            if (val != null)
            {
                return new LengthProperty(val);
            }

            return ConvertPropertyDatatype(p, propertyList, fo);
        }
    }

    public override Numeric GetNumeric()
        => _length.AsNumeric(); 

    public override Length GetLength()
        => _length;

    public override object GetObject()
        => _length;
}