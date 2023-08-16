using System;
using System.Windows.Input;

namespace CsvEditor.Plugin
{
    public interface IExportPlugin
    {
        string Name { get; }

        string FileTypeFilter { get; }

        string[] FileExtensions { get; }

        string MenuText { get; }

        ICommand Command { get; }

        void OnPluginLoaded();
    }
}
