namespace Genocs.Fonet.Fo;

internal class StringProperty(string str) : Property
{
    private readonly string _str = str;

    internal class Maker(string propName) : PropertyMaker(propName)
    {
        public override Property Make(PropertyList propertyList, string value, FObj fo)
        {
            int vlen = value.Length - 1;
            if (vlen > 0)
            {
                char q1 = value[0];
                if (q1 == '"' || q1 == '\'')
                {
                    if (value[vlen] == q1)
                    {
                        return new StringProperty(value.Substring(1, vlen - 2));
                    }
                    FonetDriver.ActiveDriver?.FireFonetWarning($"String-valued property starts with quote but doesn't end with quote: {value}");
                }
            }
            return new StringProperty(value);
        }
    }

    public override object GetObject()
        => _str;

    public override string GetString()
        => _str;
}