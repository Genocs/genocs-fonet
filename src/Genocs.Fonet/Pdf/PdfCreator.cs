using System.Collections;
using Genocs.Fonet.DataTypes;
using Genocs.Fonet.Image;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Pdf.Filter;
using Genocs.Fonet.Pdf.Security;
using Genocs.Fonet.Render.Pdf;
using Genocs.Fonet.Util;

namespace Genocs.Fonet.Pdf;

internal sealed class PdfCreator
{
    // list of objects to write in the trailer.
    private readonly ArrayList _trailerObjects = [];

    // the objects themselves
    // These objects are buffered and then written to the
    // PDF stream after each page has been rendered.  Adding
    // an object to this array does not mean it is ready for
    // writing, but it does mean that there is no need to 
    // wait until the end of the PDF stream.  The trigger
    // to write these objects out is pulled by PdfRenderer,
    // at the end of it's Render page method.
    private readonly ArrayList _objects = [];

    // The root outline object
    private PdfOutline _outlineRoot;

    // the /Resources object
    private PdfResources _resources;

    // the documents idReferences
    private IDReferences idReferences;

    // the XObjects Map.
    private Hashtable xObjectsMap = new Hashtable();

    // The cross-reference table.
    private XRefTable xrefTable;

    // The PDF information dictionary.
    private PdfInfo info;

    // The PDF encryption dictionary.
    private PdfDictionary encrypt;

    // Links wiating for internal document references
    //private ArrayList pendingLinks;

    public PdfCreator(Stream stream)
    {
        // Create the underlying PDF document.
        Doc = new PdfDocument(stream)
        {
            Version = PdfVersion.V13
        };

        _resources = new PdfResources(Doc.NextObjectId());
        AddTrailerObject(_resources);
        this.xrefTable = new XRefTable();
    }

    public void SetIdReferences(IDReferences idReferences)
    {
        this.idReferences = idReferences;
    }

    public PdfDocument Doc { get; }

    public void AddObject(PdfObject obj)
    {
        _objects.Add(obj);
    }

    public PdfXObject AddImage(FonetImage img)
    {
        // check if already created
        string url = img.Uri;
        PdfXObject xObject = (PdfXObject)this.xObjectsMap[url];
        if (xObject == null)
        {
            PdfICCStream iccStream = null;

            ColorSpace cs = img.ColorSpace;
            if (cs.HasICCProfile())
            {
                iccStream = new PdfICCStream(Doc.NextObjectId(), cs.GetICCProfile())
                {
                    NumComponents = new PdfNumeric(cs.GetNumComponents())
                };
                iccStream.AddFilter(new FlateFilter());
                this._objects.Add(iccStream);
            }

            // else, create a new one
            PdfName name = new PdfName("XO" + xObjectsMap.Count);
            xObject = new PdfXObject(img.Bitmaps, name, Doc.NextObjectId());
            xObject.SubType = PdfName.Names.Image;
            xObject.Dictionary[PdfName.Names.Width] = new PdfNumeric(img.Width);
            xObject.Dictionary[PdfName.Names.Height] = new PdfNumeric(img.Height);
            xObject.Dictionary[PdfName.Names.BitsPerComponent] = new PdfNumeric(img.BitsPerPixel);

            // Check for ICC color space
            if (iccStream != null)
            {
                PdfArray ar = new PdfArray();
                ar.Add(PdfName.Names.ICCBased);
                ar.Add(iccStream.GetReference());

                xObject.Dictionary[PdfName.Names.ColorSpace] = ar;
            }
            else
            {
                xObject.Dictionary[PdfName.Names.ColorSpace] = new PdfName(img.ColorSpace.GetColorSpacePDFString());
            }

            xObject.AddFilter(img.Filter);

            this._objects.Add(xObject);
            this.xObjectsMap.Add(url, xObject);
        }
        return xObject;
    }

    public PdfPage makePage(PdfResources resources, PdfContentStream contents, int pagewidth, int pageheight, Page currentPage)
    {
        PdfPage page = new PdfPage(
            resources, contents,
            pagewidth, pageheight,
            Doc.NextObjectId());

        if (currentPage != null)
        {
            foreach (string id in currentPage.getIDList())
            {
                idReferences.SetInternalGoToPageReference(id, page.GetReference());
            }
        }

        /* Add it to the list of objects */
        this._objects.Add(page);

        page.SetParent(Doc.Pages);
        Doc.Pages.Kids.Add(page.GetReference());

        return page;
    }

    public PdfLink makeLink(IntRectangle rect, string destination, int linkType)
    {
        PdfLink link = new PdfLink(Doc.NextObjectId(), rect);
        this._objects.Add(link);

        if (linkType == LinkSet.EXTERNAL)
        {
            if (destination.EndsWith(".pdf"))
            { // FileSpec
                PdfFileSpec fileSpec = new PdfFileSpec(Doc.NextObjectId(), destination);
                this._objects.Add(fileSpec);
                PdfGoToRemote gotoR = new PdfGoToRemote(fileSpec, Doc.NextObjectId());
                this._objects.Add(gotoR);
                link.SetAction(gotoR);
            }
            else
            { // URI
                PdfUri uri = new PdfUri(destination);
                link.SetAction(uri);
            }
        }
        else
        {
            PdfObjectReference goToReference = getGoToReference(destination);
            PdfInternalLink internalLink = new PdfInternalLink(goToReference);
            link.SetAction(internalLink);
        }
        return link;
    }

    private PdfObjectReference getGoToReference(string destination)
    {
        PdfGoTo goTo;
        // Have we seen this 'id' in the document yet?
        if (idReferences.DoesIDExist(destination))
        {
            if (idReferences.DoesGoToReferenceExist(destination))
            {
                goTo = idReferences.GetInternalLinkGoTo(destination);
            }
            else
            {
                goTo = idReferences.CreateInternalLinkGoTo(destination, Doc.NextObjectId());
                AddTrailerObject(goTo);
            }
        }
        else
        {
            // id was not found, so create it
            idReferences.CreateUnvalidatedID(destination);
            idReferences.AddToIdValidationList(destination);
            goTo = idReferences.CreateInternalLinkGoTo(destination, Doc.NextObjectId());
            AddTrailerObject(goTo);
        }
        return goTo.GetReference();
    }

    private void AddTrailerObject(PdfObject obj)
        => _trailerObjects.Add(obj);

    public PdfContentStream MakeContentStream()
    {
        PdfContentStream obj = new PdfContentStream(Doc.NextObjectId());
        obj.AddFilter(new FlateFilter());
        this._objects.Add(obj);
        return obj;
    }

    public PdfAnnotList MakeAnnotList()
    {
        PdfAnnotList obj = new PdfAnnotList(Doc.NextObjectId());
        this._objects.Add(obj);
        return obj;
    }

    public void SetOptions(PdfRendererOptions options)
    {
        // Configure the /Info dictionary.
        info = new PdfInfo(Doc.NextObjectId());
        if (options.Title != null)
        {
            info.Title = new PdfString(options.Title);
        }
        if (options.Author != null)
        {
            info.Author = new PdfString(options.Author);
        }
        if (options.Subject != null)
        {
            info.Subject = new PdfString(options.Subject);
        }
        if (options.Keywords != String.Empty)
        {
            info.Keywords = new PdfString(options.Keywords);
        }
        if (options.Creator != null)
        {
            info.Creator = new PdfString(options.Creator);
        }
        if (PdfRendererOptions.Producer != null)
        {
            info.Producer = new PdfString(PdfRendererOptions.Producer);
        }
        info.CreationDate = new PdfString(PdfDate.Format(DateTime.Now));
        this._objects.Add(info);

        // Configure the security options.
        if (options.UserPassword != null ||
            options.OwnerPassword != null ||
            options.HasPermissions)
        {
            SecurityOptions securityOptions = new SecurityOptions();
            securityOptions.UserPassword = options.UserPassword;
            securityOptions.OwnerPassword = options.OwnerPassword;
            securityOptions.EnableAdding(options.EnableAdd);
            securityOptions.EnableChanging(options.EnableModify);
            securityOptions.EnableCopying(options.EnableCopy);
            securityOptions.EnablePrinting(options.EnablePrinting);

            Doc.SecurityOptions = securityOptions;
            encrypt = Doc.Writer.SecurityManager.GetEncrypt(Doc.NextObjectId());
            this._objects.Add(encrypt);
        }
    }

    public PdfOutline getOutlineRoot()
    {
        if (_outlineRoot != null)
        {
            return _outlineRoot;
        }

        _outlineRoot = new PdfOutline(Doc.NextObjectId(), null, null);
        AddTrailerObject(_outlineRoot);
        Doc.Catalog.Outlines = _outlineRoot;
        return _outlineRoot;
    }

    public PdfOutline makeOutline(PdfOutline parent, string label, string destination)
    {
        PdfObjectReference goToRef = getGoToReference(destination);

        PdfOutline obj = new(Doc.NextObjectId(), label, goToRef);

        parent?.AddOutline(obj);

        this._objects.Add(obj);
        return obj;

    }

    public PdfResources getResources()
    {
        return this._resources;
    }

    private void WritePdfObject(PdfObject obj)
    {
        xrefTable.Add(obj.ObjectId, Doc.Writer.Position);
        Doc.Writer.WriteLine(obj);
    }

    public void output()
    {
        foreach (PdfObject obj in this._objects)
        {
            WritePdfObject(obj);
        }

        _objects.Clear();
    }

    public void outputHeader()
    {
        Doc.WriteHeader();
    }

    public void outputTrailer()
    {
        output();

        foreach (PdfXObject xobj in xObjectsMap.Values)
        {
            _resources.AddXObject(xobj);
        }

        xrefTable.Add(Doc.Catalog.ObjectId, Doc.Writer.Position);
        Doc.Writer.WriteLine(Doc.Catalog);

        xrefTable.Add(Doc.Pages.ObjectId, Doc.Writer.Position);
        Doc.Writer.WriteLine(Doc.Pages);

        foreach (PdfObject o in _trailerObjects)
        {
            WritePdfObject(o);
        }

        // output the xref table
        long xrefOffset = Doc.Writer.Position;
        xrefTable.Write(Doc.Writer);

        // output the file trailer
        PdfFileTrailer trailer = new PdfFileTrailer();
        trailer.Size = new PdfNumeric(Doc.ObjectCount + 1);
        trailer.Root = Doc.Catalog.GetReference();
        trailer.Id = Doc.FileIdentifier;
        if (info != null)
        {
            trailer.Info = info.GetReference();
        }
        if (info != null && encrypt != null)
        {
            trailer.Encrypt = encrypt.GetReference();
        }
        trailer.XRefOffset = xrefOffset;
        Doc.Writer.Write(trailer);
    }
}