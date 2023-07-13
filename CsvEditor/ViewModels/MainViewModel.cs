using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using Ude;
using Ude.Core.Probers;
using S16.Collections;
using CsvEditor.Observable;
using CsvEditor.Interfaces;
using CsvEditor.Models;

namespace CsvEditor.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        #region Variables
        public static readonly string[] EXTENSIONS = new string[] { ".csv", ".tsv" };

        private readonly FileWatcher watcher;
        private readonly BackgroundWorker loadWorker;
        private readonly SynchronizationContext syncContext;
        private bool _disposed = false;

        private readonly ConfigModel config;

        private string stateMessage = "Ready";
        private string currentFile = string.Empty;
        private Encoding encoding = Encoding.Default;
        private string delimiter = ",";
        private int columnsCount = 0;
        private bool isEdited = false;
        private bool hasHeader = false;

        private ObservableList2D<string> items = new ObservableList2D<string>();
        #endregion

        #region Events
        public event EventHandler FileLoaded = null;
        public event EventHandler<bool> FileEdited = null;
        public event EventHandler FileClosed = null;
        #endregion

        #region Properties
        public bool HasHeader
        {
            get => hasHeader;
            set
            {
                SetProperty(ref hasHeader, value, nameof(HasHeader), () =>
                {
                    config.AddFileConfig(currentFile, hasHeader);

                    if (this is IGridSource gs)
                        gs.UpdateGridColumns();
                });
            }
        }

        public string Delimiter
        {
            get => delimiter;
            private set
            {
                SetProperty(ref delimiter, value);
                PostPropertyChanged(nameof(DelimiterName));
            }
        }

        public string DelimiterName
        {
            get
            {
                if (delimiter == "\t") return "Tab";
                else if (delimiter == ";") return "Semicolon";
                else if (delimiter == "|") return "Vertical Line";
                return "Comma";
            }
        }

        public string CurrentFile
        {
            get => currentFile;
            private set
            {
                SetProperty(ref currentFile, value);
                PostPropertyChanged(nameof(CurrentFileName));
            }
        }

        public string CurrentFileName
        {
            get
            {
                if (string.IsNullOrEmpty(currentFile)) return "Untitled";
                return Path.GetFileName(currentFile);
            }
        }

        public Encoding Encoding
        {
            get => encoding;
            private set { SetProperty(ref encoding, value); }
        }

        public string StateMessage
        {
            get => stateMessage;
            internal set { SetProperty(ref stateMessage, value); }
        }

        public bool IsEdited
        {
            get => isEdited;
            private set { SetProperty(ref isEdited, value); }
        }

        public bool IsLoaded { get => !string.IsNullOrEmpty(currentFile); }

        public bool HasData { get => items.Count > 0; }

        public int ColumnsCount
        {
            get => columnsCount;
            private set { SetProperty(ref columnsCount, value); }
        }

        public int Count
        {
            get => items.Height;
        }

        public ObservableList2D<string> ItemsSource { get => items; }

        public ConfigModel Config
        {
            get => config;
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            syncContext = SynchronizationContext.Current;

            config = new ConfigModel();

            watcher = new FileWatcher();
            watcher.Changed += Watcher_Changed;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;

            loadWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            loadWorker.DoWork += LoadWorker_DoWork;
            loadWorker.RunWorkerCompleted += LoadWorker_RunWorkerCompleted;
        }
        #endregion

        #region Events Methods
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            MarkEdited(true);
            watcher.Stop();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            MarkEdited(true);
            watcher.Stop();
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            MarkEdited(true);
            watcher.Stop();
        }

        private void LoadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadResult result = new LoadResult();

            var fileName = e.Argument as string;

            if (string.IsNullOrEmpty(fileName))
            {
                e.Result = result;
                return;
            }

            try
            {
                var file = new FileInfo(fileName);
                if (file.Exists)
                {
                    using (var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var detectResult = CharsetDetector.DetectFromStream(fs);
                        if (detectResult.Detected != null && fs.CanSeek)
                        {
                            result.FilePath = fileName;
                            result.EncodingName = detectResult.Detected.EncodingName;
                            result.Encoding = detectResult.Detected.Encoding;
                            result.Confidence = detectResult.Detected.Confidence;
                            result.Prober = detectResult.Detected.Prober;
                            result.HasBOM = detectResult.Detected.HasBOM;
                            
                            result.ColumnsCount = 0;
                            result.Data = new List<string[]>();

                            fs.Seek(0, SeekOrigin.Begin);
                            using (var sr = new StreamReader(fs, detectResult.Detected.Encoding))
                            {
                                var reader = new Csv.CsvReader(sr);
                                result.Delimiter = reader.Delimiter;

                                while (reader.Read())
                                {
                                    var arr = reader.ToArray();
                                    result.ColumnsCount = Math.Max(result.ColumnsCount, arr.Length);
                                    result.Data.Add(arr);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
            }

            e.Result = result;
        }

        private void LoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (LoadResult)e.Result;

            items.SuspendHistory();
            items.Clear(false);

            if (result.FilePath != null && result.Data != null)
            {
                CurrentFile = result.FilePath;
                Encoding = result.Encoding;
                Delimiter = result.Delimiter;

                watcher.Watch(result.FilePath);

                var fileConfig = config.GetFileConfig(result.FilePath);

                if (this is IModelCommands mc)
                    mc.AddRecentFile(result.FilePath);

                HasHeader = fileConfig.HasHeader;
                ColumnsCount = result.ColumnsCount;
                
                items.Width = columnsCount;
                items.Add(result.Data);

                FileLoaded?.Invoke(this, EventArgs.Empty);
                MarkEdited(false);
            }

            StateMessage = "Ready";

            if (this is IGridSource gs)
                gs.UpdateGrid();

            items.ResumeHistory();
        }
        #endregion

        #region Methods
        public static bool IsSupportedFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            try
            {
                var result = CharsetDetector.DetectFromFile(fileName);
                if (result.Detected != null)
                {
                    var ext = Path.GetExtension(fileName);
                    if (ext != null) return Array.Exists(EXTENSIONS, x => x == ext.ToLowerInvariant());
                }
            }
            catch { }

            return false;
        }

        public async void Initialize()
        {
            await config.LoadAsync().ConfigureAwait(true);

            if (this is IUiModel uiModel)
            {
                uiModel.ShowToolbar = config.ShowToolbar;
                uiModel.ShowStatusbar = config.ShowStatusbar;
            }

            var args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                var filePath = args.Where(f => IsSupportedFile(f)).ToArray().FirstOrDefault();
                if (!string.IsNullOrEmpty(filePath))
                {
                    LoadFile(filePath);
                }
            }
        }

        public void LoadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            StateMessage = "Loading";

            watcher.Stop();
            loadWorker.RunWorkerAsync(fileName);
        }

        public void ReloadFile()
        {
            if (string.IsNullOrEmpty(currentFile) || isEdited) return;

            StateMessage = "Loading";
            watcher.Pause();

            if (File.Exists(currentFile))
            {
                loadWorker.RunWorkerAsync(currentFile);
            }
            else
            {
                CloseFile();
            }
        }

        public void CloseFile()
        {
            watcher.Stop();

            items.Clear(false);
            items.ClearHistory();

            CurrentFile = string.Empty;
            Delimiter = ",";
            MarkEdited(false);

            if (this is IGridSource gs)
                gs.ClearGrid();

            StateMessage = "Ready";
            FileClosed?.Invoke(this, EventArgs.Empty);
        }

        public void NewFile(int columnCount)
        {
            CloseFile();

            if (columnCount > 0)
            {
                ColumnsCount = columnCount;
                items.Width = columnCount;

                if (this is IGridSource gs)
                    gs.UpdateGrid();

                FileLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SaveFile(string fileName = null)
        {
            if (items.Count == 0) return;
            
            watcher.Stop();

            string filePath = string.IsNullOrEmpty(fileName) ? currentFile : fileName;

            var ext = Path.GetExtension(filePath)?.ToLower();

            string delimiter = this.Delimiter;
            if (ext == ".csv")
                delimiter = ",";
            else if (ext == ".tsv")
                delimiter = "\t";

            StateMessage = "Saving";

            var task = Task.Run(() =>
            {
                try
                {
                    using (var sw = new StreamWriter(filePath, false, Encoding, 1024))
                    {
                        using (var writer = new Csv.CsvWriter(sw, delimiter))
                        {
                            foreach (var row in items)
                            {
                                writer.WriteRow(row);
                            }
                            sw.Flush();
                        }
                    }
                }
                catch (Exception)
                { }
            });

            task.GetAwaiter().OnCompleted(() =>
            {
                syncContext.Post(args => {

                    CurrentFile = filePath;
                    Delimiter = delimiter;
                    StateMessage = "Ready";
                    MarkEdited(false);

                    watcher.Watch(filePath);

                }, this);
            });
        }

        internal void MarkEdited(bool value)
        {
            if (isEdited != value)
            {
                IsEdited = value;
                FileEdited?.Invoke(this, value);
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    config.Dispose();

                    watcher.Dispose();
                    if (this is IModelCommands mc)
                        mc.DisposeCommands();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Nested Type
        [StructLayout(LayoutKind.Sequential)]
        private struct LoadResult
        {
            public string EncodingName;
            public Encoding Encoding { get; set; }
            public float Confidence { get; set; }

            public CharsetProber Prober { get; set; }

            public bool HasBOM { get; set; }

            public string FilePath { get; set; }

            public string Delimiter { get; set; }

            public int ColumnsCount { get; set; }

            public List<string[]> Data;
        }
        #endregion
    }
}