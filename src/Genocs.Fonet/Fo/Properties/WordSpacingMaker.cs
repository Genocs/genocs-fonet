using System.Collections;

namespace Genocs.Fonet.Fo.Properties;

internal class WordSpacingMaker : LengthProperty.Maker
{
    public static PropertyMaker Maker(string propName) => new WordSpacingMaker(propName);

    protected WordSpacingMaker(string name) : base(name) { }

    public override bool IsInherited() => true;

    public override Property Make(PropertyList propertyList) =>
        Make(propertyList, "normal", propertyList.getParentFObj());

    private static Hashtable? s_htKeywords;

    private static void InitKeywords()
    {
        s_htKeywords = new Hashtable(1) { ["normal"] = "0pt" };
    }

    protected override string CheckValueKeywords(string keyword)
    {
        s_htKeywords ??= new Hashtable();
        if (s_htKeywords.Count == 0)
        {
            InitKeywords();
        }

        string? value = (string?)s_htKeywords[keyword];
        return value ?? base.CheckValueKeywords(keyword);
    }
}
