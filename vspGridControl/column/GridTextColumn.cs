using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class GridTextColumn : GridColumn
    {
        protected bool m_bVertical;
        protected StringFormat m_myStringFormat;
        protected TextFormatFlags m_textFormat;

        protected GridTextColumn()
        {
            this.m_myStringFormat = new StringFormat(StringFormatFlags.LineLimit | StringFormatFlags.NoWrap);
            this.m_textFormat = GridConstants.DefaultTextFormatFlags;
        }

        public GridTextColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
            : base(ci, nWidthInPixels, colIndex)
        {
            this.m_myStringFormat = new StringFormat(StringFormatFlags.LineLimit | StringFormatFlags.NoWrap);
            this.m_textFormat = GridConstants.DefaultTextFormatFlags;
            this.m_myStringFormat.HotkeyPrefix = HotkeyPrefix.None;
            this.m_myStringFormat.Trimming = StringTrimming.EllipsisCharacter;
            this.m_myStringFormat.LineAlignment = StringAlignment.Center;
            this.SetStringFormatRTL(GridColumn.s_defaultRTL);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.m_myStringFormat != null)
            {
                this.m_myStringFormat.Dispose();
                this.m_myStringFormat = null;
            }
        }

        public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            g.FillRectangle(bkBrush, rect);
            rect.Inflate(-GridColumn.CELL_CONTENT_OFFSET, 0);
            if (rect.Width > 0)
            {
                if (this.m_bVertical)
                {
                    this.DrawTextStringForVerticalFonts(g, textBrush, textFont, rect, storage, nRowIndex, false);
                }
                else
                {
                    TextRenderer.DrawText(g, storage.GetCellDataAsString(nRowIndex, base.m_myColumnIndex), textFont, rect, textBrush.Color, this.m_textFormat);
                }
            }
        }

        private void DrawTextStringForVerticalFonts(Graphics g, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex, bool useGdiPlus)
        {
            using (Matrix matrix = new Matrix(0f, -1f, 1f, 0f, (float)(rect.X - rect.Y), (float)((rect.X + rect.Y) + rect.Height)))
            {
                new Rectangle(rect.X, rect.Y, rect.Height, rect.Width);
                g.Transform = matrix;
                if (useGdiPlus)
                {
                    g.DrawString(storage.GetCellDataAsString(nRowIndex, base.m_myColumnIndex), textFont, textBrush, rect, this.m_myStringFormat);
                }
                else
                {
                    TextRenderer.DrawText(g, storage.GetCellDataAsString(nRowIndex, base.m_myColumnIndex), textFont, rect, textBrush.Color, this.m_textFormat);
                }
                g.ResetTransform();
            }
        }

        public override string GetAccessibleValue(long nRowIndex, IGridStorage storage)
        {
            return storage.GetCellDataAsString(nRowIndex, base.m_myColumnIndex);
        }

        public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
            rect.Inflate(-GridColumn.CELL_CONTENT_OFFSET, 0);
            if (rect.Width > 0)
            {
                if (this.m_bVertical)
                {
                    this.DrawTextStringForVerticalFonts(g, textBrush, textFont, rect, storage, nRowIndex, true);
                }
                else
                {
                    g.DrawString(storage.GetCellDataAsString(nRowIndex, base.m_myColumnIndex), textFont, textBrush, rect, this.m_myStringFormat);
                }
            }
        }

        public override void ProcessNewGridFont(Font gridFont)
        {
            if (gridFont.GdiVerticalFont)
            {
                this.m_myStringFormat.FormatFlags |= StringFormatFlags.DirectionVertical;
                this.m_bVertical = true;
            }
            else
            {
                this.m_myStringFormat.FormatFlags &= ~StringFormatFlags.DirectionVertical;
                this.m_bVertical = false;
            }
        }

        public override void SetRTL(bool bRightToLeft)
        {
            this.SetStringFormatRTL(bRightToLeft);
        }

        private void SetStringFormatRTL(bool bRTL)
        {
            if (bRTL)
            {
                this.m_myStringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                this.m_textFormat |= TextFormatFlags.RightToLeft;
            }
            else
            {
                this.m_myStringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
                this.m_textFormat &= ~TextFormatFlags.RightToLeft;
            }
            GridConstants.AdjustFormatFlagsForAlignment(ref this.m_textFormat, base.m_myAlign);
            if (base.m_myAlign == HorizontalAlignment.Left)
            {
                this.m_myStringFormat.Alignment = StringAlignment.Near;
            }
            else if (base.m_myAlign == HorizontalAlignment.Center)
            {
                this.m_myStringFormat.Alignment = StringAlignment.Center;
            }
            else
            {
                this.m_myStringFormat.Alignment = StringAlignment.Far;
            }
        }
    }
}

