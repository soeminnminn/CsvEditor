using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CsvEditor.RecentFiles
{
    public interface IRecentFilesMenu : IDisposable
    {
        event EventHandler<RecentFileSelectedEventArgs> RecentFileSelected;

        void Initialize(MenuItem miRecentFiles);

        void AddRecentFile(string fileName);

        void RemoveRecentFile(string fileName);
    }
}
