using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CsvEditor.RecentFiles;

namespace CsvEditor.Interfaces
{
    public interface IModelCommands
    {
        IRecentFilesMenu RecentFilesMenu { get; }

        void InitializeCommands(Window window, MenuItem miRecentFiles);

        void AddRecentFile(string fileName);

        void DisposeCommands();

        bool SaveConfirm();
    }
}
