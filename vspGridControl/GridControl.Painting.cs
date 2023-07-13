using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using Accessibility;
using Microsoft.Win32;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public partial class GridControl
    {
        #region Variables
        #endregion

        #region Methods
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            if (((this.m_gridStorage == null) || (this.NumRowsInt == 0L)) || (this.NumColInt == 0))
            {
                this.PaintEmptyGrid(g);
            }
            else
            {
                this.PaintGrid(g);
            }
            base.OnPaint(pe);
        }

        private System.Drawing.Color ColorFromWin32(int nWin32ColorIndex)
        {
            int sysColor = SafeNativeMethods.GetSysColor(nWin32ColorIndex);
            return System.Drawing.Color.FromArgb(NativeMethods.Util.RGB_GETRED(sysColor), NativeMethods.Util.RGB_GETGREEN(sysColor), NativeMethods.Util.RGB_GETBLUE(sysColor));
        }

        private void DisposeCachedGDIObjects()
        {
            if (this.m_gridLinesPen != null)
            {
                this.m_gridLinesPen.Dispose();
                this.m_gridLinesPen = null;
            }
            
            if (this.m_highlightBrush != null)
            {
                this.m_highlightBrush.Dispose();
                this.m_highlightBrush = null;
            }
            
            if (this.highlightNonFocusedBrush != null)
            {
                this.highlightNonFocusedBrush.Dispose();
                this.highlightNonFocusedBrush = null;
            }
            
            if (this.m_colInsertionPen != null)
            {
                this.m_colInsertionPen.Dispose();
                this.m_colInsertionPen = null;
            }
        }

        protected virtual void DoCellPainting(Graphics g, SolidBrush bkBrush, SolidBrush textBrush, Font textFont, Rectangle cellRect, GridColumn gridColumn, long rowNumber, bool enabledState)
        {
            if (enabledState)
            {
                gridColumn.DrawCell(g, bkBrush, textBrush, textFont, cellRect, this.m_gridStorage, rowNumber);
            }
            else
            {
                gridColumn.DrawDisabledCell(g, textFont, cellRect, this.m_gridStorage, rowNumber);
            }
        }

        private void DrawOneButtonCell(long nRowIndex, int nColumnIndex, bool bPushed)
        {
            ButtonCellState state;
            GridButtonColumn column = (GridButtonColumn)this.m_Columns[nColumnIndex];
            Bitmap image = null;
            
            string buttonLabel = null;
            this.m_gridStorage.GetCellDataForButton(nRowIndex, this.m_Columns[nColumnIndex].ColumnIndex, out state, out image, out buttonLabel);
            
            SolidBrush bkBrush = null;
            SolidBrush textBrush = null;
            this.GetCellGDIObjects(this.m_Columns[nColumnIndex], nRowIndex, nColumnIndex, ref bkBrush, ref textBrush);
            
            using (Graphics graphics = this.GraphicsFromHandle())
            {
                if (this.HasNonScrollableColumns && (nColumnIndex >= this.m_scrollMgr.FirstScrollableColumnIndex))
                {
                    graphics.SetClip(new Rectangle(this.m_scrollableArea.X, this.HeaderHeight, this.m_scrollableArea.Width, (base.ClientRectangle.Height - this.HeaderHeight) + 1));
                }
                column.DrawButton(graphics, bkBrush, textBrush, this.GetCellFont(nRowIndex, this.m_Columns[nColumnIndex]), this.m_captureTracker.CellRect, image, buttonLabel, bPushed ? ButtonState.Pushed : ButtonState.Normal, this.ActAsEnabled);
            }
        }

        protected virtual void PaintCurrentCellRect(Graphics g, Rectangle r)
        {
            r.X--;
            r.Width++;
            ControlPaint.DrawFocusRectangle(g, r);
        }

        protected void PaintEmptyGrid(Graphics g)
        {
            this.PaintGridBackground(g);
            if (this.m_withHeader)
            {
                if (this.NumColInt <= 0)
                {
                    this.PaintEmptyHeader(g);
                }
                else
                {
                    this.PaintHeader(g);
                }
            }
        }

        private void PaintEmptyHeader(Graphics g)
        {
            Rectangle rectangle = new Rectangle(0, 0, base.ClientRectangle.Width, this.HeaderHeight);
            ControlPaint.DrawButton(g, rectangle, ButtonState.Normal);
        }

        protected void PaintGrid(Graphics g)
        {
            this.PaintGridBackground(g);

            try
            {
                if (((this.m_gridStorage != null) && (this.NumColInt != 0)) && (0L != this.NumRowsInt))
                {
                    int firstColumnIndex = this.m_scrollMgr.FirstColumnIndex;
                    int lastColumnIndex = this.m_scrollMgr.LastColumnIndex;
                    long firstRowIndex = this.m_scrollMgr.FirstRowIndex;
                    long lastRowIndex = this.m_scrollMgr.LastRowIndex;
                    int cellHeight = this.m_scrollMgr.CellHeight;
                    
                    Rectangle empty = Rectangle.Empty;
                    this.m_gridStorage.EnsureRowsInBuf(firstRowIndex, lastRowIndex);
                    
                    Rectangle rCell = new Rectangle();
                    int num6 = ScrollManager.GRID_LINE_WIDTH;
                    int column = -1;
                    long row = -1L;
                    
                    Rectangle rEditingCellRect = Rectangle.Empty;
                    
                    if (this.IsEditing)
                    {
                        IGridEmbeddedControl curEmbeddedControl = (IGridEmbeddedControl)this.m_curEmbeddedControl;
                        column = this.GetUIColumnIndexByStorageIndex(curEmbeddedControl.ColumnIndex);
                        row = curEmbeddedControl.RowIndex;
                        
                        if (!this.IsCellVisible(column, row) && this.m_curEmbeddedControl.Visible)
                        {
                            base.Focus();
                            this.m_curEmbeddedControl.Visible = false;
                        }
                    }
                    
                    bool hasNonScrollableColumns = this.HasNonScrollableColumns;
                    int nCol = -1;
                    rCell.Y = this.HeaderHeight;
                    rCell.Height = cellHeight + num6;
                    
                    for (long i = 0L; i < this.m_scrollMgr.FirstScrollableRowIndex; i += 1L)
                    {
                        rCell.X = this.m_scrollMgr.FirstColumnPos;
                        nCol = firstColumnIndex;
                        while (nCol <= lastColumnIndex)
                        {
                            this.PaintOneCell(g, nCol, i, column, row, ref rCell, ref empty, ref rEditingCellRect);
                            nCol++;
                        }
                        
                        rCell.X = ScrollManager.GRID_LINE_WIDTH;
                        
                        for (nCol = 0; nCol < this.m_scrollMgr.FirstScrollableColumnIndex; nCol++)
                        {
                            this.PaintOneCell(g, nCol, i, column, row, ref rCell, ref empty, ref rEditingCellRect);
                        }
                        rCell.Offset(0, cellHeight + num6);
                    }
                    
                    int num11 = ScrollManager.GRID_LINE_WIDTH;
                    nCol = 0;
                    
                    while (nCol < this.m_scrollMgr.FirstScrollableColumnIndex)
                    {
                        rCell.Y = this.m_scrollMgr.FirstRowPos;
                        for (long k = firstRowIndex; k <= lastRowIndex; k += 1L)
                        {
                            rCell.X = num11;
                            this.PaintOneCell(g, nCol, k, column, row, ref rCell, ref empty, ref rEditingCellRect);
                            rCell.Y += rCell.Height;
                        }
                        num11 += rCell.Width;
                        nCol++;
                    }
                    
                    Region clip = g.Clip;
                    g.IntersectClip(this.m_scrollableArea);
                    rCell.Y = this.m_scrollMgr.FirstRowPos;
                    rCell.Height = cellHeight + num6;
                    
                    for (long j = firstRowIndex; j <= lastRowIndex; j += 1L)
                    {
                        rCell.X = this.m_scrollMgr.FirstColumnPos;
                        for (nCol = firstColumnIndex; nCol <= lastColumnIndex; nCol++)
                        {
                            this.PaintOneCell(g, nCol, j, column, row, ref rCell, ref empty, ref rEditingCellRect);
                        }
                        rCell.Offset(0, cellHeight + num6);
                    }
                    
                    g.Clip = clip;
                    
                    if (this.m_withHeader)
                    {
                        this.PaintHeader(g);
                    }
                    
                    if (this.m_lineType != GridLineType.None)
                    {
                        this.PaintHorizGridLines(g, ((lastRowIndex - firstRowIndex) + this.FirstScrollableRow) + 1L, this.HeaderHeight, 0, rCell.X - (2 * num6), true);
                        if (this.m_scrollMgr.FirstScrollableRowIndex > 0)
                        {
                            this.PaintVertGridLines(g, firstColumnIndex, lastColumnIndex, this.m_scrollMgr.FirstColumnPos - ScrollManager.GRID_LINE_WIDTH, this.HeaderHeight, this.HeaderHeight + (this.m_scrollMgr.FirstScrollableColumnIndex * (cellHeight + num6)));
                        }
                        if (hasNonScrollableColumns)
                        {
                            this.PaintVertGridLines(g, 0, this.m_scrollMgr.FirstScrollableColumnIndex - 1, 0, this.HeaderHeight, rCell.Y - num6);
                            g.IntersectClip(new Rectangle(this.m_scrollableArea.X, this.HeaderHeight, this.m_scrollableArea.Width, (base.ClientRectangle.Height - this.HeaderHeight) + 1));
                        }
                        this.PaintVertGridLines(g, firstColumnIndex, lastColumnIndex, this.m_scrollMgr.FirstColumnPos - ScrollManager.GRID_LINE_WIDTH, this.HeaderHeight, rCell.Y - num6);
                    }
                    else if (hasNonScrollableColumns)
                    {
                        g.IntersectClip(new Rectangle(this.m_scrollableArea.X, this.HeaderHeight, this.m_scrollableArea.Width, (base.ClientRectangle.Height - this.HeaderHeight) + 1));
                    }
                    
                    if (!empty.IsEmpty)
                    {
                        empty.Height += num6;
                        if ((this.m_selMgr.CurrentColumn >= this.m_scrollMgr.FirstScrollableColumnIndex) && (this.m_alwaysHighlightSelection || base.ContainsFocus))
                        {
                            this.PaintCurrentCellRect(g, empty);
                        }
                    }
                    
                    if (hasNonScrollableColumns)
                    {
                        g.Clip = clip;
                    }
                    
                    if ((!empty.IsEmpty && (this.m_alwaysHighlightSelection || base.ContainsFocus)) && (this.m_selMgr.CurrentColumn < this.m_scrollMgr.FirstScrollableColumnIndex))
                    {
                        this.PaintCurrentCellRect(g, empty);
                    }
                    
                    if (this.IsEditing && !rEditingCellRect.IsEmpty)
                    {
                        this.PositionEmbeddedEditor(rEditingCellRect, column);
                    }
                    this.m_scrollMgr.RowsNumber = this.m_gridStorage.NumRows();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected virtual void PaintGridBackground(Graphics g)
        {
            if (this.ActAsEnabled)
            {
                g.FillRectangle(this.m_backBrush, base.ClientRectangle);
            }
            else
            {
                g.FillRectangle(GridColumn.DisabledBackgroundBrush, base.ClientRectangle);
            }
        }

        protected virtual void PaintHeader(Graphics g)
        {
            if (this.HasNonScrollableColumns)
            {
                this.PaintHeaderHelper(g, 0, this.m_scrollMgr.FirstScrollableColumnIndex - 1, 0, 0);
            }
            
            Region clip = g.Clip;
            try
            {
                Rectangle rect = new Rectangle(this.m_scrollableArea.X, 0, this.m_scrollableArea.Width, this.m_scrollableArea.Y);
                g.SetClip(rect);
                this.PaintHeaderHelper(g, this.m_scrollMgr.FirstColumnIndex, this.m_scrollMgr.LastColumnIndex, this.m_scrollMgr.FirstColumnPos, 0);
            }
            finally
            {
                g.Clip = clip;
            }
        }

        protected void PaintHeaderHelper(Graphics g, int nFirstCol, int nLastCol, int nFirstColPos, int nY)
        {
            this.PaintHeaderHelper(g, nFirstCol, nLastCol, nFirstColPos, nY, false);
        }

        protected void PaintHeaderHelper(Graphics g, int nFirstCol, int nLastCol, int nFirstColPos, int nY, bool useGdiPlus)
        {
            GridButton headerGridButton = this.m_gridHeader.HeaderGridButton;
            
            if ((nFirstCol > 0) && this.m_gridHeader[nFirstCol - 1].MergedWithRight)
            {
                for (int j = nFirstCol - 1; j >= 0; j--)
                {
                    if (!this.m_gridHeader[j].MergedWithRight)
                    {
                        break;
                    }
                    nFirstColPos -= this.m_Columns[j].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
                    nFirstCol--;
                }
            }
            
            if (this.m_gridHeader[nLastCol].MergedWithRight)
            {
                nLastCol++;
                while (nLastCol < this.NumColInt)
                {
                    if (!this.m_gridHeader[nLastCol].MergedWithRight)
                    {
                        break;
                    }
                    nLastCol++;
                }
                nLastCol = Math.Min(nLastCol, this.NumColInt - 1);
            }
            
            Rectangle rect = new Rectangle(nFirstColPos, nY, 0, this.HeaderHeight);
            GridHeader.HeaderItem item = null;
            ButtonState normal = ButtonState.Normal;
            int num2 = this.NumColInt - 1;

            for (int i = nFirstCol; i <= nLastCol; i++)
            {
                rect.Width += this.m_Columns[i].WidthInPixels;
                if (!this.m_gridHeader[i].MergedWithRight || (i == num2))
                {
                    item = this.m_gridHeader[i];
                    if (g.IsVisible(rect))
                    {
                        if (item.Pushed)
                        {
                            normal = ButtonState.Pushed;
                        }
                        else
                        {
                            normal = ButtonState.Normal;
                        }
                        if ((i == (this.m_scrollMgr.FirstScrollableColumnIndex - 1)) || (i == (this.NumColInt - 1)))
                        {
                            rect.Width++;
                        }
                        if (((this.m_scrollMgr.FirstScrollableColumnIndex > 0) && (i == nFirstCol)) && (i == this.m_scrollMgr.FirstScrollableColumnIndex))
                        {
                            rect.X--;
                            rect.Width++;
                        }
                        headerGridButton.Paint(g, rect, normal, item.Text, item.Bmp, item.Align, item.TextBmpLayout, this.ActAsEnabled, useGdiPlus, GridButtonType.Header);
                    }
                    rect.X = rect.Right + ScrollManager.GRID_LINE_WIDTH;
                    rect.Width = 0;
                }
                else
                {
                    rect.Width += ScrollManager.GRID_LINE_WIDTH;
                }
            }
        }

        protected virtual void PaintHorizGridLines(Graphics g, long rows, int nFirstRowPos, int nLeftMostPoint, int nRightMostPoint, bool bAdjust)
        {
            Pen gridLinesPen = this.GridLinesPen;
            if (bAdjust)
            {
                nRightMostPoint = Math.Min(this.m_scrollableArea.Right, nRightMostPoint);
            }
            int num = nFirstRowPos;
            int cellHeight = this.m_scrollMgr.CellHeight;
            for (long i = 0L; i <= rows; i += 1L)
            {
                g.DrawLine(gridLinesPen, nLeftMostPoint, num, nRightMostPoint, num);
                num += cellHeight + ScrollManager.GRID_LINE_WIDTH;
            }
        }

        protected virtual void PaintOneCell(Graphics g, int nCol, long nRow, int nEditedCol, long nEditedRow, ref Rectangle rCell, ref Rectangle rCurrentCellRect, ref Rectangle rEditingCellRect)
        {
            GridColumn gridColumn = this.m_Columns[nCol];
            int widthInPixels = gridColumn.WidthInPixels;
            int num2 = ScrollManager.GRID_LINE_WIDTH;
            SolidBrush bkBrush = null;
            SolidBrush textBrush = null;
            Font font = this.Font;
            if ((this.m_scrollMgr.FirstScrollableColumnIndex > 0) && (nCol == this.m_scrollMgr.FirstScrollableColumnIndex))
            {
                rCell.X -= num2;
                rCell.Width = widthInPixels + (2 * num2);
            }
            else
            {
                rCell.Width = widthInPixels + num2;
            }
            if ((nEditedCol == nCol) && (nEditedRow == nRow))
            {
                rEditingCellRect = rCell;
            }
            else if (g.IsVisible(rCell))
            {
                if (this.ActAsEnabled)
                {
                    this.GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
                    this.DoCellPainting(g, bkBrush, textBrush, this.GetCellFont(nRow, gridColumn), rCell, gridColumn, nRow, true);
                    if ((this.NeedToHighlightCurrentCell && (nCol == this.m_selMgr.CurrentColumn)) && (nRow == this.m_selMgr.CurrentRow))
                    {
                        rCurrentCellRect = rCell;
                        if ((this.m_scrollMgr.FirstScrollableColumnIndex > 0) && (nCol == this.m_scrollMgr.FirstScrollableColumnIndex))
                        {
                            rCurrentCellRect.X++;
                            rCurrentCellRect.Width--;
                        }
                    }
                }
                else
                {
                    this.DoCellPainting(g, bkBrush, textBrush, this.GetCellFont(nRow, gridColumn), rCell, gridColumn, nRow, false);
                }
            }
            rCell.X = rCell.Right;
        }

        protected virtual void PaintVertGridLines(Graphics g, int nFirstCol, int nLastCol, int nFirstColPos, int nTopMostPoint, int nBottomMostPoint)
        {
            Pen gridLinesPen = this.GridLinesPen;
            int num = nFirstColPos;
            if (nFirstCol == 0)
            {
                g.DrawLine(gridLinesPen, num, nTopMostPoint, num, nBottomMostPoint);
            }
            for (int i = nFirstCol; i <= nLastCol; i++)
            {
                num += this.m_Columns[i].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
                if (this.m_Columns[i].RightGridLine)
                {
                    g.DrawLine(gridLinesPen, num, nTopMostPoint, num, nBottomMostPoint);
                }
            }
        }
        #endregion

        #region Nested Types
        internal protected class GridPrinter
        {
            private int m_firstColNum;
            private long m_firstRowNum;
            private GridControl m_grid;
            private int m_gridColNumber;
            private long m_gridRowNumber;
            private GridLineType m_linesType;

            protected GridPrinter()
            {
                this.m_linesType = GridLineType.Solid;
            }

            public GridPrinter(GridControl grid)
            {
                this.m_linesType = GridLineType.Solid;
                if (grid.ColumnsNumber <= 0)
                {
                    throw new ArgumentException(SRError.GridColumnNumberShouldBeAboveZero, "grid");
                }
                this.m_grid = grid;
                this.m_gridColNumber = grid.ColumnsNumber;
                this.m_gridRowNumber = grid.GridStorage.NumRows();
                this.m_linesType = grid.m_lineType;
            }

            private bool AdjustRowAndColumnIndeces(int nLastColNumber, long nLastRowNumber)
            {
                bool flag = true;
                if (nLastColNumber == (this.m_gridColNumber - 1))
                {
                    this.m_firstColNum = 0;
                }
                else
                {
                    this.m_firstColNum = nLastColNumber + 1;
                }
                if ((nLastRowNumber == (this.m_gridRowNumber - 1L)) || (this.m_gridRowNumber == 0L))
                {
                    if (nLastColNumber == (this.m_gridColNumber - 1))
                    {
                        flag = false;
                    }
                    return flag;
                }
                if (nLastColNumber == (this.m_gridColNumber - 1))
                {
                    this.m_firstRowNum = nLastRowNumber + 1L;
                }
                return flag;
            }

            private int CalculateLastColumn(int nPageWidth, out int nRealWidth)
            {
                int firstColNum = this.m_firstColNum;
                int num2 = 0;
                nRealWidth = ScrollManager.GRID_LINE_WIDTH;
                while (firstColNum < this.m_gridColNumber)
                {
                    num2 = this.m_grid.m_Columns[firstColNum].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
                    if ((num2 + nRealWidth) >= nPageWidth)
                    {
                        if (firstColNum == this.m_firstColNum)
                        {
                            nRealWidth = nPageWidth;
                        }
                        else
                        {
                            firstColNum--;
                        }
                        break;
                    }
                    nRealWidth += num2;
                    firstColNum++;
                }
                if (firstColNum == this.m_gridColNumber)
                {
                    firstColNum--;
                }
                return firstColNum;
            }

            private long CalculateLastRow(int nPageHeight)
            {
                if (this.m_gridRowNumber == 0L)
                {
                    return 0L;
                }
                long firstRowNum = this.m_firstRowNum;
                int num2 = this.m_grid.m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH;
                if ((this.m_firstRowNum == 0L) && this.m_grid.m_withHeader)
                {
                    firstRowNum += ((nPageHeight - this.m_grid.HeaderHeight) / num2) - 1;
                }
                else
                {
                    firstRowNum += (nPageHeight / num2) - 1;
                }
                if (firstRowNum < this.m_firstRowNum)
                {
                    firstRowNum = this.m_firstRowNum;
                }
                if (firstRowNum >= this.m_gridRowNumber)
                {
                    firstRowNum = this.m_gridRowNumber - 1L;
                }
                return firstRowNum;
            }

            protected virtual void PrintOneCell(Graphics g, int nCol, long nRow, ref Rectangle rCell)
            {
                GridColumn gridColumn = this.m_grid.m_Columns[nCol];
                int widthInPixels = gridColumn.WidthInPixels;
                int num2 = ScrollManager.GRID_LINE_WIDTH;
                SolidBrush bkBrush = null;
                SolidBrush textBrush = null;
                rCell.Width = widthInPixels + num2;
                this.m_grid.GetCellGDIObjects(gridColumn, nRow, nCol, ref bkBrush, ref textBrush);
                if (gridColumn.WithSelectionBk && this.m_grid.m_selMgr.IsCellSelected(nRow, nCol))
                {
                    bkBrush = this.m_grid.m_highlightBrush;
                }
                this.m_grid.DoCellPrinting(g, bkBrush, textBrush, this.m_grid.GetCellFont(nRow, gridColumn), rCell, gridColumn, nRow);
                rCell.X = rCell.Right;
            }

            public virtual void PrintPage(PrintPageEventArgs ev)
            {
                int nRealWidth = 0;
                int nLastCol = this.CalculateLastColumn(ev.MarginBounds.Width, out nRealWidth);
                long nLastRowNumber = this.CalculateLastRow(ev.MarginBounds.Height);
                int nFirstColPos = ev.MarginBounds.Left + 1;
                Graphics g = ev.Graphics;
                Region clip = g.Clip;
                try
                {
                    g.SetClip(ev.MarginBounds);
                    bool flag = (this.m_firstRowNum == 0L) && this.m_grid.m_withHeader;
                    if (flag)
                    {
                        this.m_grid.PaintHeaderHelper(g, this.m_firstColNum, nLastCol, nFirstColPos, ev.MarginBounds.Top + 1, true);
                    }
                    if (this.m_gridRowNumber > 0L)
                    {
                        int num5 = ScrollManager.GRID_LINE_WIDTH;
                        int cellHeight = this.m_grid.m_scrollMgr.CellHeight;
                        int top = ev.MarginBounds.Top;
                        if (flag)
                        {
                            top += this.m_grid.HeaderHeight + 1;
                        }
                        else
                        {
                            top++;
                        }
                        Rectangle rCell = new Rectangle();
                        rCell.Y = top;
                        rCell.Height = cellHeight + num5;
                        for (long i = this.m_firstRowNum; i <= nLastRowNumber; i += 1L)
                        {
                            rCell.X = nFirstColPos;
                            for (int j = this.m_firstColNum; j <= nLastCol; j++)
                            {
                                this.PrintOneCell(g, j, i, ref rCell);
                            }
                            rCell.Offset(0, cellHeight + num5);
                        }
                        if (this.m_linesType != GridLineType.None)
                        {
                            this.m_grid.PaintHorizGridLines(g, (nLastRowNumber - this.m_firstRowNum) + 1L, top, nFirstColPos - 1, rCell.X - num5, false);
                            this.m_grid.PaintVertGridLines(g, this.m_firstColNum, nLastCol, nFirstColPos - 1, top, rCell.Y);
                            g.DrawLine(this.m_grid.GridLinesPen, nFirstColPos - 1, top, nFirstColPos - 1, rCell.Y);
                        }
                    }
                    ev.HasMorePages = this.AdjustRowAndColumnIndeces(nLastCol, nLastRowNumber);
                }
                finally
                {
                    g.Clip = clip;
                }
            }
        }
        #endregion
    }
}
