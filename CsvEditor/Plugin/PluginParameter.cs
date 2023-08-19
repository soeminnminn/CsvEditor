using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using CsvEditor.ViewModels;

namespace CsvEditor.Plugin
{
    public interface IPluginParameter
    {
        Window MainWindow { get; }

        void ProcessCompleted();
    }

    public class ExportPluginParameter : IPluginParameter
    {
        #region Variables
        private readonly MainViewModel _model;
        private readonly IExportPlugin _plugin;
        #endregion

        #region Constructor
        internal ExportPluginParameter(MainViewModel model, IExportPlugin plugin)
        {
            _model = model;
            _plugin = plugin;
        }
        #endregion

        #region Properties
        internal IExportPlugin Plugin
        {
            get => _plugin;
        }

        public Window MainWindow
        {
            get => Application.Current.MainWindow;
        }

        public string FilePath
        {
            get => _model.CurrentFile;
        }

        public string FileName
        {
            get => _model.CurrentFileName;
        }

        public Encoding Encoding
        {
            get => _model.Encoding;
        }

        public string Delimiter
        {
            get => _model.Delimiter;
        }

        public bool HasHeader
        {
            get => _model.HasHeader;
        }

        public bool HasData
        {
            get => _model.HasData;
        }

        public IEnumerable<string[]> Data
        {
            get => _model.ItemsSource;
        }
        #endregion

        #region Methods
        public void ProcessCompleted()
        {
            _model.OnExportCompleted(this);
        }
        #endregion
    }
}
