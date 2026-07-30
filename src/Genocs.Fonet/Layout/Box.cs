using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout;

/// <summary>
/// Represents a box in the layout engine. A box is a rectangular area that can contain other boxes or content.
/// It is used to represent the layout of elements in a document, such as paragraphs, images, and other content.
/// </summary>
/// <param name="parent">The parent area of this box. This is used to determine the position of the box relative to its parent area.</param>
internal abstract class Box(Area? parent)
{
    /// <summary>
    /// The parent area of this box. This is used to determine the position of the box relative to its parent area.
    /// </summary>
    public Area? Parent { get; protected set; } = parent;

    /// <summary>
    /// The area tree that this box belongs to. This is used to determine the position of the box relative to the entire document.
    /// </summary>
    internal AreaTree? AreaTree { get; set; }

    /// <summary>
    /// Renders the box using the specified PdfRenderer. This method is called by the layout engine to render the box and its contents.
    /// </summary>
    /// <param name="renderer">The PdfRenderer used to render the box.</param>
    public abstract void Render(PdfRenderer renderer);

    /// <summary>
    /// Forces the parent of this box to be set to the specified area. 
    /// This method should only be called when the parent is null, and it will throw an exception if the parent is already set. 
    /// It also ensures that the parent is not null, throwing an ArgumentNullException if it is.
    /// </summary>
    /// <param name="parent">The area to set as the parent of this box.</param>
    /// <exception cref="InvalidOperationException">Thrown if the parent is already set.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the specified parent is null.</exception>
    public void ForceParent(Area parent)
    {
        // Force set only in case the Parent is null.
        // This is used to set the parent of a box when it is added to an area, but the parent is not known at the time of creation.
        if (Parent != null) throw new InvalidOperationException("Parent is already set. Use ForceParent only when Parent is null.");
        // Ensure callers don't accidentally set a null parent which could lead to NREs later in layout.
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }
}