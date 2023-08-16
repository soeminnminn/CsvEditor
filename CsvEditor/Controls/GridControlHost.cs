
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.SqlServer.Management.UI.Grid;


namespace CsvEditor.Controls
{
    public class GridControlHost : WindowsFormsHost, IGridControl
    {
        #region Variables
        private readonly GridControl gridControl;
        #endregion

        #region Events
        public event ColumnReorderRequestedEventHandler ColumnReorderRequested;
        public event ColumnsReorderedEventHandler ColumnsReordered;
        public event Microsoft.SqlServer.Management.UI.Grid.ColumnWidthChangedEventHandler ColumnWidthChanged;
        public event CustomizeCellGDIObjectsEventHandler CustomizeCellGDIObjects;
        public event EmbeddedControlContentsChangedEventHandler EmbeddedControlContentsChanged;
        public event GridSpecialEventHandler GridSpecialEvent;
        public event HeaderButtonClickedEventHandler HeaderButtonClicked;
        public event KeyPressedOnCellEventHandler KeyPressedOnCell;
        public event MouseButtonClickedEventHandler MouseButtonClicked;
        public event MouseButtonClickingEventHandler MouseButtonClicking;
        public event MouseButtonDoubleClickedEventHandler MouseButtonDoubleClicked;
        public event SelectionChangedEventHandler SelectionChanged;
        public event StandardKeyProcessingEventHandler StandardKeyProcessing;
        public event TooltipDataNeededEventHandler TooltipDataNeeded;
        #endregion

        #region Constructors
        static GridControlHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridControlHost), new FrameworkPropertyMetadata(typeof(GridControlHost)));

            FontFamilyProperty.OverrideMetadata(typeof(GridControlHost), new FrameworkPropertyMetadata(OnFontFamilyChanged));
            FontSizeProperty.OverrideMetadata(typeof(GridControlHost), new FrameworkPropertyMetadata(OnFontSizeChanged));
            FontWeightProperty.OverrideMetadata(typeof(GridControlHost), new FrameworkPropertyMetadata(OnFontWeightChanged));
            FontStyleProperty.OverrideMetadata(typeof(GridControlHost), new FrameworkPropertyMetadata(OnFontStyleChanged));
        }

        public GridControlHost()
        {
            gridControl = new GridControl();
            Child = gridControl;

            gridControl.ColumnReorderRequested += GridControl_ColumnReorderRequested;
            gridControl.ColumnsReordered += GridControl_ColumnsReordered;
            gridControl.ColumnWidthChanged += GridControl_ColumnWidthChanged;
            gridControl.CustomizeCellGDIObjects += GridControl_CustomizeCellGDIObjects;
            gridControl.EmbeddedControlContentsChanged += GridControl_EmbeddedControlContentsChanged;
            gridControl.GridSpecialEvent += GridControl_GridSpecialEvent;
            gridControl.HeaderButtonClicked += GridControl_HeaderButtonClicked;
            gridControl.KeyPressedOnCell += GridControl_KeyPressedOnCell;
            gridControl.MouseButtonClicked += GridControl_MouseButtonClicked;
            gridControl.MouseButtonClicking += GridControl_MouseButtonClicking;
            gridControl.MouseButtonDoubleClicked += GridControl_MouseButtonDoubleClicked;
            gridControl.SelectionChanged += GridControl_SelectionChanged;
            gridControl.StandardKeyProcessing += GridControl_StandardKeyProcessing;
            gridControl.TooltipDataNeeded += GridControl_TooltipDataNeeded;
        }
        #endregion

        #region Properties
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public GridControl Grid
        {
            get => gridControl;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool AlwaysHighlightSelection 
        {
            get => gridControl.AlwaysHighlightSelection;
            set { gridControl.AlwaysHighlightSelection = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int AutoScrollInterval 
        {
            get => gridControl.AutoScrollInterval;
            set { gridControl.AutoScrollInterval = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BorderStyle BorderStyle 
        {
            get => gridControl.BorderStyle;
            set { gridControl.BorderStyle = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ColumnsNumber
        {
            get => gridControl.ColumnsNumber;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ColumnsReorderableByDefault 
        { 
            get => gridControl.ColumnsReorderableByDefault; 
            set { gridControl.ColumnsReorderableByDefault = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int FirstScrollableColumn 
        {
            get => gridControl.FirstScrollableColumn;
            set { gridControl.FirstScrollableColumn = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public uint FirstScrollableRow 
        {
            get => gridControl.FirstScrollableRow;
            set { gridControl.FirstScrollableRow = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool FocusEditorOnNavigation 
        {
            get => gridControl.FocusEditorOnNavigation;
            set { gridControl.FocusEditorOnNavigation = value; } 
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public GridColumnInfoCollection GridColumnsInfo
        {
            get => gridControl.GridColumnsInfo;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public GridLineType GridLineType 
        {
            get => gridControl.GridLineType;
            set { gridControl.GridLineType = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IGridStorage GridStorage 
        {
            get => gridControl.GridStorage;
            set { gridControl.GridStorage = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Font HeaderFont 
        {
            get => gridControl.HeaderFont;
            set { gridControl.HeaderFont = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int HeaderHeight
        {
            get => gridControl.HeaderHeight;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color HighlightColor
        {
            get => gridControl.HighlightColor;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MarginsWidth
        {
            get => gridControl.MarginsWidth;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PrintDocument PrintDocument
        {
            get => gridControl.PrintDocument;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int RowHeight
        {
            get => gridControl.RowHeight;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BlockOfCellsCollection SelectedCells 
        {
            get => gridControl.SelectedCells;
            set { gridControl.SelectedCells = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public GridSelectionType SelectionType 
        {
            get => gridControl.SelectionType;
            set { gridControl.SelectionType = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int VisibleRowsNum
        {
            get => gridControl.VisibleRowsNum;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool WithHeader 
        {
            get => gridControl.WithHeader;
            set { gridControl.WithHeader = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Control EditingEmbeddedControl
        {
            get => gridControl.EditingEmbeddedControl;
        }
        #endregion

        #region Events Methods
        private void GridControl_ColumnReorderRequested(object sender, ColumnReorderRequestedEventArgs a)
        {
            ColumnReorderRequested?.Invoke(sender, a);
        }

        private void GridControl_ColumnsReordered(object sender, ColumnsReorderedEventArgs a)
        {
            ColumnsReordered?.Invoke(sender, a);
        }

        private void GridControl_ColumnWidthChanged(object sender, Microsoft.SqlServer.Management.UI.Grid.ColumnWidthChangedEventArgs args)
        {
            ColumnWidthChanged?.Invoke(sender, args);
        }

        private void GridControl_CustomizeCellGDIObjects(object sender, CustomizeCellGDIObjectsEventArgs args)
        {
            CustomizeCellGDIObjects?.Invoke(sender, args);
        }

        private void GridControl_EmbeddedControlContentsChanged(object sender, EmbeddedControlContentsChangedEventArgs args)
        {
            EmbeddedControlContentsChanged?.Invoke(sender, args);
        }

        private void GridControl_GridSpecialEvent(object sender, GridSpecialEventArgs sea)
        {
            GridSpecialEvent?.Invoke(sender, sea);
        }

        private void GridControl_HeaderButtonClicked(object sender, HeaderButtonClickedEventArgs args)
        {
            HeaderButtonClicked?.Invoke(sender, args);
        }

        private void GridControl_KeyPressedOnCell(object sender, KeyPressedOnCellEventArgs args)
        {
            KeyPressedOnCell?.Invoke(sender, args);
        }

        private void GridControl_MouseButtonClicked(object sender, MouseButtonClickedEventArgs args)
        {
            MouseButtonClicked?.Invoke(sender, args);
        }

        private void GridControl_MouseButtonClicking(object sender, MouseButtonClickingEventArgs args)
        {
            MouseButtonClicking?.Invoke(sender, args);
        }

        private void GridControl_MouseButtonDoubleClicked(object sender, MouseButtonDoubleClickedEventArgs args)
        {
            MouseButtonDoubleClicked?.Invoke(sender, args);
        }

        private void GridControl_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(sender, args);
        }

        private void GridControl_StandardKeyProcessing(object sender, StandardKeyProcessingEventArgs args)
        {
            StandardKeyProcessing?.Invoke(sender, args);
        }

        private void GridControl_TooltipDataNeeded(object sender, TooltipDataNeededEventArgs a)
        {
            TooltipDataNeeded?.Invoke(sender, a);
        }
        #endregion

        #region IGridControl Methods
        public void AddColumn(GridColumnInfo ci) 
            => gridControl.AddColumn(ci);

        public void DeleteColumn(int nIndex) 
            => gridControl.DeleteColumn(nIndex);

        public void EnsureCellIsVisible(long nRowIndex, int nColIndex) 
            => gridControl.EnsureCellIsVisible(nRowIndex, nColIndex);

        public int GetColumnWidth(int nColIndex) 
            => gridControl.GetColumnWidth(nColIndex);

        public void GetCurrentCell(out long rowIndex, out int columnIndex) 
            => gridControl.GetCurrentCell(out rowIndex, out columnIndex);

        public System.Windows.Forms.DataObject GetDataObject(bool bOnlyCurrentSelBlock, string columnsSeparator = null) 
            => gridControl.GetDataObject(bOnlyCurrentSelBlock, columnsSeparator);

        public GridColumnInfo GetGridColumnInfo(int columnIndex) 
            => gridControl.GetGridColumnInfo(columnIndex);

        public void GetHeaderInfo(int colIndex, out string headerText, out GridCheckBoxState headerCheckBox) 
            => gridControl.GetHeaderInfo(colIndex, out headerText, out headerCheckBox);

        public void GetHeaderInfo(int colIndex, out string headerText, out Bitmap headerBitmap)
            => gridControl.GetHeaderInfo(colIndex, out headerText, out headerBitmap);

        public int GetStorageColumnIndexByUIIndex(int indexInUI)
            => gridControl.GetStorageColumnIndexByUIIndex(indexInUI);

        public int GetUIColumnIndexByStorageIndex(int indexInStorage)
            => gridControl.GetUIColumnIndexByStorageIndex(indexInStorage);

        public Rectangle GetVisibleCellRectangle(long rowIndex, int columnIndex)
            => gridControl.GetVisibleCellRectangle(rowIndex, columnIndex);

        public HitTestInfo HitTest(int mouseX, int mouseY)
            => gridControl.HitTest(mouseX, mouseY);

        public void InsertColumn(int nIndex, GridColumnInfo ci)
            => gridControl.InsertColumn(nIndex, ci);

        public bool IsACellBeingEdited(out long nRowNum, out int nColNum)
            => gridControl.IsACellBeingEdited(out nRowNum, out nColNum);

        public void RegisterEmbeddedControl(int editableCellType, Control embeddedControl)
            => gridControl.RegisterEmbeddedControl(editableCellType, embeddedControl);

        public void ResetGrid()
            => gridControl.ResetGrid();

        public void ResizeColumnToShowAllContents(int columnIndex)
            => gridControl.ResizeColumnToShowAllContents(columnIndex);

        public void SetBitmapsForCheckBoxColumn(int nColIndex, Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState)
            => gridControl.SetBitmapsForCheckBoxColumn(nColIndex, checkedState, uncheckedState, indeterminateState, disabledState);

        public void SetColumnWidth(int nColIndex, GridColumnWidthType widthType, int nWidth)
            => gridControl.SetColumnWidth(nColIndex, widthType, nWidth);

        public void SetHeaderInfo(int colIndex, string strText, GridCheckBoxState checkboxState)
            => gridControl.SetHeaderInfo(colIndex, strText, checkboxState);

        public void SetHeaderInfo(int nColIndex, string strText, Bitmap bmp)
            => gridControl.SetHeaderInfo(nColIndex, strText, bmp);

        public void SetMergedHeaderResizeProportion(int colIndex, float proportion)
            => gridControl.SetMergedHeaderResizeProportion(colIndex, proportion);

        public bool StartCellEdit(long nRowIndex, int nColIndex)
            => gridControl.StartCellEdit(nRowIndex, nColIndex);

        public bool StopCellEdit(bool bCommitIntoStorage)
            => gridControl.StopCellEdit(bCommitIntoStorage);

        public void SelectCell(long nRowIndex, int nColIndex)
            => gridControl.SelectCell(nRowIndex, nColIndex);

        public void SelectCells(BlockOfCells cells)
            => gridControl.SelectCells(cells);

        public void UpdateGrid()
            => gridControl.UpdateGrid();

        public void UpdateGrid(bool bRecalcRows)
            => gridControl.UpdateGrid(bRecalcRows);
        #endregion

        #region Methods
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (!gridControl.Focused)
            {
                e.Handled = gridControl.Focus();
            }
            base.OnGotFocus(e);
        }

        #region Font Properties Changed
        private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControlHost c)
                c.OnFontChanged(e);
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControlHost c)
                c.OnFontChanged(e);
        }

        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControlHost c)
                c.OnFontChanged(e);
        }

        private static void OnFontStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridControlHost c)
                c.OnFontChanged(e);
        }

        private void OnFontChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is System.Windows.Media.FontFamily fontFamily)
            {
                var oldFont = gridControl.Font;
                try
                {
                    var font = new System.Drawing.Font(fontFamily.Source, oldFont.Size, oldFont.Style);
                    if (oldFont.Name != font.Name || oldFont.Size != font.Size || oldFont.Style != font.Style)
                        gridControl.Font = font;
                }
                catch(Exception)
                { }
            }
            else
            {
                var oldFont = gridControl.Font;
                System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Regular;

                if (FontStyles.Italic.Equals(FontStyle))
                    fontStyle &= System.Drawing.FontStyle.Italic;

                if (FontWeights.Bold.ToOpenTypeWeight() < FontWeight.ToOpenTypeWeight())
                    fontStyle &= System.Drawing.FontStyle.Bold;

                var fontFamilyName = oldFont.Name;
                
                if (FontFamily != null && !string.IsNullOrEmpty(FontFamily.Source))
                    fontFamilyName = FontFamily.Source;

                try
                {
                    var font = new System.Drawing.Font(fontFamilyName, oldFont.Size, oldFont.Style);
                    if (oldFont.Name != font.Name || oldFont.Size != font.Size || oldFont.Style != font.Style)
                        gridControl.Font = font;
                }
                catch (Exception)
                { }
            }
        }
        #endregion

        public bool? ShowPrintPreview(System.Windows.Window owner = null)
        {
            var dialog = new System.Windows.Forms.PrintPreviewDialog();
            dialog.Document = gridControl.PrintDocument;
            dialog.ShowIcon = false;

            var result = dialog.ShowDialog(WindowHandle.From(owner));

            if (result == DialogResult.OK)
                return true;
            else if (result == DialogResult.Cancel)
                return false;

            return null;
        }
        #endregion

        #region Nested Types
        private class WindowHandle : IWin32Window
        {
            #region Properties
            public IntPtr Handle { get; private set; } = IntPtr.Zero;
            #endregion

            #region Constructors
            private WindowHandle(System.Windows.Window window)
            {
                if (window != null)
                {
                    var interop = new System.Windows.Interop.WindowInteropHelper(window);
                    Handle = interop.Handle;
                }
            }
            #endregion

            #region Methods
            public static WindowHandle From(System.Windows.Window window) => new WindowHandle(window);
            #endregion

            #region Operators
            public static implicit operator WindowHandle(System.Windows.Window window)
            {
                return WindowHandle.From(window);
            }

            public static explicit operator IntPtr(WindowHandle handle)
            {
                return handle.Handle;
            }
            #endregion
        }
        #endregion
    }
}
