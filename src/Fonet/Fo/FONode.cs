using Genocs.Fonet.Layout;
using System.Collections;

namespace Genocs.Fonet.Fo;

/// <summary>
/// This class represents a formatting object node in the formatting object tree. 
/// It serves as the base class for all formatting objects and provides common functionality
/// such as managing parent/child relationships, area classes, markers, and layout processing.
/// </summary>
internal abstract class FONode
{
    public FObj? Parent { get; }

    protected string AreaClass { get; set; } = Fonet.Layout.AreaClass.UNASSIGNED;

    protected ArrayList _children = [];

    public const int MarkerStart = -1000;

    public const int MarkerBreakAfter = -1001;

    protected int _marker = MarkerStart;

    protected bool _isInTableCell = false;

    protected int _forcedStartOffset = 0;

    protected int _forcedWidth = 0;

    protected int _widows = 0;

    protected int _orphans = 0;

    protected LinkSet? _linkSet;

    public int _areasGenerated = 0;

    protected FONode(FObj? parent)
    {
        Parent = parent;

        if (Parent != null)
        {
            AreaClass = Parent.AreaClass;
        }
    }

    public virtual void SetIsInTableCell()
    {
        _isInTableCell = true;
        foreach (FONode child in _children)
        {
            child.SetIsInTableCell();
        }
    }

    public virtual void ForceStartOffset(int offset)
    {
        _forcedStartOffset = offset;
        foreach (FONode child in _children)
        {
            child.ForceStartOffset(offset);
        }
    }

    public virtual void ForceWidth(int width)
    {
        _forcedWidth = width;
        foreach (FONode child in _children)
        {
            child.ForceWidth(width);
        }
    }

    public virtual void ResetMarker()
    {
        _marker = MarkerStart;
        foreach (FONode child in _children)
        {
            child.ResetMarker();
        }
    }

    public void SetWidows(int wid)
        => _widows = wid;

    public void SetOrphans(int orph)
        => _orphans = orph;

    public virtual void RemoveAreas()
    {
    }

    protected internal virtual void AddChild(FONode child)
        => _children.Add(child);

    //public FObj getParent()
    //    => Parent;

    public virtual void SetLinkSet(LinkSet linkSet)
    {
        _linkSet = linkSet;
        foreach (FONode child in _children)
        {
            child.SetLinkSet(linkSet);
        }
    }

    public virtual LinkSet? GetLinkSet()
        => _linkSet;

    public abstract Status Layout(Area area);

    public virtual Property? GetProperty(string name)
        => null;

    public virtual ArrayList GetMarkerSnapshot(ArrayList snapshot)
    {
        snapshot.Add(_marker);

        if (_marker < 0)
        {
            return snapshot;
        }
        else if (_children.Count == 0)
        {
            return snapshot;
        }
        else
        {
            return ((FONode)_children[_marker]).GetMarkerSnapshot(snapshot);
        }
    }

    public virtual void Rollback(ArrayList snapshot)
    {
        _marker = (int)snapshot[0];
        snapshot.RemoveAt(0);

        if (_marker == MarkerStart)
        {
            ResetMarker();
            return;
        }
        else if ((_marker == -1) || _children.Count == 0)
        {
            return;
        }

        if (_marker <= MarkerStart)
        {
            return;
        }

        int numChildren = _children.Count;
        for (int i = _marker + 1; i < numChildren; i++)
        {
            FONode fo = (FONode)_children[i];
            fo.ResetMarker();
        }

        ((FONode)_children[_marker]).Rollback(snapshot);
    }

    public virtual bool MayPrecedeMarker()
        => false;
}