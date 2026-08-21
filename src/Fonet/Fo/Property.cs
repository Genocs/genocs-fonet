using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Fo.Expr;
using System.Collections;

namespace Genocs.Fonet.Fo;

/// <summary>
/// Represents a property in the formatting object model.
/// This abstract class serves as a base for various property types,
/// providing methods to retrieve specific property values such as length, color, space, and more.
/// Each derived class can override these methods to provide the appropriate behavior for its specific property type.
/// </summary>
internal abstract class Property
{
    public string? SpecifiedValue { get; set; }
    public virtual Length? GetLength() => null;
    public virtual ColorType? GetColorType() => null;
    public virtual CondLength? GetCondLength() => null;
    public virtual LengthRange? GetLengthRange() => null;
    public virtual LengthPair? GetLengthPair() => null;
    public virtual Space? GetSpace() => null;
    public virtual Keep? GetKeep() => null;
    public virtual int GetEnum() => 0;
    public virtual char GetCharacter() => (char)0;
    public virtual ArrayList? GetList() => null;
    public virtual Number? GetNumber() => null;
    public virtual Numeric? GetNumeric() => null;
    public virtual string? GetNCname() => null;
    public virtual object? GetObject() => null;

    public virtual string? GetString()
    {
        object? o = GetObject();
        return o?.ToString();
    }
}