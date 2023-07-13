using System;
using System.Drawing;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public enum HitTestResult
    {
        Nothing,
        ColumnOnly,
        RowOnly,
        ColumnResize,
        HeaderButton,
        TextCell,
        ButtonCell,
        BitmapCell,
        HyperlinkCell,
        CustomCell
    }

    public sealed class HitTestInfo
    {
        private Rectangle areaRectangle;
        private int columnIndex;
        private HitTestResult result;
        private long rowIndex;

        private HitTestInfo()
        {
        }

        public HitTestInfo(HitTestResult result, long rowIndex, int columnIndex, Rectangle areaRectangle)
        {
            this.result = result;
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
            this.areaRectangle = areaRectangle;
        }

        public Rectangle AreaRectangle
        {
            get
            {
                return this.areaRectangle;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
            }
        }

        public HitTestResult HitTestResult
        {
            get
            {
                return this.result;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.rowIndex;
            }
        }
    }
}

