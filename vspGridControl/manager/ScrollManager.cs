using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class ScrollManager
    {
        public static readonly int GRID_LINE_WIDTH = 1;
        private Rectangle m_cachedSA = Rectangle.Empty;
        private GridColumnCollection m_Columns;
        private long m_cRowsNum;
        private int m_firstColIndex;
        private int m_firstColPos;
        private long m_firstRowIndex;
        private int m_firstRowPos;
        private int m_firstScrollableColIndex;
        private uint m_firstScrollableRowIndex;
        private const int m_horizScrollUnit = 1;
        private int m_horizScrollUnitForArrows = 1;
        private IntPtr m_hWnd = IntPtr.Zero;
        private int m_lastColIndex;
        private long m_lastRowIndex;
        private int m_nCellHeight;
        private Rectangle m_SARect = Rectangle.Empty;
        private int m_totalGridWidth;

        public ScrollManager()
        {
            this.Reset();
        }

        private void AdjustGridByHorizScrollBarPos(int nHorizScrollPos)
        {
            int num;
            this.AdjustGridByHorizScrollBarPosWithoutClientRedraw(nHorizScrollPos, out num);
            NativeMethods.RECT rectScrollRegion = NativeMethods.RECT.FromXYWH(this.m_SARect.X, 0, this.m_SARect.Width, this.m_SARect.Height + this.m_SARect.Y);
            SafeNativeMethods.ScrollWindow(this.m_hWnd, num, 0, ref rectScrollRegion, ref rectScrollRegion);
        }

        private void AdjustGridByHorizScrollBarPosWithoutClientRedraw(int nHorizScrollPos, out int xDelta)
        {
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si);
            int nPos = si.nPos;
            si.nPos = nHorizScrollPos;
            si.fMask = 4;
            NativeMethods.SetScrollInfo(this.m_hWnd, 0, si, true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si);
            this.CalcFirstColumnIndexAndPosFromScrollPos(si.nPos);
            this.CalcLastColumnIndex();
            xDelta = nPos - si.nPos;
        }

        private void CalcFirstColumnIndexAndPosFromScrollPos(int nPos)
        {
            int count = this.m_Columns.Count;
            if (this.m_firstScrollableColIndex < count)
            {
                int num2 = this.m_SARect.Left + nPos;
                int num3 = this.m_SARect.Left + GRID_LINE_WIDTH;
                this.m_firstColIndex = this.m_firstScrollableColIndex;
                int num4 = num3;
                while ((this.m_firstColIndex < count) && (num3 < num2))
                {
                    num4 = num3;
                    num3 += this.m_Columns[this.m_firstColIndex].WidthInPixels + GRID_LINE_WIDTH;
                    this.m_firstColIndex++;
                }
                if (this.m_firstColIndex > this.m_firstScrollableColIndex)
                {
                    this.m_firstColIndex--;
                }
                this.m_firstColPos = (this.m_SARect.Left + num4) - num2;
            }
        }

        private void CalcFirstRowIndexAndPosFromScrollPos(long nPos)
        {
            if (nPos < 0L)
            {
                throw new ArgumentException(SRError.ScrollPosShouldBeMoreOrEqualZero, "nPos");
            }
            this.m_firstRowIndex = nPos + this.m_firstScrollableRowIndex;
            this.m_firstRowPos = this.m_SARect.Top + (((int) nPos) * (this.m_nCellHeight + GRID_LINE_WIDTH));
        }

        private int CalcHorizPageSize()
        {
            if (this.m_SARect.Width <= 0)
            {
                return -1;
            }
            return (this.m_SARect.Width / 1);
        }

        private void CalcLastColumnIndex()
        {
            this.m_lastColIndex = this.m_firstColIndex;
            int count = this.m_Columns.Count;
            if ((count != 0) && (this.m_SARect.Right > 0))
            {
                int num2 = this.m_firstColPos + GRID_LINE_WIDTH;
                while ((this.m_lastColIndex < count) && (num2 <= this.m_SARect.Right))
                {
                    num2 += this.m_Columns[this.m_lastColIndex].WidthInPixels + GRID_LINE_WIDTH;
                    this.m_lastColIndex++;
                }
                if (this.m_lastColIndex > this.m_firstColIndex)
                {
                    this.m_lastColIndex--;
                }
            }
        }

        private void CalcLastRowIndex()
        {
            this.m_lastRowIndex = this.m_firstRowIndex;
            if ((0L != this.m_cRowsNum) && (this.m_SARect.Bottom > 0))
            {
                int num = this.m_firstRowPos + GRID_LINE_WIDTH;
                while ((this.m_lastRowIndex < this.m_cRowsNum) && (num <= this.m_SARect.Bottom))
                {
                    num += this.m_nCellHeight + GRID_LINE_WIDTH;
                    this.m_lastRowIndex += 1L;
                }
                if (this.m_lastRowIndex > 0L)
                {
                    this.m_lastRowIndex -= 1L;
                }
            }
        }

        private int CalcVertPageSize()
        {
            return this.CalcVertPageSize(this.m_SARect);
        }

        internal int CalcVertPageSize(Rectangle ScrollableAreaRect)
        {
            if (ScrollableAreaRect.Height <= 0)
            {
                return -1;
            }
            return ((ScrollableAreaRect.Height - GRID_LINE_WIDTH) / (this.m_nCellHeight + GRID_LINE_WIDTH));
        }

        public void EnsureCellIsVisible(long nRowIndex, int nColIndex)
        {
            this.EnsureCellIsVisible(nRowIndex, nColIndex, true, true);
        }

        public void EnsureCellIsVisible(long nRowIndex, int nColIndex, bool bMakeFirstColFullyVisible, bool bRedraw)
        {
            if (bRedraw)
            {
                this.EnsureRowIsVisible(nRowIndex, true);
            }
            else
            {
                int num;
                this.EnsureRowIsVisbleWithoutClientRedraw(nRowIndex, true, out num);
            }
            this.EnsureColumnIsVisible(nColIndex, bMakeFirstColFullyVisible, bRedraw);
        }

        public void EnsureColumnIsVisible(int nColIndex, bool bMakeFirstFullyVisible, bool bRedraw)
        {
            if (((nColIndex < this.m_firstColIndex) && (nColIndex >= this.m_firstScrollableColIndex)) || ((nColIndex > this.m_lastColIndex) || ((bMakeFirstFullyVisible && (nColIndex == this.m_firstColIndex)) && (this.m_firstColPos < this.m_SARect.X))))
            {
                int num = 0;
                for (int i = this.m_firstScrollableColIndex; i < nColIndex; i++)
                {
                    num += this.m_Columns[i].WidthInPixels + GRID_LINE_WIDTH;
                }
                if (bRedraw)
                {
                    this.AdjustGridByHorizScrollBarPos(num / 1);
                }
                else
                {
                    int num3;
                    this.AdjustGridByHorizScrollBarPosWithoutClientRedraw(num / 1, out num3);
                }
            }
        }

        public bool EnsureRowIsVisbleWithoutClientRedraw(long nRowIndex, bool bMakeRowTheTopOne, out int yDelta)
        {
            yDelta = 0;
            if (nRowIndex < this.m_firstScrollableRowIndex)
            {
                return false;
            }
            if (((nRowIndex >= this.m_firstRowIndex) && (nRowIndex <= this.m_lastRowIndex)) && ((nRowIndex != this.m_lastRowIndex) || bMakeRowTheTopOne))
            {
                return false;
            }
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 1, si);
            long firstRowIndex = this.m_firstRowIndex;
            if (bMakeRowTheTopOne)
            {
                this.m_firstRowIndex = Math.Max(nRowIndex, (long) this.m_firstScrollableRowIndex);
            }
            else if (si.nPage > 0)
            {
                this.m_firstRowIndex = Math.Max((nRowIndex - si.nPage) + 1L, (long) this.m_firstScrollableRowIndex);
            }
            else
            {
                this.m_firstRowIndex = Math.Max(nRowIndex, (long) this.m_firstScrollableRowIndex);
            }
            this.m_firstRowIndex = Math.Min(this.m_firstRowIndex, this.GetMaxFirstRowIndex(si.nPage));
            this.CalcLastRowIndex();
            si.nPos = (int) (this.m_firstRowIndex - this.m_firstScrollableRowIndex);
            si.fMask = 4;
            NativeMethods.SetScrollInfo(this.m_hWnd, 1, si, true);
            yDelta = ((int) (firstRowIndex - this.m_firstRowIndex)) * (this.m_nCellHeight + GRID_LINE_WIDTH);
            return true;
        }

        public void EnsureRowIsVisible(long nRowIndex, bool bMakeRowTheTopOne)
        {
            int num;
            if (this.EnsureRowIsVisbleWithoutClientRedraw(nRowIndex, bMakeRowTheTopOne, out num))
            {
                NativeMethods.RECT rectScrollRegion = NativeMethods.RECT.FromXYWH(0, this.m_SARect.Y, this.m_SARect.Right, this.m_SARect.Height);
                SafeNativeMethods.ScrollWindow(this.m_hWnd, 0, num, ref rectScrollRegion, ref rectScrollRegion);
            }
        }

        private long FirstRowIndexFromThumbTrack(int nThumbPos)
        {
            return (nThumbPos + this.m_firstScrollableRowIndex);
        }

        public Rectangle GetCellRectangle(long nRowIndex, int nColIndex)
        {
            if (((nRowIndex >= this.m_firstRowIndex) && (nRowIndex <= this.m_lastRowIndex)) || ((nRowIndex >= 0L) && (nRowIndex < this.FirstScrollableRowIndex)))
            {
                int height = this.CellHeight + GRID_LINE_WIDTH;
                Rectangle rectangle = new Rectangle(0, this.m_firstRowPos + (((int) (nRowIndex - this.m_firstRowIndex)) * height), 0, height);
                rectangle.X = GRID_LINE_WIDTH;
                int num2 = -1;
                int widthInPixels = -1;
                if ((nColIndex >= 0) && (nColIndex < this.m_firstScrollableColIndex))
                {
                    for (num2 = 0; num2 < this.FirstScrollableColumnIndex; num2++)
                    {
                        rectangle.X -= GRID_LINE_WIDTH;
                        widthInPixels = this.m_Columns[num2].WidthInPixels;
                        rectangle.Width = widthInPixels + GRID_LINE_WIDTH;
                        if (num2 == nColIndex)
                        {
                            return rectangle;
                        }
                        rectangle.X += widthInPixels + (2 * GRID_LINE_WIDTH);
                    }
                }
                if ((nColIndex >= this.m_firstColIndex) && (nColIndex <= this.m_lastColIndex))
                {
                    rectangle.X = this.m_firstColPos + GRID_LINE_WIDTH;
                    for (num2 = this.m_firstColIndex; num2 <= this.m_lastColIndex; num2++)
                    {
                        rectangle.X -= GRID_LINE_WIDTH;
                        widthInPixels = this.m_Columns[num2].WidthInPixels;
                        rectangle.Width = widthInPixels + GRID_LINE_WIDTH;
                        if (num2 == nColIndex)
                        {
                            return rectangle;
                        }
                        rectangle.X += widthInPixels + (2 * GRID_LINE_WIDTH);
                    }
                }
            }
            return Rectangle.Empty;
        }

        private long GetMaxFirstRowIndex(int pageSize)
        {
            if (pageSize > 0)
            {
                return (this.m_cRowsNum - pageSize);
            }
            return (this.m_cRowsNum - 1L);
        }

        public void HandleHScroll(int nScrollRequest)
        {
            int xDelta = -1;
            NativeMethods.RECT scrollRect = new NativeMethods.RECT(0, 0, 0, 0);
            if (this.HandleHScrollWithoutClientRedraw(nScrollRequest, ref xDelta, ref scrollRect))
            {
                SafeNativeMethods.ScrollWindow(this.m_hWnd, xDelta, 0, ref scrollRect, ref scrollRect);
            }
        }

        internal bool HandleHScrollWithoutClientRedraw(int nScrollRequest, ref int xDelta, ref NativeMethods.RECT scrollRect)
        {
            bool flag = false;
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si);
            int nPos = si.nPos;
            switch (nScrollRequest)
            {
                case 0:
                    si.nPos -= this.m_horizScrollUnitForArrows / 1;
                    break;

                case 1:
                    si.nPos += this.m_horizScrollUnitForArrows / 1;
                    break;

                case 2:
                    si.nPos -= si.nPage;
                    break;

                case 3:
                    si.nPos += si.nPage;
                    break;

                case 5:
                    si.nPos = si.nTrackPos;
                    break;

                case 6:
                    si.nPos = si.nMin;
                    break;

                case 7:
                    si.nPos = si.nMax - Math.Max(si.nPage - 1, 0);
                    break;
            }
            si.fMask = 4;
            NativeMethods.SetScrollInfo(this.m_hWnd, 0, si, true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si);
            if (si.nPos != nPos)
            {
                flag = true;
                this.CalcFirstColumnIndexAndPosFromScrollPos(si.nPos);
                this.CalcLastColumnIndex();
                scrollRect = NativeMethods.RECT.FromXYWH(this.m_SARect.X, 0, this.m_SARect.Width, this.m_SARect.Height + this.m_SARect.Y);
                xDelta = nPos - si.nPos;
            }
            return flag;
        }

        public void HandleVScroll(int nScrollRequest)
        {
            int yDelta = -1;
            NativeMethods.RECT scrollRect = new NativeMethods.RECT(0, 0, 0, 0);
            if (this.HandleVScrollWithoutClientRedraw(nScrollRequest, ref yDelta, ref scrollRect))
            {
                SafeNativeMethods.ScrollWindow(this.m_hWnd, 0, yDelta, ref scrollRect, ref scrollRect);
            }
        }

        internal bool HandleVScrollWithoutClientRedraw(int nScrollRequest, ref int yDelta, ref NativeMethods.RECT scrollRect)
        {
            bool flag = false;
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 1, si);
            long firstRowIndex = this.m_firstRowIndex;
            bool flag2 = true;
            switch (nScrollRequest)
            {
                case 0:
                    this.m_firstRowIndex -= 1L;
                    break;

                case 1:
                    this.m_firstRowIndex += 1L;
                    break;

                case 2:
                    this.m_firstRowIndex -= si.nPage;
                    break;

                case 3:
                    this.m_firstRowIndex += si.nPage;
                    break;

                case 5:
                    si.nPos = si.nTrackPos;
                    this.m_firstRowIndex = this.FirstRowIndexFromThumbTrack(si.nTrackPos);
                    flag2 = false;
                    break;

                case 6:
                    this.m_firstRowIndex = this.m_firstScrollableRowIndex;
                    si.nPos = si.nMin;
                    flag2 = false;
                    break;

                case 7:
                    this.m_firstRowIndex = (this.m_cRowsNum - this.m_firstScrollableRowIndex) - Math.Max(si.nPage - 1, 0);
                    si.nPos = (si.nMax - ((int) this.m_firstScrollableRowIndex)) - Math.Max(si.nPage - 1, 0);
                    flag2 = false;
                    break;
            }
            this.m_firstRowIndex = Math.Max(this.m_firstRowIndex, (long) this.m_firstScrollableRowIndex);
            this.m_firstRowIndex = Math.Min(this.m_firstRowIndex, this.GetMaxFirstRowIndex(si.nPage));
            if (flag2)
            {
                si.nPos += (int) (this.m_firstRowIndex - firstRowIndex);
            }
            si.fMask = 4;
            NativeMethods.SetScrollInfo(this.m_hWnd, 1, si, true);
            if (this.m_firstRowIndex != firstRowIndex)
            {
                flag = true;
                this.CalcLastRowIndex();
                scrollRect = NativeMethods.RECT.FromXYWH(0, this.m_SARect.Y, this.m_SARect.Right, this.m_SARect.Height);
                yDelta = ((int) (firstRowIndex - this.m_firstRowIndex)) * (this.m_nCellHeight + GRID_LINE_WIDTH);
            }
            return flag;
        }

        public void MakeNextColumnVisible(bool bRedraw)
        {
            if (this.m_lastColIndex < (this.m_Columns.Count - 1))
            {
                NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
                SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si);
                int num = (GRID_LINE_WIDTH + this.m_Columns[this.m_firstColIndex].WidthInPixels) - Math.Abs((int) ((this.m_SARect.X + GRID_LINE_WIDTH) - this.m_firstColPos));
                for (int i = this.m_firstColIndex + 1; i <= this.m_lastColIndex; i++)
                {
                    num += GRID_LINE_WIDTH + this.m_Columns[i].WidthInPixels;
                }
                int num3 = num - this.m_SARect.Width;
                int num4 = (int) Math.Floor((double) (((num3 + (2 * GRID_LINE_WIDTH)) + this.m_Columns[this.m_lastColIndex + 1].WidthInPixels) / 1));
                if (bRedraw)
                {
                    this.AdjustGridByHorizScrollBarPos(si.nPos + num4);
                }
                else
                {
                    int num5;
                    this.AdjustGridByHorizScrollBarPosWithoutClientRedraw(si.nPos + num4, out num5);
                }
            }
        }

        public void OnSAChange(Rectangle newSA)
        {
            if (this.m_hWnd != IntPtr.Zero)
            {
                this.m_cachedSA = this.m_SARect;
                this.m_SARect = newSA;
                this.m_firstRowPos = this.m_SARect.Top;
                if (this.m_SARect.Height != this.m_cachedSA.Height)
                {
                    this.ProcessVertChange();
                    this.CalcLastRowIndex();
                    SafeNativeMethods.InvalidateRect(this.m_hWnd, null, true);
                }
                if (this.m_SARect.Width != this.m_cachedSA.Width)
                {
                    if (this.m_Columns.Count > 0)
                    {
                        int nOldScrollPos = 0;
                        int nPos = this.ProcessHorizChange(ref nOldScrollPos);
                        this.CalcFirstColumnIndexAndPosFromScrollPos(nPos);
                        this.CalcLastColumnIndex();
                    }
                    SafeNativeMethods.InvalidateRect(this.m_hWnd, null, true);
                }
            }
        }

        public void OnSAChange(int nLeft, int nRight, int nTop, int nBottom)
        {
            Rectangle newSA = new Rectangle(nLeft, nTop, nRight - nLeft, nBottom - nTop);
            this.OnSAChange(newSA);
        }

        public void ProcessDeleteCol(int nIndex, int nWidth)
        {
            this.m_totalGridWidth -= nWidth + GRID_LINE_WIDTH;
            if (this.m_lastColIndex >= this.m_Columns.Count)
            {
                this.m_lastColIndex = this.m_Columns.Count - 1;
            }
            this.ProcessHorizChange();
            this.RecalcAll(this.m_SARect);
        }

        private int ProcessHorizChange()
        {
            int nOldScrollPos = 0;
            return this.ProcessHorizChange(ref nOldScrollPos);
        }

        private int ProcessHorizChange(ref int nOldScrollPos)
        {
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si);
            nOldScrollPos = si.nPos;
            si.nMin = 0;
            si.nMax = (this.m_totalGridWidth / 1) - 1;
            si.nPage = this.CalcHorizPageSize();
            return NativeMethods.SetScrollInfo(this.m_hWnd, 0, si, true);
        }

        public void ProcessNewCol(int nIndex)
        {
            this.CalcLastColumnIndex();
            this.m_totalGridWidth += this.m_Columns[nIndex].WidthInPixels + GRID_LINE_WIDTH;
            this.ProcessHorizChange();
        }

        private long ProcessVertChange()
        {
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            SafeNativeMethods.GetScrollInfo(this.m_hWnd, 1, si);
            si.nPage = this.CalcVertPageSize();
            si.nMin = 0;
            si.nMax = (int) Math.Max((long) ((this.m_cRowsNum - 1L) - this.m_firstScrollableRowIndex), (long) 0L);
            long maxFirstRowIndex = this.GetMaxFirstRowIndex(si.nPage);
            if (this.m_firstRowIndex > maxFirstRowIndex)
            {
                this.m_firstRowIndex = Math.Max((long) this.m_firstScrollableRowIndex, maxFirstRowIndex);
            }
            si.nPos = (int) (this.m_firstRowIndex - this.m_firstScrollableRowIndex);
            NativeMethods.SetScrollInfo(this.m_hWnd, 1, si, true);
            return (long) si.nPos;
        }

        public void RecalcAll(Rectangle scrollableArea)
        {
            if ((this.m_Columns != null) && (this.m_Columns.Count != 0))
            {
                this.m_SARect = scrollableArea;
                this.m_totalGridWidth = GRID_LINE_WIDTH;
                int firstScrollableColIndex = this.m_firstScrollableColIndex;
                int count = this.m_Columns.Count;
                while (firstScrollableColIndex < count)
                {
                    this.m_totalGridWidth += this.m_Columns[firstScrollableColIndex].WidthInPixels + GRID_LINE_WIDTH;
                    firstScrollableColIndex++;
                }
                int nPos = this.ProcessHorizChange();
                this.CalcFirstColumnIndexAndPosFromScrollPos(nPos);
                this.CalcLastColumnIndex();
                this.m_firstRowPos = this.m_SARect.Top;
                this.ProcessVertChange();
                this.CalcLastRowIndex();
            }
        }

        public void Reset()
        {
            this.m_firstRowIndex = 0L;
            this.m_firstRowPos = 0;
            this.m_lastRowIndex = 0L;
            this.m_firstColIndex = 0;
            this.m_firstColPos = 0;
            this.m_lastColIndex = 0;
            this.m_cRowsNum = 0L;
            this.m_totalGridWidth = GRID_LINE_WIDTH;
            this.m_firstScrollableColIndex = 0;
            this.m_firstScrollableRowIndex = 0;
            this.ResetScrollBars();
        }

        private void ResetScrollBars()
        {
            NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
            si.nPage = si.nMin = si.nMax = 0;
            NativeMethods.SetScrollInfo(this.m_hWnd, 1, si, true);
            NativeMethods.SetScrollInfo(this.m_hWnd, 0, si, true);
        }

        public void SetColumns(GridColumnCollection columns)
        {
            if (this.m_Columns != columns)
            {
                this.m_Columns = columns;
            }
        }

        public void SetGridWindowHandle(IntPtr handle)
        {
            if (handle != this.m_hWnd)
            {
                this.m_hWnd = handle;
                this.ResetScrollBars();
            }
        }

        public void SetHorizontalScrollUnitForArrows(int numUnits)
        {
            if (numUnits <= 0)
            {
                throw new ArgumentException(SRError.HorizScrollUnitShouldBeGreaterThanZero);
            }
            this.m_horizScrollUnitForArrows = numUnits;
        }

        public void UpdateColWidth(int nIndex, int nOldWidth, int nNewWidth, bool bFinalUpdate)
        {
            this.m_totalGridWidth -= nOldWidth;
            this.m_totalGridWidth += nNewWidth;
            int nPos = this.ProcessHorizChange();
            if (bFinalUpdate)
            {
                this.CalcFirstColumnIndexAndPosFromScrollPos(nPos);
            }
            this.CalcLastColumnIndex();
        }

        public int CellHeight
        {
            get
            {
                return this.m_nCellHeight;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(SRError.CellHeightShouldBeGreaterThanZero);
                }
                this.m_nCellHeight = value;
            }
        }

        public int FirstColumnIndex
        {
            get
            {
                return this.m_firstColIndex;
            }
        }

        public int FirstColumnPos
        {
            get
            {
                return this.m_firstColPos;
            }
        }

        public long FirstRowIndex
        {
            get
            {
                return this.m_firstRowIndex;
            }
        }

        public int FirstRowPos
        {
            get
            {
                return this.m_firstRowPos;
            }
        }

        public int FirstScrollableColumnIndex
        {
            get
            {
                return this.m_firstScrollableColIndex;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(SRError.FirstScrollableColumnShouldBeValid);
                }
                if (this.m_firstScrollableColIndex != value)
                {
                    this.m_firstScrollableColIndex = value;
                    if (this.m_hWnd != IntPtr.Zero)
                    {
                        NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
                        if (SafeNativeMethods.GetScrollInfo(this.m_hWnd, 0, si))
                        {
                            this.CalcFirstColumnIndexAndPosFromScrollPos(si.nPos);
                        }
                    }
                }
            }
        }

        [CLSCompliant(false)]
        public uint FirstScrollableRowIndex
        {
            get
            {
                return this.m_firstScrollableRowIndex;
            }
            set
            {
                if (this.m_firstScrollableRowIndex != value)
                {
                    this.m_firstScrollableRowIndex = value;
                    this.m_firstRowIndex = this.m_firstScrollableRowIndex;
                    if (this.m_hWnd != IntPtr.Zero)
                    {
                        NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(true);
                        if (SafeNativeMethods.GetScrollInfo(this.m_hWnd, 1, si))
                        {
                            this.CalcFirstRowIndexAndPosFromScrollPos((long) si.nPos);
                        }
                    }
                }
            }
        }

        public int LastColumnIndex
        {
            get
            {
                return this.m_lastColIndex;
            }
        }

        public long LastRowIndex
        {
            get
            {
                return this.m_lastRowIndex;
            }
        }

        public long RowsNumber
        {
            get
            {
                return this.m_cRowsNum;
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentException(SRError.NumOfRowsShouldBeGreaterThanZero);
                }
                if (this.m_cRowsNum != value)
                {
                    this.m_cRowsNum = value;
                    long maxFirstRowIndex = this.GetMaxFirstRowIndex(this.CalcVertPageSize());
                    if (this.m_firstRowIndex > maxFirstRowIndex)
                    {
                        this.m_firstRowIndex = Math.Max((long) this.m_firstScrollableRowIndex, maxFirstRowIndex);
                    }
                    this.CalcLastRowIndex();
                    this.ProcessVertChange();
                }
            }
        }
    }
}

