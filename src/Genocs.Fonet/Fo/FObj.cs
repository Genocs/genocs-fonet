using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo;

internal class FObj : FONode
{
    internal class Maker
    {
        public virtual FObj Make(FObj parent, PropertyList propertyList)
        {
            return new FObj(parent, propertyList);
        }
    }

    public static Maker GetMaker()
    {
        return new Maker();
    }

    public PropertyList _properties;

    protected PropertyManager _propertyManager;

    protected string _name;

    private Hashtable markerClassNames;

    protected FObj(FObj parent, PropertyList propertyList)
        : base(parent)
    {
        propertyList.FObj = this;
        _properties = propertyList;
        _propertyManager = MakePropertyManager(propertyList);
        _name = "default FO";
        SetWritingMode();
    }

    protected PropertyManager MakePropertyManager(PropertyList propertyList)
        => new(propertyList);

    protected internal virtual void AddCharacters(char[] data, int start, int length)
    {
        // ignore
    }

    public override Status Layout(Area area)
    {
        return new Status(Status.OK);
    }

    public string GetName()
    {
        return _name;
    }

    protected internal virtual void Start()
    {
        // do nothing by default
    }

    protected internal virtual void End()
    {
        // do nothing by default
    }

    public override Property GetProperty(string name)
    {
        return (_properties.GetProperty(name));
    }

    public virtual int GetContentWidth()
    {
        return 0;
    }

    public virtual void RemoveID(IDReferences idReferences)
    {
        if (((FObj)this)._properties.GetProperty("id") == null
            || ((FObj)this)._properties.GetProperty("id").GetString() == null)
        {
            return;
        }
        idReferences.RemoveID(((FObj)this)._properties.GetProperty("id").GetString());
        int numChildren = this._children.Count;
        for (int i = 0; i < numChildren; i++)
        {
            FONode child = (FONode)_children[i];
            if ((child is FObj))
            {
                ((FObj)child).RemoveID(idReferences);
            }
        }
    }

    public virtual bool GeneratesReferenceAreas()
    {
        return false;
    }

    protected virtual void SetWritingMode()
    {
        FObj p;
        FObj parent;
        for (p = this;
            !p.GeneratesReferenceAreas() && (parent = p.getParent()) != null;
            p = parent)
        {
            ;
        }
        _properties.SetWritingMode(p.GetProperty("writing-mode").GetEnum());
    }

    public void AddMarker(string markerClassName)
    {
        if (_children != null)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                FONode child = (FONode)_children[i];
                if (!child.MayPrecedeMarker())
                {
                    throw new FonetException($"A fo:marker must be an initial child of '{GetName()}'");
                }
            }
        }

        if (markerClassNames == null)
        {
            markerClassNames = new Hashtable
            {
                { markerClassName, String.Empty }
            };
        }

        else if (!markerClassNames.ContainsKey(markerClassName))
        {
            markerClassNames.Add(markerClassName, String.Empty);
        }
        else
        {
            throw new FonetException($"marker-class-name '{markerClassName}' already exists for this parent");
        }
    }
}