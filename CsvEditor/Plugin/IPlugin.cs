using System;
using System.Windows;
using System.Windows.Input;

namespace CsvEditor.Plugin
{
    public interface IPlugin
    {
        string Name { get; }

        void OnPluginLoaded(Window window);
    }

    public interface IExportPlugin : IPlugin
    {
        string FileTypeFilter { get; }

        string[] FileExtensions { get; }

        string MenuText { get; }

        ICommand Command { get; }
    }
}
