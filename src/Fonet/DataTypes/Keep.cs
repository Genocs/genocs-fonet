using Genocs.Fonet.Fo;

namespace Genocs.Fonet.DataTypes;

internal class Keep : ICompoundDataType
{
    private Property? WithinLine { get; set; }

    private Property? WithinColumn { get; set; }

    private Property? WithinPage { get; set; }

    public void SetComponent(string componentName, Property componentValue, bool isDefault)
    {
        if (componentName.Equals("within-line"))
        {
            WithinLine = componentValue;
        }
        else if (componentName.Equals("within-column"))
        {
            WithinColumn = componentValue;
        }
        else if (componentName.Equals("within-page"))
        {
            WithinPage = componentValue;
        }
    }

    public Property? GetComponent(string componentName)
    {
        if (componentName.Equals("within-line"))
        {
            return WithinLine;
        }
        else if (componentName.Equals("within-column"))
        {
            return WithinColumn;
        }
        else if (componentName.Equals("within-page"))
        {
            return WithinPage;
        }
        else
        {
            return null;
        }
    }

    public override string ToString()
        => "Keep";
}