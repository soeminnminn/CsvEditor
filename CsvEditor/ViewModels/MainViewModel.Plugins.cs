using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using CsvEditor.Plugin;

namespace CsvEditor.ViewModels
{
    public partial class MainViewModel
    {
        #region Variables
        private List<IExportPlugin> exportPlugins = new List<IExportPlugin>();
        private MenuItem _exportMenu;
        #endregion

        #region Properties
        public bool HasPlugins
        {
            get => exportPlugins.Count > 0;
        }
        #endregion

        #region Methods
        public void InitializePlugins(MenuItem exportMenu)
        {
            _exportMenu = exportMenu;

            exportMenu.Visibility = Visibility.Collapsed;

            Task.Factory.StartNew(() => 
            {
                var plugins = Manager.Plugins;

                using (var emu = plugins.GetEnumerator())
                {
                    while (emu.MoveNext())
                    {
                        var plugin = emu.Current.Module;
                        if (plugin == null) continue;

                        try
                        {
                            Manager.SendOnPluginLoaded(emu.Current);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        exportPlugins.Add(plugin);
                    }
                }

                syncContext.Post(OnPluginsLoaded, this);
            });
        }

        private void OnPluginsLoaded(object state)
        {
            UpdateExportMenu();
        }

        private void UpdateExportMenu()
        {
            _exportMenu.Items.Clear();

            if (exportPlugins.Count == 0)
            {
                _exportMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                try
                {
                    lock (exportPlugins)
                    {
                        foreach (var plugin in exportPlugins)
                        {
                            if (string.IsNullOrEmpty(plugin.MenuText)) continue;
                            if (plugin.Command == null) continue;

                            var mi = new MenuItem
                            {
                                Header = plugin.MenuText,
                                Command = plugin.Command,
                                CommandParameter = new ExportPluginParameter(this, plugin),

                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                VerticalContentAlignment = VerticalAlignment.Stretch,
                            };

                            _exportMenu.Items.Add(mi);
                        }
                    }
                }
                catch (Exception)
                { }

                _exportMenu.Visibility = _exportMenu.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        internal void OnExportCompleted(ExportPluginParameter parameter)
        {
            if (parameter != null)
            {
                //
            }
        }
        #endregion
    }
}
