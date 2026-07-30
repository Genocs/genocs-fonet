using System.Collections;
using Genocs.Fonet.Fo.Flow;
using Genocs.Fonet.Layout;
using Genocs.Fonet.Render.Pdf;

namespace Genocs.Fonet.Layout
{
    internal class BlockArea : Area
    {
        protected int startIndent;
        protected int endIndent;
        protected int textIndent;
        protected int lineHeight;
        protected int halfLeading;
        protected int align;
        protected int alignLastLine;
        protected LineArea currentLineArea;
        protected LinkSet currentLinkSet;
        protected bool hasLines = false;
        protected HyphenationProps hyphProps;
        protected ArrayList pendingFootnotes = null;

        public BlockArea(FontState fontState, int allocationWidth, 
            int maxHeight, int startIndent, 
            int endIndent, int textIndent, 
            int align, int alignLastLine, 
            int lineHeight, Area? parent)
            : base(fontState, allocationWidth, maxHeight, parent)
        {
            this.startIndent = startIndent;
            this.endIndent = endIndent;
            this.textIndent = textIndent;
            this.contentRectangleWidth = allocationWidth - startIndent
                - endIndent;
            this.align = align;
            this.alignLastLine = alignLastLine;
            this.lineHeight = lineHeight;

            if (fontState != null)
            {
                this.halfLeading = (lineHeight - fontState.FontSize) / 2;
            }
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderBlockArea(this);
        }

        protected void addLineArea(LineArea la)
        {
            if (!la.isEmpty())
            {
                la.verticalAlign();
                this.AddDisplaySpace(this.halfLeading);
                int size = la.GetHeight();
                this.AddChild(la);
                this.increaseHeight(size);
                this.AddDisplaySpace(this.halfLeading);
            }
            if (pendingFootnotes != null)
            {
                foreach (FootnoteBody fb in pendingFootnotes)
                {
                    Page? page = Page;
                    if (!Footnote.LayoutFootnote(page, fb, this))
                    {
                        page.addPendingFootnote(fb);
                    }
                }
                pendingFootnotes = null;
            }
        }

        public LineArea getCurrentLineArea()
        {
            if (currentHeight + lineHeight > maxHeight)
            {
                return null;
            }
            this.currentLineArea.changeHyphenation(hyphProps);
            this.hasLines = true;
            return this.currentLineArea;
        }

        public LineArea createNextLineArea()
        {
            if (this.hasLines)
            {
                this.currentLineArea.align(this.align);
                this.addLineArea(this.currentLineArea);
            }
            GetEffectiveIndents(out int effectiveStartIndent, out int effectiveEndIndent);
            this.currentLineArea = new LineArea(FontState, lineHeight,
                                                halfLeading, allocationWidth,
                                                effectiveStartIndent, effectiveEndIndent,
                                                currentLineArea);
            this.currentLineArea.changeHyphenation(hyphProps);
            if (currentHeight + lineHeight > maxHeight)
            {
                return null;
            }
            return this.currentLineArea;
        }

        public void setupLinkSet(LinkSet ls)
        {
            if (ls != null)
            {
                this.currentLinkSet = ls;
                ls.setYOffset(currentHeight);
            }
        }

        public override void end()
        {
            if (this.hasLines)
            {
                this.currentLineArea.addPending();
                this.currentLineArea.align(this.alignLastLine);
                this.addLineArea(this.currentLineArea);
            }
        }

        public override void start()
        {
            GetEffectiveIndents(out int effectiveStartIndent, out int effectiveEndIndent);
            currentLineArea = new LineArea(FontState, lineHeight, halfLeading,
                                           allocationWidth,
                                           effectiveStartIndent + textIndent, effectiveEndIndent,
                                           null);
        }

        private void GetEffectiveIndents(out int effectiveStartIndent, out int effectiveEndIndent)
        {
            effectiveStartIndent = startIndent;
            effectiveEndIndent = endIndent;

            ColumnArea? column = ColumnArea.FindColumnArea(this);
            if (column == null)
            {
                return;
            }

            column.GetFloatIndents(currentHeight, lineHeight, out int extraStart, out int extraEnd);
            effectiveStartIndent += extraStart;
            effectiveEndIndent += extraEnd;
        }

        public int getEndIndent()
        {
            return endIndent;
        }

        public int getStartIndent()
        {
            return startIndent;
        }

        public void setIndents(int startIndent, int endIndent)
        {
            this.startIndent = startIndent;
            this.endIndent = endIndent;
            this.contentRectangleWidth = allocationWidth - startIndent
                - endIndent;
        }

        public override int spaceLeft()
        {
            return maxHeight - currentHeight -
                (getPaddingTop() + getPaddingBottom()
                    + getBorderTopWidth() + getBorderBottomWidth());
        }

        public int getHalfLeading()
        {
            return halfLeading;
        }

        public void setHyphenation(HyphenationProps hyphProps)
        {
            this.hyphProps = hyphProps;
        }

        public void addFootnote(FootnoteBody fb)
        {
            if (pendingFootnotes == null)
            {
                pendingFootnotes = new ArrayList();
            }
            pendingFootnotes.Add(fb);
        }

    }
}