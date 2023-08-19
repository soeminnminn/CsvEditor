using System;
using System.Windows.Input;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using CsvEditor.Plugin;
using NestedHtmlWriter;

namespace Plugin.ExportHtml
{
    public class ExportPlugin : IExportPlugin
    {
        #region Variables
        private static readonly RoutedCommand exportHtmlCommand = new RoutedCommand("ExportHtml", typeof(Window));
        private readonly SaveFileDialog saveFileDialog;
        #endregion

        #region Constructor
        public ExportPlugin()
        {
            saveFileDialog = new SaveFileDialog()
            {
                Filter = FileTypeFilter,
                OverwritePrompt = true,
                AddExtension = true,
            };
        }
        #endregion

        #region Properties
        public string Name
        {
            get => "ExportHtmlPlugin";
        }

        public string FileTypeFilter
        {
            get => "Html File|*.htm;*.html";
        }

        public string[] FileExtensions
        {
            get => new string[] { ".htm", ".html" };
        }

        public string MenuText
        {
            get => "To Html";
        }

        public ICommand Command
        {
            get => exportHtmlCommand;
        }
        #endregion

        #region Methods
        public void OnPluginLoaded(Window window)
        {
            window.CommandBindings.Add(new CommandBinding(exportHtmlCommand, Export_Executed, Export_CanExecute));
        }

        public void Export_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter is ExportPluginParameter parameter)
            {
                e.CanExecute = parameter.HasData;
            }
        }

        public void Export_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is ExportPluginParameter parameter)
            {
                if (saveFileDialog.ShowDialog(parameter.MainWindow) == true)
                {
                    string fileName = saveFileDialog.FileName;
                    string title = Path.GetFileName(parameter.FileName);

                    using (var writer = new StreamWriter(fileName))
                    {
                        using (var doc = new NhQuickDocument(writer, title, null, null, "en", NhDocumentType.Html5))
                        {
                            using (var table = doc.B.CreateTable())
                            {
                                using(var emu = parameter.Data.GetEnumerator())
                                {
                                    var hasHeader = parameter.HasHeader;

                                    while(emu.MoveNext())
                                    {
                                        var row = emu.Current;

                                        using (var tr = table.CreateTr())
                                        {
                                            foreach (var c in row)
                                            {
                                                if (!hasHeader)
                                                {
                                                    using (var td = tr.CreateTdInline())
                                                    {
                                                        td.WriteText(c);
                                                    }
                                                }
                                                else
                                                {
                                                    using (var th = tr.CreateThInline())
                                                    {
                                                        th.WriteText(c);
                                                    }
                                                    hasHeader = false;
                                                }
                                            }
                                        }   
                                    }
                                }
                            }
                        }
                    }

                    parameter.ProcessCompleted();
                }
            }
        }
        #endregion
    }
}
