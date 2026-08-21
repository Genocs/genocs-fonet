using Genocs.Fonet.Fo.Properties;
using System.Collections;

namespace Genocs.Fonet.Fo;

internal class PropertyList(PropertyList? parentPropertyList, string nameSpace, string element) : Hashtable
{
    private byte[]? wmtable;

    public const int LEFT = 0;
    public const int RIGHT = 1;
    public const int TOP = 2;
    public const int BOTTOM = 3;
    public const int HEIGHT = 4;
    public const int WIDTH = 5;
    public const int START = 0;
    public const int END = 1;
    public const int BEFORE = 2;
    public const int AFTER = 3;
    public const int BLOCKPROGDIM = 4;
    public const int INLINEPROGDIM = 5;

    private static readonly string[] AbsoluteNames = ["left", "right", "top", "bottom", "height", "width"];
    private static readonly string[] RelativeNames = ["start", "end", "before", "after", "block-progression-dimension", "inline-progression-dimension"];

    private static readonly Hashtable wmtables = new(4);

    private PropertyListBuilder? _builder;

    private readonly PropertyList? _parentPropertyList = parentPropertyList;

    static PropertyList()
    {
        wmtables.Add(WritingMode.LR_TB, new byte[] { START, END, BEFORE, AFTER, BLOCKPROGDIM, INLINEPROGDIM }); /* lr-tb */
        wmtables.Add(WritingMode.RL_TB, new byte[] { END, START, BEFORE, AFTER, BLOCKPROGDIM, INLINEPROGDIM }); /* rl-tb */
        wmtables.Add(WritingMode.TB_RL, new byte[] { AFTER, BEFORE, START, END, INLINEPROGDIM, BLOCKPROGDIM });/* tb-rl */
    }

    public FObj? FObj { get; set; }

    public FObj? GetParentFObj()
        => _parentPropertyList?.FObj;

    public Property? GetExplicitOrShorthandProperty(string propertyName)
    {
        int sepchar = propertyName.IndexOf('.');
        string baseName;
        if (sepchar > -1)
        {
            baseName = propertyName[..sepchar];
        }
        else
        {
            baseName = propertyName;
        }

        Property? property = GetExplicitBaseProperty(baseName);
        property ??= _builder?.GetShorthand(this, baseName);

        if (property != null && sepchar > -1)
        {
            return _builder?.GetSubpropValue(baseName, property, propertyName.Substring(sepchar + 1));
        }

        return property;
    }

    public Property? GetExplicitProperty(string propertyName)
    {
        int sepchar = propertyName.IndexOf('.');
        if (sepchar > -1)
        {
            string baseName = propertyName.Substring(0, sepchar);
            Property? p = GetExplicitBaseProperty(baseName);
            if (p != null)
            {
                return _builder?.GetSubpropValue(baseName, p, propertyName.Substring(sepchar + 1));
            }
            else
            {
                return null;
            }
        }
        return (Property?)this[propertyName];
    }

    public Property? GetExplicitBaseProperty(string propertyName)
        => (Property?)this[propertyName];

    public Property? GetInheritedProperty(string propertyName)
    {
        if (_builder == null)
            return null;

        if (_parentPropertyList != null && IsInherited(propertyName))
        {
            return _parentPropertyList.GetProperty(propertyName);
        }
        else
        {
            try
            {
                return _builder.MakeProperty(this, propertyName);
            }
            catch (FonetException e)
            {
                FonetDriver.ActiveDriver?.FireFonetError($"Exception in GetInheritedProperty(): property={propertyName} : {e}");
            }
        }
        return null;
    }

    private bool IsInherited(string propertyName)
    {
        PropertyMaker? propertyMaker = _builder?.FindMaker(propertyName);
        if (propertyMaker != null)
        {
            return propertyMaker.IsInherited();
        }
        else
        {
            FonetDriver.ActiveDriver?.FireFonetError($"Unknown property: {propertyName}");
            return false;
        }
    }

    private Property? FindProperty(string propertyName, bool tryOnInherits)
    {
        PropertyMaker? maker = _builder?.FindMaker(propertyName);

        Property? p = null;
        if (maker != null && maker.IsCorrespondingForced(this))
        {
            p = ComputeProperty(this, maker);

        }
        else
        {
            p = GetExplicitBaseProperty(propertyName);

            p ??= ComputeProperty(this, maker);
            p ??= maker?.GetShorthand(this);

            if (p == null && tryOnInherits)
            {
                if (_parentPropertyList != null && maker?.IsInherited() == true)
                {
                    p = _parentPropertyList.FindProperty(propertyName, true);
                }
            }
        }

        return p;
    }

    private static Property? ComputeProperty(PropertyList propertyList, PropertyMaker? propertyMaker)
    {
        Property? p = null;
        try
        {
            p = propertyMaker?.Compute(propertyList);
        }
        catch (FonetException e)
        {
            FonetDriver.ActiveDriver?.FireFonetError(e.Message);
        }
        return p;
    }

    public Property? GetSpecifiedProperty(string propertyName)
        => GetProperty(propertyName, false, false);

    public Property? GetProperty(string propertyName)
        => GetProperty(propertyName, true, true);

    private Property? GetProperty(string propertyName, bool tryInherit, bool tryDefault)
    {
        if (_builder == null)
        {
            FonetDriver.ActiveDriver?.FireFonetError("builder not set in PropertyList");
            return null;
        }

        int sepchar = propertyName.IndexOf('.');
        string? subpropName = null;
        if (sepchar > -1)
        {
            subpropName = propertyName[(sepchar + 1)..];
            propertyName = propertyName[..sepchar];
        }

        Property? property = FindProperty(propertyName, tryInherit);
        if (property == null && tryDefault)
        {
            try
            {
                property = _builder.MakeProperty(this, propertyName);
            }
            catch (FonetException e)
            {
                FonetDriver.ActiveDriver?.FireFonetError(e.ToString());
            }
        }

        if (subpropName != null && property != null)
        {
            return this._builder.GetSubpropValue(propertyName, property, subpropName);
        }
        else
        {
            return property;
        }
    }

    public void SetBuilder(PropertyListBuilder builder)
        => _builder = builder;

    public string GetNameSpace()
        => nameSpace;

    public string GetElement()
        => element;

    public Property? GetNearestSpecifiedProperty(string propertyName)
    {
        Property? property = null;
        for (PropertyList? plist = this; property == null && plist != null; plist = plist._parentPropertyList)
        {
            property = plist.GetExplicitProperty(propertyName);
        }

        if (property == null)
        {
            try
            {
                property = _builder?.MakeProperty(this, propertyName);
            }
            catch (FonetException e)
            {
                FonetDriver.ActiveDriver?.FireFonetError($"Exception in getNearestSpecified(): property={propertyName} : {e}");
            }
        }
        return property;
    }

    public Property? GetFromParentProperty(string propertyName)
    {
        if (_parentPropertyList != null)
        {
            return _parentPropertyList.GetProperty(propertyName);
        }

        if (_builder != null)
        {
            try
            {
                return _builder?.MakeProperty(this, propertyName);
            }
            catch (FonetException e)
            {
                FonetDriver.ActiveDriver?.FireFonetError($"Exception in getFromParent(): property={propertyName} : {e}");
            }
        }

        return null;
    }

    public string AbsoluteToRelative(int absdir)
    {
        if (wmtable != null)
        {
            return RelativeNames[wmtable[absdir]];
        }
        else
        {
            return string.Empty;
        }
    }

    public string RelativeToAbsolute(int relativeDirection)
    {
        if (wmtable != null)
        {
            for (int i = 0; i < wmtable.Length; i++)
            {
                if (wmtable[i] == relativeDirection)
                {
                    return AbsoluteNames[i];
                }
            }
        }

        return string.Empty;
    }

    public void SetWritingMode(int writingMode)
        => wmtable = (byte[]?)wmtables[writingMode];
}