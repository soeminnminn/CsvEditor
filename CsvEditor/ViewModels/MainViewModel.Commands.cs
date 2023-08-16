using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Win32;
using CsvEditor.Commands;
using CsvEditor.RecentFiles;
using CsvEditor.Interfaces;

namespace CsvEditor.ViewModels
{
    public partial class MainViewModel : IModelCommands
    {
        #region Variables
        private const string AllFileFilter = "All File|*.*";

        private List<string> openFileFilters = new List<string>();
        private OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Title = "Open file",
            CheckFileExists = true
        };

        private List<string> saveFileFilters = new List<string>();
        private SaveFileDialog saveFileDialog = new SaveFileDialog()
        {
            Title = "Save file",
            OverwritePrompt = true,
            AddExtension = true,
        };

        private Window mainWindow = Application.Current.MainWindow;

        private bool showToolbar = true;
        private bool showStatusbar = true;

        private IRecentFilesMenu recentFilesMenu;

        private ICommand newCommand = null;
        private ICommand openCommand = null;
        private ICommand saveCommand = null;
        private ICommand saveAsCommand = null;

        private ICommand insertColumnBefore = null;
        private ICommand insertColumnAfter = null;
        private ICommand removeColumn = null;

        private ICommand insertRowAbove = null;
        private ICommand insertRowBelow = null;
        private ICommand removeRow = null;

        private ICommand sortAscending = null;
        private ICommand sortDescending = null;
        #endregion

        #region Properties
        public bool ShowToolbar
        {
            get => showToolbar;
            set
            {
                SetProperty(ref showToolbar, value, nameof(ShowToolbar), () =>
                {
                    if (config.IsLoaded)
                        config.ShowToolbar = value;
                });
            }
        }

        public bool ShowStatusbar
        {
            get => showStatusbar;
            set
            {
                SetProperty(ref showStatusbar, value, nameof(ShowStatusbar), () =>
                {
                    if (config.IsLoaded)
                        config.ShowStatusbar = value;
                });
            }
        }

        public IRecentFilesMenu RecentFilesMenu { get => recentFilesMenu; }

        public ICommand NewCommand { get => newCommand; }

        public ICommand OpenCommand { get => openCommand; }

        public ICommand SaveCommand { get => saveCommand; }

        public ICommand SaveAsCommand { get => saveAsCommand; }

        public ICommand InsertColumnBefore { get => insertColumnBefore; }

        public ICommand InsertColumnAfter { get => insertColumnAfter; }

        public ICommand RemoveColumn { get => removeColumn; }

        public ICommand InsertRowAbove { get => insertRowAbove; }

        public ICommand InsertRowBelow { get => insertRowBelow; }

        public ICommand RemoveRow { get => removeRow; }

        public ICommand SortAscending { get => sortAscending; }

        public ICommand SortDescending { get => sortDescending; }
        #endregion

        #region Methods
        public void InitializeCommands(Window window, MenuItem miRecentFiles)
        {
            mainWindow = window;

            openFileFilters.Add("Csv/Tsv File|*.csv;*.tsv");

            saveFileFilters.Add("Csv File|*.csv");
            saveFileFilters.Add("Tsv File|*.tsv");

            recentFilesMenu = new RecentFilesMenu(config);
            recentFilesMenu.RecentFileSelected += RecentFile_Selected;
            recentFilesMenu.Initialize(miRecentFiles);

            newCommand = new Command(OnNewFileExecuted);
            openCommand = new Command(OnOpenFileExecuted);
            saveCommand = new Command(OnSaveExecuted, () => IsEdited);
            saveAsCommand = new Command(OnSaveAsExecuted, () => IsLoaded);

            insertColumnBefore = new Command(OnInsertColumnBeforeExecuted);
            insertColumnAfter = new Command(OnInsertColumnAfterExecuted);
            removeColumn = new Command(OnRemoveColumnExecuted);

            insertRowAbove = new Command(OnInsertRowAboveExecuted);
            insertRowBelow = new Command(OnInsertRowBelowExecuted);
            removeRow = new Command(OnRemoveRowExecuted);

            sortAscending = new Command(OnSortAscendingExecuted);
            sortDescending = new Command(OnSortDescendingExecuted);
        }

        public void AddRecentFile(string fileName)
        {
            recentFilesMenu.AddRecentFile(fileName);
        }

        public void DisposeCommands()
        {
            recentFilesMenu.Dispose();
        }

        private void RecentFile_Selected(object sender, RecentFileSelectedEventArgs e)
        {
            if (SaveConfirm()) return;
            LoadFile(e.FileName);
        }

        public bool SaveConfirm()
        {
            if (IsEdited)
            {
                var result = MessageBox.Show(SR.MessageNeedSave, SR.AppName, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        {
                            string fileName = string.Empty;
                            if (!IsLoaded)
                            {
                                saveFileDialog.Filter = string.Join("|", saveFileFilters) + "|" + AllFileFilter;
                                if (saveFileDialog.ShowDialog(mainWindow) == true)
                                {
                                    fileName = saveFileDialog.FileName;
                                }
                            }
                            SaveFile(fileName);
                        }
                        break;
                    case MessageBoxResult.Cancel:
                        return true;
                    default:
                        break;
                }
            }

            return false;
        }

        public void OnNewFileExecuted() 
        {
            if (SaveConfirm()) return;
            NewFile(DEFAULT_COLUMNS_COUNT);
        }

        public void OnOpenFileExecuted()
        {
            openFileDialog.Filter = string.Join("|", openFileFilters) + "|" + AllFileFilter;
            if (openFileDialog.ShowDialog(mainWindow) == true)
            {
                LoadFile(openFileDialog.FileName);
            }
        }

        public void OnSaveExecuted()
        {
            if (string.IsNullOrEmpty(CurrentFile))
            {
                OnSaveAsExecuted();
                return;
            }
            SaveFile();
        }

        public void OnSaveAsExecuted()
        {
            saveFileDialog.Filter = string.Join("|", saveFileFilters) + "|" + AllFileFilter;
            if (saveFileDialog.ShowDialog(mainWindow) == true)
            {
                SaveFile(saveFileDialog.FileName);
            }
        }

        public void OnInsertColumnBeforeExecuted()
        {
            if (grid == null) return;

            int colIndex = ClickedColumnIndex - 1;
            if (colIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    colIndex = cellsCol[0].X - 1;
            }
            if (colIndex < 0) return;

            items.InsertColumn(colIndex);

            ColumnsCount = columnsCount + 1;
            UpdateGrid();
            if (HasData) MarkEdited(true);

            grid.StartCellEdit(0, colIndex + 1);
        }
        
        public void OnInsertColumnAfterExecuted()
        {
            if (grid == null) return;

            int colIndex = ClickedColumnIndex;
            if (colIndex < 1)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    colIndex = cellsCol[0].X + cellsCol[0].Width;
            }
            if (colIndex < 1) return;

            if (colIndex == columnsCount)
                items.AddColumn();
            else
                items.InsertColumn(colIndex);

            ColumnsCount = columnsCount + 1;
            UpdateGrid();
            if (HasData) MarkEdited(true);

            grid.StartCellEdit(0, colIndex + 1);
        }

        public void OnRemoveColumnExecuted()
        {
            if (grid == null) return;

            int colIndex = ClickedColumnIndex - 1;
            if (colIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    colIndex = cellsCol[0].X - 1;
            }
            if (colIndex < 0) return;
            if (columnsCount - 1 == 0) return;

            items.RemoveColumn(colIndex);
            ColumnsCount = columnsCount - 1;
            UpdateGrid();
            if (HasData) MarkEdited(true);
        }

        public void OnSortAscendingExecuted()
        {
            if (grid == null || !HasData) return;

            int colIndex = ClickedColumnIndex - 1;
            if (colIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    colIndex = cellsCol[0].X - 1;
            }
            if (colIndex < 0) return;

            if (hasHeader)
                items.Sort(colIndex, 1, items.Height - 1, new StringComparer());
            else
                items.Sort(colIndex, new StringComparer());

            grid.UpdateGrid();
            MarkEdited(true);
        }
        
        public void OnSortDescendingExecuted()
        {
            if (grid == null || !HasData) return;

            int colIndex = ClickedColumnIndex - 1;
            if (colIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    colIndex = cellsCol[0].X - 1;
            }
            if (colIndex < 0) return;

            if (hasHeader)
                items.Sort(colIndex, 1, items.Height - 1, new StringComparer(true));
            else
                items.Sort(colIndex, new StringComparer(true));

            grid.UpdateGrid();
            MarkEdited(true);
        }

        public void OnInsertRowAboveExecuted()
        {
            if (grid == null) return;

            int rowIndex = ClickedRowIndex;
            if (rowIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    rowIndex = (int)cellsCol[0].Y;
            }
            if (rowIndex < 0) return;
            if (hasHeader) rowIndex = rowIndex + 1;

            var arr = new string[columnsCount];
            items.Insert(rowIndex, arr);
            grid.UpdateGrid();
            if (HasData) MarkEdited(true);

            grid.StartCellEdit(rowIndex, 1);
        }
        
        public void OnInsertRowBelowExecuted()
        {
            if (grid == null) return;

            int rowIndex = ClickedRowIndex;
            if (rowIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    rowIndex = (int)cellsCol[0].Y;
            }
            if (rowIndex < 0) return;
            if (hasHeader) rowIndex = rowIndex + 1;
            
            rowIndex = rowIndex + 1;
            var arr = new string[columnsCount];

            if (rowIndex == items.Height)
                items.Add(arr);
            else
                items.Insert(rowIndex, arr);

            grid.UpdateGrid();
            if (HasData) MarkEdited(true);

            grid.StartCellEdit(rowIndex, 1);
        }

        public void OnRemoveRowExecuted()
        {
            if (grid == null) return;

            int rowIndex = ClickedRowIndex;
            if (rowIndex < 0)
            {
                var cellsCol = grid.SelectedCells;
                if (cellsCol.Count > 0)
                    rowIndex = (int)cellsCol[0].Y;
            }
            if (rowIndex < 0) return;
            if (hasHeader) rowIndex = rowIndex + 1;

            if (rowIndex < items.Length)
            {
                items.RemoveAt(rowIndex);
                grid.UpdateGrid();
                if (HasData) MarkEdited(true);
            }
        }

        public bool CanUndo()
        {
            if (IsACellBeingEdited)
            {
                var ctrl = grid.EditingEmbeddedControl as System.Windows.Forms.TextBox;
                return ctrl != null && ctrl.CanUndo;
            }
            else
                return items.CanUndo;
        }

        public void OnUndoExecuted()
        {
            if (IsACellBeingEdited)
            {
                var ctrl = grid.EditingEmbeddedControl as System.Windows.Forms.TextBox;
                if (ctrl != null && ctrl.CanUndo)
                    ctrl.Undo();
            }
            else
            {
                items.Undo();
                UpdateGrid(false);
            }
        }

        public bool CanRedo()
        {
            return items.CanRedo;
        }

        public void OnRedoExecuted()
        {
            items.Redo();
            UpdateGrid(false);
        }
        #endregion

        #region Nested Types
        private class StringComparer : IComparer<string>
        {
            private readonly bool isDescending;

            public StringComparer()
            {
                isDescending = false;
            }

            public StringComparer(bool isDescending)
            {
                this.isDescending = isDescending;
            }

            public int Compare(string x, string y)
            {
                int result = string.Compare(x, y);
                return isDescending ? -result : result;
            }
        }
        #endregion
    }
}
