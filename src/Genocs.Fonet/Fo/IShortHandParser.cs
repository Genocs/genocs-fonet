namespace Genocs.Fonet.Fo;

internal interface IShortHandParser
{
    Property GetValueForProperty(string propName, PropertyMaker maker, PropertyList propertyList);
}