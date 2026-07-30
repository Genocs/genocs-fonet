using System.Net;
using System.Xml;
using Genocs.Fonet.Fo;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet;

/// <summary>
/// FonetDriver provides the client with a single interface to invoking FO.NET.
/// </summary>
/// <remarks>
/// The examples belows demonstrate several ways of invoking FO.NET. The
/// methodology is the same regardless of how FO.NET is embedded in your
/// system (ASP.NET, WinForm, Web Service, etc).
/// </remarks>
/// <example>
/// <code lang="csharp">
/// // This example demonstrates rendering an XSL-FO file to a PDF file.
/// <code>
/// FonetDriver driver = FonetDriver.Make();
/// driver.Render(
/// new FileStream("readme.fo", FileMode.Open), 
/// new FileStream("readme.pdf", FileMode.Create));
/// </code>
/// <code lang="vb">
/// // This example demonstrates rendering an XSL-FO file to a PDF file.
/// Dim driver As FonetDriver = FonetDriver.Make
/// driver.Render( _
/// New FileStream("readme.fo", FileMode.Open), _
/// New FileStream("readme.pdf", FileMode.Create))
/// </code>
/// <code lang="csharp">
/// // This example demonstrates using an XmlDocument as the source of the 
/// // XSL-FO tree.  The XmlDocument could easily be dynamically generated.
/// XmlDocument doc = new XmlDocument()
/// doc.Load("reader.fo");
///
/// FonetDriver driver = FonetDriver.Make();
/// driver.Render(doc, new FileStream("readme.pdf", FileMode.Create));
/// </code>
/// <code lang="vb">
/// // This example demonstrates using an XmlDocument as the source of the 
/// // XSL-FO tree.  The XmlDocument could easily be dynamically generated.
/// Dim doc As XmlDocument = New XmlDocument()
/// doc.Load("reader.fo")
///
/// Dim driver As FonetDriver = FonetDriver.Make
/// driver.Render(doc, New FileStream("readme.pdf", FileMode.Create))
/// </code>
/// </example>
public class FonetDriver
{
    /// <summary>
    /// Gets or sets the base directory used to locate external
    /// resources such as images loaded
    /// from when using relative path references.
    /// </summary>
    /// <value>
    /// Defaults to the current working directory.
    /// </value>
    public DirectoryInfo? BaseDirectory { get; set; }

    /// <summary>
    /// Gets or sets the optional image handler that is responsible for loading the image
    /// data for external graphics.
    /// </summary>
    /// <remarks>
    /// If null is returned from the image handler, then FO.NET will perform
    /// normal processing.
    /// </remarks>
    public FonetImageHandler? ImageHandler { get; set; }

    /// <summary>
    /// Gets or sets the time in milliseconds timeout used when accessing external resources via a URL.
    /// </summary>
    /// <remarks>
    /// The default value is 100000 milliseconds.
    /// </remarks>
    /// <value>
    /// The timeout value in milliseconds.
    /// </value>
    public int Timeout { get; set; } = 100000;

    /// <summary>
    /// Determines if the output stream passed to Render() should
    /// be closed upon completion or if a fatal exception occurs.
    /// </summary>
    public bool CloseOnExit { get; set; }

    /// <summary>
    /// Options that are passed to the rendering engine.
    /// </summary>
    public PdfRendererOptions? Options { get; set; }

    /// <summary>
    /// Maps a set of credentials to an internet resource.
    /// </summary>
    private CredentialCache? _credentials;

    /// <summary>
    /// The delegate subscribers must implement to receive FO.NET events.
    /// </summary>
    /// <remarks>
    /// The <paramref name="driver"/> parameter will be a reference to
    /// the active FonetDriver.  The <paramref name="e"/> parameter will
    /// contain a human-readable error message.
    /// </remarks>
    /// <param name="driver">A reference to the active FonetDriver</param>
    /// <param name="e">Encapsulates a human readable error message</param>
    public delegate void FonetEventHandler(object driver, FonetEventArgs e);

    /// <summary>
    /// The delegate subscribers must implement to handle the loading
    /// of image data in response to external-graphic formatting objects.
    /// </summary>
    public delegate byte[]? FonetImageHandler(string src);

    /// <summary>
    /// A multicast delegate. The error event FO.NET publishes.
    /// </summary>
    /// <remarks>
    /// The method signature for this event handler should match
    /// the following:
    /// <pre class="code"><span class="lang">
    /// void FonetError(object driver, FonetEventArgs e);
    /// </span></pre>
    /// The first parameter <i>driver</i> will be a reference to the
    /// active FonetDriver instance.
    /// </remarks>
    /// <example>Subscribing to the 'error' event
    /// <pre class="code"><span class="lang">[C#]</span><br/>
    /// {
    /// FonetDriver driver = FonetDriver.Make();
    /// driver.OnError += new FonetDriver.FonetEventHandler(FonetError);
    /// ...
    /// }
    /// </pre>
    /// </example>
    public event FonetEventHandler? OnError;

    /// <summary>
    /// A multicast delegate.  The warning event FO.NET publishes.
    /// </summary>
    /// <remarks>
    /// The method signature for this event handler should match
    /// the following:
    /// <pre class="code"><span class="lang">
    /// void FonetWarning(object driver, FonetEventArgs e);
    /// </span></pre>
    /// The first parameter <i>driver</i> will be a reference to the
    /// active FonetDriver instance.
    /// </remarks>
    public event FonetEventHandler? OnWarning;

    /// <summary>
    /// A multicast delegate.  The info event FO.NET publishes.
    /// </summary>
    /// <remarks>
    /// The method signature for this event handler should match
    /// the following:
    /// <pre class="code"><span class="lang">
    /// void FonetInfo(object driver, FonetEventArgs e);
    /// </span></pre>
    /// The first parameter <i>driver</i> will be a reference to the
    /// active FonetDriver instance.
    /// </remarks>
    public event FonetEventHandler? OnInfo;

    /// <summary>
    /// Constructs a new FonetDriver and registers the newly created
    /// driver as the active driver.
    /// </summary>
    /// <returns>An instance of FonetDriver.</returns>
    public static FonetDriver Make()
    {
        return new FonetDriver();
    }

    /// <summary>
    /// Sets the 'baseDir' property in the Configuration class using
    /// the value returned by Directory.GetCurrentDirectory(). Sets the
    /// default timeout to 100 seconds.
    /// </summary>
    public FonetDriver()
    {
        BaseDirectory = new DirectoryInfo(Path.GetFullPath(Directory.GetCurrentDirectory()));
        Timeout = 100000;
        ActiveDriver = this;
    }

    /// <summary>
    /// Gets or sets the active <see cref="FonetDriver"/>.
    /// </summary>
    /// <value>
    /// An instance of <see cref="FonetDriver"/> created via the factory method
    /// <see cref="Make"/>.
    /// </value>
    public static FonetDriver? ActiveDriver { get; set; }

    /// <summary>
    /// Gets a reference to a <see cref="System.Net.CredentialCache"/> object
    /// that manages credentials for multiple Internet resources.
    /// <seealso cref="System.Net.CredentialCache"/>
    /// </summary>
    /// <remarks>
    /// The purpose of this property is to associate a set of credentials against
    /// an Internet resource.  These credentials are then used by FO.NET when
    /// fetching images from one of the listed resources.
    /// </remarks>
    /// <example>
    /// FonetDriver driver = FonetDriver.Make();
    ///
    /// NetworkCredential nc1 = new NetworkCredential("foo", "password");
    /// driver.Credentials.Add(new Uri("http://www.chive.com"), "Basic", nc1);
    ///
    /// NetworkCredential nc2 = new NetworkCredential("john", "password", "UK");
    /// driver.Credentials.Add(new Uri("http://www.xyz.com"), "Digest", nc2);
    /// </example>
    public CredentialCache Credentials
    {
        get
        {
            _credentials ??= [];
            return _credentials;
        }
    }

    /// <summary>
    /// Executes the conversion reading the source tree from the supplied
    /// XmlDocument, converting it to a format dictated by the renderer
    /// and writing it to the supplied output stream.
    /// </summary>
    /// <param name="doc">
    /// An in-memory representation of an XML document (DOM).
    /// </param>
    /// <param name="outputStream">
    /// Any subclass of the Stream class.
    /// </param>
    /// <remarks>
    /// Any exceptions that occur during the Render process are arranged
    /// into three categories: information, warning and error. You may
    /// intercept any or all of theses exceptional states by registering
    /// an event listener.  See <see cref="FonetDriver.OnError"/> for an
    /// example of registering an event listener.  If there are no
    /// registered listeners, the exceptions are dumped to standard out -
    /// except for the error event which is wrapped in a
    /// <see cref="SystemException"/>.
    /// </remarks>
    public virtual void Render(XmlDocument doc, Stream outputStream)
    {
        StringWriter sw = new();
        using (XmlTextWriter writer = new(sw))
        {
            doc.Save(writer);
            writer.Close();
        }

        Render(new StringReader(sw.ToString()), outputStream);
    }

    /// <summary>
    /// Executes the conversion reading the source tree from the input
    /// reader, converting it to a format dictated by the renderer and
    /// writing it to the supplied output stream.
    /// </summary>
    /// <param name="inputReader">A character orientated stream</param>
    /// <param name="outputStream">Any subclass of the Stream class</param>
    public virtual void Render(TextReader inputReader, Stream outputStream)
        => Render(CreateXmlTextReader(inputReader), outputStream);

    /// <summary>
    /// Executes the conversion reading the source tree from the file
    /// <i>inputFile</i>, converting it to a format dictated by the
    /// renderer and writing it to the file identified by <i>outputFile</i>
    /// </summary>
    /// <remarks>
    /// If the file <i>outputFile</i> does not exist, it will created
    /// otherwise it will be overwritten.  Creating a file may
    /// generate a variety of exceptions.  See <see cref="FileStream"/>
    /// for a complete list.<br/>
    /// </remarks>
    /// <param name="inputFile">Path to an XSL-FO file</param>
    /// <param name="outputFile">Path to a file</param>
    public virtual void Render(string inputFile, string outputFile)
        => Render(CreateXmlTextReader(inputFile), new FileStream(outputFile, FileMode.Create, FileAccess.Write));

    /// <summary>
    /// Executes the conversion reading the source tree from the file
    /// <i>inputFile</i>, converting it to a format dictated by the
    /// renderer and writing it to the supplied output stream.
    /// </summary>
    /// <param name="inputFile">Path to an XSL-FO file.</param>
    /// <param name="outputStream">Any subclass of the Stream class, e.g. FileStream.</param>
    public virtual void Render(string inputFile, Stream outputStream)
        => Render(CreateXmlTextReader(inputFile), outputStream);

    /// <summary>
    /// Executes the conversion reading the source tree from the input
    /// stream, converting it to a format dictated by the Render and
    /// writing it to the supplied output stream.
    /// </summary>
    /// <param name="inputStream">Any subclass of the Stream class, e.g. FileStream</param>
    /// <param name="outputStream">Any subclass of the Stream class, e.g. FileStream</param>
    public virtual void Render(Stream inputStream, Stream outputStream)
        => Render(CreateXmlTextReader(inputStream), outputStream);

    /// <summary>
    /// Executes the conversion reading the source tree from the input 
    /// reader, converting it to a format dictated by the Render and 
    /// writing it to the supplied output stream.
    /// </summary>
    /// <remarks>
    /// The evaluation copy of this class will output an evaluation
    /// banner to standard out
    /// </remarks>
    /// <param name="inputReader">
    /// Reader that provides fast, non-cached, forward-only access 
    /// to XML data
    /// </param>
    /// <param name="outputStream">
    /// Any subclass of the Stream class, e.g. FileStream
    /// </param>
    public void Render(XmlReader inputReader, Stream outputStream)
    {
        try
        {
            // Constructs an area tree renderer and supplies the renderer options
            PdfRenderer renderer = new(outputStream);

            if (Options != null)
            {
                renderer.Options = Options;
            }

            // Create the stream-renderer.
            StreamRenderer sr = new(renderer);

            // Create the tree builder and give it the stream renderer.
            FOTreeBuilder tb = new();
            tb.SetStreamRenderer(sr);

            // Setup the mapping between xsl:fo elements and our fo classes.
            StandardElementMapping sem = new();
            StandardElementMapping.AddToBuilder(tb);

            // Start processing the xml document.
            tb.Parse(inputReader);
        }
        finally
        {
            if (CloseOnExit)
            {
                // Flush and close the output stream
                outputStream.Flush();
                outputStream.Close();
            }
        }
    }

    /// <summary>
    /// Sends an 'error' event to all registered listeners.
    /// </summary>
    /// <remarks>
    /// If there are no listeners, a <see cref="SystemException"/> is
    /// thrown immediately halting execution
    /// </remarks>
    /// <param name="message">Any error message, which may be null</param>
    /// <exception cref="SystemException">
    /// If no listener is registered for this event, a SystemException
    /// will be thrown
    /// </exception>
    internal void FireFonetError(string message)
    {
        if (OnError != null)
        {
            OnError(this, new FonetEventArgs(message));
        }
        else
        {
            Console.WriteLine("[ERRO] {0}", message);

            // throw new SystemException(message);
        }
    }

    /// <summary>
    /// Sends a 'warning' event to all registered listeners
    /// </summary>
    /// <remarks>
    /// If there are no listeners, <i>message</i> is written out 
    /// to the console instead
    /// </remarks>
    /// <param name="message">Any warning message, which may be null</param>
    internal void FireFonetWarning(string message)
    {
        if (OnWarning != null)
        {
            OnWarning(this, new FonetEventArgs(message));
        }
        else
        {
            Console.WriteLine("[WARN] {0}", message);
        }
    }

    /// <summary>
    /// Sends an 'info' event to all registered lisetners
    /// </summary>
    /// <remarks>
    /// If there are no listeners, <i>message</i> is written out 
    /// to the console instead
    /// </remarks>
    /// <param name="message">An info message, which may be null</param>
    internal void FireFonetInfo(string message)
    {
        if (OnInfo != null)
        {
            OnInfo(this, new FonetEventArgs(message));
        }
        else
        {
            Console.WriteLine("[INFO] {0}", message);
        }
    }

    /// <summary>
    /// Utility method that creates an <see cref="System.Xml.XmlTextReader"/>
    /// for the supplied file
    /// </summary>
    /// <remarks>
    /// The returned <see cref="System.Xml.XmlReader"/> interprets all whitespace
    /// </remarks>
    private static XmlTextReader CreateXmlTextReader(string inputFile)
        => new(inputFile);

    /// <summary>
    /// Utility method that creates an <see cref="System.Xml.XmlTextReader"/>
    /// for the supplied file
    /// </summary>
    /// <remarks>
    /// The returned <see cref="System.Xml.XmlReader"/> interprets all whitespace
    /// </remarks>
    private static XmlTextReader CreateXmlTextReader(Stream inputStream)
        => new(inputStream);

    /// <summary>
    /// Utility method that creates an <see cref="System.Xml.XmlTextReader"/>
    /// for the supplied file
    /// </summary>
    /// <remarks>
    /// The returned <see cref="System.Xml.XmlReader"/> interprets all whitespace
    /// </remarks>
    private static XmlTextReader CreateXmlTextReader(TextReader inputReader)
        => new(inputReader);
}