using Genocs.Fonet.Fo.Flow;
using Genocs.Fonet.Fo.Pagination;
using Genocs.Fonet.Fo.Properties;

namespace Genocs.Fonet.Fo;

internal class StandardElementMapping
{
    public const string URI = "http://www.w3.org/1999/XSL/Format";

    private static readonly Dictionary<string, FObj.Maker> foObjs;

    static StandardElementMapping()
    {
        foObjs = new Dictionary<string, FObj.Maker>
        {
            // Declarations and Pagination and Layout Formatting Objects
            { "root", Root.CreateMaker() },
            { "declarations", Declarations.CreateMaker() },
            { "color-profile", ColorProfile.CreateMaker() },
            { "page-sequence", PageSequence.CreateMaker() },
            { "layout-master-set", LayoutMasterSet.CreateMaker() },
            { "page-sequence-master", PageSequenceMaster.CreateMaker() },
            { "single-page-master-reference", SinglePageMasterReference.CreateMaker() },
            { "repeatable-page-master-reference", RepeatablePageMasterReference.CreateMaker() },
            { "repeatable-page-master-alternatives", RepeatablePageMasterAlternatives.CreateMaker() },
            { "conditional-page-master-reference", ConditionalPageMasterReference.CreateMaker() },
            { "simple-page-master", SimplePageMaster.CreateMaker() },
            { "region-body", RegionBody.CreateMaker() },
            { "region-before", RegionBefore.CreateMaker() },
            { "region-after", RegionAfter.CreateMaker() },
            { "region-start", RegionStart.CreateMaker() },
            { "region-end", RegionEnd.CreateMaker() },
            { "flow", Flow.Flow.CreateMaker() },
            { "static-content", StaticContent.CreateMaker() },
            { "title", Title.CreateMaker() },

            // Block-level Formatting Objects
            { "block", Block.CreateMaker() },
            { "block-container", BlockContainer.CreateMaker() },

            // Inline-level Formatting Objects
            { "bidi-override", BidiOverride.CreateMaker() },
            { "character", Character.CreateMaker() },
            { "initial-property-set", InitialPropertySet.CreateMaker() },
            { "external-graphic", ExternalGraphic.CreateMaker() },
            { "instream-foreign-object", InstreamForeignObject.CreateMaker() },
            { "inline", Inline.CreateMaker() },
            { "inline-container", InlineContainer.CreateMaker() },
            { "leader", Leader.CreateMaker() },
            { "page-number", PageNumber.CreateMaker() },
            { "page-number-citation", PageNumberCitation.CreateMaker() },

            // Formatting Objects for Tables
            { "table-and-caption", TableAndCaption.CreateMaker() },
            { "table", Table.CreateMaker() },
            { "table-column", TableColumn.CreateMaker() },
            { "table-caption", TableCaption.CreateMaker() },
            { "table-header", TableHeader.CreateMaker() },
            { "table-footer", TableFooter.CreateMaker() },
            { "table-body", TableBody.CreateMaker() },
            { "table-row", TableRow.CreateMaker() },
            { "table-cell", TableCell.CreateMaker() },

            // Formatting Objects for Lists
            { "list-block", ListBlock.CreateMaker() },
            { "list-item", ListItem.CreateMaker() },
            { "list-item-body", ListItemBody.CreateMaker() },
            { "list-item-label", ListItemLabel.CreateMaker() },

            // Dynamic Effects: Link and Multi Formatting Objects
            { "basic-link", BasicLink.CreateMaker() },
            { "multi-switch", MultiSwitch.CreateMaker() },
            { "multi-case", MultiCase.CreateMaker() },
            { "multi-toggle", MultiToggle.CreateMaker() },
            { "multi-properties", MultiProperties.CreateMaker() },
            { "multi-property-set", MultiPropertySet.CreateMaker() },

            // Out-of-Line Formatting Objects
            { "float", Float.CreateMaker() },
            { "footnote", Footnote.CreateMaker() },
            { "footnote-body", FootnoteBody.CreateMaker() },

            // Other Formatting Objects
            { "wrapper", Wrapper.CreateMaker() },
            { "marker", Marker.CreateMaker() },
            { "retrieve-marker", RetrieveMarker.CreateMaker() }
        };
    }

    public static void AddToBuilder(FOTreeBuilder builder)
    {
        builder.AddElementMapping(URI, foObjs);
        builder.AddPropertyMapping(URI, FOPropertyMapping.getGenericMappings());
    }
}