namespace Genocs.Fonet.Layout
{
    using Genocs.Fonet.Render.Pdf;

    internal class SpanArea : AreaContainer
    {
        private int columnCount;
        private int currentColumn = 1;
        private int columnGap = 0;
        private bool _isBalanced = false;

        public SpanArea(FontState fontState, int xPosition, int yPosition,
                        int allocationWidth, int maxHeight, int columnCount,
                        int columnGap) :
            base(fontState, xPosition, yPosition, allocationWidth, maxHeight, Fo.Properties.Position.ABSOLUTE, null)
        {
            this.contentRectangleWidth = allocationWidth;
            this.columnCount = columnCount;
            this.columnGap = columnGap;

            int columnWidth = (allocationWidth - columnGap * (columnCount - 1))
                / columnCount;
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                int colXPosition = (xPosition
                    + columnIndex * (columnWidth + columnGap));
                int colYPosition = yPosition;
                ColumnArea colArea = new ColumnArea(fontState, colXPosition,
                                                    colYPosition, columnWidth,
                                                    maxHeight, columnCount);
                AddChild(colArea);
                colArea.setColumnIndex(columnIndex + 1);
            }
        }

        public override void Render(PdfRenderer renderer)
        {
            renderer.RenderSpanArea(this);
        }

        public override void end()
        {
        }

        public override void start()
        {
        }

        public override int spaceLeft()
        {
            return _maxHeight - _currentHeight;
        }

        public int getColumnCount()
        {
            return columnCount;
        }

        public int getCurrentColumn()
        {
            return currentColumn;
        }

        public void setCurrentColumn(int currentColumn)
        {
            if (currentColumn <= columnCount)
            {
                this.currentColumn = currentColumn;
            }
            else
            {
                this.currentColumn = columnCount;
            }
        }

        public AreaContainer getCurrentColumnArea()
        {
            return (AreaContainer)Children[currentColumn - 1];
        }

        public bool isBalanced()
        {
            return _isBalanced;
        }

        public void setIsBalanced()
        {
            _isBalanced = true;
        }

        public int getTotalContentHeight()
        {
            int totalContentHeight = 0;
            foreach (AreaContainer ac in Children)
            {
                totalContentHeight += ac.getContentHeight();
            }
            return totalContentHeight;
        }

        public int getMaxContentHeight()
        {
            int maxContentHeight = 0;
            foreach (AreaContainer nextElm in Children)
            {
                if (nextElm.getContentHeight() > maxContentHeight)
                {
                    maxContentHeight = nextElm.getContentHeight();
                }
            }
            return maxContentHeight;
        }
        public override Page? Page
        {
            get
            {
                return base.Page;
            }

            set
            {
                base.Page = value;
                foreach (AreaContainer ac in Children)
                {
                    ac.Page = value;
                }
            }
        }

        public bool isLastColumn()
        {
            return (currentColumn == columnCount);
        }
    }
}