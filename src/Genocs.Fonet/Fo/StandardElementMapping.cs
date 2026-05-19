using Genocs.Fonet.Fo.Flow;
using Genocs.Fonet.Fo.Pagination;
using Genocs.Fonet.Fo.Properties;
using System.Collections;

namespace Genocs.Fonet.Fo;

internal class StandardElementMapping
{
    public const string URI = "http://www.w3.org/1999/XSL/Format";

    private static Hashtable foObjs;

    static StandardElementMapping()
    {
        foObjs = new Hashtable
        {
            // Declarations and Pagination and Layout Formatting Objects
            { "root", Root.GetMaker() },
            { "declarations", Declarations.GetMaker() },
            { "color-profile", ColorProfile.GetMaker() },
            { "page-sequence", PageSequence.GetMaker() },
            { "layout-master-set", LayoutMasterSet.GetMaker() },
            { "page-sequence-master", PageSequenceMaster.GetMaker() },
            { "single-page-master-reference", SinglePageMasterReference.GetMaker() },
            { "repeatable-page-master-reference", RepeatablePageMasterReference.GetMaker() },
            { "repeatable-page-master-alternatives", RepeatablePageMasterAlternatives.GetMaker() },
            { "conditional-page-master-reference", ConditionalPageMasterReference.GetMaker() },
            { "simple-page-master", SimplePageMaster.GetMaker() },
            { "region-body", RegionBody.GetMaker() },
            { "region-before", RegionBefore.GetMaker() },
            { "region-after", RegionAfter.GetMaker() },
            { "region-start", RegionStart.GetMaker() },
            { "region-end", RegionEnd.GetMaker() },
            { "flow", Flow.Flow.GetMaker() },
            { "static-content", StaticContent.GetMaker() },
            { "title", Title.GetMaker() },

            // Block-level Formatting Objects
            { "block", Block.GetMaker() },
            { "block-container", BlockContainer.GetMaker() },

            // Inline-level Formatting Objects
            { "bidi-override", BidiOverride.GetMaker() },
            { "character", Character.GetMaker() },
            { "initial-property-set", InitialPropertySet.GetMaker() },
            { "external-graphic", ExternalGraphic.GetMaker() },
            { "instream-foreign-object", InstreamForeignObject.GetMaker() },
            { "inline", Inline.GetMaker() },
            { "inline-container", InlineContainer.GetMaker() },
            { "leader", Leader.GetMaker() },
            { "page-number", PageNumber.GetMaker() },
            { "page-number-citation", PageNumberCitation.GetMaker() },

            // Formatting Objects for Tables
            { "table-and-caption", TableAndCaption.GetMaker() },
            { "table", Table.GetMaker() },
            { "table-column", TableColumn.GetMaker() },
            { "table-caption", TableCaption.GetMaker() },
            { "table-header", TableHeader.GetMaker() },
            { "table-footer", TableFooter.GetMaker() },
            { "table-body", TableBody.GetMaker() },
            { "table-row", TableRow.GetMaker() },
            { "table-cell", TableCell.GetMaker() },

            // Formatting Objects for Lists
            { "list-block", ListBlock.GetMaker() },
            { "list-item", ListItem.GetMaker() },
            { "list-item-body", ListItemBody.GetMaker() },
            { "list-item-label", ListItemLabel.GetMaker() },

            // Dynamic Effects: Link and Multi Formatting Objects
            { "basic-link", BasicLink.GetMaker() },
            { "multi-switch", MultiSwitch.GetMaker() },
            { "multi-case", MultiCase.GetMaker() },
            { "multi-toggle", MultiToggle.GetMaker() },
            { "multi-properties", MultiProperties.GetMaker() },
            { "multi-property-set", MultiPropertySet.GetMaker() },

            // Out-of-Line Formatting Objects
            { "float", Float.GetMaker() },
            { "footnote", Footnote.GetMaker() },
            { "footnote-body", FootnoteBody.GetMaker() },

            // Other Formatting Objects
            { "wrapper", Wrapper.GetMaker() },
            { "marker", Marker.GetMaker() },
            { "retrieve-marker", RetrieveMarker.GetMaker() }
        };
    }

    public static void AddToBuilder(FOTreeBuilder builder)
    {
        builder.AddElementMapping(URI, foObjs);
        builder.AddPropertyMapping(URI, FOPropertyMapping.getGenericMappings());
    }
}