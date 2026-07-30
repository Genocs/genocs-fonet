using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties;

internal class GenericCondLength : CondLengthProperty.Maker
{
    internal class Enums
    {
        internal class Conditionality
        {
            public const int DISCARD = Constants.DISCARD;
            public const int RETAIN = Constants.RETAIN;
        }
    }

    private static readonly PropertyMaker _lengthMaker = new LengthProperty.Maker("conditional-length-template.length");

    private class SP_ConditionalityMaker : EnumProperty.Maker
    {
        protected internal SP_ConditionalityMaker(string sPropName)
            : base(sPropName)
        {
        }

        protected internal static readonly EnumProperty _propDISCARD = new EnumProperty(Enums.Conditionality.DISCARD);

        protected internal static readonly EnumProperty _propRETAIN = new EnumProperty(Enums.Conditionality.RETAIN);

        public override Property? CheckEnumValues(string value)
        {
            if (value.Equals("discard"))
            {
                return _propDISCARD;
            }

            if (value.Equals("retain"))
            {
                return _propRETAIN;
            }

            return base.CheckEnumValues(value);
        }

    }

    private static readonly PropertyMaker _conditionalityMaker = new SP_ConditionalityMaker("conditional-length-template.conditionality");

    new public static PropertyMaker Maker(string propName)
    {
        return new GenericCondLength(propName);
    }

    protected GenericCondLength(string name)
        : base(name)
    {
        _shorthandMaker = GetSubpropMaker("length");

    }

    private PropertyMaker _shorthandMaker;

    public override Property? CheckEnumValues(string value)
        => _shorthandMaker?.CheckEnumValues(value);

    protected override bool IsCompoundMaker()
        => true;

    protected override PropertyMaker GetSubpropMaker(string subprop)
    {
        if (subprop.Equals("length"))
        {
            return _lengthMaker;
        }

        if (subprop.Equals("conditionality"))
        {
            return _conditionalityMaker;
        }

        return base.GetSubpropMaker(subprop);
    }

    protected override Property SetSubprop(Property baseProp, string subpropName, Property subProp)
    {
        CondLength? val = baseProp.GetCondLength();
        val?.SetComponent(subpropName, subProp, false);
        return baseProp;
    }

    public override Property? GetSubpropValue(Property baseProp, string subpropName)
    {
        CondLength? val = baseProp.GetCondLength();
        return val?.GetComponent(subpropName);
    }

    private Property? _defaultProp;

    public override Property Make(PropertyList propertyList)
    {
        _defaultProp ??= MakeCompound(propertyList, propertyList.GetParentFObj());
        return _defaultProp;
    }

    protected override Property MakeCompound(PropertyList pList, FObj? fo)
    {
        CondLength p = new CondLength();
        Property subProp;

        subProp = GetSubpropMaker("length").Make(pList, getDefaultForLength(), fo);
        p.SetComponent("length", subProp, true);

        subProp = GetSubpropMaker("conditionality").Make(pList, getDefaultForConditionality(), fo);
        p.SetComponent("conditionality", subProp, true);

        return new CondLengthProperty(p);
    }

    protected virtual string getDefaultForLength()
        => string.Empty;

    protected virtual string getDefaultForConditionality()
        => string.Empty;

    public override Property? ConvertProperty(Property? p, PropertyList pList, FObj? fo)
    {
        if (p is CondLengthProperty)
        {
            return p;
        }

        if (p is not EnumProperty)
        {
            p = _shorthandMaker.ConvertProperty(p, pList, fo);
        }

        if (p != null)
        {
            Property prop = MakeCompound(pList, fo);
            CondLength? pval = prop.GetCondLength();

            pval?.SetComponent("length", p, false);
            return prop;
        }
        else
        {
            return null;
        }
    }
}