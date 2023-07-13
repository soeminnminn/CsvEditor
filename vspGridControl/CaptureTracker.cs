using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class CaptureTracker
    {
        // Fields
        private bool embContrlFocused;
        private Timer hyperlinkSelTimer;
        private Rectangle m_adjustedCellRect;
        private GridButtonArea m_buttonArea;
        private bool m_buttonWasPushed;
        private HitTestResult m_captureHitTest;
        private Rectangle m_cellRect;
        private int m_colIndexToDragColAfter;
        private int m_columnIndex;
        private GridDragImageListOperation m_dragOper;
        private DragOperation m_dragState;
        private int m_headerDragY;
        private int m_lastColumnIndex;
        private int m_lastColumnWidth;
        private long m_lastRowIndex;
        private int m_leftMostMergedColumnIndex;
        private int m_minWidthDuringColResize;
        private Point m_mouseCapturePoint;
        private int m_mouseOffsetForColResize;
        private int m_origColumnWidth;
        private long m_rowIndex;
        private int m_selectionBlockIndex;
        private int m_totalGridLineAdjDuringResize;
        private bool m_wasOverButton;
        public static int NoColIndexToDragColAfter = -2;
        private DateTime timeEvent = DateTime.MinValue;

        // Methods
        public CaptureTracker()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.m_captureHitTest = HitTestResult.Nothing;
            this.m_cellRect = Rectangle.Empty;
            this.m_adjustedCellRect = Rectangle.Empty;
            this.m_rowIndex = -1L;
            this.m_columnIndex = -1;
            this.m_wasOverButton = false;
            this.m_buttonArea = GridButtonArea.Background;
            this.m_origColumnWidth = -1;
            this.m_lastColumnWidth = -1;
            this.m_leftMostMergedColumnIndex = -1;
            this.m_minWidthDuringColResize = -1;
            this.m_mouseOffsetForColResize = 0;
            this.m_totalGridLineAdjDuringResize = 0;
            this.m_lastColumnIndex = -1;
            this.m_lastRowIndex = -1L;
            this.m_buttonWasPushed = false;
            this.m_mouseCapturePoint.X = -1;
            this.m_mouseCapturePoint.Y = -1;
            this.m_selectionBlockIndex = -1;
            this.m_dragState = DragOperation.None;
            this.DragImageOperation = null;
            this.m_headerDragY = -1;
            this.m_colIndexToDragColAfter = NoColIndexToDragColAfter;
            this.embContrlFocused = false;
            this.timeEvent = DateTime.MinValue;
            if (this.hyperlinkSelTimer != null)
            {
                this.hyperlinkSelTimer.Stop();
            }
        }

        public void SetInfoFromHitTest(HitTestInfo htInfo)
        {
            this.m_rowIndex = htInfo.RowIndex;
            this.m_columnIndex = htInfo.ColumnIndex;
            this.m_cellRect = htInfo.AreaRectangle;
            this.m_captureHitTest = htInfo.HitTestResult;
        }

        public void UpdateAdjustedRectHorizontally(int x, int width)
        {
            this.m_adjustedCellRect.X = x;
            this.m_adjustedCellRect.Width = width;
        }

        // Properties
        public Rectangle AdjustedCellRect
        {
            get
            {
                return this.m_adjustedCellRect;
            }
            set
            {
                this.m_adjustedCellRect = value;
            }
        }

        public GridButtonArea ButtonArea
        {
            get
            {
                return this.m_buttonArea;
            }
            set
            {
                this.m_buttonArea = value;
            }
        }

        public bool ButtonWasPushed
        {
            get
            {
                return this.m_buttonWasPushed;
            }
            set
            {
                this.m_buttonWasPushed = value;
            }
        }

        public HitTestResult CaptureHitTest
        {
            get
            {
                return this.m_captureHitTest;
            }
            set
            {
                this.m_captureHitTest = value;
            }
        }

        public Rectangle CellRect
        {
            get
            {
                return this.m_cellRect;
            }
            set
            {
                this.m_cellRect = value;
            }
        }

        public int ColIndexToDragColAfter
        {
            get
            {
                return this.m_colIndexToDragColAfter;
            }
            set
            {
                this.m_colIndexToDragColAfter = value;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_columnIndex;
            }
            set
            {
                this.m_columnIndex = value;
            }
        }

        internal GridDragImageListOperation DragImageOperation
        {
            get
            {
                return this.m_dragOper;
            }
            set
            {
                if ((this.m_dragOper != null) && (this.m_dragOper != value))
                {
                    try
                    {
                        this.m_dragOper.Dispose();
                    }
                    catch
                    {
                    }
                    this.m_dragOper = null;
                }
                this.m_dragOper = value;
            }
        }

        public DragOperation DragState
        {
            get
            {
                return this.m_dragState;
            }
            set
            {
                this.m_dragState = value;
            }
        }

        public int HeaderDragY
        {
            get
            {
                return this.m_headerDragY;
            }
            set
            {
                this.m_headerDragY = value;
            }
        }

        internal Timer HyperLinkSelectionTimer
        {
            get
            {
                return this.hyperlinkSelTimer;
            }
            set
            {
                this.hyperlinkSelTimer = value;
            }
        }

        public int LastColumnIndex
        {
            get
            {
                return this.m_lastColumnIndex;
            }
            set
            {
                this.m_lastColumnIndex = value;
            }
        }

        public int LastColumnWidth
        {
            get
            {
                return this.m_lastColumnWidth;
            }
            set
            {
                this.m_lastColumnWidth = value;
            }
        }

        public long LastRowIndex
        {
            get
            {
                return this.m_lastRowIndex;
            }
            set
            {
                this.m_lastRowIndex = value;
            }
        }

        public int LeftMostMergedColumnIndex
        {
            get
            {
                return this.m_leftMostMergedColumnIndex;
            }
            set
            {
                this.m_leftMostMergedColumnIndex = value;
            }
        }

        public int MinWidthDuringColResize
        {
            get
            {
                return this.m_minWidthDuringColResize;
            }
            set
            {
                this.m_minWidthDuringColResize = value;
            }
        }

        public Point MouseCapturePoint
        {
            get
            {
                return this.m_mouseCapturePoint;
            }
            set
            {
                this.m_mouseCapturePoint = value;
            }
        }

        public int MouseOffsetForColResize
        {
            get
            {
                return this.m_mouseOffsetForColResize;
            }
            set
            {
                this.m_mouseOffsetForColResize = value;
            }
        }

        public int OrigColumnWidth
        {
            get
            {
                return this.m_origColumnWidth;
            }
            set
            {
                this.m_origColumnWidth = value;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_rowIndex;
            }
            set
            {
                this.m_rowIndex = value;
            }
        }

        public int SelectionBlockIndex
        {
            get
            {
                return this.m_selectionBlockIndex;
            }
            set
            {
                this.m_selectionBlockIndex = value;
            }
        }

        internal DateTime Time
        {
            get
            {
                return this.timeEvent;
            }
            set
            {
                this.timeEvent = value;
            }
        }

        public int TotalGridLineAdjDuringResize
        {
            get
            {
                return this.m_totalGridLineAdjDuringResize;
            }
            set
            {
                this.m_totalGridLineAdjDuringResize = value;
            }
        }

        internal bool WasEmbeddedControlFocused
        {
            get
            {
                return this.embContrlFocused;
            }
            set
            {
                this.embContrlFocused = value;
            }
        }

        public bool WasOverButton
        {
            get
            {
                return this.m_wasOverButton;
            }
            set
            {
                this.m_wasOverButton = value;
            }
        }

        // Nested Types
        public enum DragOperation
        {
            None,
            DragReady,
            StartedDrag
        }
    }
}
 
