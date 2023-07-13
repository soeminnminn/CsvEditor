using System;
using CsvEditor.Commands;
using Microsoft.SqlServer.Management.UI.Grid;

namespace CsvEditor
{
    public interface IGridSource : IGridStorage
    {
        event ModelSelectionChangedEventHandler SelectionChanged;

        bool HasSelections { get; }

        bool IsACellBeingEdited { get; }

        int ClickedColumnIndex { get; set; }

        int ClickedRowIndex { get; set; }

        void InitializeGrid(IGridControl grid);

        void FocusOnGrid();

        void UpdateGrid(bool reset = true);

        void UpdateGridColumns(bool updateGrid = true);

        void ClearGrid();

        void GoToLine(int line, int col);

        void SelectRow(long nRowIndex);

        void SelectColumn(int nColIndex);

        void SelectAll();

        void DeleteSelected();

        void CopyToCilpboard(bool isCut = false);

        void PasteFromCilpboard();
    }
}
