using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo;

/// <summary>
/// The FObj class is the base class for all formatting objects (FOs) in the Fonet library. 
/// It provides common functionality and properties for all FOs, including property management, layout handling, and marker management. 
/// Each specific FO type (e.g., TableBody, MultiProperties, MultiCase, Unknown) inherits from this class and implements its own behavior as needed.
/// </summary>
internal class FObj : FONode
{
    internal class Maker
    {
        private readonly Func<FObj?, PropertyList, FObj>? _factory;

        public Maker()
        {
        }

        private Maker(Func<FObj?, PropertyList, FObj> factory)
        {
            _factory = factory;
        }

        public virtual FObj Make(FObj? parent, PropertyList propertyList)
        {
            if (_factory != null)
                return _factory(parent, propertyList);
            return new FObj(parent, propertyList);
        }

        public static Maker For(Func<FObj?, PropertyList, FObj> factory) => new(factory);
    }

    protected PropertyManager _propertyManager;
    private Hashtable? _markerClassNames;

    public string Name { get; init; }
    public PropertyList Properties { get; }

    public static Maker CreateMaker()
        => new();

    protected FObj(FObj? parent, PropertyList propertyList)
        : base(parent)
    {
        propertyList.FObj = this;
        Properties = propertyList;
        _propertyManager = MakePropertyManager(propertyList);
        Name = "default FO";
        SetWritingMode();
    }

    protected static PropertyManager MakePropertyManager(PropertyList propertyList)
        => new(propertyList);

    protected internal virtual void AddCharacters(char[] data, int start, int length)
    {
        // ignore
    }

    public override Status Layout(Area area)
    {
        return new Status(Status.OK);
    }

    protected internal virtual void Start()
    {
        // do nothing by default
    }

    protected internal virtual void End()
    {
        // do nothing by default
    }

    public override Property? GetProperty(string name)
        => (Properties.GetProperty(name));

    public virtual int GetContentWidth()
        => 0;

    public virtual void RemoveID(IDReferences idReferences)
    {
        if (((FObj)this).Properties.GetProperty("id") == null
            || ((FObj)this).Properties.GetProperty("id").GetString() == null)
        {
            return;
        }
        idReferences.RemoveID(((FObj)this).Properties.GetProperty("id").GetString());
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
        => false;

    protected virtual void SetWritingMode()
    {
        FObj p;
        FObj? parent;
        for (p = this; !p.GeneratesReferenceAreas() && (parent = p.Parent) != null; p = parent)
        {
            ;
        }

        Properties.SetWritingMode(p.GetProperty("writing-mode").GetEnum());
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
                    throw new FonetException($"A fo:marker must be an initial child of '{Name}'");
                }
            }
        }

        if (_markerClassNames == null)
        {
            _markerClassNames = new Hashtable
            {
                { markerClassName, string.Empty }
            };
        }
        else if (!_markerClassNames.ContainsKey(markerClassName))
        {
            _markerClassNames.Add(markerClassName, string.Empty);
        }
        else
        {
            throw new FonetException($"marker-class-name '{markerClassName}' already exists for this parent");
        }
    }
}