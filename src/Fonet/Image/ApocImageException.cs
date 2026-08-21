namespace Genocs.Fonet.Image;

internal class FonetImageException : Exception
{
    public FonetImageException()
    {
    }

    public FonetImageException(string message) : base(message)
    {
    }

    public FonetImageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}