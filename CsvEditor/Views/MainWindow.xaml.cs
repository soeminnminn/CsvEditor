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
        private MainViewModel model = new MainViewModel();
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            model.Initialize();
            model.InitializeCommands(this, menuRecentFiles);
            model.InitializeGrid(gridCtrl);

            DataContext = model;

            gridColumnHeaderMenu.ContextMenu.DataContext = model;
            gridColumnHeaderMenu.ContextMenu.ContextMenuOpening += ContextMenu_Opening;

            gridRowHeaderMenu.ContextMenu.DataContext = model;
            gridRowHeaderMenu.ContextMenu.ContextMenuOpening += ContextMenu_Opening;

            model.FileLoaded += Model_FileLoaded;
            model.FileClosed += Model_FileClosed;
            model.FileEdited += Model_FileEdited;
            model.SelectionChanged += Model_SelectionChanged;

            gridCtrl.MouseButtonClicked += Grid_MouseButtonClicked;
            gridCtrl.HeaderButtonClicked += Grid_HeaderButtonClicked;

            goToDlg.Accepted += GoToDlg_Accepted;

            CommandBindings.Add(new CommandBindingLink(ApplicationCommands.New, model.NewCommand));
            CommandBindings.Add(new CommandBindingLink(ApplicationCommands.Open, model.OpenCommand));

            CommandBindings.Add(new CommandBindingLink(ApplicationCommands.Save, model.SaveCommand));
            CommandBindings.Add(new CommandBindingLink(ApplicationCommands.SaveAs, model.SaveAsCommand));

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

        #region Methods
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = model.SaveConfirm();

            if (!e.Cancel)
            {
                model.Dispose();
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
                if ((files != null) && (files.Length > 0))
                {
                    var filePath = files.Where(f => MainViewModel.IsSupportedFile(f)).ToArray().FirstOrDefault();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (model.SaveConfirm()) return;
                        model.LoadFile(filePath);
                    }
                }
            }
        }

        private void GoToDlg_Accepted(object sender, RoutedGotoEventArgs e)
        {
            model.GoToLine((int)e.Y, (int)e.X);
            model.FocusOnGrid();
        }
        #endregion

        #region Grid Events Methods
        private void Grid_HeaderButtonClicked(object sender, HeaderButtonClickedEventArgs args)
        {
            if (args.Button == System.Windows.Forms.MouseButtons.Left)
            {
                model.SelectColumn(args.ColumnIndex);
            }
            else if (args.Button == System.Windows.Forms.MouseButtons.Right)
            {
                model.ClickedColumnIndex = args.ColumnIndex;
                gridColumnHeaderMenu.ContextMenu.IsOpen = true;
            }
        }

        private void Grid_MouseButtonClicked(object sender, MouseButtonClickedEventArgs args)
        {
            if (args.ColumnIndex == 0)
            {
                if (args.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    model.ClickedRowIndex = (int)args.RowIndex;
                    gridRowHeaderMenu.ContextMenu.IsOpen = true;
                }

                model.SelectRow(args.RowIndex);
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
                        if (mi.Command == model.RemoveRow)
                            mi.IsEnabled = model.Count > 1;
                        else if (mi.Command == model.RemoveColumn)
                            mi.IsEnabled = model.ColumnsCount > 1;
                        else if (mi.Command == model.SortAscending || mi.Command == model.SortDescending)
                            mi.IsEnabled = model.HasData;
                    }
                }
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            model.ClickedColumnIndex = -1;
            model.ClickedRowIndex = -1;
        }
        #endregion

        #region Models Events Methods
        private void Model_FileLoaded(object sender, EventArgs e)
        {
            Title = $"{Properties.Resources.Title} - {model.CurrentFileName}";
            statusLabelSelect.Text = "Row: 0, Cell: 0";
        }

        private void Model_FileClosed(object sender, EventArgs e)
        {
            Title = Properties.Resources.Title;
        }

        private void Model_FileEdited(object sender, bool e)
        {
            string title = $"{Properties.Resources.Title} - {model.CurrentFileName}";
            Title = e ? title + "*" : title;
        }

        private void Model_SelectionChanged(object sender, ModelSelectionChangedEventArgs args)
        {
            statusLabelSelect.Text = $"Row: {args.RowIndex + 1}, Cell: {args.ColumnIndex + 1}";
        }
        #endregion

        #region Commands Methods
        private void Print_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData && model.ItemsSource.Count > 1;
        }

        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            gridCtrl.PrintDocument.Print();
        }

        private void PrintPreview_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData && model.ItemsSource.Count > 1;
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
            e.CanExecute = model.CanUndo();
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.OnUndoExecuted();
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.CanRedo();
        }

        private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.OnRedoExecuted();
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData && (model.HasSelections || model.IsACellBeingEdited);
        }

        private void CutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.CopyToCilpboard(true);
            model.FocusOnGrid();
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData && (model.HasSelections || model.IsACellBeingEdited);
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.CopyToCilpboard();
            model.FocusOnGrid();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData && model.HasSelections && Clipboard.ContainsText();
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.FocusOnGrid();
            model.PasteFromCilpboard();
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData && (model.HasSelections || model.IsACellBeingEdited);
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.DeleteSelected();
            model.FocusOnGrid();
        }

        private void FindAndReplace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData;
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.CreateFindAndReplace(findAndReplaceDlg);
            findAndReplaceDlg.Show(false);
        }

        private void ReplaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.CreateFindAndReplace(findAndReplaceDlg);
            findAndReplaceDlg.Show(true);
        }

        private void Goto_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData;
        }

        private void GotoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            goToDlg.XValue = 0;
            goToDlg.YValue = 0;
            goToDlg.Show(model.ItemsSource.Count, model.ColumnsCount - 1);
        }

        private void SelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.HasData;
        }

        private void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            model.SelectAll();
            model.FocusOnGrid();
        }

        private void SettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new SettingsWindow()
            {
                Owner = this,
                DataContext = new SettingsViewModel(model.Config),
            };
            dialog.ShowDialog();
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
