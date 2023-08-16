using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CsvEditor.Commands;
using CsvEditor.Controls;
using CsvEditor.ViewModels;
using Microsoft.SqlServer.Management.UI.Grid;

namespace CsvEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        #endregion

        #region Constructors
        static MainWindow()
        {
            DataContextProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(OnDataContextChanged));
        }

        public MainWindow()
        {
            InitializeComponent();

            gridColumnHeaderMenu.ContextMenu.ContextMenuOpening += ContextMenu_Opening;
            gridRowHeaderMenu.ContextMenu.ContextMenuOpening += ContextMenu_Opening;

            gridCtrl.MouseButtonClicked += Grid_MouseButtonClicked;
            gridCtrl.HeaderButtonClicked += Grid_HeaderButtonClicked;

            goToDlg.Accepted += GoToDlg_Accepted;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenCommand_Executed));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCommand_Executed, Save_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsCommand_Executed, SaveAs_CanExecute));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintCommand_Executed, Print_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.PrintPreview, PrintPreviewCommand_Executed, PrintPreview_CanExecute));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, ExitCommand_Executed));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, UndoCommand_Executed, Undo_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, RedoCommand_Executed, Redo_CanExecute));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, CutCommand_Executed, Cut_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCommand_Executed, Copy_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, PasteCommand_Executed, Paste_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteCommand_Executed, Delete_CanExecute));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, FindCommand_Executed, FindAndReplace_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Replace, ReplaceCommand_Executed, FindAndReplace_CanExecute));
            CommandBindings.Add(new CommandBinding(RoutedCommands.GoTo, GotoCommand_Executed, Goto_CanExecute));
            
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAllCommand_Executed, SelectAll_CanExecute));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties, SettingsCommand_Executed));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, AboutCommand_Executed));
        }
        #endregion

        #region Properties
        private MainViewModel Model
        {
            get => (MainViewModel)DataContext;
        }
        #endregion

        #region Methods
        private static void OnDataContextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is MainWindow c)
                c.OnDataContextChanged(e);
        }

        private void OnDataContextChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldModel = e.OldValue as MainViewModel;
            if (oldModel != null)
            {
                oldModel.FileLoaded -= Model_FileLoaded;
                oldModel.FileClosed -= Model_FileClosed;
                oldModel.FileEdited -= Model_FileEdited;
                oldModel.SelectionChanged -= Model_SelectionChanged;
                oldModel.Dispose();
            }

            if (Model == null)
            {
                if (IsInitialized)
                {
                    Close();
                }
                return;
            }

            Model.InitializeCommands(this, menuRecentFiles);
            Model.InitializeGrid(gridCtrl);
            Model.InitializePlugins(menuExport);

            gridColumnHeaderMenu.ContextMenu.DataContext = Model;
            gridRowHeaderMenu.ContextMenu.DataContext = Model;

            Model.FileLoaded += Model_FileLoaded;
            Model.FileClosed += Model_FileClosed;
            Model.FileEdited += Model_FileEdited;
            Model.SelectionChanged += Model_SelectionChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Model != null)
            {
                e.Cancel = Model.SaveConfirm();

                if (!e.Cancel)
                {
                    Model.Dispose();
                }
            }
            base.OnClosing(e);
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data != null)
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                string[] files = (string[])e.Data.GetData("FileDrop");
                if (Model != null && files != null && files.Length > 0)
                {
                    var filePath = files.Where(f => MainViewModel.IsSupportedFile(f)).ToArray().FirstOrDefault();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (Model.SaveConfirm()) return;
                        Model.LoadFile(filePath);
                    }
                }
            }
        }

        private void GoToDlg_Accepted(object sender, RoutedGotoEventArgs e)
        {
            if (Model != null)
            {
                Model.GoToLine((int)e.Y, (int)e.X);
                Model.FocusOnGrid();
            }
        }
        #endregion

        #region Grid Events Methods
        private void Grid_HeaderButtonClicked(object sender, HeaderButtonClickedEventArgs args)
        {
            if (args.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Model.SelectColumn(args.ColumnIndex);
            }
            else if (args.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Model.ClickedColumnIndex = args.ColumnIndex;
                gridColumnHeaderMenu.ContextMenu.IsOpen = true;
            }
        }

        private void Grid_MouseButtonClicked(object sender, MouseButtonClickedEventArgs args)
        {
            if (args.ColumnIndex == 0)
            {
                if (args.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    Model.ClickedRowIndex = (int)args.RowIndex;
                    gridRowHeaderMenu.ContextMenu.IsOpen = true;
                }

                Model.SelectRow(args.RowIndex);
            }
        }

        private void ContextMenu_Opening(object sender, ContextMenuEventArgs e)
        {
            if (sender is ContextMenu cm)
            {
                foreach (var i in cm.Items)
                {
                    if (i is MenuItem mi && mi.Command != null)
                    {
                        if (mi.Command == Model.RemoveRow)
                            mi.IsEnabled = Model.Count > 1;
                        else if (mi.Command == Model.RemoveColumn)
                            mi.IsEnabled = Model.ColumnsCount > 1;
                        else if (mi.Command == Model.SortAscending || mi.Command == Model.SortDescending)
                            mi.IsEnabled = Model.HasData;
                    }
                }
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Model.ClickedColumnIndex = -1;
            Model.ClickedRowIndex = -1;
        }
        #endregion

        #region Models Events Methods
        private void Model_FileLoaded(object sender, EventArgs e)
        {
            Title = $"{SR.MainWindowTitle} - {Model.CurrentFileName}";
            statusLabelSelect.Text = SR.GetString(SR.Keys.CurrentLocation, 0, 0);
        }

        private void Model_FileClosed(object sender, EventArgs e)
        {
            Title = SR.MainWindowTitle;
        }

        private void Model_FileEdited(object sender, bool e)
        {
            string title = $"{SR.MainWindowTitle} - {Model.CurrentFileName}";
            Title = e ? title + "*" : title;
        }

        private void Model_SelectionChanged(object sender, ModelSelectionChangedEventArgs args)
        {
            statusLabelSelect.Text = SR.GetString(SR.Keys.CurrentLocation, args.RowIndex + 1, args.ColumnIndex + 1);
        }
        #endregion

        #region Commands Methods
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.OnNewFileExecuted();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.OnOpenFileExecuted();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.IsEdited;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.OnSaveExecuted();
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.IsLoaded;
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.OnSaveAsExecuted();
        }

        private void Print_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData && Model.ItemsSource.Count > 1;
        }

        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            gridCtrl.PrintDocument.Print();
        }

        private void PrintPreview_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData && Model.ItemsSource.Count > 1;
        }

        private void PrintPreviewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            gridCtrl.ShowPrintPreview(this);
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.CanUndo();
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.OnUndoExecuted();
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.CanRedo();
        }

        private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.OnRedoExecuted();
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData && (Model.HasSelections || Model.IsACellBeingEdited);
        }

        private void CutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.CopyToCilpboard(true);
            Model?.FocusOnGrid();
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData && (Model.HasSelections || Model.IsACellBeingEdited);
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.CopyToCilpboard();
            Model?.FocusOnGrid();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData && Model.HasSelections && Clipboard.ContainsText();
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.FocusOnGrid();
            Model?.PasteFromCilpboard();
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData && (Model.HasSelections || Model.IsACellBeingEdited);
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.DeleteSelected();
            Model?.FocusOnGrid();
        }

        private void FindAndReplace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData;
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.CreateFindAndReplace(findAndReplaceDlg);
                findAndReplaceDlg.Show(false);
            }
        }

        private void ReplaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.CreateFindAndReplace(findAndReplaceDlg);
                findAndReplaceDlg.Show(true);
            }   
        }

        private void Goto_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData;
        }

        private void GotoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Model != null)
            {
                goToDlg.XValue = 0;
                goToDlg.YValue = 0;
                goToDlg.Show(Model.ItemsSource.Count, Model.ColumnsCount - 1);
            }   
        }

        private void SelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.HasData;
        }

        private void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model?.SelectAll();
            Model?.FocusOnGrid();
        }

        private void SettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Model != null)
            {
                var dialog = new SettingsWindow()
                {
                    Owner = this,
                    DataContext = new SettingsViewModel(Model.Config),
                };

                if (dialog.ShowDialog() == true)
                {
                    Model.UpdateGridFont();
                }
            }
        }

        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new AboutDialog()
            {
                Owner = this
            };
            dialog.ShowDialog();
        }
        #endregion
    }
}
