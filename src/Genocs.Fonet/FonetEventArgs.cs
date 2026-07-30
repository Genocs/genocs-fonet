namespace Genocs.Fonet;

/// <summary>
/// A class containing event data for the Error, Warning and Info
/// events defined in <see cref="FonetDriver"/>.
/// </summary>
/// <remarks>
/// Initialises a new instance of the <i>FonetEventArgs</i> class.
/// </remarks>
/// <param name="message">The text of the event message.</param>
public class FonetEventArgs(string message) : EventArgs
{
    private string message = message;

    /// <summary>
    /// Retrieves the event message.
    /// </summary>
    /// <returns>A string which may be null.</returns>
    public string GetMessage()
    {
        return message;
    }

    /// <summary>
    /// Converts this <i>FonetEventArgs</i> to a string.
    /// </summary>
    /// <returns>
    /// A string representation of this class which is identical
    /// to <see cref="GetMessage"/>.
    /// </returns>
    public override string ToString()
    {
        return GetMessage();
    }
}