using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    [CLSCompliant(false)]
    public interface IGridControl
    {
        // Events
        event ColumnReorderRequestedEventHandler ColumnReorderRequested;
        event ColumnsReorderedEventHandler ColumnsReordered;
        event ColumnWidthChangedEventHandler ColumnWidthChanged;
        event CustomizeCellGDIObjectsEventHandler CustomizeCellGDIObjects;
        event EmbeddedControlContentsChangedEventHandler EmbeddedControlContentsChanged;
        event GridSpecialEventHandler GridSpecialEvent;
        event HeaderButtonClickedEventHandler HeaderButtonClicked;
        event KeyPressedOnCellEventHandler KeyPressedOnCell;
        event MouseButtonClickedEventHandler MouseButtonClicked;
        event MouseButtonClickingEventHandler MouseButtonClicking;
        event MouseButtonDoubleClickedEventHandler MouseButtonDoubleClicked;
        event SelectionChangedEventHandler SelectionChanged;
        event StandardKeyProcessingEventHandler StandardKeyProcessing;
        event TooltipDataNeededEventHandler TooltipDataNeeded;

        // Methods
        void AddColumn(GridColumnInfo ci);
        void DeleteColumn(int nIndex);
        void EnsureCellIsVisible(long nRowIndex, int nColIndex);
        int GetColumnWidth(int nColIndex);
        void GetCurrentCell(out long rowIndex, out int columnIndex);
        DataObject GetDataObject(bool bOnlyCurrentSelBlock, string columnsSeparator = null);
        GridColumnInfo GetGridColumnInfo(int columnIndex);
        void GetHeaderInfo(int colIndex, out string headerText, out GridCheckBoxState headerCheckBox);
        void GetHeaderInfo(int colIndex, out string headerText, out Bitmap headerBitmap);
        int GetStorageColumnIndexByUIIndex(int indexInUI);
        int GetUIColumnIndexByStorageIndex(int indexInStorage);
        Rectangle GetVisibleCellRectangle(long rowIndex, int columnIndex);
        HitTestInfo HitTest(int mouseX, int mouseY);
        void InsertColumn(int nIndex, GridColumnInfo ci);
        bool IsACellBeingEdited(out long nRowNum, out int nColNum);
        void RegisterEmbeddedControl(int editableCellType, Control embeddedControl);
        void ResetGrid();
        void ResizeColumnToShowAllContents(int columnIndex);
        void SetBitmapsForCheckBoxColumn(int nColIndex, Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState);
        void SetColumnWidth(int nColIndex, GridColumnWidthType widthType, int nWidth);
        void SetHeaderInfo(int colIndex, string strText, GridCheckBoxState checkboxState);
        void SetHeaderInfo(int nColIndex, string strText, Bitmap bmp);
        void SetMergedHeaderResizeProportion(int colIndex, float proportion);
        bool StartCellEdit(long nRowIndex, int nColIndex);
        bool StopCellEdit(bool bCommitIntoStorage);
        void SelectCell(long nRowIndex, int nColIndex);
        void SelectCells(BlockOfCells cells);
        void UpdateGrid();
        void UpdateGrid(bool bRecalcRows);

        // Properties
        bool AlwaysHighlightSelection { get; set; }
        int AutoScrollInterval { get; set; }
        BorderStyle BorderStyle { get; set; }
        int ColumnsNumber { get; }
        bool ColumnsReorderableByDefault { get; set; }
        Control EditingEmbeddedControl { get; }
        int FirstScrollableColumn { get; set; }
        uint FirstScrollableRow { get; set; }
        bool FocusEditorOnNavigation { get; set; }
        GridColumnInfoCollection GridColumnsInfo { get; }
        GridLineType GridLineType { get; set; }
        IGridStorage GridStorage { get; set; }
        Font HeaderFont { get; set; }
        int HeaderHeight { get; }
        Color HighlightColor { get; }
        int MarginsWidth { get; }
        PrintDocument PrintDocument { get; }
        int RowHeight { get; }
        BlockOfCellsCollection SelectedCells { get; set; }
        GridSelectionType SelectionType { get; set; }
        int VisibleRowsNum { get; }
        bool WithHeader { get; set; }
    }
	
	public interface IGridEmbeddedControl
    {
        event ContentsChangedEventHandler ContentsChanged;

        int AddDataAsString(string Item);
        void ClearData();
        string GetCurSelectionAsString();
        int GetCurSelectionIndex();
        int SetCurSelectionAsString(string strNewSel);
        void SetCurSelectionIndex(int nIndex);

        int ColumnIndex { get; set; }

        bool Enabled { get; set; }

        long RowIndex { get; set; }
    }
	
	public interface IGridEmbeddedControlManagement
    {
        void SetHorizontalAlignment(HorizontalAlignment alignment);

        bool WantMouseClick { get; }
    }
	
	public interface IGridEmbeddedControlManagement2 : IGridEmbeddedControlManagement
    {
        void PostProcessFocusFromKeyboard(System.Windows.Forms.Keys keyStroke, System.Windows.Forms.Keys modifiers);
        void ReceiveChar(char c);
        void ReceiveKeyboardEvent(KeyEventArgs ke);
    }
	
	public interface IGridEmbeddedSpinControl
    {
        decimal Increment { get; set; }

        decimal Maximum { get; set; }

        decimal Minimum { get; set; }
    }
	
	public interface IGridStorage
    {
        long EnsureRowsInBuf(long FirstRowIndex, long LastRowIndex);
        void FillControlWithData(long nRowIndex, int nColIndex, IGridEmbeddedControl control);
        Bitmap GetCellDataAsBitmap(long nRowIndex, int nColIndex);
        string GetCellDataAsString(long nRowIndex, int nColIndex);
        void GetCellDataForButton(long nRowIndex, int nColIndex, out ButtonCellState state, out Bitmap image, out string buttonLabel);
        GridCheckBoxState GetCellDataForCheckBox(long nRowIndex, int nColIndex);
        int IsCellEditable(long nRowIndex, int nColIndex);
        long NumRows();
        bool SetCellDataFromControl(long nRowIndex, int nColIndex, IGridEmbeddedControl control);
    }
}