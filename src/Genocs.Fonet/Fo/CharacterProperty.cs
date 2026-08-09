namespace Genocs.Fonet.Fo;

internal class CharacterProperty : Property
{
    internal class Maker : PropertyMaker
    {
        public Maker(string propName)
            : base(propName)
        {
        }

        public override Property Make(PropertyList propertyList, string value, FObj? fo)
        {
            char c = value[0];
            return new CharacterProperty(c);
        }

    }

    private readonly char _character;

    public CharacterProperty(char character)
    {
        _character = character;
    }

    public override object GetObject()
    {
        return _character;
    }

    public override char GetCharacter()
    {
        return this._character;
    }

    public override string GetString()
    {
        return _character.ToString();
    }
}