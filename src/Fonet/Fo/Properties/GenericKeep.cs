using Genocs.Fonet.DataTypes;

namespace Genocs.Fonet.Fo.Properties;

internal class GenericKeep : KeepProperty.Maker
{
    internal abstract class Enums
    {
        internal abstract class WithinPage
        {
            public const int AUTO = Constants.AUTO;
            public const int ALWAYS = Constants.ALWAYS;
        }

        internal abstract class WithinLine
        {
            public const int AUTO = Constants.AUTO;
            public const int ALWAYS = Constants.ALWAYS;
        }

        internal abstract class WithinColumn
        {
            public const int AUTO = Constants.AUTO;
            public const int ALWAYS = Constants.ALWAYS;
        }
    }

    private class SP_WithinPageMaker : NumberProperty.Maker
    {
        protected internal SP_WithinPageMaker(string sPropName) : base(sPropName)
        {
        }

        protected internal static readonly EnumProperty s_propAUTO = new(Enums.WithinPage.AUTO);
        protected internal static readonly EnumProperty s_propALWAYS = new(Enums.WithinPage.ALWAYS);

        public override Property? CheckEnumValues(string value)
        {
            if (value.Equals("auto"))
            {
                return s_propAUTO;
            }

            if (value.Equals("always"))
            {
                return s_propALWAYS;
            }

            return base.CheckEnumValues(value);
        }

    }

    private static readonly PropertyMaker s_WithinPageMaker =
        new SP_WithinPageMaker("generic-keep.within-page");

    private class SP_WithinLineMaker : NumberProperty.Maker
    {
        protected internal SP_WithinLineMaker(string propertyName) : base(propertyName)
        {
        }

        protected internal static readonly EnumProperty Property_AUTO = new(Enums.WithinLine.AUTO);
        protected internal static readonly EnumProperty Property_ALWAYS = new(Enums.WithinLine.ALWAYS);

        public override Property? CheckEnumValues(string value)
        {
            if (value.Equals("auto"))
            {
                return Property_AUTO;
            }

            if (value.Equals("always"))
            {
                return Property_ALWAYS;
            }

            return base.CheckEnumValues(value);
        }
    }

    private static readonly PropertyMaker WithinLineMaker = new SP_WithinLineMaker("generic-keep.within-line");

    private class SP_WithinColumnMaker : NumberProperty.Maker
    {
        protected internal SP_WithinColumnMaker(string sPropName) : base(sPropName) { }

        protected internal static readonly EnumProperty Property_AUTO = new(Enums.WithinColumn.AUTO);

        protected internal static readonly EnumProperty Property_ALWAYS = new(Enums.WithinColumn.ALWAYS);

        public override Property CheckEnumValues(string value)
        {
            if (value.Equals("auto"))
            {
                return Property_AUTO;
            }

            if (value.Equals("always"))
            {
                return Property_ALWAYS;
            }

            return base.CheckEnumValues(value);
        }

    }

    private static readonly PropertyMaker s_WithinColumnMaker =
        new SP_WithinColumnMaker("generic-keep.within-column");


    new public static PropertyMaker Maker(string propName)
    {
        return new GenericKeep(propName);
    }

    protected GenericKeep(string name)
        : base(name)
    {
        _shorthandMaker = GetSubpropMaker("within-page");

    }


    private PropertyMaker _shorthandMaker;

    public override Property CheckEnumValues(string value)
    {
        return _shorthandMaker.CheckEnumValues(value);
    }

    protected override bool IsCompoundMaker()
    {
        return true;
    }

    protected override PropertyMaker GetSubpropMaker(string subprop)
    {
        if (subprop.Equals("within-page"))
        {
            return s_WithinPageMaker;
        }

        if (subprop.Equals("within-line"))
        {
            return WithinLineMaker;
        }

        if (subprop.Equals("within-column"))
        {
            return s_WithinColumnMaker;
        }

        return base.GetSubpropMaker(subprop);
    }

    protected override Property SetSubprop(Property baseProp, string subpropName, Property subProp)
    {
        var keep = baseProp.GetKeep();
        keep?.SetComponent(subpropName, subProp, false);
        return baseProp;
    }

    public override Property GetSubpropValue(Property baseProp, string subpropName)
    {
        var keep = baseProp.GetKeep();
        return keep?.GetComponent(subpropName);
    }

    private Property? _defaultProperty = null;

    public override Property Make(PropertyList propertyList)
        => _defaultProperty ??= MakeCompound(propertyList, propertyList.GetParentFObj());


    protected override Property MakeCompound(PropertyList pList, FObj fo)
    {
        var keep = new Keep();
        Property subProp;

        subProp = GetSubpropMaker("within-page").Make(pList, getDefaultForWithinPage(), fo);
        keep.SetComponent("within-page", subProp, true);

        subProp = GetSubpropMaker("within-line").Make(pList, getDefaultForWithinLine(), fo);
        keep.SetComponent("within-line", subProp, true);

        subProp = GetSubpropMaker("within-column").Make(pList, getDefaultForWithinColumn(), fo);
        keep.SetComponent("within-column", subProp, true);

        return new KeepProperty(keep);
    }

    protected virtual string getDefaultForWithinPage()
    {
        return "auto";
    }

    protected virtual string getDefaultForWithinLine()
    {
        return "auto";
    }

    protected virtual string getDefaultForWithinColumn()
    {
        return "auto";
    }

    public override Property? ConvertProperty(Property property, PropertyList pList, FObj? fo)
    {
        if (property is KeepProperty)
        {
            return property;
        }

        if (property is not EnumProperty)
        {
            property = _shorthandMaker.ConvertProperty(property, pList, fo);
        }

        if (property != null)
        {
            Property prop = MakeCompound(pList, fo);
            var keep = prop.GetKeep();

            keep?.SetComponent("within-page", property, false);
            keep?.SetComponent("within-line", property, false);
            keep?.SetComponent("within-column", property, false);

            return prop;
        }
        else
        {
            return null;
        }
    }
}