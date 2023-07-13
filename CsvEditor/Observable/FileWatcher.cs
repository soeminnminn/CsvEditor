using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace CsvEditor.Observable
{
    public class FileWatcher : IDisposable
    {
        #region Variables
        private FileSystemWatcher watcher = null;
        private System.Timers.Timer timer = null;
        private int resetTimer = 0;
        private ConcurrentQueue<DateTime> queue = null;
        private FileInfo fileInfo = null;
        private DateTime lastWriteTime;
        private readonly SynchronizationContext syncContext;
        #endregion

        #region Events
        public event FileSystemEventHandler Changed = null;
        public event FileSystemEventHandler Deleted = null;
        public event RenamedEventHandler Renamed = null;
        #endregion

        #region Constructor
        public FileWatcher()
        {
            syncContext = SynchronizationContext.Current;
        }
        #endregion

        #region Properties
        public bool IsWatching
        {
            get => watcher != null && watcher.EnableRaisingEvents;
        }
        #endregion

        #region Methods
        public bool Watch(string filePath)
        {
            if (IsWatching) Stop();

            if (string.IsNullOrEmpty(filePath)) return false;

            if (fileInfo == null || fileInfo.FullName != filePath)
            {
                fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    fileInfo = null;
                    return false;
                }

                queue = new ConcurrentQueue<DateTime>();

                timer = new System.Timers.Timer(1000)
                {
                    Enabled = true
                };
                timer.Elapsed += OnElapsed;

                watcher = new FileSystemWatcher(fileInfo.DirectoryName)
                {
                    Filter = fileInfo.Name,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite
                };

                watcher.Changed += OnWatcherChanged;
                watcher.Renamed += OnWatcherRenamed;
                watcher.Deleted += OnWatcherDeleted;
                watcher.Error += OnWatcherError;
            }

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = true;
                return true;
            }

            return false;
        }

        public void Stop()
        {
            if (watcher != null)
            {
                watcher.Changed -= OnWatcherChanged;
                watcher.Renamed -= OnWatcherRenamed;
                watcher.Deleted -= OnWatcherDeleted;
                watcher.Error -= OnWatcherError;

                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }

            if (timer != null)
            {
                timer.Elapsed -= OnElapsed;
                timer.Enabled = false;
                timer.Dispose();
                timer = null;
            }

            queue = null;
            resetTimer = 0;
            fileInfo = null;
        }

        public void Pause()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
            }
            if (timer != null)
            {
                timer.Enabled = false;
                resetTimer = 0;
            }
        }

        public void Resume()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = true;
            }
            if (timer != null)
            {
                timer.Enabled = true;
                resetTimer = 0;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void OnWatcherChanged(object sender, FileSystemEventArgs e)
        {
            queue.Enqueue(fileInfo.LastWriteTime);
        }

        private void OnWatcherRenamed(object sender, RenamedEventArgs e)
        {
            syncContext.Post(args => 
            {
                Renamed?.Invoke(this, args as RenamedEventArgs);
            }, e);
        }

        private void OnWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            syncContext.Post(args =>
            {
                Deleted?.Invoke(this, args as FileSystemEventArgs);
            }, e);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            watcher.EnableRaisingEvents = false;

            int iMaxAttempts = 120;
            int iTimeOut = 30000;
            int i = 0;
            while (watcher.EnableRaisingEvents == false && i < iMaxAttempts)
            {
                i += 1;
                try
                {
                    watcher.EnableRaisingEvents = true;
                }
                catch
                {
                    watcher.EnableRaisingEvents = false;
                    Thread.Sleep(iTimeOut);
                }
            }
        }

        private void OnElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (queue != null && !queue.IsEmpty && queue.TryDequeue(out DateTime dateTime))
            {
                if (dateTime != lastWriteTime)
                {
                    syncContext.Post(PostChanged, e);
                    lastWriteTime = dateTime;
                }
            }

            resetTimer += 1000;
            if (resetTimer > 600000)
            {
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.EnableRaisingEvents = true;
                }
                resetTimer = 0;
            }
        }

        private void PostChanged(object state)
        {
            var eventArgs = new FileSystemEventArgs(WatcherChangeTypes.Changed, fileInfo.DirectoryName, fileInfo.Name);
            Changed?.Invoke(this, eventArgs);
        }
        #endregion
    }
}
