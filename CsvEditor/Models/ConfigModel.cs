﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;
using CsvEditor.Commons;
using CsvEditor.RecentFiles;
using SharpConfig;

namespace CsvEditor.Models
{
    public class ConfigModel : IRecentFilesStore, IDisposable
    {
        #region Variables
        private const string DEFAULT_FILENAME = "config.ini";
        private const int MAX_FILE = 10;

        private static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string appDataConfigDir = string.Empty;

        private readonly string _configFileName;
        private readonly ConfigPersistence _persistence;
        private bool _disposed = false;

        private bool showToolbar = true;
        private bool showStatusbar = true;
        
        private string defaultDelimiter = ",";
        
        private int defaultEncoding = -1;
        private bool useEncodingWithBom = false;
        private bool useDefaultEncoding = false;

        private string editorFontFamily = string.Empty;
        private double editorFontSize = 0;

        private StartUpModes startUpOpen = StartUpModes.Blank;
        private string lastOpenedFile = string.Empty;

        private readonly List<FileConfig> fileConfigs = new List<FileConfig>();
        private readonly List<string> recentFiles = new List<string>();
        #endregion

        #region Constructors
        public ConfigModel(string fileName = DEFAULT_FILENAME)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            _configFileName = Path.Combine(AppDataConfigDir, fileName);
            _persistence = new ConfigPersistence(this);

            defaultEncoding = Encoding.UTF8.CodePage;

            var font = FontModel.Default;
            editorFontFamily = font.FamilyName;
            editorFontSize = font.Size;
        }
        #endregion

        #region Properties
        public static string AppDataConfigDir
        {
            get
            {
                if (!string.IsNullOrEmpty(appDataConfigDir)) return appDataConfigDir;

                var assm = Assembly.GetEntryAssembly();
                if (assm != null)
                {
                    string appName = assm.GetName().Name;
                    var titleAttr = assm.GetCustomAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
                    if (titleAttr != null && !string.IsNullOrEmpty(titleAttr.Title))
                    {
                        appName = titleAttr.Title;
                    }

                    string companyName = string.Empty;
                    var companyAttr = assm.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
                    if (companyAttr != null && !string.IsNullOrEmpty(companyAttr.Company))
                    {
                        companyName = companyAttr.Company;
                    }

                    string configDir = Path.Combine(appDataPath, companyName, appName);
                    if (!Directory.Exists(configDir))
                    {
                        try
                        {
                            var dirInfo = Directory.CreateDirectory(configDir);
                            if (dirInfo.Exists)
                            {
                                appDataConfigDir = dirInfo.FullName;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        appDataConfigDir = configDir;
                    }

                }
                return appDataConfigDir;
            }
        }

        public bool IsLoaded { get; private set; } = false;

        public bool ShowToolbar
        {
            get => showToolbar;
            set { showToolbar = value; }
        }

        public bool ShowStatusbar
        {
            get => showStatusbar;
            set { showStatusbar = value; }
        }

        public string DefaultDelimiter
        {
            get => defaultDelimiter;
            set { defaultDelimiter = value; }
        }

        public Encoding DefaultEncoding
        {
            get => defaultEncoding < 0 ? Encoding.UTF8 : Encoding.GetEncoding(defaultEncoding);
            set { defaultEncoding = value.CodePage; }
        }

        public bool UseEncodingWithBom
        {
            get => useEncodingWithBom;
            set { useEncodingWithBom = value; }
        }

        public bool UseDefaultEncoding
        {
            get => useDefaultEncoding;
            set { useDefaultEncoding = value; }
        }

        public string EditorFontFamily
        {
            get => editorFontFamily;
            set { editorFontFamily = value; }
        }

        public double EditorFontSize
        {
            get => editorFontSize;
            set { editorFontSize = value; }
        }

        public StartUpModes StartUpOpen
        {
            get => startUpOpen;
            set { startUpOpen = value; }
        }

        public string LastOpenedFile
        {
            get => lastOpenedFile;
            set { lastOpenedFile = value; }
        }

        public List<FileConfig> FileConfigs
        {
            get => fileConfigs;
        }

        public List<string> RecentFiles
        {
            get => recentFiles;
        }
        #endregion

        #region Methods
        private static bool HasWriteAccess(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath)) return false;

            var isInRoleWithAccess = false;
            var accessRights = FileSystemRights.Write;

            try
            {
                var di = new DirectoryInfo(dirPath);
                if (!di.Exists) return false;

                var acl = di.GetAccessControl();
                var rules = acl.GetAccessRules(true, true, typeof(NTAccount));

                var currentUser = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(currentUser);
                foreach (AuthorizationRule rule in rules)
                {
                    var fsAccessRule = rule as FileSystemAccessRule;
                    if (fsAccessRule == null)
                        continue;

                    if ((fsAccessRule.FileSystemRights & accessRights) > 0)
                    {
                        var ntAccount = rule.IdentityReference as NTAccount;
                        if (ntAccount == null)
                            continue;

                        if (principal.IsInRole(ntAccount.Value))
                        {
                            if (fsAccessRule.AccessControlType == AccessControlType.Deny)
                                return false;
                            isInRoleWithAccess = true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            return isInRoleWithAccess;
        }

        public void LoadSettings()
        {
            var settings = Properties.Settings.Default;

            if (!string.IsNullOrEmpty(settings.StartUpOpen) && Enum.TryParse(settings.StartUpOpen, out StartUpModes mode))
                startUpOpen = mode;

            if (!string.IsNullOrEmpty(settings.DefaultDelimiter))
                defaultDelimiter = settings.DefaultDelimiter.Replace("\\t", "\t");

            if (settings.DefaultEncoding > 0)
                defaultEncoding = settings.DefaultEncoding;

            useEncodingWithBom = settings.UseEncodingWithBom;
            useDefaultEncoding = settings.UseDefaultEncoding;

            if (!string.IsNullOrEmpty(settings.LastOpenedFile))
                lastOpenedFile = settings.LastOpenedFile;

            showToolbar = settings.ShowToolbar;
            showStatusbar = settings.ShowStatusbar;

            if (!string.IsNullOrEmpty(settings.EditorFontFamily))
                editorFontFamily = settings.EditorFontFamily;

            if (settings.EditorFontSize > 0.0)
                editorFontSize = settings.EditorFontSize;

            if (settings.Files != null)
            {
                foreach(var str in settings.Files)
                {
                    var file = new FileConfig(str);
                    fileConfigs.Add(file);
                }
            }

            if (settings.RecentFiles != null)
            {
                foreach (var str in settings.RecentFiles)
                {
                    recentFiles.Add(str);
                }
            }

            IsLoaded = true;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.StartUpOpen = startUpOpen.ToString();
            Properties.Settings.Default.DefaultDelimiter = defaultDelimiter;
            Properties.Settings.Default.DefaultEncoding = defaultEncoding;
            Properties.Settings.Default.UseEncodingWithBom = useEncodingWithBom;
            Properties.Settings.Default.UseDefaultEncoding = useDefaultEncoding;
            Properties.Settings.Default.LastOpenedFile = lastOpenedFile;

            Properties.Settings.Default.ShowToolbar = showToolbar;
            Properties.Settings.Default.ShowStatusbar = showStatusbar;
            Properties.Settings.Default.EditorFontFamily = editorFontFamily;
            Properties.Settings.Default.EditorFontSize = (float)editorFontSize;

            var filesCol = new StringCollection();
            for (int i = 0; i < fileConfigs.Count; i++)
            {
                filesCol.Add(fileConfigs[i].ToString());
            }
            Properties.Settings.Default.Files = filesCol;

            var recentFilesCol = new StringCollection();
            for (var i = 0; i < recentFiles.Count; i++)
            {
                recentFilesCol.Add(recentFiles[i]);
            }
            Properties.Settings.Default.RecentFiles = recentFilesCol;

            Properties.Settings.Default.Save();
        }

        public async Task LoadAsync()
        {
#if NET
            await _persistence.LoadAsync().ConfigureAwait(false);
            IsLoaded = true;
#else
            await Task.Run(() => 
            {
                LoadSettings();
            });
#endif
        }

        public async Task SaveAsync()
        {
#if NET
            await _persistence.SaveAsync().ConfigureAwait(false);
#else
            await Task.Run(() =>
            {
                SaveSettings();
            });
#endif
        }

        public void AddFileConfig(string fileName, bool hasHeader = false)
        {
            try
            {
                fileName = Path.GetFullPath(fileName);
            }
            catch
            {
                return;
            }

            lock (fileConfigs)
            {
                var idx = fileConfigs.FindIndex(x => x.Path == fileName);
                if (idx > -1)
                {
                    fileConfigs.RemoveAt(idx);
                }
                fileConfigs.Add(new FileConfig(fileName, hasHeader));

                if (fileConfigs.Count > MAX_FILE)
                {
                    fileConfigs.RemoveAt(0);
                }
            }
        }

        public FileConfig GetFileConfig(string fileName)
        {
            FileConfig item;
            lock (fileConfigs)
            {
                var idx = fileConfigs.FindIndex(x => x.Path == fileName);
                if (idx > -1) return fileConfigs[idx];

                item = new FileConfig(fileName, false);
                fileConfigs.Add(item);
            }

            return item;
        }

        public Task<List<string>> GetRecentFiles()
        {
            return Task.Run(() =>
            {
                return recentFiles;
            });
        }

        public void AddRecentFile(string fileName, int maxFiles)
        {
            try
            {
                fileName = Path.GetFullPath(fileName);
            }
            catch
            {
                return;
            }

            lock (recentFiles)
            {
                recentFiles.Remove(fileName);
                recentFiles.Insert(0, fileName);

                if (recentFiles.Count > maxFiles)
                {
                    recentFiles.RemoveAt(maxFiles);
                }
            }
        }

        public void RemoveRecentFile(string fileName)
        {
            lock (recentFiles)
            {
                recentFiles.Remove(fileName);
            }
        }

        public void ClearRecentFiles()
        {
            lock (recentFiles)
            {
                recentFiles.Clear();
            }
        }

        private bool Deserialize(Configuration config)
        {
            fileConfigs.Clear();
            recentFiles.Clear();

            if (config == null) return false;

            try
            {
                var generalSection = config["General"];

                if (generalSection["StartUpOpen"].TryGetValue(out string startUp) && !string.IsNullOrEmpty(startUp) && Enum.TryParse(startUp, out StartUpModes mode))
                    startUpOpen = mode;

                if (generalSection["DefaultDelimiter"].TryGetValue(out string delimiter) && !string.IsNullOrEmpty(delimiter))
                    defaultDelimiter = delimiter.Replace("\\t", "\t");

                if (generalSection["DefaultEncoding"].TryGetValue(out int codePage) && codePage > 0)
                    defaultEncoding = codePage;

                if (generalSection["UseEncodingWithBom"].TryGetValue(out bool withBom))
                    useEncodingWithBom = withBom;

                if (generalSection["UseDefaultEncoding"].TryGetValue(out bool useEncoding))
                    useDefaultEncoding = useEncoding;

                if (generalSection["LastOpenedFile"].TryGetValue(out string lastFile) && !string.IsNullOrEmpty(lastFile))
                    lastOpenedFile = lastFile;

                var uiSection = config["UI"];

                if (uiSection["ShowToolbar"].TryGetValue(out bool toolbar))
                    showToolbar = toolbar;

                if (uiSection["ShowStatusbar"].TryGetValue(out bool statusbar))
                    showStatusbar = statusbar;

                if (uiSection["EditorFontFamily"].TryGetValue(out string fontFamily) && !string.IsNullOrEmpty(fontFamily))
                    editorFontFamily = fontFamily;

                if (uiSection["EditorFontSize"].TryGetValue(out double fontSize) && fontSize > 0)
                    editorFontSize = fontSize;

                config["Files"].ForEach(setting => 
                {
                    if (setting.TryGetValue(out string str))
                    {
                        var file = new FileConfig(str);
                        fileConfigs.Add(file);
                    }
                });

                config["RecentFiles"].ForEach(setting => 
                {
                    if (setting.TryGetValue(out string str))
                    {
                        recentFiles.Add(str);
                    }
                });

                return true;
            }
            catch (Exception)
            { }

            return false;
        }

        private void Serialize(Stream stream)
        {
            if (stream == null) return;
            if (!stream.CanWrite) return;

            var config = new Configuration();

            try
            {
                var generalSection = config.Add("General");

                generalSection.Add("StartUpOpen", startUpOpen);
                generalSection.Add("DefaultDelimiter", defaultDelimiter.Replace("\t", "\\t"));
                generalSection.Add("DefaultEncoding", defaultEncoding);
                generalSection.Add("UseEncodingWithBom", useEncodingWithBom);
                generalSection.Add("UseDefaultEncoding", useDefaultEncoding);
                generalSection.Add("LastOpenedFile", lastOpenedFile);

                var uiSection = config.Add("UI");

                uiSection.Add("ShowToolbar", showToolbar);
                uiSection.Add("ShowStatusbar", showStatusbar);
                uiSection.Add("EditorFontFamily", editorFontFamily);
                uiSection.Add("EditorFontSize", editorFontSize);

                var filesSection = config.Add("Files");                

                for (int i = 0; i < fileConfigs.Count; i++)
                {
                    filesSection.Add($"File{i}", fileConfigs[i].ToString());
                }

                var recentFilesSection = config.Add("RecentFiles");
                for (var i = 0; i < recentFiles.Count; i++)
                {
                    recentFilesSection.Add($"File{i}", recentFiles[i]);
                }

                config.SaveToStream(stream);
            }
            catch(Exception)
            { }
        }

#if NET
        protected async void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                    await _persistence.SaveAsync().ConfigureAwait(true);
                    _persistence.Dispose();
                }
                _disposed = true;
            }
        }
#else
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SaveSettings();
                }
                _disposed = true;
            }
        }
#endif

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Nested Types
        public struct FileConfig
        {
            #region Variables
            public string Path;
            public bool HasHeader;
            #endregion

            #region Constructors
            public FileConfig(string path, bool hasHeader)
            {
                Path = path;
                HasHeader = hasHeader;
            }

            public FileConfig(string url)
            {
                Uri uri;
                if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    QueryString qs = QueryString.Parse(uri.Query);
                    Path = uri.LocalPath;
                    HasHeader = string.Equals(qs["hasHader"], "true", StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    Path = null;
                    HasHeader = false;
                }
            }
            #endregion

            #region Methods
            public override string ToString()
            {
                Uri uri;
                if (Uri.TryCreate(Path, UriKind.Absolute, out uri))
                {
                    QueryString qs = new QueryString
                    {
                        { "hasHader", HasHeader.ToString() }
                    };

                    return uri.ToString() + "?" + qs.ToString();
                }

                return string.Empty;
            }
            #endregion
        }

        private class ConfigPersistence : IDisposable
        {
            #region Variables
            private const int MUTEX_TIMEOUT = 3000;

            private readonly string _fileName;
            private readonly Mutex _mutex;

            private readonly ConfigModel _config;
            #endregion

            #region Constructor
            public ConfigPersistence(ConfigModel config)
            {
                _config = config;
                _fileName = config._configFileName;
                _mutex = new Mutex(false, $"Global\\{_fileName.Replace('\\', '_')}");
            }
            #endregion

            #region Methods
            public Task LoadAsync()
            {
                return Task.Run(() =>
                {
                    var filePath = _fileName;

                    try
                    {
                        if (_mutex.WaitOne(MUTEX_TIMEOUT))
                        {
                            Configuration config = null;
                            try
                            {
                                var fileInfo = new FileInfo(filePath);
                                if (fileInfo.Exists)
                                {
                                    try
                                    {
                                        using (var reader = new StreamReader(fileInfo.OpenRead()))
                                        {
                                            var content = reader.ReadToEnd();
                                            config = Configuration.LoadFromString(content);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        System.Diagnostics.Debug.WriteLine(e.Message);
                                    }
                                }
                            }
                            catch
                            {
                                return;
                            }
                            finally
                            {
                                _mutex.ReleaseMutex();
                            }

                            lock (_config)
                            {
                                _config.Deserialize(config);
                            }
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                });
            }

            public Task SaveAsync()
            {
                return Task.Run(() =>
                {
                    var filePath = _fileName;

                    try
                    {
                        if (_mutex.WaitOne(MUTEX_TIMEOUT))
                        {
                            try
                            {
                                lock (_config)
                                {
                                    var fileInfo = new FileInfo(filePath);
                                    if (HasWriteAccess(fileInfo.DirectoryName))
                                    {
                                        if (fileInfo.Exists)
                                        {
                                            fileInfo.Delete();
                                        }

                                        using (var stream = fileInfo.Create())
                                        {
                                            _config.Serialize(stream);
                                        }
                                    }
                                }
                            }
                            catch { }
                            finally
                            {
                                _mutex.ReleaseMutex();
                            }
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                });
            }

            public void Dispose()
            {
                _mutex.Dispose();
            }
            #endregion
        }
        #endregion
    }
}
