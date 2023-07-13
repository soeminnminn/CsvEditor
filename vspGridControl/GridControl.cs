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
    [DefaultEvent("MouseButtonClicked"), Designer("System.Windows.Forms.Design.ControlDesigner, System.Design"), DefaultProperty("SelectionType")]
    public partial class GridControl : Control, ISupportInitialize, IGridControl
    {
        #region Variables
        private static readonly int s_nMaxNumOfVisibleRows = 80;
        private const int maximumColumnWidth = 0x4e20;
        private const double hyperlinkSelectionDelay = 0.5;

        private bool focusOnNav = true;
        private SolidBrush highlightNonFocusedBrush;
        private bool m_alwaysHighlightSelection = true;
        private Timer m_autoScrollTimer = new Timer();
        private bool m_bColumnsReorderableByDefault;
        private bool m_bInGridStorageCall;
        private System.Windows.Forms.BorderStyle m_borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private int m_cAvCharWidth;
        private Pen m_colInsertionPen;
        private CustomizeCellGDIObjectsEventArgs m_CustomizeCellGDIObjectsArgs = new CustomizeCellGDIObjectsEventArgs();
        private Hashtable m_EmbeddedControls = new Hashtable();
        private int m_headerHeight;
        private GridLineType m_lineType = GridLineType.Solid;
        private int m_nIsInitializingCount;
        private ContentsChangedEventHandler m_OnEmbeddedControlContentsChangedDelegate;
        private EventHandler m_OnEmbeddedControlLostFocusDelegate;
        private UpdateGridInvoker m_UpdateGridInternalDelegate;
        private int m_wheelDelta;
        private bool m_withHeader = true;

        protected Brush m_backBrush = new SolidBrush(Control.DefaultBackColor);
        protected CaptureTracker m_captureTracker = new CaptureTracker();
        protected GridColumnCollection m_Columns = new GridColumnCollection();
        protected Control m_curEmbeddedControl;
        protected Font m_gridFont = Control.DefaultFont;
        protected GridHeader m_gridHeader = new GridHeader();
        protected Pen m_gridLinesPen;
        protected IGridStorage m_gridStorage;
        protected ToolTip m_gridTooltip;
        protected SolidBrush m_highlightBrush;
        protected TooltipInfo m_hooverOverArea = new TooltipInfo();
        protected Font m_linkFont;
        protected Rectangle m_scrollableArea;
        protected ScrollManager m_scrollMgr = new ScrollManager();
        protected SelectionManager m_selMgr = new SelectionManager();
        protected const string s_GridEventsCategory = "Grid Events";
        protected const string s_GridPropsCategory = "Appearance";
        #endregion

        #region Events
        [Description("Occurs when user wants to initiate drag operation of the header of given column"), Category("Grid Events")]
        public event ColumnReorderRequestedEventHandler ColumnReorderRequested;

        [Category("Grid Events"), Description("Occurs when user used drag and drop operation to move a column to a new location within the grid")]
        public event ColumnsReorderedEventHandler ColumnsReordered;

        [Category("Grid Events"), Description("Called when width of a column has changed")]
        public event ColumnWidthChangedEventHandler ColumnWidthChanged;

        [Category("Grid Events"), Description("Occurs when the grid is about to draw given cell. The background brush will be used only if the cell is in NON-SELECTED state.")]
        public event CustomizeCellGDIObjectsEventHandler CustomizeCellGDIObjects;

        [Category("Grid Events"), Description("Occurs when contents of the currently active embedded control has changed")]
        public event EmbeddedControlContentsChangedEventHandler EmbeddedControlContentsChanged;

        [Description("Occurs in response to some custom behavior within the grid control itself")]
        public event GridSpecialEventHandler GridSpecialEvent;

        [Description("Called when user clicked on some header button"), Category("Grid Events")]
        public event HeaderButtonClickedEventHandler HeaderButtonClicked;

        [Category("Grid Events"), Description("Occurs when a user pressed some keyboard key while the grid had the focus and a current cell")]
        public event KeyPressedOnCellEventHandler KeyPressedOnCell;

        [Category("Grid Events"), Description("Called when user clicked on some cell AFTER all the processing was done but BEFORE the grid is redrawn")]
        public event MouseButtonClickedEventHandler MouseButtonClicked;

        [Category("Grid Events"), Description("Called when user clicked on some cell BEFORE any processing is done.")]
        public event MouseButtonClickingEventHandler MouseButtonClicking;

        [Category("Grid Events"), Description("Called when user double clicked on some part of the grid")]
        public event MouseButtonDoubleClickedEventHandler MouseButtonDoubleClicked;

        [Description("Called when selection has changed"), Category("Grid Events")]
        public event SelectionChangedEventHandler SelectionChanged;

        [Description("Called BEFORE grid does standard processing of some keys (arrows, Tab etc)"), Category("Grid Events")]
        public event StandardKeyProcessingEventHandler StandardKeyProcessing;

        [Description("Occurs when grid detects that it is time to show tooltip"), Category("Grid Events")]
        public event TooltipDataNeededEventHandler TooltipDataNeeded;
        #endregion

        #region Constructors
        public GridControl()
        {
            this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.UserMouse, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.BackColor = SystemColors.Window;
            
            this.m_scrollMgr.SetColumns(this.m_Columns);
            this.m_scrollMgr.RowsNumber = 0L;
            
            this.m_gridTooltip = new ToolTip();
            this.m_gridTooltip.InitialDelay = 0x3e8;
            this.m_gridTooltip.ShowAlways = true;
            this.m_gridTooltip.Active = false;
            this.m_gridTooltip.SetToolTip(this, "");

            this.ResetHeaderFont();
            this.InitializeCachedGDIObjects();
            
            this.m_UpdateGridInternalDelegate = new UpdateGridInvoker(this.UpdateGridInternal);
            
            this.m_OnEmbeddedControlContentsChangedDelegate = new ContentsChangedEventHandler(this.OnEmbeddedControlContentsChangedEventHandler);
            this.m_OnEmbeddedControlLostFocusDelegate = new EventHandler(this.OnEmbeddedControlLostFocusInternal);
            
            this.RegisterEmbeddedControlInternal(1, new EmbeddedTextBox(this, this.MarginsWidth));
            this.RegisterEmbeddedControlInternal(2, new EmbeddedComboBox(this, this.MarginsWidth, ComboBoxStyle.DropDown));
            this.RegisterEmbeddedControlInternal(3, new EmbeddedComboBox(this, this.MarginsWidth, ComboBoxStyle.DropDownList));
            this.RegisterEmbeddedControlInternal(4, new EmbeddedSpinBox(this, this.MarginsWidth));
            
            this.OnFontChanged(EventArgs.Empty);
            
            this.m_autoScrollTimer.Interval = 0x4b;
            this.m_autoScrollTimer.Tick += new EventHandler(this.AutoscrollTimerProcessor);
            
            this.m_linkFont = new Font(this.Font, FontStyle.Underline | this.Font.Style);
        }
        #endregion

        #region Methods
        public void AddColumn(GridColumnInfo ci)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new AddColumnInvoker(this.AddColumnInternal), new object[] { ci });
            }
            else
            {
                this.AddColumnInternal(ci);
            }
        }

        protected virtual void AddColumnInternal(GridColumnInfo ci)
        {
            this.InsertColumnInternal(this.m_Columns.Count, ci);
        }

        protected virtual BlockOfCellsCollection AdjustColumnIndexesInSelectedCells(BlockOfCellsCollection originalCol, bool bFromUIToStorage)
        {
            if (((this.m_selMgr.SelectionType == GridSelectionType.CellBlocks) || (this.m_selMgr.SelectionType == GridSelectionType.ColumnBlocks)) || ((this.m_selMgr.SelectionType == GridSelectionType.RowBlocks) || (this.m_selMgr.SelectionType == GridSelectionType.SingleRow)))
            {
                return originalCol;
            }
            if ((originalCol == null) || (originalCol.Count == 0))
            {
                return originalCol;
            }
            BlockOfCellsCollection cellss = new BlockOfCellsCollection();
            BlockOfCells node = null;
            foreach (BlockOfCells cells2 in originalCol)
            {
                if (bFromUIToStorage)
                {
                    node = new BlockOfCells(cells2.Y, this.m_Columns[cells2.X].ColumnIndex);
                }
                else
                {
                    node = new BlockOfCells(cells2.Y, this.GetUIColumnIndexByStorageIndex(cells2.X));
                }
                cellss.Add(node);
            }
            return cellss;
        }

        private void AdjustEditingCellHorizontally(ref Rectangle rEditingCellRect, int nEditingCol)
        {
            rEditingCellRect.X -= ScrollManager.GRID_LINE_WIDTH;
            rEditingCellRect.Width += ScrollManager.GRID_LINE_WIDTH;
            if ((this.HasNonScrollableColumns && (nEditingCol >= this.m_scrollMgr.FirstScrollableColumnIndex)) && (rEditingCellRect.X < this.m_scrollableArea.X))
            {
                rEditingCellRect.Width -= (this.m_scrollableArea.X - rEditingCellRect.X) - ScrollManager.GRID_LINE_WIDTH;
                rEditingCellRect.X = this.m_scrollableArea.X - ScrollManager.GRID_LINE_WIDTH;
            }
            if (rEditingCellRect.X < -ScrollManager.GRID_LINE_WIDTH)
            {
                rEditingCellRect.Width -= -rEditingCellRect.X - ScrollManager.GRID_LINE_WIDTH;
                rEditingCellRect.X = 0;
            }
            if (rEditingCellRect.Right > this.m_scrollableArea.Right)
            {
                rEditingCellRect.Width -= rEditingCellRect.Right - this.m_scrollableArea.Right;
            }
        }

        protected virtual bool AdjustSelectionForButtonCellMouseClick()
        {
            this.m_selMgr.Clear();
            this.m_selMgr.StartNewBlock(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex);
            this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            return true;
        }

        protected virtual GridBitmapColumn AllocateBitmapColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            return new GridBitmapColumn(ci, nWidthInPixels, colIndex);
        }

        protected virtual GridButtonColumn AllocateButtonColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            return new GridButtonColumn(ci, nWidthInPixels, colIndex);
        }

        protected virtual GridCheckBoxColumn AllocateCheckBoxColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            return new GridCheckBoxColumn(ci, nWidthInPixels, colIndex);
        }

        protected virtual GridHyperlinkColumn AllocateHyperlinkColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            GridHyperlinkColumn column = new GridHyperlinkColumn(ci, nWidthInPixels, colIndex);
            LinkLabel label = new LinkLabel();
            if (column.TextBrush != null)
            {
                column.TextBrush.Dispose();
                column.TextBrush = null;
            }
            column.TextBrush = new SolidBrush(label.LinkColor);
            return column;
        }

        protected virtual GridTextColumn AllocateTextColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            return new GridTextColumn(ci, nWidthInPixels, colIndex);
        }

        protected virtual GridLineNumberColumn AllocateLineNumberColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            return new GridLineNumberColumn(ci, nWidthInPixels, colIndex);
        }

        protected virtual GridColumn AllocateCustomColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        private GridColumn AllocateColumn(int colType, GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            switch (colType)
            {
                case GridColumnType.Text:
                    return this.AllocateTextColumn(ci, nWidthInPixels, colIndex);

                case GridColumnType.Button:
                    return this.AllocateButtonColumn(ci, nWidthInPixels, colIndex);

                case GridColumnType.Bitmap:
                    return this.AllocateBitmapColumn(ci, nWidthInPixels, colIndex);

                case GridColumnType.Checkbox:
                    return this.AllocateCheckBoxColumn(ci, nWidthInPixels, colIndex);

                case GridColumnType.Hyperlink:
                    return this.AllocateHyperlinkColumn(ci, nWidthInPixels, colIndex);

                case GridColumnType.LineNumber:
                    return this.AllocateLineNumberColumn(ci, nWidthInPixels, colIndex);
            }
            return this.AllocateCustomColumn(ci, nWidthInPixels, colIndex);
        }

        protected virtual GridPrinter AllocateGridPrinter()
        {
            return new GridPrinter(this);
        }

        protected virtual void AlwaysHighlightSelectionInt(bool bAlwaysHighlight)
        {
            if (this.m_alwaysHighlightSelection != bAlwaysHighlight)
            {
                this.m_alwaysHighlightSelection = bAlwaysHighlight;
                if (this.m_selMgr.SelectedBlocks.Count > 0)
                {
                    base.Invalidate();
                }
            }
        }

        private void AutoscrollTimerProcessor(object myObject, EventArgs myEventArgs)
        {
            Point mousePosition = Control.MousePosition;
            mousePosition = base.PointToClient(mousePosition);
            if (!this.m_scrollableArea.Contains(mousePosition))
            {
                int yDelta = -1;
                NativeMethods.RECT scrollRect = new NativeMethods.RECT(0, 0, 0, 0);
                HitTestInfo info = this.HitTestInternal(mousePosition.X, mousePosition.Y);
                long rowIndex = info.RowIndex;
                int columnIndex = info.ColumnIndex;
                if (info.HitTestResult != HitTestResult.ColumnOnly)
                {
                    columnIndex = this.m_captureTracker.LastColumnIndex;
                }
                if (info.HitTestResult != HitTestResult.RowOnly)
                {
                    rowIndex = this.m_captureTracker.LastRowIndex;
                }
                if (mousePosition.Y < this.m_scrollableArea.Y)
                {
                    this.m_scrollMgr.HandleVScrollWithoutClientRedraw(2, ref yDelta, ref scrollRect);
                    rowIndex = this.m_scrollMgr.FirstRowIndex;
                }
                else if (mousePosition.Y >= this.m_scrollableArea.Bottom)
                {
                    this.m_scrollMgr.HandleVScrollWithoutClientRedraw(3, ref yDelta, ref scrollRect);
                    rowIndex = this.m_scrollMgr.LastRowIndex;
                }
                if (mousePosition.X < this.m_scrollableArea.X)
                {
                    this.m_scrollMgr.HandleHScrollWithoutClientRedraw(2, ref yDelta, ref scrollRect);
                    columnIndex = this.m_scrollMgr.FirstColumnIndex;
                }
                else if (mousePosition.X >= this.m_scrollableArea.Right)
                {
                    this.m_scrollMgr.HandleHScrollWithoutClientRedraw(3, ref yDelta, ref scrollRect);
                    columnIndex = this.m_scrollMgr.LastColumnIndex;
                }
                this.UpdateSelectionBlockFromMouse(rowIndex, columnIndex);
            }
            else
            {
                if (this.m_autoScrollTimer.Enabled)
                {
                    this.m_autoScrollTimer.Stop();
                }
                this.HandleStdCellLBtnMouseMove(mousePosition.X, mousePosition.Y);
            }
        }

        public virtual void BeginInit()
        {
            lock (this)
            {
                this.m_nIsInitializingCount++;
            }
        }

        private int CalcNonScrollableColumnsWidth()
        {
            return this.CalcNonScrollableColumnsWidth(this.m_scrollMgr.FirstScrollableColumnIndex - 1);
        }

        private int CalcNonScrollableColumnsWidth(int lastIndex)
        {
            int num = 0;
            if ((lastIndex >= 0) && (lastIndex < this.m_Columns.Count))
            {
                num = ScrollManager.GRID_LINE_WIDTH;
                for (int i = 0; i <= lastIndex; i++)
                {
                    num += this.m_Columns[i].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
                }
            }
            return num;
        }

        protected virtual int CalculateHeaderHeight(Font headerFont)
        {
            int num;
            int num2;
            this.GetFontInfo(headerFont, out num2, out num);
            return (num2 + GridButton.ButtonAdditionalHeight);
        }

        protected virtual int CalcValidColWidth(int X)
        {
            int minWidthDuringColResize = (X - this.m_captureTracker.CellRect.Left) - this.m_captureTracker.MouseOffsetForColResize;
            if (minWidthDuringColResize < this.m_captureTracker.MinWidthDuringColResize)
            {
                minWidthDuringColResize = this.m_captureTracker.MinWidthDuringColResize;
            }
            if (minWidthDuringColResize > maximumColumnWidth)
            {
                minWidthDuringColResize = maximumColumnWidth;
            }
            return minWidthDuringColResize;
        }

        private void CancelEditCell()
        {
            IGridEmbeddedControl curEmbeddedControl = (IGridEmbeddedControl) this.m_curEmbeddedControl;
            if (curEmbeddedControl == null)
            {
                throw new InvalidOperationException(SRError.CurControlIsNotIGridEmbedded);
            }
            curEmbeddedControl.ContentsChanged -= this.m_OnEmbeddedControlContentsChangedDelegate;
            this.OnStoppedCellEdit();
            this.m_curEmbeddedControl.Visible = false;
            this.m_curEmbeddedControl = null;
            base.Focus();
        }

        private bool CheckAndProcessCurrentEditingCellForKeyboard()
        {
            if (this.IsEditing)
            {
                if (!this.m_curEmbeddedControl.ContainsFocus)
                {
                    this.CancelEditCell();
                }
                else if (!this.StopEditCell())
                {
                    return false;
                }
            }
            return true;
        }

        protected void CheckAndRePositionEmbeddedControlForSmallSizes()
        {
            long num;
            int num2;
            if (this.IsACellBeingEdited(out num, out num2))
            {
                Rectangle cellRectangle = this.m_scrollMgr.GetCellRectangle(num, num2);
                if (this.m_scrollableArea.Height <= this.m_scrollMgr.CellHeight)
                {
                    if (cellRectangle == Rectangle.Empty)
                    {
                        if (this.m_curEmbeddedControl.Visible)
                        {
                            base.Focus();
                            this.m_curEmbeddedControl.Visible = false;
                        }
                    }
                    else if (cellRectangle.Width >= this.m_scrollableArea.Width)
                    {
                        this.PositionEmbeddedEditor(cellRectangle, num2);
                    }
                }
            }
        }

        private void CompleteArrowsNatigation(bool bStartEditNewCurrentCell, System.Windows.Forms.Keys keyPressed, System.Windows.Forms.Keys modifiers)
        {
            if (bStartEditNewCurrentCell)
            {
                if (this.StartEditCell(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn, this.m_gridStorage.IsCellEditable(this.m_selMgr.CurrentRow, this.m_Columns[this.m_selMgr.CurrentColumn].ColumnIndex), this.FocusEditorOnNavigation) && this.m_curEmbeddedControl.ContainsFocus)
                {
                    this.NotifyControlAboutFocusFromKeyboard(keyPressed, modifiers);
                }
                this.m_selMgr.StartNewBlock(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn);
            }
            this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            this.Refresh();
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if ((base.AccessibleName == null) || (base.AccessibleName == ""))
            {
                base.AccessibleName = SR.GridControlAaName;
            }
            base.AccessibleRole = AccessibleRole.Table;
            return new GridControlAccessibleObject(this);
        }

        public void DeleteColumn(int nIndex)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new DeleteColumnInvoker(this.DeleteColumnInternal), new object[] { nIndex });
            }
            else
            {
                this.DeleteColumnInternal(nIndex);
            }
        }

        protected virtual void DeleteColumnInternal(int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= this.m_Columns.Count))
            {
                throw new ArgumentOutOfRangeException("nIndex", nIndex, "");
            }
            if ((this.m_scrollMgr.FirstScrollableColumnIndex == (this.m_Columns.Count - 1)) && (this.m_Columns.Count != 1))
            {
                throw new ArgumentException(SRError.FirstScrollableWillBeBad, "nIndex");
            }
            if (this.IsEditing)
            {
                this.CancelEditCell();
            }
            using (GridColumn column = this.m_Columns[nIndex])
            {
                this.m_Columns.RemoveAtAndAdjust(nIndex);
                this.m_scrollMgr.ProcessDeleteCol(nIndex, column.WidthInPixels);
            }
            this.m_gridHeader.DeleteItem(nIndex);
            if (base.IsHandleCreated && !this.IsInitializing)
            {
                this.Refresh();
            }
        }

        protected virtual void DoCellPrinting(Graphics g, SolidBrush bkBrush, SolidBrush textBrush, Font textFont, Rectangle cellRect, GridColumn gridColumn, long rowNumber)
        {
            gridColumn.PrintCell(g, bkBrush, textBrush, textFont, cellRect, this.m_gridStorage, rowNumber);
        }

        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                if (this.m_backBrush != null)
                {
                    this.m_backBrush.Dispose();
                    this.m_backBrush = null;
                }
                this.DisposeCachedGDIObjects();
                if (this.m_gridHeader != null)
                {
                    this.m_gridHeader.Dispose();
                    this.m_gridHeader = null;
                }
                if (this.m_autoScrollTimer != null)
                {
                    this.m_autoScrollTimer.Dispose();
                    this.m_autoScrollTimer = null;
                }
                if (this.m_linkFont != null)
                {
                    this.m_linkFont.Dispose();
                    this.m_linkFont = null;
                }
                if (this.m_gridTooltip != null)
                {
                    this.m_gridTooltip.Dispose();
                    this.m_gridTooltip = null;
                }
                for (int i = 0; i < this.m_Columns.Count; i++)
                {
                    this.m_Columns[i].Dispose();
                }
                if (this.m_EmbeddedControls != null)
                {
                    foreach (DictionaryEntry entry in this.m_EmbeddedControls)
                    {
                        if (entry.Value is IDisposable)
                        {
                            (entry.Value as IDisposable).Dispose();
                        }
                    }
                    this.m_EmbeddedControls.Clear();
                    this.m_EmbeddedControls = null;
                }
            }
            base.Dispose(bDisposing);
        }

        public virtual void EndInit()
        {
            lock (this)
            {
                this.m_nIsInitializingCount--;
                if ((this.m_nIsInitializingCount == 0) && base.IsHandleCreated)
                {
                    this.UpdateScrollableAreaRect();
                }
            }
        }

        public void EnsureCellIsVisible(long nRowIndex, int nColIndex)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            if (base.InvokeRequired)
            {
                base.BeginInvoke(new EnsureCellIsVisibleInvoker(this.EnsureCellIsVisibleInternal), new object[] { nRowIndex, uIColumnIndexByStorageIndex });
            }
            else
            {
                this.EnsureCellIsVisibleInternal(nRowIndex, uIColumnIndexByStorageIndex);
            }
        }

        protected virtual void EnsureCellIsVisibleInternal(long nRowIndex, int nColIndex)
        {
            if ((nRowIndex < 0L) || (nRowIndex >= this.NumRowsInt))
            {
                throw new ArgumentOutOfRangeException("nRowIndex", nRowIndex, SRError.RowIndexShouldBeInRange);
            }
            if ((nColIndex < 0) || (nColIndex >= this.m_Columns.Count))
            {
                throw new ArgumentOutOfRangeException("nColIndex", nColIndex, SRError.ColumnIndexShouldBeInRange);
            }
            if (((nRowIndex < this.m_scrollMgr.FirstRowIndex) || (nRowIndex > this.m_scrollMgr.LastRowIndex)) || ((nColIndex < this.m_scrollMgr.FirstColumnIndex) || (nColIndex > this.m_scrollMgr.LastColumnIndex)))
            {
                this.m_scrollMgr.EnsureCellIsVisible(nRowIndex, nColIndex);
            }
        }

        private void ForwardKeyStrokeToControl(KeyEventArgs ke)
        {
            IGridEmbeddedControlManagement2 curEmbeddedControl = this.m_curEmbeddedControl as IGridEmbeddedControlManagement2;
            if (curEmbeddedControl != null)
            {
                curEmbeddedControl.ReceiveKeyboardEvent(ke);
            }
        }

        protected override AccessibleObject GetAccessibilityObjectById(int objectId)
        {
            return base.AccessibilityObject.GetChild(objectId - 1);
        }

        protected ButtonCellState GetButtonCellState(long nRowIndex, int nColIndex)
        {
            ButtonCellState state;
            Bitmap image = null;
            string buttonLabel = null;
            this.m_gridStorage.GetCellDataForButton(nRowIndex, this.m_Columns[nColIndex].ColumnIndex, out state, out image, out buttonLabel);
            return state;
        }

        protected virtual Font GetCellFont(long rowIndex, GridColumn gridColumn)
        {
            if (gridColumn.ColumnType == 5)
            {
                return this.m_linkFont;
            }
            return this.Font;
        }

        protected virtual void GetCellGDIObjects(GridColumn gridColumn, long nRow, int nCol, ref SolidBrush bkBrush, ref SolidBrush textBrush)
        {
            textBrush = gridColumn.TextBrush;
            bkBrush = gridColumn.BackgroundBrush;

            if (this.CustomizeCellGDIObjects != null)
            {
                this.m_CustomizeCellGDIObjectsArgs.SetRowAndColumn(nRow, this.m_Columns[nCol].ColumnIndex);
                this.m_CustomizeCellGDIObjectsArgs.TextBrush = textBrush;
                this.m_CustomizeCellGDIObjectsArgs.BKBrush = bkBrush;
                this.CustomizeCellGDIObjects(this, this.m_CustomizeCellGDIObjectsArgs);
                textBrush = this.m_CustomizeCellGDIObjectsArgs.TextBrush;
                bkBrush = this.m_CustomizeCellGDIObjectsArgs.BKBrush;
            }

            if (gridColumn.WithSelectionBk && this.m_selMgr.IsCellSelected(nRow, nCol))
            {
                if (base.ContainsFocus)
                {
                    bkBrush = this.m_highlightBrush;
                }
                else if (this.m_alwaysHighlightSelection)
                {
                    bkBrush = this.highlightNonFocusedBrush;
                }
            }
        }

        protected virtual string GetCellStringForResizeToShowAll(long rowIndex, int storageColIndex, out StringFormat sf)
        {
            sf = new StringFormat(StringFormatFlags.LineLimit | StringFormatFlags.NoWrap);
            sf.HotkeyPrefix = HotkeyPrefix.None;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            sf.LineAlignment = StringAlignment.Center;
            return this.m_gridStorage.GetCellDataAsString(rowIndex, storageColIndex);
        }

        protected virtual string GetCellStringForResizeToShowAll(long rowIndex, int storageColIndex, out TextFormatFlags tff)
        {
            tff = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
            return this.m_gridStorage.GetCellDataAsString(rowIndex, storageColIndex);
        }

        private string GetClipboardTextForCells(long nStartRow, long nEndRow, int nStartCol, int nEndCol, string columnsSeparator)
        {
            if (this.m_gridStorage == null)
            {
                throw new InvalidOperationException();
            }
            StringBuilder clipboardText = new StringBuilder(0x100);
            if (!this.OnBeforeGetClipboardTextForCells(clipboardText, nStartRow, nEndRow, nStartCol, nEndCol))
            {
                Bitmap image = null;
                string buttonLabel = null;
                ButtonCellState empty = ButtonCellState.Empty;
                for (long i = nStartRow; i <= nEndRow; i += 1L)
                {
                    for (int j = nStartCol; j <= nEndCol; j++)
                    {
                        if ((this.m_Columns[j].ColumnType == 1) || (this.m_Columns[j].ColumnType == 5) || (this.m_Columns[j].ColumnType == 6))
                        {
                            clipboardText.Append(this.GetTextBasedColumnStringForClipboardText(i, j));
                        }
                        else if (this.m_Columns[j].ColumnType == 3)
                        {
                            clipboardText.Append(this.StringForBitmapData);
                        }
                        else if (this.m_Columns[j].ColumnType == 2)
                        {
                            this.m_gridStorage.GetCellDataForButton(i, this.m_Columns[j].ColumnIndex, out empty, out image, out buttonLabel);
                            if (buttonLabel != null)
                            {
                                clipboardText.Append(buttonLabel);
                            }
                            else
                            {
                                clipboardText.Append(this.StringForButtonsWithBmpOnly);
                            }
                        }
                        else if (this.m_Columns[j].ColumnType == 4)
                        {
                            clipboardText.Append(this.m_gridStorage.GetCellDataForCheckBox(i, this.m_Columns[j].ColumnIndex).ToString());
                        }
                        else
                        {
                            clipboardText.Append(this.GetCustomColumnStringForClipboardText(i, j));
                        }
                        if (j < nEndCol)
                        {
                            clipboardText.Append(this.ColumnsSeparator);
                        }
                    }
                    if (i < nEndRow)
                    {
                        clipboardText.Append(this.NewLineCharacters);
                    }
                }
            }
            return clipboardText.ToString();
        }

        private string GetClipboardTextForSelectionBlock(int nBlockNum, string columnsSeparator)
        {
            long y;
            long bottom;
            int x;
            int right;
            if ((this.m_selMgr.SelectionType == GridSelectionType.ColumnBlocks) || (this.m_selMgr.SelectionType == GridSelectionType.SingleColumn))
            {
                y = 0L;
                bottom = this.NumRowsInt - 1L;
                x = this.m_selMgr.SelectedBlocks[nBlockNum].X;
                right = this.m_selMgr.SelectedBlocks[nBlockNum].Right;
            }
            else if ((this.m_selMgr.SelectionType == GridSelectionType.RowBlocks) || (this.m_selMgr.SelectionType == GridSelectionType.SingleRow))
            {
                x = 0;
                right = this.NumColInt - 1;
                y = this.m_selMgr.SelectedBlocks[nBlockNum].Y;
                bottom = this.m_selMgr.SelectedBlocks[nBlockNum].Bottom;
            }
            else
            {
                x = this.m_selMgr.SelectedBlocks[nBlockNum].X;
                right = this.m_selMgr.SelectedBlocks[nBlockNum].Right;
                y = this.m_selMgr.SelectedBlocks[nBlockNum].Y;
                bottom = this.m_selMgr.SelectedBlocks[nBlockNum].Bottom;
            }
            if (((right < x) || (bottom < y)) || ((x < 0) || (y < 0L)))
            {
                throw new InvalidOperationException(SRError.InvalidGridStateForClipboard);
            }
            return this.GetClipboardTextForCells(y, bottom, x, right, columnsSeparator);
        }

        private void GetColumnsNumberInternalForInvoke(InvokerInOutArgs a)
        {
            a.InOutParam = this.m_Columns.Count;
        }

        public int GetColumnWidth(int nColIndex)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            if (base.InvokeRequired)
            {
                InvokerInOutArgs args = new InvokerInOutArgs();
                base.Invoke(new GetColumnWidthInternalInvoker(this.GetColumnWidthInternalForInvoke), new object[] { uIColumnIndexByStorageIndex, args });
                return (int) args.InOutParam;
            }
            return this.GetColumnWidthInternal(uIColumnIndexByStorageIndex);
        }

        private int GetColumnWidthInPixels(int nWidth, GridColumnWidthType widthType)
        {
            int num = nWidth;
            if (widthType == GridColumnWidthType.InAverageFontChar)
            {
                num *= this.m_cAvCharWidth;
            }
            return num;
        }

        protected virtual int GetColumnWidthInternal(int nColIndex)
        {
            if ((nColIndex < 0) || (nColIndex >= this.NumColInt))
            {
                throw new ArgumentOutOfRangeException("nColIndex", nColIndex, "");
            }
            return this.m_Columns[nColIndex].WidthInPixels;
        }

        private void GetColumnWidthInternalForInvoke(int nColIndex, InvokerInOutArgs args)
        {
            args.InOutParam = this.GetColumnWidthInternal(nColIndex);
        }

        public void GetCurrentCell(out long rowIndex, out int columnIndex)
        {
            rowIndex = this.m_selMgr.CurrentRow;
            columnIndex = this.m_selMgr.CurrentColumn;
        }

        protected virtual string GetCustomColumnStringForClipboardText(long rowIndex, int colIndex)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        public DataObject GetDataObject(bool bOnlyCurrentSelBlock, string columnsSeparator = null)
        {
            if (base.InvokeRequired)
            {
                throw new InvalidOperationException(SRError.InvalidThreadForMethod);
            }
            return this.GetDataObjectInternal(bOnlyCurrentSelBlock, string.IsNullOrEmpty(columnsSeparator) ? this.ColumnsSeparator : columnsSeparator);
        }

        protected virtual DataObject GetDataObjectInternal(bool bOnlyCurrentSelBlock, string columnsSeparator)
        {
            if (this.m_selMgr.SelectedBlocks.Count <= 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(0x100);
            if (bOnlyCurrentSelBlock)
            {
                if (this.m_selMgr.CurrentSelectionBlockIndex < 0)
                {
                    throw new InvalidOperationException(SRError.InvalidCurrentSelBlockForClicpboard);
                }
                builder.Append(this.GetClipboardTextForSelectionBlock(this.m_selMgr.CurrentSelectionBlockIndex, columnsSeparator));
            }
            else
            {
                int count = this.m_selMgr.SelectedBlocks.Count;
                for (int i = 0; i < count; i++)
                {
                    builder.Append(this.GetClipboardTextForSelectionBlock(i, columnsSeparator));
                    if (i < (count - 1))
                    {
                        builder.Append(this.NewLineCharacters);
                    }
                }
            }
            DataObject obj2 = new DataObject();
            obj2.SetData(DataFormats.UnicodeText, true, builder.ToString());
            return obj2;
        }

        private int GetFirstNonMergedToTheLeft(int colIndex)
        {
            int num = colIndex;
            while (num > 0)
            {
                if (!this.m_gridHeader[num - 1].MergedWithRight)
                {
                    return num;
                }
                num--;
            }
            return num;
        }

        private void GetFontInfo(Font font, out int height, out int aveWidth)
        {
            IntPtr nullIntPtr = NativeMethods.NullIntPtr;
            if (base.IsHandleCreated)
            {
                nullIntPtr = base.Handle;
            }
            using (Graphics graphics = Graphics.FromHwnd(nullIntPtr))
            {
                height = (int) Math.Round((double) font.GetHeight(graphics));
                aveWidth = (int) Math.Round((double) (((double) TextRenderer.MeasureText(graphics, "The quick brown fox jumped over the lazy dog.", font).Width) / 44.549996948242189));
            }
        }

        public GridColumnInfo GetGridColumnInfo(int columnStorageIndex)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(columnStorageIndex);
            GridColumnInfo info = new GridColumnInfo();
            info.BackgroundColor = this.m_Columns[uIColumnIndexByStorageIndex].BackgroundBrush.Color;
            info.TextColor = this.m_Columns[uIColumnIndexByStorageIndex].TextBrush.Color;
            info.ColumnAlignment = this.m_Columns[uIColumnIndexByStorageIndex].TextAlign;
            info.ColumnType = this.m_Columns[uIColumnIndexByStorageIndex].ColumnType;
            info.ColumnWidth = this.m_Columns[uIColumnIndexByStorageIndex].WidthInPixels;
            info.HeaderAlignment = this.m_gridHeader[uIColumnIndexByStorageIndex].Align;
            info.HeaderType = this.m_gridHeader[uIColumnIndexByStorageIndex].Type;
            info.IsHeaderClickable = this.m_gridHeader[uIColumnIndexByStorageIndex].Clickable;
            info.IsHeaderMergedWithRight = this.m_gridHeader[uIColumnIndexByStorageIndex].MergedWithRight;
            info.IsUserResizable = this.m_gridHeader[uIColumnIndexByStorageIndex].Resizable;
            info.IsWithRightGridLine = this.m_Columns[uIColumnIndexByStorageIndex].RightGridLine;
            info.IsWithSelectionBackground = this.m_Columns[uIColumnIndexByStorageIndex].WithSelectionBk;
            info.MergedHeaderResizeProportion = this.m_gridHeader[uIColumnIndexByStorageIndex].MergedHeaderResizeProportion;
            info.TextBmpCellsLayout = this.m_Columns[uIColumnIndexByStorageIndex].TextBitmapLayout;
            info.TextBmpHeaderLayout = this.m_gridHeader[uIColumnIndexByStorageIndex].TextBmpLayout;
            info.WidthType = GridColumnWidthType.InPixels;
            return info;
        }

        public void GetHeaderInfo(int colIndex, out string headerText, out GridCheckBoxState headerCheckBox)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(colIndex);
            if ((this.m_gridHeader[uIColumnIndexByStorageIndex].Type != GridColumnHeaderType.CheckBox) && (this.m_gridHeader[uIColumnIndexByStorageIndex].Type != GridColumnHeaderType.TextAndCheckBox))
            {
                throw new InvalidOperationException(SRError.ShouldSetHeaderStateForRegualrCol(colIndex));
            }
            headerCheckBox = (GridCheckBoxState) this.GetHeaderInfoCommon(uIColumnIndexByStorageIndex, false, out headerText);
        }

        public void GetHeaderInfo(int colIndex, out string headerText, out Bitmap headerBitmap)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(colIndex);
            if ((this.m_gridHeader[uIColumnIndexByStorageIndex].Type == GridColumnHeaderType.CheckBox) || (this.m_gridHeader[uIColumnIndexByStorageIndex].Type == GridColumnHeaderType.TextAndCheckBox))
            {
                throw new InvalidOperationException(SRError.ShouldSetHeaderStateForCheckBox(colIndex));
            }
            headerBitmap = this.GetHeaderInfoCommon(uIColumnIndexByStorageIndex, true, out headerText) as Bitmap;
        }

        private object GetHeaderInfoCommon(int colIndex, bool bitmapHeader, out string headerText)
        {
            Bitmap bitmap;
            GridCheckBoxState state;
            if (base.InvokeRequired)
            {
                InvokerInOutArgs args = new InvokerInOutArgs();
                base.Invoke(new GetHeaderInfoInvoker(this.GetHeaderInfoInternalForInvoke), new object[] { colIndex, args });
                headerText = null;
                if (args.InOutParam != null)
                {
                    headerText = args.InOutParam as string;
                }
                if (!bitmapHeader)
                {
                    return args.InOutParam3;
                }
                if (args.InOutParam2 == null)
                {
                    return null;
                }
                return args.InOutParam2;
            }
            this.GetHeaderInfoInternal(colIndex, out headerText, out bitmap, out state);
            if (bitmapHeader)
            {
                return bitmap;
            }
            return state;
        }

        protected virtual void GetHeaderInfoInternal(int colIndex, out string headerText, out Bitmap bmp, out GridCheckBoxState checkBoxState)
        {
            if ((colIndex < 0) || (colIndex >= this.m_Columns.Count))
            {
                throw new ArgumentOutOfRangeException("colIndex", colIndex, "");
            }
            if ((this.m_gridHeader[colIndex].Type == GridColumnHeaderType.CheckBox) || (this.m_gridHeader[colIndex].Type == GridColumnHeaderType.TextAndCheckBox))
            {
                bmp = null;
                checkBoxState = this.m_gridHeader[colIndex].CheckboxState;
            }
            else
            {
                bmp = this.m_gridHeader[colIndex].Bmp;
                checkBoxState = GridCheckBoxState.None;
            }
            headerText = this.m_gridHeader[colIndex].Text;
        }

        private void GetHeaderInfoInternalForInvoke(int colIndex, InvokerInOutArgs outArgs)
        {
            string str;
            Bitmap bitmap;
            GridCheckBoxState state;
            this.GetHeaderInfoInternal(colIndex, out str, out bitmap, out state);
            outArgs.InOutParam = str;
            outArgs.InOutParam2 = bitmap;
            outArgs.InOutParam3 = state;
        }

        protected virtual int GetMinWidthOfColumn(int colIndex)
        {
            return Math.Min(this.m_cAvCharWidth, this.m_Columns[colIndex].WidthInPixels);
        }

        protected Control GetRegisteredEmbeddedControl(int editableCellType)
        {
            return (Control) this.m_EmbeddedControls[editableCellType];
        }

        private SolidBrush GetSeeThroghBkBrush(System.Drawing.Color systemBaseColor)
        {
            System.Drawing.Color color = this.ColorFromWin32(5);
            System.Drawing.Color color2 = System.Drawing.Color.FromArgb(((13 * color.R) + (systemBaseColor.R * 7)) / 20, ((13 * color.G) + (systemBaseColor.G * 7)) / 20, ((13 * color.B) + (systemBaseColor.B * 7)) / 20);
            if ((((Math.Abs((int) (color2.R - systemBaseColor.R)) * 2) + (Math.Abs((int) (color2.G - systemBaseColor.G)) * 5)) + Math.Abs((int) (color2.B - systemBaseColor.B))) < ((color == System.Drawing.Color.Black) ? 0x49 : 0x85))
            {
                if (((((color2.R * 2) + (color2.G * 5)) + color2.B) / 8) >= 0x80)
                {
                    color2 = System.Drawing.Color.FromArgb((color2.R * 13) / 20, (color2.G * 13) / 20, (color2.B * 13) / 20);
                }
                else
                {
                    color2 = System.Drawing.Color.FromArgb(color2.R + ((5 * (0xff - color2.R)) / 20), color2.G + ((5 * (0xff - color2.G)) / 20), color2.B + ((5 * (0xff - color2.B)) / 20));
                }
            }
            return new SolidBrush(color2);
        }

        public int GetStorageColumnIndexByUIIndex(int indexInUI)
        {
            if (indexInUI < 0)
            {
                throw new ArgumentException();
            }
            return this.m_Columns[indexInUI].ColumnIndex;
        }

        protected virtual string GetTextBasedColumnStringForClipboardText(long rowIndex, int colIndex)
        {
            return this.m_gridStorage.GetCellDataAsString(rowIndex, this.m_Columns[colIndex].ColumnIndex);
        }

        public int GetUIColumnIndexByStorageIndex(int indexInStorage)
        {
            if (indexInStorage < 0)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < this.m_Columns.Count; i++)
            {
                if (this.m_Columns[i].ColumnIndex == indexInStorage)
                {
                    return i;
                }
            }
            return -1;
        }

        public Rectangle GetVisibleCellRectangle(long rowIndex, int columnIndex)
        {
            return this.m_scrollMgr.GetCellRectangle(rowIndex, columnIndex);
        }

        protected Graphics GraphicsFromHandle()
        {
            return Graphics.FromHwnd(base.Handle);
        }

        protected bool HandleButtonLBtnDown()
        {
            if ((this.m_captureTracker.RowIndex >= 0L) && !this.OnMouseButtonClicking(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, Control.ModifierKeys, MouseButtons.Left))
            {
                return false;
            }
            this.m_captureTracker.AdjustedCellRect = this.m_captureTracker.CellRect;
            int num = this.CalcNonScrollableColumnsWidth();
            if (((num > 0) && (this.m_captureTracker.ColumnIndex >= this.m_scrollMgr.FirstScrollableColumnIndex)) && (this.m_captureTracker.CellRect.X <= num))
            {
                int right = this.m_captureTracker.AdjustedCellRect.Right;
                int x = num + ScrollManager.GRID_LINE_WIDTH;
                this.m_captureTracker.UpdateAdjustedRectHorizontally(x, right - x);
            }
            this.m_captureTracker.WasOverButton = true;
            if (this.m_captureTracker.RowIndex == -1L)
            {
                this.m_captureTracker.DragState = CaptureTracker.DragOperation.DragReady;
                bool clickable = this.m_gridHeader[this.m_captureTracker.ColumnIndex].Clickable;
                if (clickable)
                {
                    this.m_gridHeader[this.m_captureTracker.ColumnIndex].Pushed = true;
                }
                this.m_captureTracker.ButtonArea = this.HitTestGridButton(-1L, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, this.m_captureTracker.MouseCapturePoint);
                if (clickable)
                {
                    this.RefreshHeader(-1);
                }
            }
            else
            {
                ButtonCellState buttonCellState = this.GetButtonCellState(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex);
                switch (buttonCellState)
                {
                    case ButtonCellState.Empty:
                    case ButtonCellState.Disabled:
                        return false;
                }
                this.m_captureTracker.ButtonWasPushed = buttonCellState == ButtonCellState.Pushed;
                this.DrawOneButtonCell(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, true);
            }
            return true;
        }

        protected void HandleButtonLBtnUp(int X, int Y)
        {
            bool clickable = this.m_gridHeader[this.m_captureTracker.ColumnIndex].Clickable;
            if (this.m_captureTracker.RowIndex == -1L)
            {
                if (clickable)
                {
                    this.m_gridHeader[this.m_captureTracker.ColumnIndex].Pushed = false;
                }
                if (this.m_captureTracker.DragImageOperation != null)
                {
                    this.m_captureTracker.DragImageOperation = null;
                    if (((this.m_captureTracker.ColIndexToDragColAfter != CaptureTracker.NoColIndexToDragColAfter) && (this.m_captureTracker.ColIndexToDragColAfter != this.m_captureTracker.ColumnIndex)) && (this.m_captureTracker.ColIndexToDragColAfter != (this.m_captureTracker.ColumnIndex - 1)))
                    {
                        int colIndexToDragColAfter = this.m_captureTracker.ColIndexToDragColAfter;
                        if (colIndexToDragColAfter < this.m_captureTracker.ColumnIndex)
                        {
                            colIndexToDragColAfter++;
                        }
                        this.OnColumnWasReordered(this.m_captureTracker.ColumnIndex, colIndexToDragColAfter);
                        return;
                    }
                    this.RefreshHeader(-1);
                    return;
                }
            }
            if (this.m_captureTracker.AdjustedCellRect.Contains(X, Y))
            {
                if (this.m_captureTracker.RowIndex == -1L)
                {
                    if (clickable)
                    {
                        GridButtonArea headerArea = this.HitTestGridButton(-1L, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, new Point(X, Y));
                        if (headerArea != this.m_captureTracker.ButtonArea)
                        {
                            headerArea = GridButtonArea.Nothing;
                        }
                        if (this.OnHeaderButtonClicked(this.m_captureTracker.ColumnIndex, MouseButtons.Left, headerArea))
                        {
                            this.Refresh();
                        }
                        else
                        {
                            this.RefreshHeader(-1);
                        }
                    }
                }
                else
                {
                    bool flag3 = false;
                    if (((this.m_selMgr.SelectionType == GridSelectionType.SingleCell) || (this.m_selMgr.SelectionType == GridSelectionType.CellBlocks)) && !this.IsEditing)
                    {
                        flag3 = this.AdjustSelectionForButtonCellMouseClick();
                    }
                    this.OnMouseButtonClicked(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.AdjustedCellRect, MouseButtons.Left);
                    if (flag3)
                    {
                        this.Refresh();
                        return;
                    }
                }
            }
            if (this.m_captureTracker.RowIndex != -1L)
            {
                ButtonCellState buttonCellState = this.GetButtonCellState(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex);
                this.DrawOneButtonCell(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, buttonCellState == ButtonCellState.Pushed);
            }
            else if (clickable)
            {
                this.RefreshHeader(-1);
            }
        }

        protected void HandleButtonMouseMove(int X, int Y)
        {
            if (this.m_captureTracker.DragImageOperation == null)
            {
                if (((this.m_captureTracker.RowIndex != -1L) || (this.m_captureTracker.DragState != CaptureTracker.DragOperation.DragReady)) || !this.HandleHeaderButtonMouseMove(X, Y))
                {
                    if (!this.m_captureTracker.AdjustedCellRect.Contains(X, Y))
                    {
                        if (this.m_captureTracker.WasOverButton)
                        {
                            if (this.m_captureTracker.RowIndex == -1L)
                            {
                                if (this.m_gridHeader[this.m_captureTracker.ColumnIndex].Clickable)
                                {
                                    this.m_gridHeader[this.m_captureTracker.ColumnIndex].Pushed = false;
                                    this.RefreshHeader(-1);
                                }
                            }
                            else
                            {
                                this.DrawOneButtonCell(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.ButtonWasPushed);
                            }
                            this.m_captureTracker.WasOverButton = false;
                        }
                    }
                    else if (!this.m_captureTracker.WasOverButton)
                    {
                        if (this.m_captureTracker.RowIndex == -1L)
                        {
                            if (this.m_gridHeader[this.m_captureTracker.ColumnIndex].Clickable)
                            {
                                this.m_gridHeader[this.m_captureTracker.ColumnIndex].Pushed = true;
                                this.RefreshHeader(-1);
                            }
                        }
                        else
                        {
                            this.DrawOneButtonCell(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, true);
                        }
                        this.m_captureTracker.WasOverButton = true;
                    }
                }
            }
            else
            {
                this.HandleButtonMouseMoveWhileDraggingHeader(X, Y);
            }
        }

        protected void HandleButtonMouseMoveWhileDraggingHeader(int X, int Y)
        {
            bool flag = false;
            int xPosForInsertionMark = -1;
            if ((X < this.m_scrollableArea.X) || (X >= this.m_scrollableArea.Right))
            {
                if (this.m_captureTracker.ColIndexToDragColAfter != CaptureTracker.NoColIndexToDragColAfter)
                {
                    this.m_captureTracker.ColIndexToDragColAfter = CaptureTracker.NoColIndexToDragColAfter;
                    flag = true;
                }
            }
            else
            {
                HitTestInfo info = this.HitTestInternal(X, 2);
                if ((info.HitTestResult != HitTestResult.ColumnResize) && (info.HitTestResult != HitTestResult.HeaderButton))
                {
                    if (this.m_captureTracker.ColIndexToDragColAfter != CaptureTracker.NoColIndexToDragColAfter)
                    {
                        this.m_captureTracker.ColIndexToDragColAfter = CaptureTracker.NoColIndexToDragColAfter;
                        flag = true;
                    }
                }
                else
                {
                    int columnIndex = info.ColumnIndex;
                    if (X < ((info.AreaRectangle.Left + info.AreaRectangle.Right) / 2))
                    {
                        columnIndex--;
                        xPosForInsertionMark = info.AreaRectangle.Left;
                    }
                    else
                    {
                        xPosForInsertionMark = info.AreaRectangle.Right;
                    }
                    if (this.HasNonScrollableColumns && (xPosForInsertionMark < this.m_scrollableArea.X))
                    {
                        xPosForInsertionMark = -1;
                    }
                    if (columnIndex != this.m_captureTracker.ColIndexToDragColAfter)
                    {
                        if ((columnIndex == (this.NumColInt - 1)) && (xPosForInsertionMark != -1))
                        {
                            xPosForInsertionMark -= 2;
                        }
                        this.m_captureTracker.ColIndexToDragColAfter = columnIndex;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                GridDragImageList.DragShowNolock(false);
                this.RefreshHeader(xPosForInsertionMark);
                GridDragImageList.DragShowNolock(true);
            }
            GridDragImageList.DragMove(new Point(X, this.m_captureTracker.HeaderDragY));
        }

        protected void HandleColResizeLBtnDown()
        {
            int columnIndex = this.m_captureTracker.ColumnIndex;
            this.m_captureTracker.LastColumnWidth = this.m_Columns[columnIndex].WidthInPixels;
            this.m_captureTracker.OrigColumnWidth = this.m_captureTracker.CellRect.Width;
            this.m_captureTracker.MouseOffsetForColResize = (this.m_captureTracker.MouseCapturePoint.X - this.m_captureTracker.CellRect.Left) - this.m_captureTracker.OrigColumnWidth;
            this.m_captureTracker.MinWidthDuringColResize = this.GetMinWidthOfColumn(columnIndex);
            this.m_gridHeader[columnIndex].MergedHeaderResizeProportion = 1f;
            this.m_Columns[columnIndex].OrigWidthInPixelsDuringResize = this.m_Columns[columnIndex].WidthInPixels;
            this.m_captureTracker.LeftMostMergedColumnIndex = columnIndex;
            for (int i = columnIndex - 1; i >= 0; i--)
            {
                if (!this.m_gridHeader[i].MergedWithRight)
                {
                    break;
                }
                this.m_Columns[i].OrigWidthInPixelsDuringResize = this.m_Columns[i].WidthInPixels;
                this.m_captureTracker.LeftMostMergedColumnIndex = i;
                this.m_captureTracker.MinWidthDuringColResize += this.GetMinWidthOfColumn(i) + ScrollManager.GRID_LINE_WIDTH;
            }
            this.m_captureTracker.TotalGridLineAdjDuringResize = ScrollManager.GRID_LINE_WIDTH * (columnIndex - this.m_captureTracker.LeftMostMergedColumnIndex);
        }

        protected void HandleColResizeLBtnUp(int X, int Y)
        {
            this.HandleColResizeMouseMove(X, Y, true);
            int nNewColWidth = 0;
            for (int i = this.m_captureTracker.LeftMostMergedColumnIndex; i <= this.m_captureTracker.ColumnIndex; i++)
            {
                nNewColWidth += this.m_Columns[i].WidthInPixels;
            }
            nNewColWidth += ScrollManager.GRID_LINE_WIDTH * (this.m_captureTracker.ColumnIndex - this.m_captureTracker.LeftMostMergedColumnIndex);
            if (nNewColWidth != this.m_captureTracker.OrigColumnWidth)
            {
                this.OnColumnWidthChanged(this.m_captureTracker.ColumnIndex, nNewColWidth);
            }
            if (this.m_captureTracker.WasEmbeddedControlFocused)
            {
                this.SetFocusToEmbeddedControl();
            }
        }

        protected void HandleColResizeMouseMove(int X, int Y, bool bLastUpdate)
        {
            int num = this.CalcValidColWidth(X);
            int sizeDelta = num - this.m_captureTracker.OrigColumnWidth;
            this.m_captureTracker.LastColumnWidth = num;
            this.ResizeMultipleColumns(this.m_captureTracker.LeftMostMergedColumnIndex, this.m_captureTracker.ColumnIndex, sizeDelta, bLastUpdate, null);
            bool flag = false;
            if (this.m_captureTracker.ColumnIndex < this.m_scrollMgr.FirstScrollableColumnIndex)
            {
                flag = this.ProcessNonScrollableVerticalAreaChange(false);
            }
            if (flag)
            {
                this.m_scrollMgr.RecalcAll(this.m_scrollableArea);
            }
            else
            {
                this.Refresh();
            }
        }

        protected virtual void HandleCustomCellDoubleClick(System.Windows.Forms.Keys modKeys, MouseButtons btn)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        protected virtual bool HandleCustomCellMouseBtnDown(System.Windows.Forms.Keys modKeys, MouseButtons btn)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        protected virtual void HandleCustomCellMouseBtnUp(int X, int Y, MouseButtons btn)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        protected virtual void HandleCustomCellMouseMove(int X, int Y, MouseButtons btn)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        protected bool HandleHeaderButtonMouseMove(int X, int Y)
        {
            int num = SystemInformation.DragSize.Width / 2;
            int num2 = SystemInformation.DragSize.Height / 2;
            if ((Math.Abs((int) (X - this.m_captureTracker.MouseCapturePoint.X)) <= num) && (Math.Abs((int) (Y - this.m_captureTracker.MouseCapturePoint.Y)) <= num2))
            {
                return false;
            }
            bool flag = this.m_captureTracker.ColumnIndex >= this.m_scrollMgr.FirstScrollableColumnIndex;
            if (flag)
            {
                flag = this.IsColumnHeaderDraggable(this.m_captureTracker.ColumnIndex);
            }
            if (!flag)
            {
                this.m_captureTracker.DragState = CaptureTracker.DragOperation.None;
                return false;
            }
            Size borderSize = new Size(0, 0);
            if (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
            {
                borderSize = SystemInformation.Border3DSize;
            }
            else if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
            {
                borderSize = SystemInformation.BorderSize;
            }
            using (Bitmap bitmap = new Bitmap(this.m_captureTracker.CellRect.Width, this.m_captureTracker.CellRect.Height, Graphics.FromHwnd(base.Handle)))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    int columnIndex = this.m_captureTracker.ColumnIndex;
                    while ((columnIndex > 0) && this.m_gridHeader[columnIndex - 1].MergedWithRight)
                    {
                        columnIndex--;
                    }
                    this.PaintHeaderHelper(graphics, columnIndex, this.m_captureTracker.ColumnIndex, 0, 0);
                    GridDragImageList dil = new GridDragImageList(bitmap.Width, bitmap.Height);
                    dil.Add(bitmap, SystemColors.Control);
                    this.m_captureTracker.HeaderDragY = this.m_captureTracker.MouseCapturePoint.Y - this.m_captureTracker.CellRect.Top;
                    this.m_captureTracker.DragImageOperation = new GridDragImageListOperation(dil, new Point((this.m_captureTracker.MouseCapturePoint.X - this.m_captureTracker.CellRect.Left) - borderSize.Width, this.m_captureTracker.HeaderDragY - borderSize.Height), base.Handle, this.m_captureTracker.MouseCapturePoint, true);
                }
            }
            this.m_captureTracker.DragState = CaptureTracker.DragOperation.StartedDrag;
            return true;
        }

        private bool HandleHyperlinkLBtnDown()
        {
            using (Graphics graphics = this.GraphicsFromHandle())
            {
                return this.m_Columns[this.m_captureTracker.ColumnIndex].IsPointOverTextInCell(this.m_captureTracker.MouseCapturePoint, this.m_captureTracker.CellRect, this.m_gridStorage, this.m_captureTracker.RowIndex, graphics, this.m_linkFont);
            }
        }

        private void HandleHyperlinkLBtnUp(int currentX, int currentY)
        {
            DateTime now = DateTime.Now;
            this.m_captureTracker.HyperLinkSelectionTimer.Stop();
            Point pt = new Point(currentX, currentY);
            bool flag = false;
            
            using (Graphics graphics = this.GraphicsFromHandle())
            {
                flag = this.m_Columns[this.m_captureTracker.ColumnIndex].IsPointOverTextInCell(pt, this.m_captureTracker.CellRect, this.m_gridStorage, this.m_captureTracker.RowIndex, graphics, this.m_linkFont);
            }
            
            if (flag)
            {
                TimeSpan span = (TimeSpan) (now - this.m_captureTracker.Time);
                if (span.TotalSeconds < hyperlinkSelectionDelay)
                {
                    this.OnGridSpecialEvent(0, null, this.m_captureTracker.CaptureHitTest, this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, MouseButtons.None, this.m_captureTracker.ButtonArea);
                    this.m_selMgr.Clear();
                    this.m_selMgr.StartNewBlock(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex);
                    this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
                    base.Invalidate();
                }
                else
                {
                    this.HandleStdCellLBtnUp(currentX, currentY);
                }
            }
            else
            {
                this.HandleStdCellLBtnUp(currentX, currentY);
            }
        }

        protected virtual bool HandleKeyboard(KeyEventArgs ke)
        {
            if (!this.ActAsEnabled)
            {
                return false;
            }
            if (this.IsEditing && this.ShouldMakeControlVisible(ke))
            {
                IGridEmbeddedControl curEmbeddedControl = (IGridEmbeddedControl) this.m_curEmbeddedControl;
                this.EnsureCellIsVisibleInternal(curEmbeddedControl.RowIndex, this.GetUIColumnIndexByStorageIndex(curEmbeddedControl.ColumnIndex));
            }
            System.Windows.Forms.Keys keyCode = ke.KeyCode;
            if (keyCode <= System.Windows.Forms.Keys.Return)
            {
                switch (keyCode)
                {
                    case System.Windows.Forms.Keys.Tab:
                        return (!ke.Control && this.ProcessLeftRightKeys(ke.Shift, System.Windows.Forms.Keys.None, true));

                    case System.Windows.Forms.Keys.Return:
                        if (!this.IsEditing)
                        {
                            return false;
                        }
                        if (this.IsEditing && this.m_curEmbeddedControl.ContainsFocus)
                        {
                            if (!this.StopEditCell())
                            {
                                return true;
                            }
                        }
                        else if (this.IsEditing && (this.m_selMgr.CurrentRow < (this.NumRowsInt - 1L)))
                        {
                            this.CancelEditCell();
                        }
                        if (this.m_selMgr.CurrentRow < (this.NumRowsInt - 1L))
                        {
                            this.ProcessUpDownKeys(true, System.Windows.Forms.Keys.None);
                        }
                        return true;
                }
            }
            else
            {
                switch (keyCode)
                {
                    case System.Windows.Forms.Keys.Escape:
                        if (!this.IsEditing)
                        {
                            return false;
                        }
                        this.CancelEditCell();
                        this.NotifyAccAboutNewSelection(false, true);
                        return true;

                    case System.Windows.Forms.Keys.IMEConvert:
                    case System.Windows.Forms.Keys.IMENonconvert:
                    case System.Windows.Forms.Keys.IMEAccept:
                    case System.Windows.Forms.Keys.IMEModeChange:
                    case System.Windows.Forms.Keys.Space:
                        break;

                    case System.Windows.Forms.Keys.Prior:
                    case System.Windows.Forms.Keys.Next:
                        return this.ProcessPageUpDownKeys(ke.KeyCode == System.Windows.Forms.Keys.Prior, ke.Modifiers);

                    case System.Windows.Forms.Keys.End:
                    case System.Windows.Forms.Keys.Home:
                        this.ProcessHomeEndKeys(ke.KeyCode == System.Windows.Forms.Keys.Home, ke.Modifiers);
                        return true;

                    case System.Windows.Forms.Keys.Left:
                    case System.Windows.Forms.Keys.Right:
                        if ((!ke.Shift || this.m_selMgr.OnlyOneSelItem) || this.IsEditing)
                        {
                            return this.ProcessLeftRightKeys(ke.KeyCode == System.Windows.Forms.Keys.Left, ke.Modifiers, false);
                        }
                        this.ProcessLeftRightUpDownKeysForBlockSel(ke.KeyCode);
                        return true;

                    case System.Windows.Forms.Keys.Up:
                    case System.Windows.Forms.Keys.Down:
                        if (!ke.Alt)
                        {
                            if ((ke.Shift && !this.m_selMgr.OnlyOneSelItem) && !this.IsEditing)
                            {
                                this.ProcessLeftRightUpDownKeysForBlockSel(ke.KeyCode);
                                return true;
                            }
                            return this.ProcessUpDownKeys(ke.KeyCode == System.Windows.Forms.Keys.Down, ke.Modifiers);
                        }
                        return false;

                    default:
                        if (keyCode != System.Windows.Forms.Keys.F2)
                        {
                            break;
                        }
                        if (!this.IsEditing && this.m_selMgr.OnlyOneCellSelected)
                        {
                            int editType = this.m_gridStorage.IsCellEditable(this.m_selMgr.CurrentRow, this.m_Columns[this.m_selMgr.CurrentColumn].ColumnIndex);
                            if (editType != 0)
                            {
                                long currentRow = this.m_selMgr.CurrentRow;
                                int currentColumn = this.m_selMgr.CurrentColumn;
                                this.m_selMgr.Clear();
                                this.StartEditCell(currentRow, currentColumn, editType, true);
                                this.m_selMgr.StartNewBlock(currentRow, currentColumn);
                                this.ForwardKeyStrokeToControl(ke);
                            }
                        }
                        return false;
                }
            }
            if ((this.m_selMgr.CurrentColumn >= 0) && (this.m_selMgr.CurrentRow >= 0L))
            {
                this.OnKeyPressedOnCell(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn, ke.KeyCode, ke.Modifiers);
            }
            return false;
        }

        private bool HandleStartCellEditFromStdCellLBtnDown(int editType, long nRowIndex, int nColIndex, bool bNotifySelChange)
        {
            bool bSendMouseClick = false;
            if (!this.StartEditCell(nRowIndex, nColIndex, this.m_captureTracker.CellRect, editType, ref bSendMouseClick))
            {
                return false;
            }
            if (bSendMouseClick)
            {
                this.SendMouseClickToEmbeddedControl();
            }
            this.m_selMgr.StartNewBlock(nRowIndex, nColIndex);
            if (!this.OnMouseButtonClicked(nRowIndex, nColIndex, this.m_captureTracker.CellRect, MouseButtons.Left))
            {
                this.Refresh();
            }
            if (bNotifySelChange)
            {
                this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            }
            return true;
        }

        protected bool HandleStdCellLBtnDown(System.Windows.Forms.Keys modKeys)
        {
            long rowIndex = this.m_captureTracker.RowIndex;
            int columnIndex = this.m_captureTracker.ColumnIndex;
            if (!this.OnMouseButtonClicking(rowIndex, columnIndex, this.m_captureTracker.CellRect, modKeys, MouseButtons.Left))
            {
                return false;
            }
            int selecttionBlockNumberForCell = this.m_selMgr.GetSelecttionBlockNumberForCell(rowIndex, columnIndex);
            long nRowIndex = -1L;
            int nColIndex = -1;
            if (this.IsEditing)
            {
                nRowIndex = this.m_selMgr.CurrentRow;
                nColIndex = this.m_selMgr.CurrentColumn;
                if (!this.StopEditCell())
                {
                    return false;
                }
            }
            if (rowIndex >= this.m_gridStorage.NumRows())
            {
                return false;
            }
            int editType = 0;
            bool bShouldStartEditing = false;
            bool bDragCanceled = false;
            bool bNotifySelChange = this.ProcessSelAndEditingForLeftClickOnCell(modKeys, rowIndex, columnIndex, out editType, out bShouldStartEditing, out bDragCanceled);
            bool startEditing = false;
            if (bShouldStartEditing)
            {
                startEditing = !this.HandleStartCellEditFromStdCellLBtnDown(editType, rowIndex, columnIndex, bNotifySelChange);
                if (!startEditing)
                {
                    return false;
                }
            }
            if (startEditing)
            {
                if ((nRowIndex >= 0L) && (nColIndex >= 0))
                {
                    this.m_selMgr.Clear();
                    this.m_selMgr.StartNewBlock(nRowIndex, nColIndex);
                }
                return false;
            }
            bool startNewBlock = true;
            if ((modKeys & System.Windows.Forms.Keys.Shift) != System.Windows.Forms.Keys.None)
            {
                this.m_selMgr.UpdateCurrentBlock(rowIndex, columnIndex);
            }
            else if ((modKeys & System.Windows.Forms.Keys.Control) != System.Windows.Forms.Keys.None)
            {
                if (this.m_selMgr.IsCellSelected(rowIndex, columnIndex) && this.m_selMgr.SingleRowOrColumnSelectedInMultiSelectionMode)
                {
                    startNewBlock = false;
                }
                else
                {
                    startNewBlock = this.m_selMgr.StartNewBlockOrExcludeCell(rowIndex, columnIndex);
                    if (!startNewBlock)
                    {
                        bNotifySelChange = true;
                    }
                }
            }
            else
            {
                startNewBlock = true;
                if ((((this.m_selMgr.OnlyOneSelItem && (selecttionBlockNumberForCell == -1)) || (!this.m_selMgr.OnlyOneSelItem && (columnIndex < this.m_scrollMgr.FirstScrollableColumnIndex))) || (!this.m_selMgr.OnlyOneSelItem && (rowIndex < this.m_scrollMgr.FirstScrollableRowIndex))) && !bDragCanceled)
                {
                    bNotifySelChange = true;
                    if (selecttionBlockNumberForCell == -1)
                    {
                        selecttionBlockNumberForCell = 0;
                        this.m_selMgr.StartNewBlock(rowIndex, columnIndex);
                    }
                }
                if ((selecttionBlockNumberForCell != -1) && !bDragCanceled)
                {
                    this.m_captureTracker.SelectionBlockIndex = selecttionBlockNumberForCell;
                    this.m_captureTracker.DragState = CaptureTracker.DragOperation.DragReady;
                }
                else
                {
                    this.m_selMgr.StartNewBlock(rowIndex, columnIndex);
                    if (bDragCanceled)
                    {
                        this.m_captureTracker.DragState = CaptureTracker.DragOperation.None;
                        if (this.m_selMgr.OnlyOneSelItem)
                        {
                            startNewBlock = false;
                        }
                    }
                }
            }
            if (!startNewBlock)
            {
                if (!this.OnMouseButtonClicked(rowIndex, columnIndex, this.m_captureTracker.CellRect, MouseButtons.Left))
                {
                    this.Refresh();
                }
            }
            else
            {
                this.m_captureTracker.LastColumnIndex = columnIndex;
                this.m_captureTracker.LastRowIndex = rowIndex;
                this.Refresh();
            }
            if (bNotifySelChange)
            {
                this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            }
            return startNewBlock;
        }

        protected void HandleStdCellLBtnMouseMove(int nCurrentMouseX, int nCurrentMouseY)
        {
            if (this.m_captureTracker.DragState == CaptureTracker.DragOperation.DragReady)
            {
                int num = SystemInformation.DragSize.Width / 2;
                int num2 = SystemInformation.DragSize.Height / 2;
                if ((Math.Abs((int) (nCurrentMouseX - this.m_captureTracker.MouseCapturePoint.X)) > num) || (Math.Abs((int) (nCurrentMouseY - this.m_captureTracker.MouseCapturePoint.Y)) > num2))
                {
                    this.m_captureTracker.DragState = CaptureTracker.DragOperation.StartedDrag;
                    try
                    {
                        this.OnStartCellDragOperation();
                    }
                    catch (Exception)
                    {
                        this.HandleStdCellLBtnUp(nCurrentMouseX, nCurrentMouseY);
                    }
                    return;
                }
            }
            if (((this.m_captureTracker.ColumnIndex >= this.m_scrollMgr.FirstScrollableColumnIndex) && !this.m_selMgr.OnlyOneSelItem) && (this.m_captureTracker.DragState != CaptureTracker.DragOperation.DragReady))
            {
                if (!this.m_scrollableArea.Contains(nCurrentMouseX, nCurrentMouseY))
                {
                    if (!this.m_autoScrollTimer.Enabled)
                    {
                        this.m_autoScrollTimer.Start();
                    }
                }
                else
                {
                    HitTestInfo info = this.HitTestInternal(nCurrentMouseX, nCurrentMouseY);
                    if ((((info.HitTestResult == HitTestResult.ButtonCell) || (info.HitTestResult == HitTestResult.CustomCell)) || (((info.HitTestResult == HitTestResult.BitmapCell) || (info.HitTestResult == HitTestResult.TextCell)) || (info.HitTestResult == HitTestResult.HyperlinkCell))) && !this.InHyperlinkTimer)
                    {
                        this.UpdateSelectionBlockFromMouse(info.RowIndex, info.ColumnIndex);
                    }
                }
            }
        }

        protected void HandleStdCellLBtnUp(int nCurrentMouseX, int nCurrentMouseY)
        {
            if (this.m_autoScrollTimer.Enabled)
            {
                this.m_autoScrollTimer.Stop();
            }
            if (this.m_captureTracker.DragState != CaptureTracker.DragOperation.StartedDrag)
            {
                HitTestInfo info = this.HitTestInternal(nCurrentMouseX, nCurrentMouseY);
                if ((info.RowIndex == this.m_captureTracker.RowIndex) && (info.ColumnIndex == this.m_captureTracker.ColumnIndex))
                {
                    bool flag = false;
                    if (this.m_captureTracker.DragState == CaptureTracker.DragOperation.DragReady)
                    {
                        int editType = this.m_gridStorage.IsCellEditable(info.RowIndex, this.m_Columns[info.ColumnIndex].ColumnIndex);
                        switch (this.m_Columns[info.ColumnIndex].ColumnType)
                        {
                            case 2:
                            case 4:
                                editType = 0;
                                break;
                        }
                        if ((this.m_selMgr.SelectionType != GridSelectionType.SingleCell) || (editType != 0))
                        {
                            this.m_selMgr.Clear();
                            if (editType != 0)
                            {
                                this.HandleStartCellEditFromStdCellLBtnDown(editType, info.RowIndex, info.ColumnIndex, false);
                            }
                            else
                            {
                                this.m_selMgr.StartNewBlock(info.RowIndex, info.ColumnIndex);
                            }
                            flag = true;
                        }
                    }
                    if (!this.OnMouseButtonClicked(info.RowIndex, info.ColumnIndex, this.m_captureTracker.CellRect, MouseButtons.Left) || flag)
                    {
                        this.Refresh();
                    }
                }
                if (!this.m_selMgr.OnlyOneSelItem)
                {
                    this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
                }
            }
        }

        protected virtual bool HandleTabOnLastOrFirstCell(bool goingLeft)
        {
            return true;
        }

        public HitTestInfo HitTest(int mouseX, int mouseY)
        {
            HitTestInfo info = this.HitTestInternal(mouseX, mouseY);
            if (info.ColumnIndex >= 0)
            {
                return new HitTestInfo(info.HitTestResult, info.RowIndex, this.m_Columns[info.ColumnIndex].ColumnIndex, info.AreaRectangle);
            }
            return info;
        }

        private HitTestResult HitTestColumns(int nMouseX, int nMouseY, out int nColIndex, ref Rectangle rCellRect)
        {
            int num = this.CalcNonScrollableColumnsWidth();
            if (this.HasNonScrollableColumns && (nMouseX <= num))
            {
                return this.HitTestColumnsHelper(nMouseX, nMouseY, 0, this.m_scrollMgr.FirstScrollableColumnIndex - 1, ScrollManager.GRID_LINE_WIDTH, out nColIndex, ref rCellRect);
            }
            return this.HitTestColumnsHelper(nMouseX, nMouseY, this.m_scrollMgr.FirstColumnIndex, this.m_scrollMgr.LastColumnIndex, this.m_scrollMgr.FirstColumnPos, out nColIndex, ref rCellRect);
        }

        private HitTestResult HitTestColumnsHelper(int nMouseX, int nMouseY, int nFirstCol, int nLastCol, int nPos, out int nColIndex, ref Rectangle rCellRect)
        {
            HitTestResult nothing = HitTestResult.Nothing;
            int widthInPixels = 0;
            int num2 = 0;
            GridColumn column = null;
            nColIndex = -1;
            for (int i = nFirstCol; i <= nLastCol; i++)
            {
                column = this.m_Columns[i];
                num2 = widthInPixels;
                widthInPixels = column.WidthInPixels;
                if ((nMouseY < this.HeaderHeight) && (nMouseY >= 0))
                {
                    if (((nMouseX <= (nPos + GridColumn.CELL_CONTENT_OFFSET)) && (nMouseX >= nPos)) && ((nMouseX > (GridColumn.CELL_CONTENT_OFFSET + ScrollManager.GRID_LINE_WIDTH)) || ((nPos > ScrollManager.GRID_LINE_WIDTH) && (i > 0))))
                    {
                        nColIndex = i - 1;
                        if (!this.m_gridHeader[nColIndex].Resizable)
                        {
                            nothing = HitTestResult.HeaderButton;
                            nColIndex = i;
                            rCellRect.X = nPos + ScrollManager.GRID_LINE_WIDTH;
                            rCellRect.Width = widthInPixels;
                        }
                        else
                        {
                            if (num2 == 0)
                            {
                                num2 = this.m_Columns[nColIndex].WidthInPixels;
                            }
                            rCellRect.X = nPos - num2;
                            rCellRect.Width = num2;
                            nothing = HitTestResult.ColumnResize;
                        }
                    }
                    else if ((nMouseX >= ((nPos + widthInPixels) - GridColumn.CELL_CONTENT_OFFSET)) && (nMouseX < ((nPos + widthInPixels) + GridColumn.CELL_CONTENT_OFFSET)))
                    {
                        nColIndex = i;
                        rCellRect.X = nPos + ScrollManager.GRID_LINE_WIDTH;
                        rCellRect.Width = widthInPixels;
                        if (!this.m_gridHeader[nColIndex].Resizable)
                        {
                            nothing = HitTestResult.HeaderButton;
                        }
                        else
                        {
                            nothing = HitTestResult.ColumnResize;
                        }
                    }
                    else
                    {
                        if ((nMouseX < nPos) || (nMouseX >= ((nPos + widthInPixels) + ScrollManager.GRID_LINE_WIDTH)))
                        {
                            goto Label_0169;
                        }
                        nColIndex = i;
                        nothing = HitTestResult.HeaderButton;
                    }
                    break;
                }
                if ((nMouseX >= nPos) && (nMouseX < ((nPos + widthInPixels) + ScrollManager.GRID_LINE_WIDTH)))
                {
                    nColIndex = i;
                    nothing = HitTestResult.TextCell;
                    break;
                }
            Label_0169:
                nPos += widthInPixels + ScrollManager.GRID_LINE_WIDTH;
            }
            if (nothing != HitTestResult.Nothing)
            {
                if (HitTestResult.ColumnResize != nothing)
                {
                    rCellRect.X = nPos;
                    rCellRect.Width = widthInPixels + ScrollManager.GRID_LINE_WIDTH;
                }
                if ((HitTestResult.HeaderButton != nothing) && (HitTestResult.ColumnResize != nothing))
                {
                    return nothing;
                }
                rCellRect.Y = 0;
                rCellRect.Height = this.HeaderHeight;
            }
            return nothing;
        }

        protected GridButtonArea HitTestGridButton(long rowIndex, int colIndex, Rectangle btnRect, Point ptToHitTest)
        {
            if (rowIndex >= 0L)
            {
                return GridButtonArea.Background;
            }
            GridButton headerGridButton = this.m_gridHeader.HeaderGridButton;
            GridHeader.HeaderItem item = this.m_gridHeader[colIndex];
            using (Graphics graphics = this.GraphicsFromHandle())
            {
                return headerGridButton.HitTest(graphics, ptToHitTest, btnRect, item.Text, item.Bmp, item.Align, item.TextBmpLayout);
            }
        }

        protected virtual HitTestInfo HitTestInternal(int nMouseX, int nMouseY)
        {
            int num2;
            HitTestResult customCell;
            long rowIndex = -1L;
            Rectangle empty = Rectangle.Empty;
            if (this.NumColInt == 0)
            {
                num2 = -1;
                return new HitTestInfo(HitTestResult.Nothing, rowIndex, num2, empty);
            }
            HitTestResult headerButton = this.HitTestColumns(nMouseX, nMouseY, out num2, ref empty);
            if ((headerButton != HitTestResult.ColumnResize) && (headerButton != HitTestResult.HeaderButton))
            {
                if (this.NumRowsInt <= 0L)
                {
                    return new HitTestInfo(HitTestResult.ColumnOnly, rowIndex, num2, empty);
                }
                HitTestResult result2 = this.HitTestRows(nMouseX, nMouseY, out rowIndex, ref empty);
                if (headerButton == HitTestResult.Nothing)
                {
                    if (result2 == HitTestResult.Nothing)
                    {
                        return new HitTestInfo(HitTestResult.Nothing, rowIndex, num2, empty);
                    }
                    return new HitTestInfo(HitTestResult.RowOnly, rowIndex, num2, empty);
                }
                if (result2 == HitTestResult.Nothing)
                {
                    return new HitTestInfo(HitTestResult.ColumnOnly, rowIndex, num2, empty);
                }
                GridColumn column = this.m_Columns[num2];
                switch (column.ColumnType)
                {
                    case GridColumnType.Text:
                        customCell = HitTestResult.TextCell;
                        break;

                    case GridColumnType.Bitmap:
                    case GridColumnType.Checkbox:
                        customCell = HitTestResult.BitmapCell;
                        break;

                    case GridColumnType.Button:
                    case GridColumnType.LineNumber:
                        customCell = HitTestResult.ButtonCell;
                        break;

                    case GridColumnType.Hyperlink:
                        customCell = HitTestResult.HyperlinkCell;
                        break;

                    default:
                        customCell = HitTestResult.CustomCell;
                        break;
                }
                
                return new HitTestInfo(customCell, rowIndex, num2, empty);
            }
            else
            {
                bool flag = (num2 > 0) && this.m_gridHeader[num2 - 1].MergedWithRight;
                if (this.m_gridHeader[num2].MergedWithRight || flag)
                {
                    int num3 = num2;
                    if ((num2 > 0) && flag)
                    {
                        while (num2 > 0)
                        {
                            if (!this.m_gridHeader[num2 - 1].MergedWithRight)
                            {
                                break;
                            }
                            empty.X -= this.m_Columns[num2 - 1].WidthInPixels + ScrollManager.GRID_LINE_WIDTH;
                            num2--;
                        }
                        empty.X += ScrollManager.GRID_LINE_WIDTH;
                        if ((num2 == 0) && this.HasNonScrollableColumns)
                        {
                            empty.X = 0;
                        }
                    }
                    int num4 = this.NumColInt - 1;
                    if ((this.m_scrollMgr.FirstScrollableColumnIndex > 0) && (num2 <= (this.m_scrollMgr.FirstScrollableColumnIndex - 1)))
                    {
                        num4 = Math.Min(num4, this.m_scrollMgr.FirstScrollableColumnIndex - 1);
                    }
                    empty.Width = 0;
                    while (num2 <= num4)
                    {
                        empty.Width += this.m_Columns[num2].WidthInPixels;
                        if (!this.m_gridHeader[num2].MergedWithRight || (num2 == num4))
                        {
                            break;
                        }
                        empty.Width += ScrollManager.GRID_LINE_WIDTH;
                        num2++;
                    }
                    if ((headerButton == HitTestResult.ColumnResize) && this.m_gridHeader[num3].MergedWithRight)
                    {
                        headerButton = HitTestResult.HeaderButton;
                    }
                }
                return new HitTestInfo(headerButton, rowIndex, num2, empty);
            }
        }

        private HitTestResult HitTestRows(int nMouseX, int nMouseY, out long nRowIndex, ref Rectangle rCellRect)
        {
            nRowIndex = -1L;
            long firstRowIndex = this.m_scrollMgr.FirstRowIndex;
            long lastRowIndex = this.m_scrollMgr.LastRowIndex;
            if (nMouseY < ((this.NonScrollableRowsHeight() + this.HeaderHeight) + ScrollManager.GRID_LINE_WIDTH))
            {
                nRowIndex = (nMouseY - this.HeaderHeight) / (this.m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH);
                if (nRowIndex < 0L)
                {
                    nRowIndex = 0L;
                }
                rCellRect.Y = (((int) nRowIndex) * (this.m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH)) + this.HeaderHeight;
                rCellRect.Height = this.m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH;
                return HitTestResult.TextCell;
            }
            int firstRowPos = this.m_scrollMgr.FirstRowPos;
            int cellHeight = this.m_scrollMgr.CellHeight;
            long num5 = firstRowIndex;
            while (num5 <= lastRowIndex)
            {
                if ((nMouseY >= firstRowPos) && (nMouseY < ((firstRowPos + cellHeight) + ScrollManager.GRID_LINE_WIDTH)))
                {
                    nRowIndex = num5;
                    rCellRect.Y = firstRowPos;
                    rCellRect.Height = cellHeight + ScrollManager.GRID_LINE_WIDTH;
                    break;
                }
                firstRowPos += cellHeight + ScrollManager.GRID_LINE_WIDTH;
                num5 += 1L;
            }
            if (num5 > lastRowIndex)
            {
                return HitTestResult.Nothing;
            }
            return HitTestResult.TextCell;
        }

        private void InitializeCachedGDIObjects()
        {
            this.m_colInsertionPen = new Pen(System.Drawing.Color.Red);
            if (!SystemInformation.HighContrast)
            {
                this.m_gridLinesPen = new Pen(SystemColors.Control);
                this.m_highlightBrush = this.GetSeeThroghBkBrush(this.ColorFromWin32(13));
                this.highlightNonFocusedBrush = this.GetSeeThroghBkBrush(this.ColorFromWin32(3));
            }
            else
            {
                this.m_highlightBrush = new SolidBrush(this.ColorFromWin32(13));
                this.highlightNonFocusedBrush = new SolidBrush(this.ColorFromWin32(3));
                this.m_gridLinesPen = new Pen(SystemColors.WindowFrame);
            }
            if (this.m_lineType == GridLineType.Solid)
            {
                this.m_gridLinesPen.Width = ScrollManager.GRID_LINE_WIDTH;
                this.m_gridLinesPen.DashStyle = DashStyle.Solid;
            }
        }

        public void InsertColumn(int nIndex, GridColumnInfo ci)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new InsertColumnInvoker(this.InsertColumnInternal), new object[] { nIndex, ci });
            }
            else
            {
                this.InsertColumnInternal(nIndex, ci);
            }
        }

        protected virtual void InsertColumnInternal(int nIndex, GridColumnInfo ci)
        {
            if ((nIndex < 0) || (nIndex > this.m_Columns.Count))
            {
                throw new ArgumentOutOfRangeException("nIndex", nIndex, "");
            }
            if ((ci.MergedHeaderResizeProportion < 0f) || (ci.MergedHeaderResizeProportion > 1f))
            {
                throw new ArgumentException(SRError.InvalidMergedHeaderResizeProportion, "GridColumnInfo.MergedHeaderResizeProportion");
            }
            this.ValidateColumnType(ci.ColumnType);
            int columnWidthInPixels = this.GetColumnWidthInPixels(ci.ColumnWidth, ci.WidthType);
            if (columnWidthInPixels > 0x4e20)
            {
                throw new ArgumentException(SRError.ColumnWidthShouldBeLessThanMax(0x4e20), "nWidth");
            }
            if (this.IsEditing)
            {
                this.CancelEditCell();
            }
            GridColumn node = this.AllocateColumn(ci.ColumnType, ci, columnWidthInPixels, nIndex);
            node.ProcessNewGridFont(this.Font);
            node.SetRTL(this.IsRTL);
            this.m_Columns.Insert(nIndex, node);
            this.m_gridHeader.InsertHeaderItem(nIndex, ci);
            if (this.m_nIsInitializingCount == 0)
            {
                this.m_scrollMgr.ProcessNewCol(nIndex);
                if (base.IsHandleCreated)
                {
                    this.Refresh();
                }
            }
        }

        public bool IsACellBeingEdited(out long nRowNum, out int nColNum)
        {
            if (base.InvokeRequired)
            {
                InvokerInOutArgs args = new InvokerInOutArgs();
                base.Invoke(new IsACellBeingEditedInternalInvoker(this.IsACellBeingEditedInternalForInvoke), new object[] { args });
                bool inOutParam = (bool) args.InOutParam;
                if (inOutParam)
                {
                    nRowNum = (long) args.InOutParam2;
                    nColNum = this.m_Columns[(int) args.InOutParam3].ColumnIndex;
                    return inOutParam;
                }
                nRowNum = -1L;
                nColNum = -1;
                return inOutParam;
            }
            bool flag2 = this.IsACellBeingEditedInternal(out nRowNum, out nColNum);
            if (flag2)
            {
                nColNum = this.m_Columns[nColNum].ColumnIndex;
            }
            return flag2;
        }

        protected virtual bool IsACellBeingEditedInternal(out long rowIndex, out int columnIndex)
        {
            if (this.m_curEmbeddedControl != null)
            {
                IGridEmbeddedControl curEmbeddedControl = (IGridEmbeddedControl) this.m_curEmbeddedControl;
                rowIndex = curEmbeddedControl.RowIndex;
                columnIndex = this.GetUIColumnIndexByStorageIndex(curEmbeddedControl.ColumnIndex);
                return true;
            }
            rowIndex = -1L;
            columnIndex = -1;
            return false;
        }

        private void IsACellBeingEditedInternalForInvoke(InvokerInOutArgs args)
        {
            long num;
            int num2;
            args.InOutParam = this.IsACellBeingEditedInternal(out num, out num2);
            args.InOutParam2 = num;
            args.InOutParam3 = num2;
        }

        protected virtual bool IsCellEditableFromKeyboardNav()
        {
            long currentRow = this.m_selMgr.CurrentRow;
            int currentColumn = this.m_selMgr.CurrentColumn;
            int columnType = this.m_Columns[currentColumn].ColumnType;
            return (((columnType != 2) && (columnType != 4)) && (0 != this.m_gridStorage.IsCellEditable(currentRow, this.m_Columns[currentColumn].ColumnIndex)));
        }

        public bool IsCellVisible(int column, long row)
        {
            if (column < this.FirstScrollableColumn)
            {
                if (row < this.FirstScrollableRow)
                {
                    return true;
                }
                if (row < this.m_scrollMgr.FirstRowIndex)
                {
                    return false;
                }
                if (row <= this.m_scrollMgr.LastRowIndex)
                {
                    return true;
                }
            }
            else
            {
                if (column < this.m_scrollMgr.FirstColumnIndex)
                {
                    return false;
                }
                if (column <= this.m_scrollMgr.LastColumnIndex)
                {
                    if (row < this.FirstScrollableRow)
                    {
                        return true;
                    }
                    if (row < this.m_scrollMgr.FirstRowIndex)
                    {
                        return false;
                    }
                    if (row <= this.m_scrollMgr.LastRowIndex)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool IsColumnHeaderDraggable(int colIndex)
        {
            if (this.ColumnReorderRequested == null)
            {
                return this.m_bColumnsReorderableByDefault;
            }
            ColumnReorderRequestedEventArgs a = new ColumnReorderRequestedEventArgs(this.m_Columns[colIndex].ColumnIndex, this.m_bColumnsReorderableByDefault);
            this.ColumnReorderRequested(this, a);
            return a.AllowReorder;
        }

        [UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool IsInputChar(char charCode)
        {
            return this.IsInputCharInternal(charCode);
        }

        private bool IsInputCharInternal(char charCode)
        {
            if ((charCode >= '\x0001') && (charCode <= '\x001a'))
            {
                return false;
            }
            return ((((charCode != '\b') && (charCode != '\0')) && ((charCode != '\n') && (charCode != '\r'))) && (charCode != '\x007f'));
        }

        protected virtual int MeasureWidthOfCustomColumnRows(int columnIndex, int columnType, long nFirstRow, long nLastRow, Graphics g)
        {
            return 0;
        }

        protected int MeasureWidthOfRows(int columnIndex, int columnType, long nFirstRow, long nLastRow, Graphics g)
        {
            int width = 0;
            Rectangle r = new Rectangle(0, 0, 0x186a0, 0x186a0);
            Bitmap image = null;
            TextFormatFlags defaultTextFormatFlags = GridConstants.DefaultTextFormatFlags;
            string buttonLabel = "";
            Size proposedSize = new Size(0x7fffffff, this.RowHeight);
            int nColIndex = this.m_Columns[columnIndex].ColumnIndex;
            GridCheckBoxColumn column = null;
            if (columnType == 4)
            {
                column = this.m_Columns[columnIndex] as GridCheckBoxColumn;
            }
            for (long i = nFirstRow; i <= nLastRow; i += 1L)
            {
                SizeF empty = SizeF.Empty;
                if (columnType == 2)
                {
                    int num2;
                    ButtonCellState state;
                    this.m_gridStorage.GetCellDataForButton(i, nColIndex, out state, out image, out buttonLabel);
                    empty = (SizeF) GridButton.CalculateInitialContentsRect(g, r, buttonLabel, image, HorizontalAlignment.Left, this.Font, this.IsRTL, ref defaultTextFormatFlags, out num2).Size;
                }
                else if ((columnType == 1) || (columnType == 5))
                {
                    TextFormatFlags flags2;
                    buttonLabel = this.GetCellStringForResizeToShowAll(i, nColIndex, out flags2);
                    if ((buttonLabel != null) && (buttonLabel != ""))
                    {
                        empty = (SizeF) TextRenderer.MeasureText(g, buttonLabel, this.GetCellFont(i, this.m_Columns[columnIndex]), proposedSize, flags2);
                    }
                }
                else if (columnType == 3)
                {
                    image = this.m_gridStorage.GetCellDataAsBitmap(i, nColIndex);
                    if (image != null)
                    {
                        empty = (SizeF) image.Size;
                    }
                }
                else if (columnType == 4)
                {
                    image = column.BitmapFromGridCheckBoxState(GridCheckBoxState.Unchecked);
                    if (image != null)
                    {
                        empty = (SizeF) image.Size;
                    }
                }
                if (empty.Width > width)
                {
                    width = (int) empty.Width;
                }
            }
            if (columnType != 3)
            {
                width += 2 * this.MarginsWidth;
            }
            return width;
        }

        private uint NonScrollableRowsHeight()
        {
            if (this.m_scrollMgr.FirstScrollableRowIndex == 0)
            {
                return 0;
            }
            return (uint) ((this.m_scrollMgr.CellHeight + ScrollManager.GRID_LINE_WIDTH) * this.m_scrollMgr.FirstScrollableRowIndex);
        }

        protected void NotifyAccAboutNewSelection(bool notifySelection, bool notifyFocus)
        {
            int currentColumn = this.m_selMgr.CurrentColumn;
            int childID = ((int) this.m_selMgr.CurrentRow) + (this.WithHeader ? 1 : 0);
            int objectID = (currentColumn + 2) + 1;
            if (notifySelection)
            {
                base.AccessibilityNotifyClients(AccessibleEvents.Selection, objectID, childID);
            }
            if (notifyFocus)
            {
                base.AccessibilityNotifyClients(AccessibleEvents.Focus, objectID, childID);
            }
        }

        private void NotifyControlAboutFocusFromKeyboard(System.Windows.Forms.Keys keyPressed, System.Windows.Forms.Keys modifiers)
        {
            IGridEmbeddedControlManagement2 curEmbeddedControl = this.m_curEmbeddedControl as IGridEmbeddedControlManagement2;
            if (curEmbeddedControl != null)
            {
                curEmbeddedControl.PostProcessFocusFromKeyboard(keyPressed, modifiers);
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (this.m_backBrush != null)
            {
                this.m_backBrush.Dispose();
                this.m_backBrush = null;
            }
            this.m_backBrush = new SolidBrush(this.BackColor);
            base.OnBackColorChanged(e);
        }

        protected virtual bool OnBeforeGetClipboardTextForCells(StringBuilder clipboardText, long nStartRow, long nEndRow, int nStartCol, int nEndCol)
        {
            return false;
        }

        protected virtual bool OnCanInitiateDragFromCell(long rowIndex, int colIndex)
        {
            return ((this.m_Columns[colIndex].ColumnType != 3) && (this.m_Columns[colIndex].ColumnType != 4));
        }

        protected virtual void OnColumnsReordered(int oldIndex, int newIndex)
        {
            if (this.ColumnsReordered != null)
            {
                ColumnsReorderedEventArgs a = new ColumnsReorderedEventArgs(oldIndex, newIndex);
                this.ColumnsReordered(this, a);
            }
        }

        protected virtual void OnColumnWasReordered(int nOldIndex, int nNewIndex)
        {
            if (this.IsEditing)
            {
                this.CancelEditCell();
            }
            int firstNonMergedToTheLeft = this.GetFirstNonMergedToTheLeft(nOldIndex);
            bool flag = false;
            if (firstNonMergedToTheLeft > nNewIndex)
            {
                int toIndex = this.GetFirstNonMergedToTheLeft(nNewIndex);
                flag = true;
                while (firstNonMergedToTheLeft <= nOldIndex)
                {
                    this.m_Columns.Move(firstNonMergedToTheLeft, toIndex);
                    this.m_gridHeader.Move(firstNonMergedToTheLeft, toIndex);
                    firstNonMergedToTheLeft++;
                    toIndex++;
                }
            }
            else
            {
                for (int i = (nOldIndex - firstNonMergedToTheLeft) + 1; i > 0; i--)
                {
                    this.m_Columns.Move(firstNonMergedToTheLeft, nNewIndex);
                    this.m_gridHeader.Move(firstNonMergedToTheLeft, nNewIndex);
                }
            }
            if (flag)
            {
                this.m_scrollMgr.RecalcAll(this.m_scrollableArea);
            }
            this.Refresh();
            this.OnColumnsReordered(nOldIndex, nNewIndex);
        }

        protected virtual void OnColumnWidthChanged(int nColIndex, int nNewColWidth)
        {
            if (this.ColumnWidthChanged != null)
            {
                ColumnWidthChangedEventArgs args = new ColumnWidthChangedEventArgs(this.m_Columns[nColIndex].ColumnIndex, nNewColWidth);
                this.ColumnWidthChanged(this, args);
            }
        }

        protected virtual void OnEmbeddedControlContentsChanged(IGridEmbeddedControl embeddedControl)
        {
            if (this.EmbeddedControlContentsChanged != null)
            {
                this.EmbeddedControlContentsChanged(this, new EmbeddedControlContentsChangedEventArgs(embeddedControl));
            }
        }

        private void OnEmbeddedControlContentsChangedEventHandler(object sender, EventArgs a)
        {
            this.OnEmbeddedControlContentsChanged((IGridEmbeddedControl) sender);
        }

        protected virtual void OnEmbeddedControlLostFocus()
        {
            if (this.ShouldCommitEmbeddedControlOnLostFocus && !base.ContainsFocus)
            {
                this.StopEditCell();
                if (this.m_selMgr.SelectedBlocks.Count > 0)
                {
                    base.Invalidate();
                }
            }
        }

        private void OnEmbeddedControlLostFocusInternal(object sender, EventArgs a)
        {
            this.OnEmbeddedControlLostFocus();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (!this.ActAsEnabled)
            {
                if (this.IsEditing)
                {
                    this.CancelEditCell();
                }
                this.m_hooverOverArea.Reset();
            }
            base.OnEnabledChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            int num2;
            int cAvCharWidth = this.m_cAvCharWidth;
            this.GetFontInfo(this.Font, out num2, out this.m_cAvCharWidth);
            this.m_scrollMgr.CellHeight = num2 + GridButton.ButtonAdditionalHeight;
            this.m_scrollMgr.SetHorizontalScrollUnitForArrows(this.m_cAvCharWidth);
            this.UpdateEmbeddedControlsFont();
            this.m_Columns.ProcessNewGridFont(this.Font);
            if (cAvCharWidth > 0)
            {
                int num3 = 0;
                foreach (GridColumn column in this.m_Columns)
                {
                    if (column.IsWidthInChars)
                    {
                        num3 = column.WidthInPixels / cAvCharWidth;
                        if (num3 > 0)
                        {
                            column.WidthInPixels = num3 * this.m_cAvCharWidth;
                        }
                    }
                }
            }
            if (this.m_scrollMgr.FirstScrollableColumnIndex > 0)
            {
                this.ProcessNonScrollableVerticalAreaChange(false);
            }
            else
            {
                this.UpdateScrollableAreaRect();
            }
            if (base.IsHandleCreated && (this.m_nIsInitializingCount == 0))
            {
                this.m_scrollMgr.RecalcAll(this.m_scrollableArea);
            }
            if (this.m_linkFont != null)
            {
                this.m_linkFont.Dispose();
            }
            this.m_linkFont = new Font(this.Font, FontStyle.Underline | this.Font.Style);
            base.OnFontChanged(e);
        }

        protected override void OnGotFocus(EventArgs a)
        {
            if ((!this.m_bInGridStorageCall && (this.m_selMgr.SelectedBlocks.Count > 0)) && !this.IsEditing)
            {
                base.Invalidate();
            }
            base.OnGotFocus(a);
        }

        protected void OnGridSpecialEvent(int eventType, object data, HitTestResult htResult, long rowIndex, int colIndex, Rectangle cellRect, MouseButtons mouseState, GridButtonArea headerArea)
        {
            if (this.GridSpecialEvent != null)
            {
                GridSpecialEventArgs sea = new GridSpecialEventArgs(eventType, data, htResult, rowIndex, colIndex, cellRect, mouseState, headerArea);
                this.GridSpecialEvent(this, sea);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPrefChanged);
            this.m_scrollMgr.SetGridWindowHandle(base.Handle);
            this.UpdateGridInternal(true);
            base.OnHandleCreated(e);
            base.Width--;
            base.Width++;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPrefChanged);
            base.OnHandleDestroyed(e);
        }

        protected virtual bool OnHeaderButtonClicked(int nColIndex, MouseButtons btn, GridButtonArea headerArea)
        {
            if (this.HeaderButtonClicked != null)
            {
                HeaderButtonClickedEventArgs args = new HeaderButtonClickedEventArgs(this.m_Columns[nColIndex].ColumnIndex, btn, headerArea);
                this.HeaderButtonClicked(this, args);
                return args.RepaintWholeGrid;
            }
            return false;
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            if (((ke.KeyCode == System.Windows.Forms.Keys.ShiftKey) || (ke.KeyCode == System.Windows.Forms.Keys.Menu)) || (ke.KeyCode == System.Windows.Forms.Keys.ControlKey))
            {
                if (this.InHyperlinkTimer)
                {
                    this.TransitionHyperlinkToStd(this.m_captureTracker.HyperLinkSelectionTimer, null);
                }
                if (this.Cursor == Cursors.Hand)
                {
                    this.Cursor = Cursors.Arrow;
                    this.m_gridTooltip.Active = false;
                }
            }
            if (this.OnStandardKeyProcessing(ke))
            {
                this.HandleKeyboard(ke);
                if ((this.IsEditing && !this.m_curEmbeddedControl.ContainsFocus) && this.IsCellEditableFromKeyboardNav())
                {
                    this.ForwardKeyStrokeToControl(ke);
                    if (this.m_curEmbeddedControl.ContainsFocus)
                    {
                        ke.Handled = true;
                        return;
                    }
                }
                base.OnKeyDown(ke);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.ShouldForwardCharToEmbeddedControl && this.IsInputCharInternal(e.KeyChar))
            {
                this.m_curEmbeddedControl.Focus();
                IGridEmbeddedControlManagement2 curEmbeddedControl = this.m_curEmbeddedControl as IGridEmbeddedControlManagement2;
                if (curEmbeddedControl != null)
                {
                    curEmbeddedControl.ReceiveChar(e.KeyChar);
                    e.Handled = true;
                    return;
                }
            }
            base.OnKeyPress(e);
        }

        protected virtual void OnKeyPressedOnCell(long nCurRow, int nCurCol, System.Windows.Forms.Keys key, System.Windows.Forms.Keys mod)
        {
            if (this.KeyPressedOnCell != null)
            {
                KeyPressedOnCellEventArgs args = new KeyPressedOnCellEventArgs(nCurRow, this.m_Columns[nCurCol].ColumnIndex, key, mod);
                this.KeyPressedOnCell(this, args);
            }
        }

        protected override void OnKeyUp(KeyEventArgs ke)
        {
            base.OnKeyUp(ke);
            if (((ke.KeyCode == System.Windows.Forms.Keys.ShiftKey) || (ke.KeyCode == System.Windows.Forms.Keys.Menu)) || (ke.KeyCode == System.Windows.Forms.Keys.ControlKey))
            {
                Point point = base.PointToClient(Control.MousePosition);
                MouseEventArgs mevent = new MouseEventArgs(Control.MouseButtons, 0, point.X, point.Y, 0);
                this.ProcessMouseMoveWithoutCapture(mevent);
            }
        }

        protected override void OnLostFocus(EventArgs a)
        {
            if ((this.ShouldCommitEmbeddedControlOnLostFocus && this.IsEditing) && !base.ContainsFocus)
            {
                this.StopEditCell();
            }
            if ((!this.m_bInGridStorageCall && !base.ContainsFocus) && (this.m_selMgr.SelectedBlocks.Count > 0))
            {
                base.Invalidate();
            }
            base.OnLostFocus(a);
        }

        protected virtual bool OnMouseButtonClicked(long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn)
        {
            if (this.MouseButtonClicked != null)
            {
                MouseButtonClickedEventArgs args = new MouseButtonClickedEventArgs(nRowIndex, this.m_Columns[nColIndex].ColumnIndex, rCellRect, btn);
                this.MouseButtonClicked(this, args);
                return !args.ShouldRedraw;
            }
            return false;
        }

        protected virtual bool OnMouseButtonClicking(long nRowIndex, int nColIndex, Rectangle rCellRect, System.Windows.Forms.Keys modKeys, MouseButtons btn)
        {
            if (this.MouseButtonClicking != null)
            {
                MouseButtonClickingEventArgs args = new MouseButtonClickingEventArgs(nRowIndex, this.m_Columns[nColIndex].ColumnIndex, rCellRect, modKeys, btn);
                this.MouseButtonClicking(this, args);
                return args.ShouldHandle;
            }
            return true;
        }

        protected virtual void OnMouseButtonDoubleClicked(HitTestResult htArea, long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn, GridButtonArea headerArea)
        {
            if (this.MouseButtonDoubleClicked != null)
            {
                int num = (nColIndex >= 0) ? this.m_Columns[nColIndex].ColumnIndex : nColIndex;
                MouseButtonDoubleClickedEventArgs args = new MouseButtonDoubleClickedEventArgs(htArea, nRowIndex, num, rCellRect, btn, headerArea);
                this.MouseButtonDoubleClicked(this, args);
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (!this.IsEmpty)
            {
                try
                {
                    if ((this.m_captureTracker.CaptureHitTest != HitTestResult.Nothing) && base.Capture)
                    {
                        this.ProcessLeftButtonUp(mevent.X, mevent.Y);
                    }
                    HitTestInfo htInfo = this.HitTestInternal(mevent.X, mevent.Y);
                    this.m_captureTracker.SetInfoFromHitTest(htInfo);
                    this.m_captureTracker.MouseCapturePoint = new Point(mevent.X, mevent.Y);
                    if ((mevent.Clicks == 2) && this.ActAsEnabled)
                    {
                        if (this.m_captureTracker.CaptureHitTest == HitTestResult.CustomCell)
                        {
                            this.HandleCustomCellDoubleClick(Control.ModifierKeys, mevent.Button);
                        }
                        else
                        {
                            GridButtonArea nothing = GridButtonArea.Nothing;
                            if (this.m_captureTracker.CaptureHitTest == HitTestResult.HeaderButton)
                            {
                                nothing = this.HitTestGridButton(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, this.m_captureTracker.MouseCapturePoint);
                            }
                            this.OnMouseButtonDoubleClicked(this.m_captureTracker.CaptureHitTest, this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, mevent.Button, nothing);
                        }
                        this.m_captureTracker.Reset();
                    }
                    else
                    {
                        if ((mevent.Button != MouseButtons.Left) || !this.ActAsEnabled)
                        {
                            switch (this.m_captureTracker.CaptureHitTest)
                            {
                                case HitTestResult.HeaderButton:
                                    if (this.m_gridHeader[this.m_captureTracker.ColumnIndex].Clickable && this.OnHeaderButtonClicked(this.m_captureTracker.ColumnIndex, mevent.Button, this.HitTestGridButton(-1L, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, this.m_captureTracker.MouseCapturePoint)))
                                    {
                                        this.Refresh();
                                    }
                                    break;

                                case HitTestResult.TextCell:
                                case HitTestResult.ButtonCell:
                                case HitTestResult.BitmapCell:
                                case HitTestResult.HyperlinkCell:
                                    if (!this.OnMouseButtonClicked(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex, this.m_captureTracker.CellRect, mevent.Button))
                                    {
                                        this.Refresh();
                                    }
                                    break;

                                case HitTestResult.CustomCell:
                                    this.HandleCustomCellMouseBtnDown(Control.ModifierKeys, mevent.Button);
                                    break;
                            }
                            this.m_captureTracker.Reset();
                            return;
                        }
                        switch (this.m_captureTracker.CaptureHitTest)
                        {
                            case HitTestResult.Nothing:
                            case HitTestResult.ColumnOnly:
                            case HitTestResult.RowOnly:
                                if ((this.IsEditing && this.m_captureTracker.WasEmbeddedControlFocused) && !this.StopCellEdit(true))
                                {
                                    this.m_curEmbeddedControl.Focus();
                                }
                                return;

                            case HitTestResult.ColumnResize:
                                this.HandleColResizeLBtnDown();
                                return;

                            case HitTestResult.HeaderButton:
                            case HitTestResult.ButtonCell:
                                if (!this.HandleButtonLBtnDown())
                                {
                                    this.m_captureTracker.Reset();
                                    base.Capture = false;
                                }
                                if ((this.m_captureTracker.CaptureHitTest == HitTestResult.ButtonCell) && (this.m_Columns[this.m_captureTracker.ColumnIndex] is GridButtonColumn))
                                {
                                    ((GridButtonColumn) this.m_Columns[this.m_captureTracker.ColumnIndex]).SetForcedButtonState(this.m_captureTracker.RowIndex, ButtonState.Pushed);
                                }
                                return;

                            case HitTestResult.TextCell:
                            case HitTestResult.BitmapCell:
                                if (!this.HandleStdCellLBtnDown(Control.ModifierKeys))
                                {
                                    this.m_captureTracker.Reset();
                                    base.Capture = false;
                                }
                                return;

                            case HitTestResult.HyperlinkCell:
                                if (Control.ModifierKeys == System.Windows.Forms.Keys.None)
                                {
                                    break;
                                }
                                if (!this.HandleStdCellLBtnDown(Control.ModifierKeys))
                                {
                                    this.m_captureTracker.Reset();
                                    base.Capture = false;
                                }
                                this.m_captureTracker.CaptureHitTest = HitTestResult.TextCell;
                                return;

                            case HitTestResult.CustomCell:
                                if (!this.HandleCustomCellMouseBtnDown(Control.ModifierKeys, mevent.Button))
                                {
                                    this.m_captureTracker.Reset();
                                    base.Capture = false;
                                }
                                return;
                        }
                    }
                    if (!this.HandleHyperlinkLBtnDown())
                    {
                        if (!this.HandleStdCellLBtnDown(Control.ModifierKeys))
                        {
                            this.m_captureTracker.Reset();
                            base.Capture = false;
                        }
                        this.m_captureTracker.CaptureHitTest = HitTestResult.TextCell;
                    }
                    else
                    {
                        this.m_captureTracker.Time = DateTime.Now;
                        if (this.m_captureTracker.HyperLinkSelectionTimer == null)
                        {
                            this.m_captureTracker.HyperLinkSelectionTimer = new Timer();
                            this.m_captureTracker.HyperLinkSelectionTimer.Tick += new EventHandler(this.TransitionHyperlinkToStd);
                            this.m_captureTracker.HyperLinkSelectionTimer.Interval = Convert.ToInt32((double) 500.0);
                        }
                        this.m_captureTracker.HyperLinkSelectionTimer.Start();
                    }
                }
                catch (Exception)
                {
                    this.m_captureTracker.Reset();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            base.OnMouseMove(mevent);
            try
            {
                if (!base.Capture)
                {
                    if ((mevent.Button == MouseButtons.None) && !this.IsEmpty)
                    {
                        this.ProcessMouseMoveWithoutCapture(mevent);
                    }
                }
                else if (mevent.Button == MouseButtons.Left)
                {
                    switch (this.m_captureTracker.CaptureHitTest)
                    {
                        case HitTestResult.ColumnResize:
                            this.HandleColResizeMouseMove(mevent.X, mevent.Y, false);
                            return;

                        case HitTestResult.HeaderButton:
                        case HitTestResult.ButtonCell:
                            this.HandleButtonMouseMove(mevent.X, mevent.Y);
                            return;

                        case HitTestResult.TextCell:
                        case HitTestResult.BitmapCell:
                        case HitTestResult.HyperlinkCell:
                            this.HandleStdCellLBtnMouseMove(mevent.X, mevent.Y);
                            return;

                        case HitTestResult.CustomCell:
                            this.HandleCustomCellMouseMove(mevent.X, mevent.Y, mevent.Button);
                            return;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            try
            {
                if (base.Capture)
                {
                    base.Capture = false;
                }
                if (mevent.Button == MouseButtons.Left)
                {
                    this.ProcessLeftButtonUp(mevent.X, mevent.Y);
                }
                else if (this.m_captureTracker.CaptureHitTest == HitTestResult.CustomCell)
                {
                    this.HandleCustomCellMouseBtnUp(mevent.X, mevent.Y, mevent.Button);
                }
                this.m_captureTracker.Reset();
                base.OnMouseUp(mevent);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            this.m_wheelDelta += e.Delta;
            float num = ((float) this.m_wheelDelta) / 120f;
            int num2 = (int) (SystemInformation.MouseWheelScrollLines * num);
            if (num2 != 0)
            {
                this.m_wheelDelta = 0;
                int num3 = Math.Abs(num2);
                bool flag = (Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.None;
                for (int i = 0; i < num3; i++)
                {
                    if (flag)
                    {
                        if ((this.NumRowsInt > 0L) && (this.NumRowsInt > this.m_scrollMgr.FirstScrollableRowIndex))
                        {
                            if (num2 > 0)
                            {
                                this.m_scrollMgr.HandleVScroll(0);
                            }
                            else
                            {
                                this.m_scrollMgr.HandleVScroll(1);
                            }
                        }
                    }
                    else if ((this.NumColInt > 0) && (this.NumColInt > this.m_scrollMgr.FirstScrollableColumnIndex))
                    {
                        if (num2 > 0)
                        {
                            this.m_scrollMgr.HandleHScroll(0);
                        }
                        else
                        {
                            this.m_scrollMgr.HandleHScroll(1);
                        }
                    }
                }
            }
        }

        protected virtual void OnMouseWheelInEmbeddedControl(MouseEventArgs e)
        {
            this.OnMouseWheel(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (((base.Parent != null) && (base.Parent.Font != null)) && !this.ShouldSerializeHeaderFont())
            {
                this.ResetHeaderFont();
            }
            base.OnParentChanged(e);
        }

        protected override void OnParentFontChanged(EventArgs e)
        {
            if (((base.Parent != null) && (base.Parent.Font != null)) && !this.ShouldSerializeHeaderFont())
            {
                this.ResetHeaderFont();
            }
            base.OnParentFontChanged(e);
        }

        protected virtual void OnResetFirstScrollableColumn(int prevousFirstScrollableColumn, int newFirstScrollableColumn)
        {
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            this.m_Columns.SetRTL(this.IsRTL);
            this.m_gridHeader.HeaderGridButton.RTL = this.IsRTL;
            this.UpdateEmbeddedControlsRTL();
            base.OnRightToLeftChanged(e);
        }

        protected virtual void OnSelectionChanged(BlockOfCellsCollection selectedCells)
        {
            if (!this.IsEditing)
            {
                this.NotifyAccAboutNewSelection(true, true);
            }
            if (this.SelectionChanged != null)
            {
                SelectionChangedEventArgs args = new SelectionChangedEventArgs(this.AdjustColumnIndexesInSelectedCells(selectedCells, true));
                this.SelectionChanged(this, args);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (this.m_scrollMgr.FirstScrollableColumnIndex > 0)
            {
                if (this.ProcessNonScrollableVerticalAreaChange(false))
                {
                    this.m_scrollMgr.RecalcAll(this.m_scrollableArea);
                }
            }
            else
            {
                this.UpdateScrollableAreaRect();
            }
        }

        protected virtual bool OnStandardKeyProcessing(KeyEventArgs ke)
        {
            if (this.StandardKeyProcessing != null)
            {
                StandardKeyProcessingEventArgs args = new StandardKeyProcessingEventArgs(ke);
                this.StandardKeyProcessing(this, args);
                return args.ShouldHandle;
            }
            return true;
        }

        protected virtual void OnStartCellDragOperation()
        {
            string clipboardTextForSelectionBlock = this.GetClipboardTextForSelectionBlock(this.m_captureTracker.SelectionBlockIndex, this.ColumnsSeparator);
            DataObject data = new DataObject();
            data.SetData(DataFormats.UnicodeText, true, clipboardTextForSelectionBlock);
            base.DoDragDrop(data, DragDropEffects.Copy);
        }

        protected virtual void OnStartedCellEdit()
        {
            this.m_curEmbeddedControl.LostFocus += this.m_OnEmbeddedControlLostFocusDelegate;
        }

        protected virtual void OnStoppedCellEdit()
        {
            this.m_curEmbeddedControl.LostFocus -= this.m_OnEmbeddedControlLostFocusDelegate;
        }

        protected virtual bool OnTooltipDataNeeded(HitTestResult ht, long rowNumber, int colNumber, ref string toolTipText)
        {
            if ((colNumber < 0) || (this.TooltipDataNeeded == null))
            {
                return false;
            }
            TooltipDataNeededEventArgs a = new TooltipDataNeededEventArgs(ht, rowNumber, this.m_Columns[colNumber].ColumnIndex);
            this.TooltipDataNeeded(this, a);
            if ((a.TooltipText == null) || (a.TooltipText == ""))
            {
                return false;
            }
            toolTipText = a.TooltipText;
            return true;
        }

        private void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (pref.Category == UserPreferenceCategory.Color)
            {
                this.DisposeCachedGDIObjects();
                this.InitializeCachedGDIObjects();
                if (base.IsHandleCreated)
                {
                    base.Invalidate();
                }
                LinkLabel label = new LinkLabel();
                SolidBrush brush = new SolidBrush(label.LinkColor);
                foreach (GridColumn column in this.m_Columns)
                {
                    if (column.ColumnType == 5)
                    {
                        if (column.TextBrush != null)
                        {
                            column.TextBrush.Dispose();
                            column.TextBrush = null;
                        }
                        column.TextBrush = brush.Clone() as SolidBrush;
                    }
                }
                GridConstants.RegenerateCheckBoxBitmaps();
            }
        }

        protected void PositionEmbeddedEditor(Rectangle rEditingCellRect, int nEditingCol)
        {
            this.AdjustEditingCellHorizontally(ref rEditingCellRect, nEditingCol);
            rEditingCellRect.Height = this.m_curEmbeddedControl.Bounds.Height;
            this.m_curEmbeddedControl.Bounds = rEditingCellRect;
            if (!this.m_curEmbeddedControl.Visible)
            {
                this.m_curEmbeddedControl.Visible = true;
            }
            if ((base.ContainsFocus && !this.m_curEmbeddedControl.ContainsFocus) && this.FocusEditorOnNavigation)
            {
                this.m_curEmbeddedControl.Focus();
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData)
        {
            switch ((keyData & System.Windows.Forms.Keys.KeyCode))
            {
                case System.Windows.Forms.Keys.Escape:
                case System.Windows.Forms.Keys.Prior:
                case System.Windows.Forms.Keys.Next:
                case System.Windows.Forms.Keys.End:
                case System.Windows.Forms.Keys.Home:
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Right:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.Tab:
                case System.Windows.Forms.Keys.Return:
                {
                    KeyEventArgs ke = new KeyEventArgs(keyData);
                    if (this.OnStandardKeyProcessing(ke) && this.HandleKeyboard(ke))
                    {
                        return true;
                    }
                    break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        protected virtual void ProcessForTooltip(HitTestResult ht, long nRowNumber, int nColNumber)
        {
            string str;
            if (((ht == this.m_hooverOverArea.HitTest) && (nRowNumber == this.m_hooverOverArea.RowNumber)) && ((nColNumber == this.m_hooverOverArea.ColumnNumber) && this.m_gridTooltip.Active))
            {
                if ((ht == HitTestResult.HyperlinkCell) && (Control.ModifierKeys == System.Windows.Forms.Keys.None))
                {
                    using (Graphics graphics = this.GraphicsFromHandle())
                    {
                        if (!this.m_Columns[nColNumber].IsPointOverTextInCell(base.PointToClient(Control.MousePosition), this.m_scrollMgr.GetCellRectangle(nRowNumber, nColNumber), this.m_gridStorage, nRowNumber, graphics, this.m_linkFont))
                        {
                            this.m_gridTooltip.Active = false;
                        }
                        else if (this.Cursor == Cursors.Hand)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            str = string.Empty;
            if (((ht == HitTestResult.HyperlinkCell) && (Control.ModifierKeys == System.Windows.Forms.Keys.None)) && (this.Cursor == Cursors.Hand))
            {
                try
                {
                    using (Graphics graphics2 = this.GraphicsFromHandle())
                    {
                        if (this.m_Columns[nColNumber].IsPointOverTextInCell(base.PointToClient(Control.MousePosition), this.m_scrollMgr.GetCellRectangle(nRowNumber, nColNumber), this.m_gridStorage, nRowNumber, graphics2, this.m_linkFont))
                        {
                            str = SR.ToolTipUrl(this.m_gridStorage.GetCellDataAsString(nRowNumber, this.m_Columns[nColNumber].ColumnIndex));
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            this.m_hooverOverArea.Reset();
            if (!this.OnTooltipDataNeeded(ht, nRowNumber, nColNumber, ref str) && (str == string.Empty))
            {
                if (this.m_gridTooltip.Active)
                {
                    this.m_gridTooltip.Active = false;
                }
            }
            else
            {
                if (this.m_gridTooltip.Active)
                {
                    this.m_gridTooltip.Active = false;
                }
                this.m_hooverOverArea.HitTest = ht;
                this.m_hooverOverArea.RowNumber = nRowNumber;
                this.m_hooverOverArea.ColumnNumber = nColNumber;
                this.m_gridTooltip.SetToolTip(this, str);
                this.m_gridTooltip.Active = true;
            }
        }

        protected void ProcessHomeEndKeys(bool bHome, System.Windows.Forms.Keys mod)
        {
            int numColInt;
            int num4;
            long firstScrollableRowIndex;
            long currentRow = this.m_selMgr.CurrentRow;
            int currentColumn = this.m_selMgr.CurrentColumn;
            if (mod == System.Windows.Forms.Keys.Control)
            {
                if (bHome)
                {
                    firstScrollableRowIndex = this.m_scrollMgr.FirstScrollableRowIndex;
                    num4 = numColInt = this.m_scrollMgr.FirstScrollableColumnIndex;
                }
                else
                {
                    firstScrollableRowIndex = this.NumRowsInt - 1L;
                    numColInt = this.NumColInt;
                    num4 = numColInt - 1;
                }
            }
            else
            {
                if (bHome)
                {
                    numColInt = num4 = this.m_scrollMgr.FirstScrollableColumnIndex;
                }
                else
                {
                    numColInt = this.NumColInt;
                    num4 = numColInt - 1;
                }
                firstScrollableRowIndex = this.m_selMgr.CurrentRow;
                if (firstScrollableRowIndex < 0L)
                {
                    firstScrollableRowIndex = this.m_scrollMgr.FirstScrollableRowIndex;
                }
            }
            bool flag = (currentRow != firstScrollableRowIndex) || (currentColumn != num4);
            if (flag)
            {
                if (!this.CheckAndProcessCurrentEditingCellForKeyboard())
                {
                    return;
                }
                this.m_selMgr.Clear();
            }
            this.m_scrollMgr.EnsureCellIsVisible(firstScrollableRowIndex, numColInt, true, false);
            if (flag)
            {
                int editType = this.m_gridStorage.IsCellEditable(firstScrollableRowIndex, this.m_Columns[num4].ColumnIndex);
                if (editType != 0)
                {
                    this.StartEditCell(firstScrollableRowIndex, num4, editType, this.FocusEditorOnNavigation);
                }
                this.m_selMgr.StartNewBlock(firstScrollableRowIndex, num4);
                this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            }
            this.Refresh();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (m.Msg == 0x100)
            {
                KeyEventArgs ke = new KeyEventArgs(((System.Windows.Forms.Keys) ((int) m.WParam)) | Control.ModifierKeys);
                switch (ke.KeyCode)
                {
                    case System.Windows.Forms.Keys.Escape:
                    case System.Windows.Forms.Keys.Prior:
                    case System.Windows.Forms.Keys.Next:
                    case System.Windows.Forms.Keys.End:
                    case System.Windows.Forms.Keys.Home:
                    case System.Windows.Forms.Keys.Left:
                    case System.Windows.Forms.Keys.Right:
                    case System.Windows.Forms.Keys.Tab:
                    case System.Windows.Forms.Keys.Return:
                        if (!this.OnStandardKeyProcessing(ke) || !this.HandleKeyboard(ke))
                        {
                            break;
                        }
                        return true;

                    case System.Windows.Forms.Keys.Up:
                    case System.Windows.Forms.Keys.Down:
                        if (this.OnStandardKeyProcessing(ke) && this.HandleKeyboard(ke))
                        {
                            return true;
                        }
                        break;
                }
            }
            return base.ProcessKeyPreview(ref m);
        }

        private void ProcessLeftButtonUp(int x, int y)
        {
            switch (this.m_captureTracker.CaptureHitTest)
            {
                case HitTestResult.ColumnResize:
                    this.HandleColResizeLBtnUp(x, y);
                    return;

                case HitTestResult.HeaderButton:
                case HitTestResult.ButtonCell:
                    if ((this.m_captureTracker.CaptureHitTest == HitTestResult.ButtonCell) && (this.m_Columns[this.m_captureTracker.ColumnIndex] is GridButtonColumn))
                    {
                        ((GridButtonColumn) this.m_Columns[this.m_captureTracker.ColumnIndex]).SetForcedButtonState(-1L, ButtonState.Pushed);
                    }
                    this.HandleButtonLBtnUp(x, y);
                    return;

                case HitTestResult.TextCell:
                case HitTestResult.BitmapCell:
                    this.HandleStdCellLBtnUp(x, y);
                    return;

                case HitTestResult.HyperlinkCell:
                    this.HandleHyperlinkLBtnUp(x, y);
                    return;

                case HitTestResult.CustomCell:
                    this.HandleCustomCellMouseBtnUp(x, y, MouseButtons.Left);
                    return;
            }
        }

        protected bool ProcessLeftRightKeys(bool bLeft, System.Windows.Forms.Keys mod, bool bChangeRowIfNeeded)
        {
            if (((this.m_selMgr.CurrentColumn == -1) || (this.m_selMgr.CurrentRow == -1L)) || (this.NumRowsInt <= 0L))
            {
                return false;
            }
            bool flag = (((this.m_selMgr.CurrentColumn == 0) && (this.m_selMgr.CurrentRow == 0L)) && bLeft) || (((this.m_selMgr.CurrentColumn == (this.NumColInt - 1)) && (this.m_selMgr.CurrentRow == (this.NumRowsInt - 1L))) && !bLeft);
            if (flag & bChangeRowIfNeeded)
            {
                return this.HandleTabOnLastOrFirstCell(bLeft);
            }
            if ((!this.IsEditing || flag) || this.CheckAndProcessCurrentEditingCellForKeyboard())
            {
                bool bMakeFirstColFullyVisible = bLeft;
                bool flag3 = false;
                this.m_scrollMgr.EnsureCellIsVisible(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn, bMakeFirstColFullyVisible, false);
                if (bLeft)
                {
                    this.m_selMgr.CurrentColumn--;
                    if (this.m_selMgr.CurrentColumn < 0)
                    {
                        if (bChangeRowIfNeeded && !flag)
                        {
                            this.m_selMgr.CurrentColumn = this.NumColInt - 1;
                            this.m_selMgr.CurrentRow -= 1L;
                            flag3 = true;
                        }
                        else
                        {
                            this.m_selMgr.CurrentColumn = 0;
                        }
                    }
                }
                else if (this.m_selMgr.CurrentColumn == (this.m_scrollMgr.FirstScrollableColumnIndex - 1))
                {
                    this.m_selMgr.CurrentColumn = this.m_scrollMgr.FirstScrollableColumnIndex;
                }
                else
                {
                    this.m_selMgr.CurrentColumn++;
                    if (this.m_selMgr.CurrentColumn > (this.NumColInt - 1))
                    {
                        if (bChangeRowIfNeeded && !flag)
                        {
                            this.m_selMgr.CurrentColumn = 0;
                            this.m_selMgr.CurrentRow += 1L;
                            flag3 = true;
                        }
                        else
                        {
                            this.m_selMgr.CurrentColumn = this.NumColInt - 1;
                        }
                    }
                }
                this.m_selMgr.Clear(false);
                bool bStartEditNewCurrentCell = this.IsCellEditableFromKeyboardNav();
                if (!bStartEditNewCurrentCell)
                {
                    this.m_selMgr.StartNewBlock(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn);
                }
                if ((bLeft || (!bLeft && flag3)) || (this.m_selMgr.CurrentColumn < this.m_scrollMgr.FirstColumnIndex))
                {
                    this.m_scrollMgr.EnsureCellIsVisible(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn, true, false);
                }
                else if (this.m_selMgr.CurrentColumn > this.m_scrollMgr.LastColumnIndex)
                {
                    this.m_scrollMgr.MakeNextColumnVisible(false);
                }
                this.CompleteArrowsNatigation(bStartEditNewCurrentCell, bLeft ? System.Windows.Forms.Keys.Left : System.Windows.Forms.Keys.Right, mod);
            }
            return true;
        }

        protected void ProcessLeftRightUpDownKeysForBlockSel(System.Windows.Forms.Keys key)
        {
            long lastUpdatedRow = this.m_selMgr.LastUpdatedRow;
            int lastUpdatedColumn = this.m_selMgr.LastUpdatedColumn;
            if (key == System.Windows.Forms.Keys.Up)
            {
                if (this.m_selMgr.SelectionType == GridSelectionType.ColumnBlocks)
                {
                    this.m_scrollMgr.HandleVScroll(0);
                    return;
                }
                if ((lastUpdatedRow == -1L) || (lastUpdatedRow <= 0L))
                {
                    return;
                }
                this.m_selMgr.UpdateCurrentBlock(lastUpdatedRow - 1L, lastUpdatedColumn);
            }
            else if (key == System.Windows.Forms.Keys.Down)
            {
                if (this.m_selMgr.SelectionType == GridSelectionType.ColumnBlocks)
                {
                    this.m_scrollMgr.HandleVScroll(1);
                    return;
                }
                if ((lastUpdatedRow == -1L) || (lastUpdatedRow >= (this.NumRowsInt - 1L)))
                {
                    return;
                }
                this.m_selMgr.UpdateCurrentBlock(lastUpdatedRow + 1L, lastUpdatedColumn);
            }
            else if (key == System.Windows.Forms.Keys.Left)
            {
                if (this.m_selMgr.SelectionType == GridSelectionType.RowBlocks)
                {
                    this.m_scrollMgr.HandleHScroll(0);
                    return;
                }
                if ((lastUpdatedColumn == -1) || (lastUpdatedColumn <= 0))
                {
                    return;
                }
                this.m_selMgr.UpdateCurrentBlock(lastUpdatedRow, lastUpdatedColumn - 1);
            }
            else
            {
                if (this.m_selMgr.SelectionType == GridSelectionType.RowBlocks)
                {
                    this.m_scrollMgr.HandleHScroll(1);
                    return;
                }
                if ((lastUpdatedColumn != -1) && (lastUpdatedColumn < (this.NumColInt - 1)))
                {
                    this.m_selMgr.UpdateCurrentBlock(lastUpdatedRow, lastUpdatedColumn + 1);
                }
                else
                {
                    return;
                }
            }
            this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            if (this.m_selMgr.SelectionType == GridSelectionType.CellBlocks)
            {
                this.m_scrollMgr.EnsureCellIsVisible(this.m_selMgr.LastUpdatedRow, this.m_selMgr.LastUpdatedColumn, true, false);
            }
            else if (this.m_selMgr.SelectionType == GridSelectionType.ColumnBlocks)
            {
                this.m_scrollMgr.EnsureColumnIsVisible(this.m_selMgr.LastUpdatedColumn, true, false);
            }
            else
            {
                int num3;
                this.m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(this.m_selMgr.LastUpdatedRow, true, out num3);
            }
            this.Refresh();
        }

        private void ProcessMouseMoveWithoutCapture(MouseEventArgs mevent)
        {
            HitTestInfo info = this.HitTestInternal(mevent.X, mevent.Y);
            this.SetCursorFromHitTest(info.HitTestResult, info.RowIndex, info.ColumnIndex, info.AreaRectangle);
            if (mevent.Button == MouseButtons.None)
            {
                this.ProcessForTooltip(info.HitTestResult, info.RowIndex, info.ColumnIndex);
            }
        }

        private bool ProcessNonScrollableVerticalAreaChange(bool recalcGridIfNeeded)
        {
            int num = this.CalcNonScrollableColumnsWidth();
            int width = base.ClientRectangle.Width;
            bool flag = false;
            if ((num >= width) && base.IsHandleCreated)
            {
                int firstScrollableColumn = this.FirstScrollableColumn;
                this.SetFirstScrollableColumnInt(0, recalcGridIfNeeded);
                flag = !recalcGridIfNeeded;
                this.m_scrollMgr.EnsureColumnIsVisible(0, true, recalcGridIfNeeded);
                this.OnResetFirstScrollableColumn(firstScrollableColumn, 0);
                return flag;
            }
            this.UpdateScrollableAreaRect();
            return flag;
        }

        protected bool ProcessPageUpDownKeys(bool bPageUp, System.Windows.Forms.Keys mod)
        {
            long currentRow = this.m_selMgr.CurrentRow;
            int currentColumn = this.m_selMgr.CurrentColumn;
            if ((currentRow < 0L) || (currentColumn < 0))
            {
                return false;
            }
            if (((currentRow != 0L) || !bPageUp) && ((currentRow != (this.NumRowsInt - 1L)) || bPageUp))
            {
                int yDelta = -1;
                NativeMethods.RECT scrollRect = new NativeMethods.RECT(0, 0, 0, 0);
                long firstRowIndex = this.m_scrollMgr.FirstRowIndex;
                int num4 = this.m_scrollMgr.CalcVertPageSize(this.m_scrollableArea);
                if (!this.CheckAndProcessCurrentEditingCellForKeyboard())
                {
                    return true;
                }
                if (bPageUp)
                {
                    if ((currentRow >= this.m_scrollMgr.FirstRowIndex) && (currentRow <= this.m_scrollMgr.LastRowIndex))
                    {
                        this.m_scrollMgr.HandleVScrollWithoutClientRedraw(2, ref yDelta, ref scrollRect);
                    }
                    else
                    {
                        this.m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(currentRow - num4, true, out yDelta);
                    }
                }
                else if ((currentRow >= this.m_scrollMgr.FirstRowIndex) && (currentRow <= this.m_scrollMgr.LastRowIndex))
                {
                    this.m_scrollMgr.HandleVScrollWithoutClientRedraw(3, ref yDelta, ref scrollRect);
                }
                else
                {
                    this.m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(currentRow + num4, true, out yDelta);
                }
                this.m_selMgr.Clear();
                currentRow += (bPageUp ? -1 : 1) * num4;
                if (currentRow < 0L)
                {
                    currentRow = 0L;
                }
                if (currentRow > (this.NumRowsInt - 1L))
                {
                    currentRow = this.NumRowsInt - 1L;
                }
                int editType = this.m_gridStorage.IsCellEditable(currentRow, this.m_Columns[currentColumn].ColumnIndex);
                if (editType != 0)
                {
                    this.StartEditCell(currentRow, currentColumn, editType, this.FocusEditorOnNavigation);
                }
                this.m_selMgr.StartNewBlock(currentRow, currentColumn);
                this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
                this.Refresh();
            }
            return true;
        }

        private bool ProcessSelAndEditingForLeftClickOnCell(System.Windows.Forms.Keys modKeys, long nRowIndex, int nColIndex, out int editType, out bool bShouldStartEditing, out bool bDragCanceled)
        {
            bDragCanceled = false;
            editType = this.m_gridStorage.IsCellEditable(nRowIndex, this.m_Columns[nColIndex].ColumnIndex);
            switch (this.m_Columns[nColIndex].ColumnType)
            {
                case 2:
                case 4:
                    editType = 0;
                    break;
            }
            bShouldStartEditing = false;
            bool flag = false;
            if (((modKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.None) && ((modKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.None))
            {
                bDragCanceled = !this.OnCanInitiateDragFromCell(this.m_captureTracker.RowIndex, this.m_captureTracker.ColumnIndex);
                if (!this.m_selMgr.IsCellSelected(nRowIndex, nColIndex))
                {
                    bShouldStartEditing = editType != 0;
                    this.m_selMgr.Clear();
                    return true;
                }
                if (bDragCanceled)
                {
                    bShouldStartEditing = editType != 0;
                    flag = !this.m_selMgr.OnlyOneSelItem;
                    if (bShouldStartEditing)
                    {
                        this.m_selMgr.Clear();
                        return flag;
                    }
                    if (flag)
                    {
                        this.m_selMgr.Clear();
                    }
                }
                return flag;
            }
            bShouldStartEditing = false;
            if (this.m_selMgr.OnlyOneSelItem)
            {
                flag = !this.m_selMgr.IsCellSelected(nRowIndex, nColIndex);
                this.m_selMgr.Clear();
            }
            return flag;
        }

        protected bool ProcessUpDownKeys(bool bDown, System.Windows.Forms.Keys mod)
        {
            if ((System.Windows.Forms.Keys.Control == mod) && !this.IsEditing)
            {
                this.m_scrollMgr.HandleVScroll(bDown ? 1 : 0);
                return true;
            }
            if (((this.m_selMgr.CurrentColumn == -1) || (this.m_selMgr.CurrentRow == -1L)) || (this.NumRowsInt <= 0L))
            {
                return false;
            }
            if (((this.m_selMgr.CurrentRow != 0L) || bDown) && ((this.m_selMgr.CurrentRow != (this.NumRowsInt - 1L)) || !bDown))
            {
                int num;
                if (this.IsEditing && !this.CheckAndProcessCurrentEditingCellForKeyboard())
                {
                    return true;
                }
                if (bDown)
                {
                    this.m_selMgr.CurrentRow = Math.Min((long) (this.NumRowsInt - 1L), (long) (this.m_selMgr.CurrentRow + 1L));
                }
                else
                {
                    this.m_selMgr.CurrentRow = Math.Max((long) 0L, (long) (this.m_selMgr.CurrentRow - 1L));
                }
                this.m_selMgr.Clear(false);
                bool bStartEditNewCurrentCell = this.IsCellEditableFromKeyboardNav();
                if (!bStartEditNewCurrentCell)
                {
                    this.m_selMgr.StartNewBlock(this.m_selMgr.CurrentRow, this.m_selMgr.CurrentColumn);
                }
                bool bMakeRowTheTopOne = !bDown;
                this.m_scrollMgr.EnsureRowIsVisbleWithoutClientRedraw(this.m_selMgr.CurrentRow, bMakeRowTheTopOne, out num);
                this.m_scrollMgr.EnsureColumnIsVisible(this.m_selMgr.CurrentColumn, false, false);
                this.CompleteArrowsNatigation(bStartEditNewCurrentCell, bDown ? System.Windows.Forms.Keys.Down : System.Windows.Forms.Keys.Up, mod);
            }
            return true;
        }

        private void RefreshHeader(int xPosForInsertionMark)
        {
            using (Graphics graphics = this.GraphicsFromHandle())
            {
                this.PaintHeader(graphics);
                if (xPosForInsertionMark >= 0)
                {
                    graphics.DrawLine(this.m_colInsertionPen, xPosForInsertionMark, 0, xPosForInsertionMark, this.HeaderHeight - 1);
                }
            }
        }

        public void RegisterEmbeddedControl(int editableCellType, Control embeddedControl)
        {
            if (editableCellType < 0x400)
            {
                throw new ArgumentException(SRError.InvalidCellType, "editableCellType");
            }
            if (embeddedControl == null)
            {
                throw new ArgumentNullException("embeddedControl");
            }
            if (!(embeddedControl is IGridEmbeddedControlManagement))
            {
                throw new ArgumentException(SRError.NoIGridEmbeddedControlManagement, "embeddedControl");
            }
            if (!(embeddedControl is IGridEmbeddedControl))
            {
                throw new ArgumentException(SRError.NoIGridEmbeddedControl, "embeddedControl");
            }
            if (base.InvokeRequired)
            {
                base.BeginInvoke(new RegisterEmbeddedControlInternalInvoker(this.RegisterEmbeddedControlInternal), new object[] { editableCellType, embeddedControl });
            }
            else
            {
                this.RegisterEmbeddedControlInternal(editableCellType, embeddedControl);
            }
        }

        protected virtual void RegisterEmbeddedControlInternal(int editableCellType, Control embeddedControl)
        {
            this.m_EmbeddedControls[editableCellType] = embeddedControl;
            embeddedControl.Font = this.Font;
            embeddedControl.Height = this.m_scrollMgr.CellHeight + (2 * ScrollManager.GRID_LINE_WIDTH);
            embeddedControl.RightToLeft = this.IsRTL ? RightToLeft.Yes : RightToLeft.No;
        }

        public void ResetGrid()
        {
            if (base.InvokeRequired)
            {
                base.BeginInvoke(new MethodInvoker(this.ResetGridInternal));
            }
            else
            {
                this.ResetGridInternal();
            }
        }

        protected virtual void ResetGridInternal()
        {
            if (this.IsEditing)
            {
                this.CancelEditCell();
            }
            this.m_gridStorage = null;
            this.m_captureTracker.Reset();
            this.m_scrollMgr.Reset();
            this.m_Columns.Clear();
            this.m_gridHeader.Reset();
            this.m_selMgr.Clear();
            this.m_hooverOverArea.Reset();
            this.Refresh();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetHeaderFont()
        {
            this.m_gridHeader.Font = null;
            this.TuneToHeaderFont(this.DefaultHeaderFont);
        }

        public void ResizeColumnToShowAllContents(int columnIndex)
        {
            if ((columnIndex < 0) || (columnIndex >= this.NumColInt))
            {
                throw new IndexOutOfRangeException("nColIndex");
            }
            if (base.InvokeRequired)
            {
                base.Invoke(new ResizeColumnToShowAllContentsInternalInvoker(this.ResizeColumnToShowAllContentsInternal), new object[] { this.GetUIColumnIndexByStorageIndex(columnIndex) });
            }
            else
            {
                this.ResizeColumnToShowAllContentsInternal(this.GetUIColumnIndexByStorageIndex(columnIndex));
            }
        }

        public void ResizeColumnToShowAllContents(int columnIndex, bool considerAllRows)
        {
            if ((columnIndex < 0) || (columnIndex >= this.NumColInt))
            {
                throw new IndexOutOfRangeException("nColIndex");
            }
            if (base.InvokeRequired)
            {
                base.Invoke(new ResizeColumnToShowAllContentsInternalInvoker2(this.ResizeColumnToShowAllContentsInternal), new object[] { this.GetUIColumnIndexByStorageIndex(columnIndex), considerAllRows });
            }
            else
            {
                this.ResizeColumnToShowAllContentsInternal(this.GetUIColumnIndexByStorageIndex(columnIndex), considerAllRows);
            }
        }

        protected virtual void ResizeColumnToShowAllContentsInternal(int columnIndex)
        {
            this.ResizeColumnToShowAllContentsInternal(columnIndex, false);
        }

        protected virtual void ResizeColumnToShowAllContentsInternal(int columnIndex, bool considerAllRows)
        {
            if ((columnIndex < 0) || (columnIndex >= this.NumColInt))
            {
                throw new ArgumentOutOfRangeException("columnIndex");
            }
            if (!base.IsHandleCreated)
            {
                throw new InvalidOperationException();
            }
            try
            {
                using (Graphics graphics = this.GraphicsFromHandle())
                {
                    int num = 0;
                    int num2 = columnIndex;
                    while ((num2 > 0) && this.m_gridHeader[num2 - 1].MergedWithRight)
                    {
                        num2--;
                    }
                    int firstColumnIndex = num2;
                    int num4 = 0;
                    int[] numArray = new int[(columnIndex - num2) + 1];
                    while (num2 <= columnIndex)
                    {
                        numArray[num2 - firstColumnIndex] = this.m_Columns[num2].WidthInPixels;
                        if (this.NumRowsInt > 0L)
                        {
                            if (this.m_Columns[num2].ColumnType >= 0x400)
                            {
                                if (considerAllRows)
                                {
                                    num = this.MeasureWidthOfCustomColumnRows(num2, this.m_Columns[num2].ColumnType, 0L, this.NumRowsInt - 1L, graphics);
                                }
                                else
                                {
                                    num = this.MeasureWidthOfCustomColumnRows(num2, this.m_Columns[num2].ColumnType, this.m_scrollMgr.FirstRowIndex, this.m_scrollMgr.LastRowIndex, graphics);
                                }
                            }
                            else if (considerAllRows)
                            {
                                num = this.MeasureWidthOfRows(num2, this.m_Columns[num2].ColumnType, 0L, this.NumRowsInt - 1L, graphics);
                            }
                            else
                            {
                                num = this.MeasureWidthOfRows(num2, this.m_Columns[num2].ColumnType, this.m_scrollMgr.FirstRowIndex, this.m_scrollMgr.LastRowIndex, graphics);
                            }
                        }
                        if (this.m_scrollMgr.FirstScrollableRowIndex > 0)
                        {
                            if (this.m_Columns[num2].ColumnType >= 0x400)
                            {
                                num = Math.Max(num, this.MeasureWidthOfCustomColumnRows(num2, this.m_Columns[num2].ColumnType, this.m_scrollMgr.FirstRowIndex, this.m_scrollMgr.LastRowIndex, graphics));
                            }
                            else
                            {
                                num = Math.Max(num, this.MeasureWidthOfRows(num2, this.m_Columns[num2].ColumnType, 0L, (long) (this.m_scrollMgr.FirstScrollableRowIndex - 1), graphics));
                            }
                        }
                        num4 += num;
                        if (num2 < columnIndex)
                        {
                            num4 += ScrollManager.GRID_LINE_WIDTH;
                        }
                        if (num > 0)
                        {
                            numArray[num2 - firstColumnIndex] = num;
                        }
                        if (this.WithHeader && (num2 == columnIndex))
                        {
                            int num5;
                            Rectangle r = new Rectangle(0, 0, 0x186a0, 0x186a0);
                            Bitmap bmp = this.m_gridHeader[columnIndex].Bmp;
                            string text = this.m_gridHeader[columnIndex].Text;
                            TextFormatFlags defaultTextFormatFlags = GridConstants.DefaultTextFormatFlags;
                            int width = GridButton.CalculateInitialContentsRect(graphics, r, text, bmp, HorizontalAlignment.Left, this.HeaderFont, this.IsRTL, ref defaultTextFormatFlags, out num5).Width;
                            if (width > 0)
                            {
                                width += 2 * this.MarginsWidth;
                            }
                            if (width > num4)
                            {
                                int num7 = 0;
                                int index = 0;
                                index = firstColumnIndex;
                                while (index <= columnIndex)
                                {
                                    this.m_Columns[index].OrigWidthInPixelsDuringResize = numArray[index - firstColumnIndex];
                                    num7 += numArray[index - firstColumnIndex];
                                    index++;
                                }
                                int[] columnWidths = new int[numArray.Length];
                                num7 += ScrollManager.GRID_LINE_WIDTH * (columnIndex - firstColumnIndex);
                                this.ResizeMultipleColumns(firstColumnIndex, columnIndex, width - num7, true, columnWidths);
                                for (index = 0; index <= (columnIndex - firstColumnIndex); index++)
                                {
                                    if (this.NumRowsInt > 0L)
                                    {
                                        numArray[index] = Math.Max(numArray[index], columnWidths[index]);
                                    }
                                    else
                                    {
                                        numArray[index] = Math.Max(columnWidths[index], this.GetMinWidthOfColumn(index));
                                    }
                                }
                            }
                        }
                        num2++;
                    }
                    for (int i = 0; i <= (columnIndex - firstColumnIndex); i++)
                    {
                        if (numArray[i] > 0)
                        {
                            this.SetColumnWidthInternal(i + firstColumnIndex, GridColumnWidthType.InPixels, Math.Min(numArray[i], 0x4e20));
                        }
                    }
                }
            }
            finally
            {
                base.Update();
            }
        }

        private void ResizeMultipleColumns(int firstColumnIndex, int lastColumnIndex, int sizeDelta, bool bFinalUpdate, int[] columnWidths)
        {
            int nOldWidth = 0;
            float num2 = 1f;
            float mergedHeaderResizeProportion = 0f;
            int num4 = 0;
            int num5 = sizeDelta;
            int colIndex = firstColumnIndex;
            while (colIndex <= lastColumnIndex)
            {
                if (num2 > 0f)
                {
                    nOldWidth = this.m_Columns[colIndex].WidthInPixels;
                    mergedHeaderResizeProportion = this.m_gridHeader[colIndex].MergedHeaderResizeProportion;
                    if (mergedHeaderResizeProportion > num2)
                    {
                        mergedHeaderResizeProportion = num2;
                    }
                    if (colIndex == lastColumnIndex)
                    {
                        num4 = num5;
                    }
                    else
                    {
                        num4 = (int) (sizeDelta * mergedHeaderResizeProportion);
                        if (num4 == 0)
                        {
                            colIndex++;
                            continue;
                        }
                    }
                    int num7 = Math.Max(this.m_Columns[colIndex].OrigWidthInPixelsDuringResize + num4, this.GetMinWidthOfColumn(colIndex));
                    num5 += -Math.Sign(sizeDelta) * Math.Abs((int) (this.m_Columns[colIndex].OrigWidthInPixelsDuringResize - num7));
                    num2 -= mergedHeaderResizeProportion;
                    if (columnWidths == null)
                    {
                        this.m_Columns[colIndex].WidthInPixels = num7;
                        if (((nOldWidth != this.m_Columns[colIndex].WidthInPixels) || bFinalUpdate) && (this.m_captureTracker.ColumnIndex >= this.m_scrollMgr.FirstScrollableColumnIndex))
                        {
                            this.m_scrollMgr.UpdateColWidth(colIndex, nOldWidth, this.m_Columns[colIndex].WidthInPixels, bFinalUpdate);
                        }
                    }
                    else
                    {
                        columnWidths[colIndex - firstColumnIndex] = num7;
                    }
                }
                colIndex++;
            }
        }

        public void SelectCell(long nRowIndex, int nColIndex)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            BlockOfCells cells = new BlockOfCells(nRowIndex, uIColumnIndexByStorageIndex);
            this.SelectCells(cells);
        }

        public void SelectCells(BlockOfCells cells)
        {
            if (base.InvokeRequired)
            {
                InvokerInOutArgs args = new InvokerInOutArgs();
                args.InOutParam = cells;
                base.Invoke(new SelectedCellsInternalInvoker(this.SelectCellsInternalForInvoke), new object[] { args, true });
            }
            this.SelectCellsInternal(cells, true);
        }

        private void SelectCellsInternalForInvoke(InvokerInOutArgs args, bool bSet)
        {
            BlockOfCells cells = (BlockOfCells)args.InOutParam;
            SelectCellsInternal(cells, bSet);
        }

        protected virtual void SelectCellsInternal(BlockOfCells cells, bool bSet)
        {
            if (!base.IsHandleCreated)
            {
                throw new InvalidOperationException();
            }
            if ((cells.X < 0) || (cells.Y < 0L))
            {
                throw new ArgumentException(SRError.NonExistingGridSelectionBlock(cells.X.ToString(), cells.Y.ToString(), cells.Right.ToString(), cells.Bottom.ToString()), "SelectionBlocks");
            }
            if ((cells.Y >= this.NumRowsInt) || (cells.Bottom >= this.NumRowsInt))
            {
                throw new ArgumentException(SRError.NonExistingGridSelectionBlock(cells.X.ToString(), cells.Y.ToString(), cells.Right.ToString(), cells.Bottom.ToString()), "SelectionBlocks");
            }
            if ((cells.X >= this.NumColInt) || (cells.Right >= this.NumColInt))
            {
                throw new ArgumentException(SRError.NonExistingGridSelectionBlock(cells.X.ToString(), cells.Y.ToString(), cells.Right.ToString(), cells.Bottom.ToString()), "SelectionBlocks");
            }
            if (this.IsEditing)
            {
                this.CancelEditCell();
            }

            this.m_selMgr.Clear();

            if (bSet) this.EnsureCellIsVisibleInternal(cells.Y, cells.X);

            this.m_selMgr.StartNewBlock(cells.Y, cells.X);
            this.m_selMgr.UpdateCurrentBlock(cells.Bottom, cells.Right);

            this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
            this.Invalidate();
        }

        protected virtual void SelectedCellsInternal(BlockOfCellsCollection col, bool bSet)
        {
            if (!bSet)
            {
                col.Clear();
                col.AddRange(this.m_selMgr.SelectedBlocks);
            }
            else
            {
                if (((col != null) && (col.Count > 1)) && this.m_selMgr.OnlyOneSelItem)
                {
                    throw new ArgumentException(SRError.NoMultiBlockSelInSingleSelMode, "SelectionBlocks");
                }
                if (col != null)
                {
                    foreach (BlockOfCells checkCells in col)
                    {
                        if ((checkCells.X < 0) || (checkCells.Y < 0L))
                        {
                            throw new ArgumentException(SRError.NonExistingGridSelectionBlock(checkCells.X.ToString(), checkCells.Y.ToString(), checkCells.Right.ToString(), checkCells.Bottom.ToString()), "SelectionBlocks");
                        }
                        if ((checkCells.Y >= this.NumRowsInt) || (checkCells.Bottom >= this.NumRowsInt))
                        {
                            throw new ArgumentException(SRError.NonExistingGridSelectionBlock(checkCells.X.ToString(), checkCells.Y.ToString(), checkCells.Right.ToString(), checkCells.Bottom.ToString()), "SelectionBlocks");
                        }
                        if ((checkCells.X >= this.NumColInt) || (checkCells.Right >= this.NumColInt))
                        {
                            throw new ArgumentException(SRError.NonExistingGridSelectionBlock(checkCells.X.ToString(), checkCells.Y.ToString(), checkCells.Right.ToString(), checkCells.Bottom.ToString()), "SelectionBlocks");
                        }
                    }
                }
                if (this.IsEditing)
                {
                    this.CancelEditCell();
                }
                this.m_selMgr.Clear();
                if (col != null)
                {
                    foreach (BlockOfCells cells in col)
                    {
                        this.m_selMgr.StartNewBlock(cells.Y, cells.X);
                        this.m_selMgr.UpdateCurrentBlock(cells.Bottom, cells.Right);
                    }
                }
                this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
                base.Invalidate();
            }
        }

        private void SelectedCellsInternalForInvoke(InvokerInOutArgs a, bool bSet)
        {
            BlockOfCellsCollection inOutParam = (BlockOfCellsCollection) a.InOutParam;
            this.SelectedCellsInternal(inOutParam, bSet);
        }

        protected void SendMouseClickToEmbeddedControl()
        {
            if (this.m_curEmbeddedControl != null)
            {
                Point mousePosition = Control.MousePosition;
                mousePosition = this.m_curEmbeddedControl.PointToClient(mousePosition);
                if (this.m_curEmbeddedControl.ClientRectangle.Contains(mousePosition))
                {
                    NativeMethods.SendMessage(this.m_curEmbeddedControl.Handle, 0x201, IntPtr.Zero, NativeMethods.Util.MAKELPARAM(mousePosition.X, 2));
                    NativeMethods.SendMessage(this.m_curEmbeddedControl.Handle, 0x202, IntPtr.Zero, NativeMethods.Util.MAKELPARAM(mousePosition.X, 2));
                }
            }
        }

        public void SetBitmapsForCheckBoxColumn(int nColIndex, Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState)
        {
            if (base.InvokeRequired)
            {
                throw new InvalidOperationException(SRError.InvalidThreadForMethod);
            }
            if ((nColIndex < 0) || (nColIndex >= this.NumColInt))
            {
                throw new IndexOutOfRangeException("nColIndex");
            }
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            if (this.m_Columns[uIColumnIndexByStorageIndex].ColumnType != 4)
            {
                throw new InvalidOperationException(SRError.ColumnIsNotCheckBox(nColIndex));
            }
            ((GridCheckBoxColumn) this.m_Columns[uIColumnIndexByStorageIndex]).SetCheckboxBitmaps(checkedState, uncheckedState, indeterminateState, disabledState);
        }

        public void SetColumnWidth(int nColIndex, GridColumnWidthType widthType, int nWidth)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            if (base.InvokeRequired)
            {
                base.Invoke(new SetColumnWidthInternalInvoker(this.SetColumnWidthInternalForPublic), new object[] { uIColumnIndexByStorageIndex, widthType, nWidth });
            }
            else
            {
                this.SetColumnWidthInternalForPublic(uIColumnIndexByStorageIndex, widthType, nWidth);
            }
        }

        protected virtual void SetColumnWidthInternal(int nColIndex, GridColumnWidthType widthType, int nWidth)
        {
            if ((nColIndex < 0) || (nColIndex >= this.NumColInt))
            {
                throw new ArgumentOutOfRangeException("nColIndex", nColIndex, "");
            }
            if (nWidth <= 0)
            {
                throw new ArgumentException(SRError.ColumnWidthShouldBeGreaterThanZero, "nWidth");
            }
            int columnWidthInPixels = this.GetColumnWidthInPixels(nWidth, widthType);
            if (columnWidthInPixels > 0x4e20)
            {
                throw new ArgumentException(SRError.ColumnWidthShouldBeLessThanMax(0x4e20), "nWidth");
            }
            int widthInPixels = this.m_Columns[nColIndex].WidthInPixels;
            this.m_Columns[nColIndex].WidthInPixels = columnWidthInPixels;
            bool flag = false;
            if (nColIndex < this.m_scrollMgr.FirstScrollableColumnIndex)
            {
                flag = this.ProcessNonScrollableVerticalAreaChange(false);
            }
            else
            {
                this.m_scrollMgr.UpdateColWidth(nColIndex, widthInPixels, columnWidthInPixels, true);
            }
            if (this.m_nIsInitializingCount == 0)
            {
                if (flag)
                {
                    this.m_scrollMgr.RecalcAll(this.m_scrollableArea);
                }
                else
                {
                    base.Invalidate();
                }
                this.OnColumnWidthChanged(nColIndex, columnWidthInPixels);
            }
        }

        private void SetColumnWidthInternalForPublic(int nColIndex, GridColumnWidthType widthType, int nWidth)
        {
            this.SetColumnWidthInternal(nColIndex, widthType, nWidth);
            if (this.m_nIsInitializingCount == 0)
            {
                base.Update();
            }
        }

        protected virtual void SetCursorForCustomCell(long nRowIndex, int nColumnIndex, Rectangle r)
        {
            throw new NotImplementedException(SRError.DeriveToImplementCustomColumn);
        }

        protected virtual void SetCursorFromHitTest(HitTestResult ht, long nRowIndex, int nColumnIndex, Rectangle cellRect)
        {
            if (ht == HitTestResult.ColumnResize)
            {
                this.Cursor = Cursors.VSplit;
            }
            else if (ht == HitTestResult.CustomCell)
            {
                this.SetCursorForCustomCell(nRowIndex, nColumnIndex, cellRect);
            }
            else
            {
                if ((ht == HitTestResult.HyperlinkCell) && (Control.ModifierKeys == System.Windows.Forms.Keys.None))
                {
                    using (Graphics graphics = this.GraphicsFromHandle())
                    {
                        if (this.m_Columns[nColumnIndex].IsPointOverTextInCell(base.PointToClient(Control.MousePosition), cellRect, this.m_gridStorage, nRowIndex, graphics, this.m_linkFont))
                        {
                            this.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            this.Cursor = Cursors.Arrow;
                        }
                        return;
                    }
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void SetFirstScrollableColumnInt(int columnIndex, bool recalcGrid)
        {
            if ((columnIndex <= 0) || (this.CalcNonScrollableColumnsWidth(columnIndex - 1) < base.ClientRectangle.Width))
            {
                this.m_scrollMgr.FirstScrollableColumnIndex = columnIndex;
                this.UpdateScrollableAreaRect();
                if (recalcGrid)
                {
                    this.UpdateGridInternal(true);
                }
            }
        }

        private void SetFocusToEmbeddedControl()
        {
            if (!this.m_curEmbeddedControl.ContainsFocus)
            {
                this.m_curEmbeddedControl.Focus();
            }
        }

        public void SetHeaderInfo(int colIndex, string strText, GridCheckBoxState checkboxState)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(colIndex);
            if ((this.m_gridHeader[uIColumnIndexByStorageIndex].Type != GridColumnHeaderType.CheckBox) && (this.m_gridHeader[uIColumnIndexByStorageIndex].Type != GridColumnHeaderType.TextAndCheckBox))
            {
                throw new InvalidOperationException(SRError.ShouldSetHeaderStateForRegualrCol(colIndex));
            }
            if (base.InvokeRequired)
            {
                object[] args = new object[4];
                args[0] = uIColumnIndexByStorageIndex;
                args[1] = strText;
                args[3] = checkboxState;
                base.Invoke(new SetHeaderInfoInvoker(this.SetHeaderInfoInternal), args);
            }
            else
            {
                this.SetHeaderInfoInternal(uIColumnIndexByStorageIndex, strText, null, checkboxState);
            }
        }

        public void SetHeaderInfo(int nColIndex, string strText, Bitmap bmp)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            if ((this.m_gridHeader[uIColumnIndexByStorageIndex].Type == GridColumnHeaderType.CheckBox) || (this.m_gridHeader[uIColumnIndexByStorageIndex].Type == GridColumnHeaderType.TextAndCheckBox))
            {
                throw new InvalidOperationException(SRError.ShouldSetHeaderStateForCheckBox(nColIndex));
            }
            if (base.InvokeRequired)
            {
                base.Invoke(new SetHeaderInfoInvoker(this.SetHeaderInfoInternal), new object[] { uIColumnIndexByStorageIndex, strText, bmp, GridCheckBoxState.None });
            }
            else
            {
                this.SetHeaderInfoInternal(uIColumnIndexByStorageIndex, strText, bmp, GridCheckBoxState.None);
            }
        }

        protected virtual void SetHeaderInfoInternal(int nIndex, string strText, Bitmap bmp, GridCheckBoxState checkboxState)
        {
            if ((nIndex < 0) || (nIndex >= this.m_Columns.Count))
            {
                throw new ArgumentOutOfRangeException("nIndex", nIndex, "");
            }
            this.m_gridHeader.SetHeaderItemInfo(nIndex, strText, bmp, checkboxState);
        }

        public void SetMergedHeaderResizeProportion(int colIndex, float proportion)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(colIndex);
            if (base.InvokeRequired)
            {
                base.Invoke(new SetMergedHeaderResizeProportionInternalInvoker(this.SetMergedHeaderResizeProportionInternal), new object[] { uIColumnIndexByStorageIndex, proportion });
            }
            else
            {
                this.SetMergedHeaderResizeProportionInternal(uIColumnIndexByStorageIndex, proportion);
            }
        }

        protected virtual void SetMergedHeaderResizeProportionInternal(int colIndex, float proportion)
        {
            if ((proportion < 0f) || (proportion > 1f))
            {
                throw new ArgumentException(SRError.InvalidMergedHeaderResizeProportion, "proportion");
            }
            if (!this.m_gridHeader[colIndex].MergedWithRight)
            {
                throw new ArgumentException(SRError.InvalidColIndexForMergedResizeProp, "colIndex");
            }
            this.m_gridHeader[colIndex].MergedHeaderResizeProportion = proportion;
        }

        private bool ShouldMakeControlVisible(KeyEventArgs ke)
        {
            System.Windows.Forms.Keys[] keysArray = new System.Windows.Forms.Keys[] { System.Windows.Forms.Keys.Escape, System.Windows.Forms.Keys.Tab, System.Windows.Forms.Keys.Capital, System.Windows.Forms.Keys.Menu, System.Windows.Forms.Keys.ShiftKey, System.Windows.Forms.Keys.ControlKey, System.Windows.Forms.Keys.Alt, System.Windows.Forms.Keys.Home, System.Windows.Forms.Keys.End, System.Windows.Forms.Keys.Next, System.Windows.Forms.Keys.Prior, System.Windows.Forms.Keys.Snapshot, System.Windows.Forms.Keys.Insert };
            if (!this.FocusEditorOnNavigation)
            {
                System.Windows.Forms.Keys[] keysArray2 = new System.Windows.Forms.Keys[] { System.Windows.Forms.Keys.Left, System.Windows.Forms.Keys.Right, System.Windows.Forms.Keys.Up, System.Windows.Forms.Keys.Down, System.Windows.Forms.Keys.Return };
                System.Windows.Forms.Keys[] array = new System.Windows.Forms.Keys[keysArray2.Length + keysArray.Length];
                keysArray.CopyTo(array, 0);
                keysArray2.CopyTo(array, (int) (keysArray.Length - 1));
                keysArray = array;
            }
            for (int i = 0; i < keysArray.Length; i++)
            {
                if (ke.KeyCode == keysArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool ShouldSerializeHeaderFont()
        {
            return (this.m_gridHeader.Font != null);
        }

        public bool StartCellEdit(long nRowIndex, int nColIndex)
        {
            int uIColumnIndexByStorageIndex = this.GetUIColumnIndexByStorageIndex(nColIndex);
            if (base.InvokeRequired)
            {
                InvokerInOutArgs args = new InvokerInOutArgs();
                base.Invoke(new StartCellEditInternalInvoker(this.StartCellEditInternalForInvoke), new object[] { nRowIndex, uIColumnIndexByStorageIndex, args });
                return (bool) args.InOutParam;
            }
            return this.StartCellEditInternal(nRowIndex, uIColumnIndexByStorageIndex);
        }

        protected virtual bool StartCellEditInternal(long nRowIndex, int nColIndex)
        {
            if (!base.IsHandleCreated)
            {
                throw new InvalidOperationException();
            }
            if ((nRowIndex < 0L) || (nRowIndex >= this.NumRowsInt))
            {
                throw new ArgumentOutOfRangeException("nRowIndex", nRowIndex, "");
            }
            if ((nColIndex < 0) || (nColIndex >= this.NumColInt))
            {
                throw new ArgumentOutOfRangeException("nColIndex", nColIndex, "");
            }
            if ((this.m_Columns[nColIndex].ColumnType == 2) || (this.m_Columns[nColIndex].ColumnType == 4))
            {
                return false;
            }
            int editType = this.m_gridStorage.IsCellEditable(nRowIndex, this.m_Columns[nColIndex].ColumnIndex);
            if (editType == 0)
            {
                return false;
            }
            this.m_selMgr.Clear();
            this.EnsureCellIsVisibleInternal(nRowIndex, nColIndex);
            bool flag = this.StartEditCell(nRowIndex, nColIndex, editType);
            if (flag)
            {
                this.m_selMgr.StartNewBlock(nRowIndex, nColIndex);
                this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
                this.Refresh();
            }
            return flag;
        }

        private void StartCellEditInternalForInvoke(long nRowIndex, int nColIndex, InvokerInOutArgs args)
        {
            args.InOutParam = this.StartCellEditInternal(nRowIndex, nColIndex);
        }

        private bool StartEditCell(long nRowIndex, int nColIndex, int editType)
        {
            return this.StartEditCell(nRowIndex, nColIndex, editType, true);
        }

        private bool StartEditCell(long nRowIndex, int nColIndex, int editType, bool focusEmbedded)
        {
            Rectangle cellRectangle = this.m_scrollMgr.GetCellRectangle(nRowIndex, nColIndex);
            if (!cellRectangle.IsEmpty)
            {
                bool bSendMouseClick = false;
                return this.StartEditCell(nRowIndex, nColIndex, cellRectangle, editType, ref bSendMouseClick, focusEmbedded);
            }
            return false;
        }

        private bool StartEditCell(long nRowIndex, int nColIndex, Rectangle rCellRect, int editType, ref bool bSendMouseClick)
        {
            return this.StartEditCell(nRowIndex, nColIndex, rCellRect, editType, ref bSendMouseClick, true);
        }

        private bool StartEditCell(long nRowIndex, int nColIndex, Rectangle rCellRect, int editType, ref bool bSendMouseClick, bool focusEmbedded)
        {
            if (!this.m_EmbeddedControls.Contains(editType))
            {
                return false;
            }
            if (this.IsEditing && !this.StopEditCell())
            {
                return false;
            }
            try
            {
                Control control = this.m_EmbeddedControls[editType] as Control;
                IGridEmbeddedControlManagement management = control as IGridEmbeddedControlManagement;
                bSendMouseClick = management.WantMouseClick;
                management.SetHorizontalAlignment(this.m_Columns[nColIndex].TextAlign);
                IGridEmbeddedControl control2 = (IGridEmbeddedControl) control;
                control2.ClearData();
                control2.Enabled = true;
                try
                {
                    this.m_bInGridStorageCall = true;
                    this.m_gridStorage.FillControlWithData(nRowIndex, this.m_Columns[nColIndex].ColumnIndex, control2);
                }
                finally
                {
                    this.m_bInGridStorageCall = false;
                }
                control2.ContentsChanged += this.m_OnEmbeddedControlContentsChangedDelegate;
                control2.RowIndex = nRowIndex;
                control2.ColumnIndex = this.m_Columns[nColIndex].ColumnIndex;
                rCellRect.Height += ScrollManager.GRID_LINE_WIDTH;
                this.AdjustEditingCellHorizontally(ref rCellRect, nColIndex);
                control.Bounds = rCellRect;
                SolidBrush bkBrush = null;
                SolidBrush textBrush = null;
                this.GetCellGDIObjects(this.m_Columns[nColIndex], nRowIndex, nColIndex, ref bkBrush, ref textBrush);
                control.BackColor = bkBrush.Color;
                control.ForeColor = textBrush.Color;
                this.m_selMgr.CurrentColumn = nColIndex;
                this.m_selMgr.CurrentRow = nRowIndex;
                this.m_curEmbeddedControl = control;
                this.m_curEmbeddedControl.Visible = true;
                if (focusEmbedded)
                {
                    this.SetFocusToEmbeddedControl();
                }
                this.OnStartedCellEdit();
                return true;
            }
            catch (Exception)
            {
                this.m_curEmbeddedControl = null;
                return false;
            }
        }

        public bool StopCellEdit(bool bCommitIntoStorage)
        {
            if (base.InvokeRequired)
            {
                InvokerInOutArgs args = new InvokerInOutArgs();
                base.Invoke(new StopCellEditInternalInvoker(this.StopCellEditInternalForInvoke), new object[] { args, bCommitIntoStorage });
                return (bool) args.InOutParam;
            }
            return this.StopCellEditInternal(bCommitIntoStorage);
        }

        protected virtual bool StopCellEditInternal(bool bCommitIntoStorage)
        {
            if (this.m_curEmbeddedControl == null)
            {
                return false;
            }
            if (bCommitIntoStorage)
            {
                return this.StopEditCell();
            }
            this.CancelEditCell();
            return true;
        }

        private void StopCellEditInternalForInvoke(InvokerInOutArgs args, bool bCommitIntoStorage)
        {
            args.InOutParam = this.StopCellEditInternal(bCommitIntoStorage);
        }

        private bool StopEditCell()
        {
            bool flag2;
            IGridEmbeddedControl curEmbeddedControl = (IGridEmbeddedControl) this.m_curEmbeddedControl;
            if (curEmbeddedControl == null)
            {
                throw new InvalidOperationException(SRError.CurControlIsNotIGridEmbedded);
            }
            bool containsFocus = base.ContainsFocus;
            this.m_bInGridStorageCall = true;
            try
            {
                if (this.m_gridStorage.SetCellDataFromControl(curEmbeddedControl.RowIndex, curEmbeddedControl.ColumnIndex, curEmbeddedControl))
                {
                    this.m_bInGridStorageCall = false;
                    curEmbeddedControl.ContentsChanged -= this.m_OnEmbeddedControlContentsChangedDelegate;
                    this.OnStoppedCellEdit();
                    this.m_curEmbeddedControl.Visible = false;
                    this.m_curEmbeddedControl = null;
                    if (containsFocus)
                    {
                        base.Focus();
                    }
                    return true;
                }
                if (containsFocus && (this.m_curEmbeddedControl != null))
                {
                    this.SetFocusToEmbeddedControl();
                }
                flag2 = false;
            }
            finally
            {
                this.m_bInGridStorageCall = false;
            }
            return flag2;
        }

        private void TransitionHyperlinkToStd(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            if (timer != null)
            {
                timer.Stop();
                this.Cursor = Cursors.Arrow;
                this.HandleStdCellLBtnDown(System.Windows.Forms.Keys.None);
                this.m_captureTracker.CaptureHitTest = HitTestResult.TextCell;
            }
        }

        private void TuneToHeaderFont(Font f)
        {
            if (!f.Equals(this.m_gridHeader.HeaderGridButton.TextFont) || (this.m_headerHeight == 0))
            {
                this.m_gridHeader.HeaderGridButton.TextFont = f;
                this.m_headerHeight = this.CalculateHeaderHeight(f);
                if (base.IsHandleCreated)
                {
                    this.UpdateScrollableAreaRect();
                    this.Refresh();
                }
            }
        }

        private void UpdateEmbeddedControlsFont()
        {
            if (this.m_scrollMgr.CellHeight > 0)
            {
                Control control = null;
                foreach (DictionaryEntry entry in this.m_EmbeddedControls)
                {
                    control = (Control) entry.Value;
                    control.Font = this.Font;
                    control.Height = this.m_scrollMgr.CellHeight + (2 * ScrollManager.GRID_LINE_WIDTH);
                }
            }
        }

        private void UpdateEmbeddedControlsRTL()
        {
            RightToLeft left = this.IsRTL ? RightToLeft.Yes : RightToLeft.No;
            Control control = null;
            foreach (DictionaryEntry entry in this.m_EmbeddedControls)
            {
                control = (Control) entry.Value;
                control.RightToLeft = left;
            }
        }

        public void UpdateGrid()
        {
            this.UpdateGrid(true);
        }

        public void UpdateGrid(bool bRecalcRows)
        {
            if (base.InvokeRequired)
            {
                base.BeginInvoke(this.m_UpdateGridInternalDelegate, new object[] { bRecalcRows });
            }
            else
            {
                this.UpdateGridInternal(bRecalcRows);
            }
        }

        protected virtual void UpdateGridInternal(bool bRecalcRows)
        {
            this.ValidateFirstScrollableColumn();
            this.ValidateFirstScrollableRow();
            if (bRecalcRows)
            {
                if (this.m_gridStorage != null)
                {
                    this.m_scrollMgr.RowsNumber = this.m_gridStorage.NumRows();
                }
                else
                {
                    this.m_scrollMgr.RowsNumber = 0L;
                    this.m_selMgr.Clear();
                }
                if (!this.m_scrollableArea.IsEmpty && base.IsHandleCreated)
                {
                    this.m_scrollMgr.RecalcAll(this.m_scrollableArea);
                }
            }
            if (base.IsHandleCreated && (this.m_nIsInitializingCount == 0))
            {
                this.Refresh();
            }
        }

        private void UpdateScrollableAreaRect()
        {
            this.m_scrollableArea = base.ClientRectangle;
            if (this.m_withHeader)
            {
                int height = this.m_scrollableArea.Height;
                this.m_scrollableArea.Y = this.HeaderHeight;
                this.m_scrollableArea.Height -= this.HeaderHeight;
            }
            if (this.m_scrollMgr.FirstScrollableColumnIndex != 0)
            {
                int num = this.CalcNonScrollableColumnsWidth();
                this.m_scrollableArea.X = num;
                this.m_scrollableArea.Width -= num;
            }
            if (this.m_scrollMgr.FirstScrollableRowIndex > 0)
            {
                uint num2 = this.NonScrollableRowsHeight();
                this.m_scrollableArea.Y += (int) num2;
                this.m_scrollableArea.Height -= (int) num2;
            }
            this.m_scrollMgr.OnSAChange(this.m_scrollableArea);
        }

        private void UpdateSelectionBlockFromMouse(long nRowIndex, int nColIndex)
        {
            GridSelectionType selectionType = this.m_selMgr.SelectionType;
            this.m_selMgr.UpdateCurrentBlock(nRowIndex, nColIndex);
            if (((((selectionType != GridSelectionType.CellBlocks) || (this.m_captureTracker.LastColumnIndex != nColIndex)) || (this.m_captureTracker.LastRowIndex != nRowIndex)) && ((selectionType != GridSelectionType.RowBlocks) || (nRowIndex != this.m_captureTracker.LastRowIndex))) && ((selectionType != GridSelectionType.ColumnBlocks) || (nColIndex != this.m_captureTracker.LastColumnIndex)))
            {
                this.Refresh();
            }
            this.m_captureTracker.LastColumnIndex = nColIndex;
            this.m_captureTracker.LastRowIndex = nRowIndex;
        }

        private void ValidateColumnType(int nType)
        {
            if ((nType != 3) && (nType != 2) && (nType != 1) && (nType != 4) && (nType != 5) && (nType != 6) && (nType < 0x400))
            {
                throw new ArgumentException(SRError.InvalidCustomColType, "ci.ColumnType");
            }
        }

        private void ValidateFirstScrollableColumn()
        {
            int firstScrollableColumnIndex = this.m_scrollMgr.FirstScrollableColumnIndex;
            if (this.m_Columns.Count > 0)
            {
                if ((firstScrollableColumnIndex < 0) || (firstScrollableColumnIndex >= this.m_Columns.Count))
                {
                    throw new ArgumentException(SRError.FirstScrollableColumnShouldBeValid, "FirstScrollalbeColumn");
                }
                if ((firstScrollableColumnIndex > 0) && this.m_gridHeader[firstScrollableColumnIndex - 1].MergedWithRight)
                {
                    throw new ArgumentException(SRError.LastNonScrollCannotBeMerged, "FirstScrollalbeColumn");
                }
            }
        }

        private void ValidateFirstScrollableRow()
        {
            if (this.m_scrollMgr.FirstScrollableRowIndex < 0)
            {
                throw new ArgumentException(SRError.FirstScrollableRowShouldBeValid, "FirstScrollalbeRow");
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_LBUTTONDOWN:
                    this.m_captureTracker.WasEmbeddedControlFocused = this.IsEditing && this.m_curEmbeddedControl.ContainsFocus;
                    base.WndProc(ref m);
                    return;

                case NativeMethods.WM_RBUTTONUP:
                    try
                    {
                        this.OnMouseUp(new MouseEventArgs(MouseButtons.Right, 1, (short) ((int) m.LParam), ((int) m.LParam) >> 0x10, 0));
                        return;
                    }
                    finally
                    {
                        if (base.Capture)
                        {
                            base.Capture = false;
                        }
                        this.DefWndProc(ref m);
                    }

                case NativeMethods.WM_HSCROLL:
                    this.m_scrollMgr.HandleHScroll(NativeMethods.Util.LOWORD(m.WParam));
                    if (!this.m_withHeader)
                    {
                        this.CheckAndRePositionEmbeddedControlForSmallSizes();
                    }
                    return;

                case NativeMethods.WM_VSCROLL:
                    this.m_scrollMgr.HandleVScroll(NativeMethods.Util.LOWORD(m.WParam));
                    this.CheckAndRePositionEmbeddedControlForSmallSizes();
                    return;

                case NativeMethods.WM_NCPAINT:
                    if (!DrawManager.DrawNCBorder(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;
            }
            base.WndProc(ref m);
        }
        #endregion

        #region Propeties
        protected virtual bool ActAsEnabled
        {
            get
            {
                return base.Enabled;
            }
        }

        [Category("Appearance"), DefaultValue(true)]
        public bool AlwaysHighlightSelection
        {
            get
            {
                return this.m_alwaysHighlightSelection;
            }
            set
            {
                if (base.InvokeRequired)
                {
                    base.Invoke(new AlwaysHighlightSelectionIntInvoker(this.AlwaysHighlightSelectionInt), new object[] { value });
                }
                else
                {
                    this.AlwaysHighlightSelectionInt(value);
                }
            }
        }

        [Description("Interval in miliseconds for autoscroll"), DefaultValue(0x4b), Category("Appearance")]
        public int AutoScrollInterval
        {
            get
            {
                return this.m_autoScrollTimer.Interval;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(SRError.AutoScrollMoreThanZero, "value");
                }
                this.m_autoScrollTimer.Interval = value;
            }
        }

        [DefaultValue(2), Description("Border style of the grid"), Category("Appearance")]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.m_borderStyle;
            }
            set
            {
                if (!Enum.IsDefined(typeof(System.Windows.Forms.BorderStyle), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (this.m_borderStyle != value)
                {
                    this.m_borderStyle = value;
                    base.RecreateHandle();
                }
            }
        }

        [Browsable(false)]
        public int ColumnsNumber
        {
            get
            {
                if (!base.InvokeRequired)
                {
                    return this.m_Columns.Count;
                }
                InvokerInOutArgs args = new InvokerInOutArgs();
                base.Invoke(new GetColumnsNumberInternalInvoker(this.GetColumnsNumberInternalForInvoke), new object[] { args });
                return (int) args.InOutParam;
            }
        }

        [DefaultValue(false), Category("Appearance")]
        public bool ColumnsReorderableByDefault
        {
            get
            {
                return this.m_bColumnsReorderableByDefault;
            }
            set
            {
                this.m_bColumnsReorderableByDefault = value;
            }
        }

        protected virtual string ColumnsSeparator
        {
            get
            {
                return "\t";
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                switch (this.m_borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        if (!Application.RenderWithVisualStyles)
                        {
                            createParams.ExStyle |= 0x200;
                            break;
                        }
                        createParams.Style |= 0x800000;
                        break;
                }
                createParams.Style |= 0x46300000;
                return createParams;
            }
        }

        protected virtual Font DefaultHeaderFont
        {
            get
            {
                if ((base.Parent != null) && (base.Parent.Font != null))
                {
                    return base.Parent.Font;
                }
                if (this.Site != null)
                {
                    AmbientProperties service = (AmbientProperties) this.Site.GetService(typeof(AmbientProperties));
                    if (service != null)
                    {
                        return service.Font;
                    }
                }
                return Control.DefaultFont;
            }
        }

        [Category("Appearance"), Description("Index of the first scrollable column"), DefaultValue(0)]
        public int FirstScrollableColumn
        {
            get
            {
                return this.m_scrollMgr.FirstScrollableColumnIndex;
            }
            set
            {
                if (this.m_scrollMgr.FirstScrollableColumnIndex != value)
                {
                    if (value < 0)
                    {
                        throw new ArgumentException(SRError.FirstScrollableColumnShouldBeValid, "value");
                    }
                    this.SetFirstScrollableColumnInt(value, true);
                }
            }
        }

        [DefaultValue(0), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), CLSCompliant(false)]
        public uint FirstScrollableRow
        {
            get
            {
                return this.m_scrollMgr.FirstScrollableRowIndex;
            }
            set
            {
                if (this.m_scrollMgr.FirstScrollableRowIndex != value)
                {
                    this.m_scrollMgr.FirstScrollableRowIndex = value;
                    this.UpdateScrollableAreaRect();
                    this.UpdateGridInternal(true);
                }
            }
        }

        [Category("Appearance"), DefaultValue(true), Description("Whether or not Embedded Controls will receive focus when navigated to via keyboard"), Browsable(true)]
        public bool FocusEditorOnNavigation
        {
            get
            {
                return this.focusOnNav;
            }
            set
            {
                this.focusOnNav = value;
            }
        }

        [Browsable(false)]
        public static int GridButtonHorizOffset
        {
            get
            {
                return GridButton.ExtraHorizSpace;
            }
        }

        [Browsable(false)]
        public GridColumnInfoCollection GridColumnsInfo
        {
            get
            {
                GridColumnInfoCollection infos = new GridColumnInfoCollection();
                for (int i = 0; i < this.m_Columns.Count; i++)
                {
                    infos.Add(this.GetGridColumnInfo(i));
                }
                return infos;
            }
        }

        protected virtual Pen GridLinesPen
        {
            get
            {
                if (!this.ActAsEnabled)
                {
                    return SystemPens.ControlDark;
                }
                return this.m_gridLinesPen;
            }
        }

        [Description("Type of grid lines"), DefaultValue(1), Category("Appearance")]
        public GridLineType GridLineType
        {
            get
            {
                return this.m_lineType;
            }
            set
            {
                if (value != this.m_lineType)
                {
                    if (!Enum.IsDefined(typeof(GridLineType), value))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(GridLineType));
                    }
                    this.m_lineType = value;
                    foreach (GridColumn column2 in this.m_Columns)
                    {
                        if (column2 is GridButtonColumn)
                        {
                            ((GridButtonColumn) column2).SetGridLinesMode(this.m_lineType != GridLineType.None);
                        }
                    }
                    if (base.IsHandleCreated && (this.m_nIsInitializingCount == 0))
                    {
                        base.Invalidate();
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGridStorage GridStorage
        {
            get
            {
                return this.m_gridStorage;
            }
            set
            {
                if (this.m_gridStorage != value)
                {
                    this.m_gridStorage = value;
                    if (base.IsHandleCreated && (this.m_nIsInitializingCount == 0))
                    {
                        this.UpdateGrid();
                    }
                }
            }
        }

        private bool HasNonScrollableColumns
        {
            get
            {
                return ((this.m_scrollMgr.FirstScrollableColumnIndex > 0) && (this.m_scrollMgr.FirstScrollableColumnIndex < this.m_Columns.Count));
            }
        }

        [AmbientValue((string) null), Category("Appearance"), Description("Font for the header of the grid"), Localizable(true)]
        public Font HeaderFont
        {
            get
            {
                if (this.m_gridHeader.Font != null)
                {
                    return this.m_gridHeader.Font;
                }
                return this.DefaultHeaderFont;
            }
            set
            {
                if (value == null)
                {
                    value = this.DefaultHeaderFont;
                }
                this.m_gridHeader.Font = value;
                this.TuneToHeaderFont(value);
            }
        }

        [Browsable(false)]
        public int HeaderHeight
        {
            get
            {
                if (!this.m_withHeader)
                {
                    return 0;
                }
                return this.m_headerHeight;
            }
        }

        [Browsable(false)]
        public System.Drawing.Color HighlightColor
        {
            get
            {
                if (this.m_highlightBrush != null)
                {
                    return this.m_highlightBrush.Color;
                }
                return SystemColors.Highlight;
            }
        }

        private bool InHyperlinkTimer
        {
            get
            {
                if (this.m_captureTracker.HyperLinkSelectionTimer == null)
                {
                    return false;
                }
                return this.m_captureTracker.HyperLinkSelectionTimer.Enabled;
            }
        }

        protected bool IsEditing
        {
            get
            {
                return (this.m_curEmbeddedControl != null);
            }
        }

        [Browsable(false)]
        public Control EditingEmbeddedControl
        {
            get
            {
                return this.m_curEmbeddedControl;
            }
        }

        private bool IsEmpty
        {
            get
            {
                if (this.NumColInt != 0)
                {
                    return (this.m_gridStorage == null);
                }
                return true;
            }
        }

        protected bool IsInitializing
        {
            get
            {
                return (this.m_nIsInitializingCount > 0);
            }
        }

        private bool IsRTL
        {
            get
            {
                return (this.RightToLeft == RightToLeft.Yes);
            }
        }

        [Category("Appearance"), Description("Widths of the left and right margins for an embedded control")]
        public int MarginsWidth
        {
            get
            {
                return GridColumn.CELL_CONTENT_OFFSET;
            }
        }

        protected virtual bool NeedToHighlightCurrentCell
        {
            get
            {
                return (this.m_selMgr.CurrentColumn >= 0);
            }
        }

        protected virtual string NewLineCharacters
        {
            get
            {
                return "\r\n";
            }
        }

        private int NumColInt
        {
            get
            {
                return this.m_Columns.Count;
            }
        }

        private long NumRowsInt
        {
            get
            {
                return this.m_scrollMgr.RowsNumber;
            }
        }

        [Browsable(false)]
        public System.Drawing.Printing.PrintDocument PrintDocument
        {
            get
            {
                if (this.IsEmpty)
                {
                    throw new InvalidOperationException(SRError.CannotPrintEmptyGrid);
                }
                if ((this.NumRowsInt == 0L) && !this.m_withHeader)
                {
                    throw new InvalidOperationException(SRError.CannotPrintEmptyGrid);
                }
                return new GridPrintDocument(this.AllocateGridPrinter());
            }
        }

        [Browsable(false)]
        public int RowHeight
        {
            get
            {
                if (this.m_scrollMgr != null)
                {
                    return this.m_scrollMgr.CellHeight;
                }
                return -1;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public BlockOfCellsCollection SelectedCells
        {
            get
            {
                BlockOfCellsCollection col = new BlockOfCellsCollection();
                if (base.InvokeRequired)
                {
                    InvokerInOutArgs args = new InvokerInOutArgs();
                    args.InOutParam = col;
                    base.Invoke(new SelectedCellsInternalInvoker(this.SelectedCellsInternalForInvoke), new object[] { args, false });
                }
                else
                {
                    this.SelectedCellsInternal(col, false);
                }
                return this.AdjustColumnIndexesInSelectedCells(col, true);
            }
            set
            {
                if (base.InvokeRequired)
                {
                    InvokerInOutArgs args = new InvokerInOutArgs();
                    args.InOutParam = this.AdjustColumnIndexesInSelectedCells(value, false);
                    base.Invoke(new SelectedCellsInternalInvoker(this.SelectedCellsInternalForInvoke), new object[] { args, true });
                }
                else
                {
                    this.SelectedCellsInternal(this.AdjustColumnIndexesInSelectedCells(value, false), true);
                }
            }
        }

        [Description("Selection type of the grid"), DefaultValue(1), Category("Appearance")]
        public GridSelectionType SelectionType
        {
            get
            {
                return this.m_selMgr.SelectionType;
            }
            set
            {
                if (this.m_selMgr.SelectionType != value)
                {
                    if (!Enum.IsDefined(typeof(GridSelectionType), value))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(GridSelectionType));
                    }
                    this.m_selMgr.SelectionType = value;
                    if (base.IsHandleCreated && (this.m_nIsInitializingCount == 0))
                    {
                        this.m_selMgr.Clear();
                        this.OnSelectionChanged(this.m_selMgr.SelectedBlocks);
                        base.Invalidate();
                    }
                }
            }
        }

        protected virtual bool ShouldCommitEmbeddedControlOnLostFocus
        {
            get
            {
                return !this.m_bInGridStorageCall;
            }
        }

        private bool ShouldForwardCharToEmbeddedControl
        {
            get
            {
                return (((this.IsEditing && !this.FocusEditorOnNavigation) && this.IsCellEditableFromKeyboardNav()) && !this.m_curEmbeddedControl.ContainsFocus);
            }
        }

        [Browsable(false)]
        public static int StandardGridCheckBoxSize
        {
            get
            {
                return 13;
            }
        }

        protected virtual string StringForBitmapData
        {
            get
            {
                return string.Empty;
            }
        }

        protected virtual string StringForButtonsWithBmpOnly
        {
            get
            {
                return "<button>";
            }
        }

        [Browsable(false)]
        public int VisibleRowsNum
        {
            get
            {
                int num = this.m_scrollMgr.CalcVertPageSize(this.m_scrollableArea);
                if (num <= 0)
                {
                    num = s_nMaxNumOfVisibleRows;
                }
                return num;
            }
        }

        [DefaultValue(true), Description("Whether or not the header will be shown"), Category("Appearance")]
        public bool WithHeader
        {
            get
            {
                return this.m_withHeader;
            }
            set
            {
                if (this.m_withHeader != value)
                {
                    this.m_withHeader = value;
                    this.UpdateScrollableAreaRect();
                }
            }
        }
        #endregion

        #region Nested Types
        private delegate void AddColumnInvoker(GridColumnInfo ci);

        private delegate void AlwaysHighlightSelectionIntInvoker(bool bAlwaysHighlight);

        private delegate void DeleteColumnInvoker(int nIndex);

        private delegate void EnsureCellIsVisibleInvoker(long nRowIndex, int nColIndex);

        private delegate void GetColumnsNumberInternalInvoker(GridControl.InvokerInOutArgs args);

        private delegate void GetColumnWidthInternalInvoker(int nColIndex, GridControl.InvokerInOutArgs args);

        private delegate void GetHeaderInfoInvoker(int nIndex, GridControl.InvokerInOutArgs outArgs);

        private delegate void InsertColumnInvoker(int nIndex, GridColumnInfo ci);

        private delegate void IsACellBeingEditedInternalInvoker(GridControl.InvokerInOutArgs args);

        private delegate void RegisterEmbeddedControlInternalInvoker(int editableCellType, Control embeddedControl);

        private delegate void ResizeColumnToShowAllContentsInternalInvoker(int uiColumnIndex);

        private delegate void ResizeColumnToShowAllContentsInternalInvoker2(int uiColumnIndex, bool considerAllRows);

        private delegate void SelectedCellsInternalInvoker(GridControl.InvokerInOutArgs args, bool bSet);

        private delegate void SetColumnWidthInternalInvoker(int nColIndex, GridColumnWidthType widthType, int nWidth);

        private delegate void SetHeaderInfoInvoker(int nIndex, string strText, Bitmap bmp, GridCheckBoxState checkboxState);

        private delegate void SetMergedHeaderResizeProportionInternalInvoker(int colIndex, float proportion);

        private delegate void StartCellEditInternalInvoker(long nRowIndex, int nColIndex, GridControl.InvokerInOutArgs args);

        private delegate void StopCellEditInternalInvoker(GridControl.InvokerInOutArgs args, bool bCommitIntoStorage);

        private delegate void UpdateGridInvoker(bool bRecalcRows);

        internal protected class GridControlAccessibleObject : Control.ControlAccessibleObject
        {
            private IntPtr m_cachedHandle;
            private AccessibleObjectOnIAccessible m_cachedHScrollIAccessible;
            private AccessibleObjectOnIAccessible m_cachedVScrollIAccessible;
            protected internal static int s_HorizSBChildIndex = 0;
            protected internal static int s_VertSBChildIndex = 1;

            public GridControlAccessibleObject(GridControl owner) : base(owner)
            {
                this.m_cachedHandle = IntPtr.Zero;
            }

            protected internal AccessibleObject GetAccessibleForScrollBar(bool bHoriz)
            {
                if (this.m_cachedHandle != base.Handle)
                {
                    this.m_cachedHandle = base.Handle;
                    Guid refiid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
                    object pAcc = null;
                    NativeMethods.CreateStdAccessibleObject(base.Handle, -5, ref refiid, ref pAcc);
                    if (pAcc != null)
                    {
                        this.m_cachedVScrollIAccessible = new AccessibleObjectOnIAccessible((IAccessible) pAcc, this);
                    }
                    pAcc = null;
                    NativeMethods.CreateStdAccessibleObject(base.Handle, -6, ref refiid, ref pAcc);
                    if (pAcc != null)
                    {
                        this.m_cachedHScrollIAccessible = new AccessibleObjectOnIAccessible((IAccessible) pAcc, this);
                    }
                }
                if (!bHoriz)
                {
                    return this.m_cachedVScrollIAccessible;
                }
                return this.m_cachedHScrollIAccessible;
            }

            public override AccessibleObject GetChild(int index)
            {
                if (index == s_HorizSBChildIndex)
                {
                    return this.GetAccessibleForScrollBar(true);
                }
                if (index == s_VertSBChildIndex)
                {
                    return this.GetAccessibleForScrollBar(false);
                }
                index -= 2;
                if (index < this.Grid.m_Columns.Count)
                {
                    return new ColumnAccessibleObject(this, index);
                }
                return null;
            }

            public override int GetChildCount()
            {
                return (2 + this.Grid.m_Columns.Count);
            }

            public override AccessibleObject HitTest(int xLeft, int yTop)
            {
                Point point = this.Grid.PointToClient(new Point(xLeft, yTop));
                HitTestInfo info = this.Grid.HitTestInternal(point.X, point.Y);
                if ((((info.HitTestResult != HitTestResult.BitmapCell) && (info.HitTestResult != HitTestResult.ButtonCell)) && ((info.HitTestResult != HitTestResult.TextCell) && (info.HitTestResult != HitTestResult.CustomCell))) && (((info.HitTestResult != HitTestResult.ColumnResize) && (info.HitTestResult != HitTestResult.HeaderButton)) && (info.HitTestResult != HitTestResult.HyperlinkCell)))
                {
                    return null;
                }
                return new ColumnAccessibleObject(this, info.ColumnIndex);
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.Grid.RectangleToScreen(this.Grid.ClientRectangle);
                }
            }

            protected internal GridControl Grid
            {
                get
                {
                    return (GridControl) base.Owner;
                }
            }

            internal protected class AccessibleObjectOnIAccessible : AccessibleObject
            {
                private IAccessible m_acc;
                private int m_childID;
                private AccessibleObject m_parent;

                public AccessibleObjectOnIAccessible(IAccessible acc, AccessibleObject parent) : this(acc, parent, 0)
                {
                }

                public AccessibleObjectOnIAccessible(IAccessible acc, AccessibleObject parent, int nChildId)
                {
                    this.m_acc = acc;
                    this.m_parent = parent;
                    this.m_childID = nChildId;
                }

                public override void DoDefaultAction()
                {
                    if (this.m_acc != null)
                    {
                        try
                        {
                            this.m_acc.accDoDefaultAction(this.m_childID);
                        }
                        catch (COMException exception)
                        {
                            if (exception.ErrorCode != -2147352573)
                            {
                                throw exception;
                            }
                        }
                    }
                }

                public override AccessibleObject GetChild(int index)
                {
                    if (this.m_acc == null)
                    {
                        return null;
                    }
                    IAccessible acc = (IAccessible) this.m_acc.get_accChild(index + 1);
                    if (acc != null)
                    {
                        return new GridControl.GridControlAccessibleObject.AccessibleObjectOnIAccessible(acc, this);
                    }
                    return new GridControl.GridControlAccessibleObject.AccessibleObjectOnIAccessible(this.m_acc, this, index + 1);
                }

                public override int GetChildCount()
                {
                    if ((this.m_acc != null) && (this.m_childID == 0))
                    {
                        return this.m_acc.accChildCount;
                    }
                    return -1;
                }

                public override int GetHelpTopic(out string fileName)
                {
                    if (this.m_acc != null)
                    {
                        try
                        {
                            return this.m_acc.get_accHelpTopic(out fileName, this.m_childID);
                        }
                        catch (COMException exception)
                        {
                            if (exception.ErrorCode != -2147352573)
                            {
                                throw exception;
                            }
                        }
                    }
                    fileName = null;
                    return -1;
                }

                public override AccessibleObject Navigate(AccessibleNavigation navdir)
                {
                    if (this.m_acc != null)
                    {
                        int childCount = this.GetChildCount();
                        if (childCount > 0)
                        {
                            if (navdir == AccessibleNavigation.FirstChild)
                            {
                                return this.GetChild(0);
                            }
                            if (navdir == AccessibleNavigation.LastChild)
                            {
                                return this.GetChild(childCount - 1);
                            }
                        }
                        try
                        {
                            IAccessible acc = (IAccessible) this.m_acc.accNavigate((int) navdir, this.m_childID);
                            if (acc != null)
                            {
                                return new GridControl.GridControlAccessibleObject.AccessibleObjectOnIAccessible(acc, this);
                            }
                            return null;
                        }
                        catch (COMException exception)
                        {
                            if (exception.ErrorCode != -2147352573)
                            {
                                throw exception;
                            }
                        }
                    }
                    return null;
                }

                public override void Select(AccessibleSelection flags)
                {
                    if (this.m_acc != null)
                    {
                        try
                        {
                            this.m_acc.accSelect((int) flags, this.m_childID);
                        }
                        catch (COMException exception)
                        {
                            if (exception.ErrorCode != -2147352573)
                            {
                                throw exception;
                            }
                        }
                    }
                }

                public override Rectangle Bounds
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            int pxLeft = 0;
                            int pyTop = 0;
                            int pcxWidth = 0;
                            int pcyHeight = 0;
                            try
                            {
                                this.m_acc.accLocation(out pxLeft, out pyTop, out pcxWidth, out pcyHeight, this.m_childID);
                                return new Rectangle(pxLeft, pyTop, pcxWidth, pcyHeight);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return Rectangle.Empty;
                    }
                }

                public override string DefaultAction
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                return this.m_acc.get_accDefaultAction(this.m_childID);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return null;
                    }
                }

                public override string Description
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                return this.m_acc.get_accDescription(this.m_childID);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return null;
                    }
                }

                public override string Help
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                return this.m_acc.get_accHelp(this.m_childID);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return null;
                    }
                }

                public override string KeyboardShortcut
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                return this.m_acc.get_accKeyboardShortcut(this.m_childID);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return null;
                    }
                }

                public override string Name
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                return this.m_acc.get_accName(this.m_childID);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return null;
                    }
                    set
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                this.m_acc.set_accName(this.m_childID, value);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                    }
                }

                public override AccessibleObject Parent
                {
                    get
                    {
                        return this.m_parent;
                    }
                }

                public override AccessibleRole Role
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            return (AccessibleRole) this.m_acc.get_accRole(this.m_childID);
                        }
                        return AccessibleRole.None;
                    }
                }

                public override AccessibleStates State
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            return (AccessibleStates) this.m_acc.get_accState(this.m_childID);
                        }
                        return AccessibleStates.None;
                    }
                }

                public override string Value
                {
                    get
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                return this.m_acc.get_accValue(this.m_childID);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                        return "";
                    }
                    set
                    {
                        if (this.m_acc != null)
                        {
                            try
                            {
                                this.m_acc.set_accValue(this.m_childID, value);
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147352573)
                                {
                                    throw exception;
                                }
                            }
                        }
                    }
                }
            }

            internal protected class ColumnAccessibleObject : AccessibleObject
            {
                protected int m_colIndex;
                private GridControl.GridControlAccessibleObject m_parent;

                private ColumnAccessibleObject()
                {
                    this.m_colIndex = -1;
                }

                public ColumnAccessibleObject(GridControl.GridControlAccessibleObject parent, int nColIndex)
                {
                    this.m_colIndex = -1;
                    this.m_colIndex = nColIndex;
                    this.m_parent = parent;
                }

                public override AccessibleObject GetChild(int index)
                {
                    if (this.Grid.WithHeader)
                    {
                        if (index == 0)
                        {
                            return new HeaderAccessibleObject(this, this.m_colIndex);
                        }
                        index--;
                    }
                    if (index < this.Grid.GridStorage.NumRows())
                    {
                        return new CellAccessibleObject(this, (long) index, this.m_colIndex);
                    }
                    return null;
                }

                public override int GetChildCount()
                {
                    int num = 0;
                    if (this.Grid.WithHeader)
                    {
                        num = 1;
                    }
                    return (num + ((int) this.Grid.GridStorage.NumRows()));
                }

                public override AccessibleObject GetFocused()
                {
                    if (this.Grid.m_selMgr.CurrentColumn == this.m_colIndex)
                    {
                        return new CellAccessibleObject(this, this.Grid.m_selMgr.CurrentRow, this.m_colIndex);
                    }
                    return null;
                }

                public override AccessibleObject HitTest(int xLeft, int yTop)
                {
                    Point point = this.Grid.PointToClient(new Point(xLeft, yTop));
                    HitTestInfo info = this.Grid.HitTestInternal(point.X, point.Y);
                    if (((info.HitTestResult == HitTestResult.BitmapCell) || (info.HitTestResult == HitTestResult.ButtonCell)) || (((info.HitTestResult == HitTestResult.TextCell) || (info.HitTestResult == HitTestResult.CustomCell)) || (info.HitTestResult == HitTestResult.HyperlinkCell)))
                    {
                        if (info.ColumnIndex == this.m_colIndex)
                        {
                            return new CellAccessibleObject(this, info.RowIndex, info.ColumnIndex);
                        }
                    }
                    else if ((info.HitTestResult == HitTestResult.HeaderButton) && (info.ColumnIndex == this.m_colIndex))
                    {
                        return new HeaderAccessibleObject(this, info.ColumnIndex);
                    }
                    return null;
                }

                public override Rectangle Bounds
                {
                    get
                    {
                        Rectangle cellRectangle = this.Grid.m_scrollMgr.GetCellRectangle(this.Grid.m_scrollMgr.FirstRowIndex, this.m_colIndex);
                        if (!cellRectangle.Equals(Rectangle.Empty))
                        {
                            cellRectangle.Y = 0;
                            cellRectangle.Height = this.Grid.ClientRectangle.Height;
                            return this.Grid.RectangleToScreen(cellRectangle);
                        }
                        return Rectangle.Empty;
                    }
                }

                protected internal GridControl Grid
                {
                    get
                    {
                        return this.m_parent.Grid;
                    }
                }

                public override string Name
                {
                    get
                    {
                        return SR.ColumnNumber(this.m_colIndex);
                    }
                    set
                    {
                    }
                }

                public override AccessibleObject Parent
                {
                    get
                    {
                        return this.m_parent;
                    }
                }

                public override AccessibleRole Role
                {
                    get
                    {
                        return AccessibleRole.Column;
                    }
                }

                public override AccessibleStates State
                {
                    get
                    {
                        AccessibleStates states = AccessibleStates.Selectable | AccessibleStates.Focusable;
                        if ((this.m_colIndex >= this.Grid.m_scrollMgr.FirstScrollableColumnIndex) && ((this.m_colIndex < this.Grid.m_scrollMgr.FirstColumnIndex) || (this.m_colIndex > this.Grid.m_scrollMgr.LastColumnIndex)))
                        {
                            states |= AccessibleStates.Invisible;
                        }
                        if (this.Grid.m_selMgr.CurrentColumn == this.m_colIndex)
                        {
                            states |= AccessibleStates.Focused;
                        }
                        return states;
                    }
                }

                protected class CellAccessibleObject : AccessibleObject
                {
                    protected int m_colIndex = -1;
                    private GridControl.GridControlAccessibleObject.ColumnAccessibleObject m_parent;
                    protected long m_rowIndex = -1L;

                    public CellAccessibleObject(GridControl.GridControlAccessibleObject.ColumnAccessibleObject parent, long nRowIndex, int nColIndex)
                    {
                        this.m_parent = parent;
                        this.m_rowIndex = nRowIndex;
                        this.m_colIndex = nColIndex;
                    }

                    public override void DoDefaultAction()
                    {
                        if (((this.m_colIndex >= 0) && (this.m_colIndex < this.GridColumnsNumber)) && ((this.m_rowIndex >= 0L) && (this.m_rowIndex < this.GridRowsNumber)))
                        {
                            Rectangle cellRectangle = this.Grid.m_scrollMgr.GetCellRectangle(this.m_rowIndex, this.m_colIndex);
                            if (cellRectangle.Equals(Rectangle.Empty))
                            {
                                this.Grid.EnsureCellIsVisible(this.m_rowIndex, this.m_colIndex);
                                cellRectangle = this.Grid.m_scrollMgr.GetCellRectangle(this.m_rowIndex, this.m_colIndex);
                            }
                            if (this.Grid.OnMouseButtonClicking(this.m_rowIndex, this.m_colIndex, cellRectangle, Control.ModifierKeys, MouseButtons.Left))
                            {
                                this.Grid.m_selMgr.Clear();
                                int editType = this.Grid.m_gridStorage.IsCellEditable(this.m_rowIndex, this.Grid.m_Columns[this.m_colIndex].ColumnIndex);
                                if (editType != 0)
                                {
                                    bool bSendMouseClick = false;
                                    this.Grid.StartEditCell(this.m_rowIndex, this.m_colIndex, cellRectangle, editType, ref bSendMouseClick);
                                }
                                this.Grid.m_selMgr.StartNewBlock(this.m_rowIndex, this.m_colIndex);
                                this.Grid.OnMouseButtonClicked(this.m_rowIndex, this.m_colIndex, cellRectangle, MouseButtons.Left);
                                this.Grid.Refresh();
                                this.Grid.OnSelectionChanged(this.Grid.m_selMgr.SelectedBlocks);
                            }
                        }
                    }

                    public override AccessibleObject GetChild(int childID)
                    {
                        if (!this.IsCellBeingEdited)
                        {
                            return null;
                        }
                        if (childID == 0)
                        {
                            return this.Grid.m_curEmbeddedControl.AccessibilityObject;
                        }
                        return new EmbeddedEditOperationAccessibleObject(this, this.m_rowIndex, this.m_colIndex, childID == 1);
                    }

                    public override int GetChildCount()
                    {
                        if (this.IsCellBeingEdited)
                        {
                            return 3;
                        }
                        return 0;
                    }

                    public override AccessibleObject HitTest(int xLeft, int yTop)
                    {
                        if (this.IsCellBeingEdited)
                        {
                            return this.Grid.m_curEmbeddedControl.AccessibilityObject;
                        }
                        return null;
                    }

                    public override void Select(AccessibleSelection accessibleSelection)
                    {
                        if ((AccessibleSelection.TakeSelection & accessibleSelection) != AccessibleSelection.None)
                        {
                            this.Grid.m_selMgr.Clear();
                            if (AccessibleSelection.TakeSelection == accessibleSelection)
                            {
                                this.Grid.m_selMgr.StartNewBlock(this.m_rowIndex, this.m_colIndex);
                                this.Grid.Refresh();
                                return;
                            }
                        }
                        if ((AccessibleSelection.TakeFocus & accessibleSelection) != AccessibleSelection.None)
                        {
                            if (((AccessibleSelection.TakeSelection & accessibleSelection) != AccessibleSelection.None) || ((AccessibleSelection.AddSelection & accessibleSelection) != AccessibleSelection.None))
                            {
                                this.Grid.m_selMgr.StartNewBlock(this.m_rowIndex, this.m_colIndex);
                            }
                            else if ((AccessibleSelection.ExtendSelection & accessibleSelection) != AccessibleSelection.None)
                            {
                                this.Grid.m_selMgr.UpdateCurrentBlock(this.m_rowIndex, this.m_colIndex);
                                this.Grid.m_selMgr.CurrentRow = this.m_rowIndex;
                                this.Grid.m_selMgr.CurrentColumn = this.m_colIndex;
                            }
                            else if ((AccessibleSelection.RemoveSelection & accessibleSelection) == AccessibleSelection.None)
                            {
                                this.Grid.m_selMgr.StartNewBlock(this.m_rowIndex, this.m_colIndex);
                            }
                            this.Grid.Refresh();
                            return;
                        }
                        switch (accessibleSelection)
                        {
                            case AccessibleSelection.ExtendSelection:
                                this.Grid.m_selMgr.UpdateCurrentBlock(this.m_rowIndex, this.m_colIndex);
                                goto Label_0158;

                            case AccessibleSelection.AddSelection:
                                this.Grid.m_selMgr.StartNewBlock(this.m_rowIndex, this.m_colIndex);
                                break;

                            case AccessibleSelection.RemoveSelection:
                                this.Grid.m_selMgr.StartNewBlockOrExcludeCell(this.m_rowIndex, this.m_colIndex);
                                goto Label_0158;
                        }
                    Label_0158:
                        this.Grid.Refresh();
                    }

                    public override Rectangle Bounds
                    {
                        get
                        {
                            return this.Grid.RectangleToScreen(this.Grid.m_scrollMgr.GetCellRectangle(this.m_rowIndex, this.m_colIndex));
                        }
                    }

                    public override string DefaultAction
                    {
                        get
                        {
                            if (this.Grid.m_gridStorage.IsCellEditable(this.m_rowIndex, this.Grid.m_Columns[this.m_colIndex].ColumnIndex) != 0)
                            {
                                return SR.StartEditCell;
                            }
                            return SR.ClearSelAndSelectCell;
                        }
                    }

                    protected GridControl Grid
                    {
                        get
                        {
                            return this.m_parent.Grid;
                        }
                    }

                    protected int GridColumnsNumber
                    {
                        get
                        {
                            return this.Grid.m_Columns.Count;
                        }
                    }

                    protected long GridRowsNumber
                    {
                        get
                        {
                            return this.Grid.GridStorage.NumRows();
                        }
                    }

                    protected bool IsCellBeingEdited
                    {
                        get
                        {
                            long num;
                            int num2;
                            this.Grid.IsACellBeingEdited(out num, out num2);
                            return ((this.m_rowIndex == num) && (this.m_colIndex == num2));
                        }
                    }

                    public override string Name
                    {
                        get
                        {
                            return SR.CellDefaultAccessibleName(this.m_rowIndex, this.m_colIndex);
                        }
                    }

                    public override AccessibleObject Parent
                    {
                        get
                        {
                            return this.m_parent;
                        }
                    }

                    public override AccessibleRole Role
                    {
                        get
                        {
                            return AccessibleRole.Cell;
                        }
                    }

                    public override AccessibleStates State
                    {
                        get
                        {
                            if ((this.m_colIndex >= this.GridColumnsNumber) || (this.m_rowIndex >= this.GridRowsNumber))
                            {
                                return AccessibleStates.None;
                            }
                            AccessibleStates accessibleState = this.Grid.m_Columns[this.m_colIndex].GetAccessibleState(this.m_rowIndex, this.Grid.GridStorage);
                            AccessibleStates states2 = AccessibleStates.Selectable | AccessibleStates.Focusable;
                            if ((this.Grid.m_selMgr.CurrentColumn == this.m_colIndex) && (this.Grid.m_selMgr.CurrentRow == this.m_rowIndex))
                            {
                                states2 |= AccessibleStates.Focused;
                            }
                            if (this.Grid.m_selMgr.IsCellSelected(this.m_rowIndex, this.m_colIndex))
                            {
                                states2 |= AccessibleStates.Selected;
                            }
                            if (this.Grid.SelectionType != GridSelectionType.SingleCell)
                            {
                                states2 |= AccessibleStates.MultiSelectable;
                            }
                            if (Rectangle.Empty.Equals(this.Grid.m_scrollMgr.GetCellRectangle(this.m_rowIndex, this.m_colIndex)))
                            {
                                states2 |= AccessibleStates.Invisible;
                            }
                            if (states2 != AccessibleStates.None)
                            {
                                return (accessibleState | states2);
                            }
                            return states2;
                        }
                    }

                    public override string Value
                    {
                        get
                        {
                            if ((this.m_colIndex < this.GridColumnsNumber) && (this.m_rowIndex < this.GridRowsNumber))
                            {
                                return this.Grid.m_Columns[this.m_colIndex].GetAccessibleValue(this.m_rowIndex, this.Grid.GridStorage);
                            }
                            return null;
                        }
                        set
                        {
                            if (((this.m_colIndex < this.GridColumnsNumber) && (this.m_rowIndex < this.GridRowsNumber)) && ((value != null) && !this.Grid.IsEditing))
                            {
                                int num = this.Grid.GridStorage.IsCellEditable(this.m_rowIndex, this.Grid.m_Columns[this.m_colIndex].ColumnIndex);
                                if (num != 0)
                                {
                                    Control control = (Control) this.Grid.m_EmbeddedControls[num];
                                    if (control != null)
                                    {
                                        IGridEmbeddedControl control2 = (IGridEmbeddedControl) control;
                                        control2.AddDataAsString(value);
                                        control2.SetCurSelectionAsString(value);
                                        try
                                        {
                                            this.Grid.m_bInGridStorageCall = true;
                                            if (this.Grid.GridStorage.SetCellDataFromControl(this.m_rowIndex, this.Grid.m_Columns[this.m_colIndex].ColumnIndex, control2))
                                            {
                                                this.Grid.Refresh();
                                            }
                                        }
                                        finally
                                        {
                                            this.Grid.m_bInGridStorageCall = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    protected class EmbeddedEditOperationAccessibleObject : AccessibleObject
                    {
                        protected int m_colIndex;
                        protected bool m_commit;
                        protected GridControl.GridControlAccessibleObject.ColumnAccessibleObject.CellAccessibleObject m_parent;
                        protected long m_rowIndex;

                        public EmbeddedEditOperationAccessibleObject(GridControl.GridControlAccessibleObject.ColumnAccessibleObject.CellAccessibleObject parent, long nRowIndex, int nColIndex, bool bCommit)
                        {
                            this.m_parent = parent;
                            this.m_commit = bCommit;
                            this.m_rowIndex = nRowIndex;
                            this.m_colIndex = nColIndex;
                        }

                        public override void DoDefaultAction()
                        {
                            if (this.m_parent.IsCellBeingEdited)
                            {
                                this.m_parent.Grid.StopCellEdit(this.m_commit);
                            }
                        }

                        public override string DefaultAction
                        {
                            get
                            {
                                if (this.m_commit)
                                {
                                    return SR.EmbeddedEditCommitDefaultAction;
                                }
                                return SR.EmbeddedEditCancelDefaultAction;
                            }
                        }

                        public override string Name
                        {
                            get
                            {
                                if (!this.m_commit)
                                {
                                    return SR.EmbeddedEditCancelAAName;
                                }
                                return SR.EmbeddedEditCommitAAName;
                            }
                        }

                        public override AccessibleObject Parent
                        {
                            get
                            {
                                return this.m_parent;
                            }
                        }
                    }
                }

                protected class HeaderAccessibleObject : AccessibleObject
                {
                    private int m_colIndex = -1;
                    private GridControl.GridControlAccessibleObject.ColumnAccessibleObject m_parent;

                    public HeaderAccessibleObject(GridControl.GridControlAccessibleObject.ColumnAccessibleObject parent, int nColIndex)
                    {
                        this.m_parent = parent;
                        this.m_colIndex = nColIndex;
                    }

                    public override void DoDefaultAction()
                    {
                        this.Grid.OnHeaderButtonClicked(this.m_colIndex, MouseButtons.Left, GridButtonArea.Nothing);
                        this.Grid.Refresh();
                    }

                    public override Rectangle Bounds
                    {
                        get
                        {
                            Rectangle cellRectangle = this.Grid.m_scrollMgr.GetCellRectangle(this.Grid.m_scrollMgr.FirstRowIndex, this.m_colIndex);
                            if (!cellRectangle.Equals(Rectangle.Empty))
                            {
                                cellRectangle.Y = 0;
                                cellRectangle.Height = this.Grid.HeaderHeight;
                                return this.Grid.RectangleToScreen(cellRectangle);
                            }
                            return Rectangle.Empty;
                        }
                    }

                    public override string DefaultAction
                    {
                        get
                        {
                            return SR.ColumnHeaderAADefaultAction;
                        }
                    }

                    protected GridControl Grid
                    {
                        get
                        {
                            return this.m_parent.Grid;
                        }
                    }

                    public override string Name
                    {
                        get
                        {
                            return SR.ColumnHeaderAAName(this.m_colIndex);
                        }
                    }

                    public override AccessibleObject Parent
                    {
                        get
                        {
                            return this.m_parent;
                        }
                    }

                    public override AccessibleRole Role
                    {
                        get
                        {
                            return AccessibleRole.ColumnHeader;
                        }
                    }

                    public override AccessibleStates State
                    {
                        get
                        {
                            AccessibleStates none = AccessibleStates.None;
                            if ((this.m_colIndex < this.Grid.m_scrollMgr.FirstScrollableColumnIndex) || ((this.m_colIndex >= this.Grid.m_scrollMgr.FirstColumnIndex) && (this.m_colIndex <= this.Grid.m_scrollMgr.LastColumnIndex)))
                            {
                                return none;
                            }
                            return (none | AccessibleStates.Invisible);
                        }
                    }

                    public override string Value
                    {
                        get
                        {
                            if ((this.m_colIndex >= 0) && (this.m_colIndex < this.Grid.m_Columns.Count))
                            {
                                return this.Grid.m_gridHeader[this.m_colIndex].Text;
                            }
                            return null;
                        }
                        set
                        {
                        }
                    }
                }
            }
        }

        private class InvokerInOutArgs
        {
            internal object InOutParam;
            internal object InOutParam2;
            internal object InOutParam3;
        }

        protected class TooltipInfo
        {
            public int ColumnNumber = -1;
            public HitTestResult HitTest;
            public long RowNumber = -1L;

            public void Reset()
            {
                this.RowNumber = -1L;
                this.ColumnNumber = -1;
                this.HitTest = HitTestResult.Nothing;
            }
        }

        #endregion
    }
}

