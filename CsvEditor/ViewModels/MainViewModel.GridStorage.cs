using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using CsvEditor.Commands;
using CsvEditor.Controls;
using CsvEditor.Models;
using Microsoft.SqlServer.Management.UI.Grid;

namespace CsvEditor.ViewModels
{
    public partial class MainViewModel : IGridSource
    {
        #region Variables
        private IGridControl grid = null;
        private int clickedColunm = -1;
        private int clickedRow = -1;
        #endregion

        #region Events
        public event ModelSelectionChangedEventHandler SelectionChanged = null;
        #endregion

        #region Properties
        public bool HasSelections
        {
            get
            {
                if (grid == null) return false;
                if (HasData)
                {
                    if (grid.SelectedCells.Count > 1)
                        return true;
                    else if (grid.SelectedCells.Count == 1)
                    {
                        if (grid.SelectedCells[0].X == 0)
                            return grid.SelectedCells[0].Width > 1;
                        else
                            return true;
                    }
                }
                return false;
            }
        }

        public bool IsACellBeingEdited
        {
            get
            {
                return grid.IsACellBeingEdited(out long rowEdited, out int colEdited);
            }
        }

        public int ClickedColumnIndex
        {
            get => clickedColunm;
            set { SetProperty(ref clickedColunm, value); }
        }

        public int ClickedRowIndex
        {
            get => clickedRow;
            set { SetProperty(ref clickedRow, value); }
        }
        #endregion

        #region Grid Events
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.SelectedBlocks != null && args.SelectedBlocks.Count > 0)
            {
                this.grid.GetCurrentCell(out long rowIndex, out int colIndex);
                TriggerSelectionChanged(rowIndex, colIndex);
            }
        }
        #endregion

        #region Methods
        public static string ToColumnName(int column)
        {
            string result = string.Empty;
            while (column >= 26)
            {
                int i = column / 26;
                result += ((char)('A' + i - 1)).ToString(CultureInfo.InvariantCulture);
                column = column - (i * 26);
            }

            result += ((char)('A' + column)).ToString(CultureInfo.InvariantCulture);
            return result;
        }

        public static string ToRowName(long row)
        {
            return (row + 1).ToString(CultureInfo.InvariantCulture);
        }

        public void InitializeGrid(IGridControl grid)
        {
            if (this.grid != null)
            {
                this.grid.SelectionChanged -= Grid_SelectionChanged;
            }

            this.grid = grid;
            this.grid.SelectionType = GridSelectionType.CellBlocks;

            this.grid.SelectionChanged += Grid_SelectionChanged;
        }

        private void TriggerSelectionChanged(long nRowIndex, int nColIndex)
        {
            int rowIdx = hasHeader ? (int)(nRowIndex + 1) : (int)nRowIndex;
            if (rowIdx > -1 && items.Height > rowIdx)
            {
                var colIdx = nColIndex - 1;

                string value = string.Empty;

                if (colIdx > -1 && items.Width > colIdx)
                {
                    value = items[rowIdx, colIdx];
                }

                var e = new ModelSelectionChangedEventArgs()
                {
                    ColumnIndex = Math.Max(0, colIdx),
                    RowIndex = rowIdx,
                    CellValue = value
                };

                SelectionChanged?.Invoke(this, e);
            }
        }

        public void UpdateGrid(bool reset = true)
        {
            if (grid == null) return;

            if (reset)
            {
                grid.ResetGrid();
                if (columnsCount > 0)
                {
                    var count = items.Height + 1;

                    grid.AddColumn(new GridColumnInfo()
                    {
                        ColumnType = GridColumnType.LineNumber,
                        ColumnAlignment = System.Windows.Forms.HorizontalAlignment.Right,
                        ColumnWidth = Math.Max($"{count}".Length + 1, 7),
                        WidthType = GridColumnWidthType.InAverageFontChar,
                        IsWithSelectionBackground = false,
                        IsHeaderClickable = false,
                        IsUserResizable = false
                    });
                    grid.SetHeaderInfo(0, "#", null);

                    int[] colWidths = new int[columnsCount];
                    items.ForEach((row) =>
                    {
                        for (int c = 0; c < row.Length && c < columnsCount; c++)
                        {
                            if (row[c] == null) continue;
                            colWidths[c] = Math.Max(colWidths[c], row[c].Length + 2);
                        }
                    });

                    for (int i = 0; i < columnsCount; i++)
                    {
                        var width = colWidths[i] == 0 ? 16 : colWidths[i];
                        grid.AddColumn(new GridColumnInfo()
                        {
                            ColumnWidth = Math.Min(Math.Max(8, width), 80),
                            WidthType = GridColumnWidthType.InAverageFontChar,
                        });
                    }

                    UpdateGridColumns(false);
                    grid.FirstScrollableColumn = 1;

                    grid.GridStorage = this;
                    grid.UpdateGrid();
                }
            }
            else
            {
                grid.UpdateGrid();
            }
        }

        public void UpdateGridColumns(bool updateGrid = true)
        {
            if (grid == null) return;

            if (columnsCount > 0)
            {
                for (int i = 0; i < columnsCount; i++)
                {
                    string header = ToColumnName(i);
                    if (hasHeader && items.Height > 0 && i < items.Width)
                    {
                        header = items[0, i];
                    }
                    grid.SetHeaderInfo(i + 1, header, null);
                }

                if (updateGrid)
                {
                    grid.UpdateGrid();
                }
            }
        }

        public void UpdateGridFont()
        {
            if (grid != null && grid is GridControlHost host && !string.IsNullOrEmpty(config.EditorFontFamily))
            {
                try
                {
                    var font = new FontModel(config.EditorFontFamily, config.EditorFontSize);
                    host.Grid.Font = font.Font;
                }
                catch (Exception)
                { }
            }
        }

        public void ClearGrid()
        {
            grid.GridStorage = null;
            grid.ResetGrid();
        }

        public void GoToLine(int line, int col)
        {
            if (line < 1 || line > items.Height) return;
            if (col < 0 || col >= columnsCount) return;
            grid.SelectCell(line - 1, Math.Max(1, col + 1));
        }

        public void FocusOnGrid()
        {
            if (IsACellBeingEdited) return;

            if (grid is Controls.GridControlHost hostCtrl)
            {
                hostCtrl.Focus();
            }
            else if (grid is GridControl ctrl)
            {
                ctrl.Focus();
            }
        }

        public Tuple<int, int> GetSelectedIndex()
        {
            var cellsCol = grid.SelectedCells;
            if (cellsCol.Count == 0) return null;

            int rowIdx = (int)(hasHeader ? cellsCol[0].Y + 1L : cellsCol[0].Y);
            int colIdx = cellsCol[0].X - 1;

            return new Tuple<int, int>(rowIdx, colIdx);
        }

        public void SelectRow(long nRowIndex)
        {
            int rowIdx = hasHeader ? (int)(nRowIndex + 1) : (int)nRowIndex;
            if (rowIdx >= items.Height) return;

            BlockOfCells cells = new BlockOfCells(nRowIndex, 1);
            cells.Width = items.Width;
            cells.Height = 1;
            grid.SelectCells(cells);
        }

        public void SelectColumn(int nColIndex)
        {
            int colIdx = nColIndex - 1;
            if (colIdx < 0 || colIdx >= items.Width) return;

            BlockOfCells cells = new BlockOfCells(0, nColIndex);
            cells.Width = 1;
            cells.Height = hasHeader ? items.Height - 1 : items.Height;
            grid.SelectCells(cells);
        }

        public void SelectCell(int row, int col, bool edit, int selectStart = -1, int selectLength = 0)
        {
            if (row < 0 || row >= items.Height) return;
            if (col < 0 || col >= columnsCount) return;

            int rowIdx = hasHeader ? row - 1 : row;
            int colIdx = col + 1;

            if (edit && grid.StartCellEdit(rowIdx, colIdx) && grid.EditingEmbeddedControl != null)
            {
                var textBox = grid.EditingEmbeddedControl as System.Windows.Forms.TextBox;
                if (textBox != null && selectStart > -1 && selectLength > 0)
                {
                    textBox.SelectionStart = selectStart;
                    textBox.SelectionLength = selectLength;
                }
            }
            else
            {
                BlockOfCells cells = new BlockOfCells(rowIdx, colIdx);
                cells.Width = 1;
                cells.Height = 1;
                grid.SelectCells(cells);
            }
        }

        public void SelectAll()
        {
            BlockOfCells cells = new BlockOfCells(0, 1);
            cells.Width = items.Width;
            cells.Height = hasHeader ? items.Height - 1 : items.Height;

            grid.SelectCells(cells);
        }

        public void DeleteSelected()
        {
            var cellsCol = grid.SelectedCells;
            if (cellsCol.Count == 0) return;

            try
            {
                bool hasEdited = grid.IsACellBeingEdited(out long rowEdited, out int colEdited);
                if (hasEdited && grid.EditingEmbeddedControl != null && grid.EditingEmbeddedControl is System.Windows.Forms.TextBox textBox)
                {
                    textBox.Focus();
                    if (textBox.SelectionLength > 0)
                    {
                        textBox.SelectedText = string.Empty;
                        MarkEdited(true);
                    }
                }
                else
                {
                    int count = cellsCol.Count;
                    for (int i = 0; i < count; i++)
                    {
                        long nStartRow = cellsCol[i].Y;
                        long nEndRow = cellsCol[i].Bottom;
                        int nStartCol = cellsCol[i].X;
                        int nEndCol = cellsCol[i].Right;

                        for (long r = nStartRow; r <= nEndRow; r += 1L)
                        {
                            int rowIdx = hasHeader ? (int)(r + 1) : (int)r;
                            if (rowIdx < 0 || rowIdx >= items.Height) continue;

                            for (int c = nStartCol; c <= nEndCol; c++)
                            {
                                int colIdx = c - 1;
                                items[rowIdx, colIdx] = string.Empty;
                            }
                        }
                    }

                    MarkEdited(true);
                    grid.UpdateGrid();
                }
            }
            catch (Exception)
            { }
        }

        public void CopyToCilpboard(bool isCut = false)
        {
            var cellsCol = grid.SelectedCells;
            if (cellsCol.Count == 0) return;

            try
            {
                string text = string.Empty;
                bool hasEdited = grid.IsACellBeingEdited(out long rowEdited, out int colEdited);

                if (hasEdited && grid.EditingEmbeddedControl != null && grid.EditingEmbeddedControl is System.Windows.Forms.TextBox textBox)
                {
                    textBox.Focus();
                    if (isCut)
                    {
                        textBox.Cut();
                        MarkEdited(true);
                    }
                    else
                        textBox.Copy();
                }
                else
                {
                    using (var s = new StringWriter())
                    {
                        using (var writer = new Csv.CsvWriter(s, delimiter))
                        {
                            int count = cellsCol.Count;
                            for (int i = 0; i < count; i++)
                            {
                                long nStartRow = cellsCol[i].Y;
                                long nEndRow = cellsCol[i].Bottom;
                                int nStartCol = cellsCol[i].X;
                                int nEndCol = cellsCol[i].Right;

                                for (long r = nStartRow; r <= nEndRow; r += 1L)
                                {
                                    int rowIdx = hasHeader ? (int)(r + 1) : (int)r;
                                    if (rowIdx < 0 || rowIdx >= items.Height) continue;

                                    for (int c = nStartCol; c <= nEndCol; c++)
                                    {
                                        int colIdx = c - 1;
                                        if (colIdx > -1 && colIdx < items.Width)
                                        {
                                            writer.WriteField(items[rowIdx, colIdx]);
                                        }

                                        if (isCut)
                                        {
                                            items[rowIdx, colIdx] = string.Empty;
                                        }
                                    }

                                    if (r < nEndRow)
                                    {
                                        writer.NextRecord();
                                    }
                                }

                                if (i < (count - 1))
                                {
                                    writer.NextRecord();
                                }
                            }
                        }

                        text = s.ToString();
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        if (isCut)
                        {
                            MarkEdited(true);
                            grid.UpdateGrid();
                        }

                        System.Windows.Clipboard.SetText(text, System.Windows.TextDataFormat.UnicodeText);
                    }
                }
            }
            catch (Exception)
            { }
        }

        public void PasteFromCilpboard()
        {
            var cellsCol = grid.SelectedCells;
            if (cellsCol.Count == 0) return;

            try
            {
                bool hasEdited = grid.IsACellBeingEdited(out long rowEdited, out int colEdited);
                if (hasEdited && grid.EditingEmbeddedControl != null && grid.EditingEmbeddedControl is System.Windows.Forms.TextBox textBox)
                {
                    textBox.Focus();
                    textBox.Paste();
                    MarkEdited(true);
                }
                else
                {
                    string text = string.Empty;
                    if (System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.UnicodeText))
                    {
                        text = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.UnicodeText);
                    }
                    else if (System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.Text))
                    {
                        text = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.Text);
                    }

                    if (string.IsNullOrEmpty(text)) return;

                    List<string[]> list = new List<string[]>();
                    using (var reader = new Csv.CsvReader(text))
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.ToArray());
                        }
                    }

                    if (list.Count > 0)
                    {
                        int rowIdx = (int)(hasHeader ? cellsCol[0].Y + 1L : cellsCol[0].Y);
                        int colIdx = cellsCol[0].X - 1;

                        if (rowIdx < 0 || rowIdx >= items.Count) return;
                        if (colIdx < 0 || colIdx >= columnsCount) return;

                        foreach (var row in list)
                        {
                            for (int c = 0; c < columnsCount; c++)
                            {
                                if (c < row.Length)
                                {
                                    items[rowIdx, c] = row[c];
                                }
                            }
                            rowIdx++;
                        }

                        MarkEdited(true);
                        grid.UpdateGrid();
                    }
                }
            }
            catch (Exception)
            { }
        }
        #endregion

        #region IGridStorage Members
        public long EnsureRowsInBuf(long firstRowIndex, long lastRowIndex)
        {
            return lastRowIndex - firstRowIndex;
        }

        public void FillControlWithData(long nRowIndex, int nColIndex, IGridEmbeddedControl control)
        {
            int rowIdx = hasHeader ? (int)(nRowIndex + 1) : (int)nRowIndex;
            if (nColIndex > 0 && rowIdx > -1 && items.Height > rowIdx)
            {
                int col = nColIndex - 1;
                string text = col < items.Width ? items[rowIdx, col] : string.Empty;

                EmbeddedTextBox textBox = control as EmbeddedTextBox;
                textBox.Text = text;
            }
        }

        public Bitmap GetCellDataAsBitmap(long nRowIndex, int nColIndex)
        {
            return null;
        }

        public string GetCellDataAsString(long nRowIndex, int nColIndex)
        {
            int rowIdx = hasHeader ? (int)(nRowIndex + 1) : (int)nRowIndex;
            if (rowIdx > -1 && items.Height > rowIdx)
            {
                int col = nColIndex - 1;
                return col < items.Width ? items[rowIdx, col] : string.Empty;
            }
            return string.Empty;
        }

        public void GetCellDataForButton(long nRowIndex, int nColIndex, out ButtonCellState state, out Bitmap image, out string buttonLabel)
        {
            state = ButtonCellState.Normal;
            image = null;

            int rowIdx = hasHeader ? (int)(nRowIndex + 1) : (int)nRowIndex;
            if (rowIdx > -1 && nColIndex == 0)
            {
                if (ItemsSource.Height == 0 || ItemsSource.Height == rowIdx)
                {
                    buttonLabel = "+";
                }
                else
                {
                    buttonLabel = ToRowName(nRowIndex);
                }
            }
            else
            {
                buttonLabel = string.Empty;
            }
        }

        public GridCheckBoxState GetCellDataForCheckBox(long nRowIndex, int nColIndex)
        {
            return GridCheckBoxState.None;
        }

        public int IsCellEditable(long nRowIndex, int nColIndex)
        {
            return nColIndex == 0 ? 0 : 1;
        }

        public long NumRows()
        {
            return Math.Max(1, hasHeader ? items.Height : items.Height + 1);
        }

        public bool SetCellDataFromControl(long nRowIndex, int nColIndex, IGridEmbeddedControl control)
        {
            int rowIdx = hasHeader ? (int)(nRowIndex + 1) : (int)nRowIndex;
            int colIdx = nColIndex - 1;

            if (rowIdx > -1 && colIdx > -1 && colIdx < columnsCount)
            {
                EmbeddedTextBox textBox = control as EmbeddedTextBox;
                if (textBox != null)
                {
                    var text = textBox.Text;
                    if (items.Height > rowIdx)
                    {
                        var old = items[rowIdx, colIdx];
                        if (old != textBox.Text)
                        {
                            items[rowIdx, colIdx] = text;
                            MarkEdited(true);
                        }
                    }
                    else if (!string.IsNullOrEmpty(text))
                    {
                        var arr = new string[columnsCount];
                        arr[0] = text;
                        items.Add(arr);
                        MarkEdited(true);
                    }
                }

                return true;
            }
            return false;
        }
        #endregion
    }
}
