using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class GridBitmapColumn : GridColumn
    {
        protected bool m_isRTL;

        protected GridBitmapColumn()
        {
            this.m_isRTL = GridColumn.s_defaultRTL;
        }

        public GridBitmapColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex) : base(ci, nWidthInPixels, colIndex)
        {
            this.m_isRTL = GridColumn.s_defaultRTL;
        }

        protected virtual void DrawBitmap(Graphics g, Brush bkBrush, Rectangle rect, Bitmap myBmp, bool bEnabled)
        {
            if (myBmp != null)
            {
                Rectangle rectangle = rect;
                if (myBmp.Width < rect.Width)
                {
                    if (base.m_myAlign == HorizontalAlignment.Center)
                    {
                        rectangle.X = rect.X + ((rect.Width - myBmp.Width) / 2);
                    }
                    else if (((base.m_myAlign == HorizontalAlignment.Left) && !this.m_isRTL) || ((base.m_myAlign == HorizontalAlignment.Right) && this.m_isRTL))
                    {
                        rectangle.X = rect.X;
                    }
                    else
                    {
                        rectangle.X = rect.Right - myBmp.Width;
                    }
                    rectangle.Width = myBmp.Width;
                }
                if (myBmp.Height < rect.Height)
                {
                    rectangle.Y = (rect.Y + ((rect.Height - myBmp.Height) / 2)) + 1;
                    rectangle.Height = myBmp.Height;
                }
                if (bEnabled)
                {
                    g.DrawImage(myBmp, rectangle);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(g, myBmp, rectangle.X, rectangle.Y, ((SolidBrush) bkBrush).Color);
                }
            }
        }

        public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            Bitmap cellDataAsBitmap = storage.GetCellDataAsBitmap(nRowIndex, base.m_myColumnIndex);
            g.FillRectangle(bkBrush, rect);
            this.DrawBitmap(g, bkBrush, rect, cellDataAsBitmap, true);
        }

        public override void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            Bitmap cellDataAsBitmap = storage.GetCellDataAsBitmap(nRowIndex, base.m_myColumnIndex);
            g.FillRectangle(GridColumn.s_DisabledCellBKBrush, rect);
            this.DrawBitmap(g, GridColumn.s_DisabledCellBKBrush, rect, cellDataAsBitmap, false);
        }

        public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            Bitmap cellDataAsBitmap = storage.GetCellDataAsBitmap(nRowIndex, base.m_myColumnIndex);
            g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
            this.DrawBitmap(g, bkBrush, rect, cellDataAsBitmap, true);
        }

        public override void SetRTL(bool bRightToLeft)
        {
            this.m_isRTL = bRightToLeft;
        }
    }
}

