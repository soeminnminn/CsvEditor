using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class GridHyperlinkColumn : GridTextColumn
    {
        public GridHyperlinkColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex) : base(ci, nWidthInPixels, colIndex)
        {
        }

        protected virtual string GetCellStringToMeasure(long rowIndex, IGridStorage storage)
        {
            return string.Intern(storage.GetCellDataAsString(rowIndex, base.ColumnIndex));
        }

        public override bool IsPointOverTextInCell(Point pt, Rectangle cellRect, IGridStorage storage, long row, Graphics g, Font f)
        {
            string cellStringToMeasure = this.GetCellStringToMeasure(row, storage);
            if ((cellStringToMeasure == null) || (cellStringToMeasure.Length <= 0))
            {
                return false;
            }
            cellRect.Inflate(-GridColumn.CELL_CONTENT_OFFSET, 0);
            Size size = TextRenderer.MeasureText(g, cellStringToMeasure, f, cellRect.Size, base.m_textFormat);
            pt.Y -= cellRect.Top + ((cellRect.Height - size.Height) / 2);
            if (base.m_myAlign == HorizontalAlignment.Left)
            {
                pt.X -= cellRect.Left;
            }
            else if (base.m_myAlign == HorizontalAlignment.Center)
            {
                int num = cellRect.Left + ((cellRect.Width - size.Width) / 2);
                pt.X -= num;
            }
            else
            {
                int num2 = cellRect.Left + (cellRect.Width - size.Width);
                pt.X -= num2;
            }
            return ((((pt.X >= 0) && (pt.X <= size.Width)) && (pt.Y >= 0)) && (pt.Y <= size.Height));
        }
    }
}

