using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CsvEditor.Views;

namespace CsvEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainModel = new ViewModels.MainViewModel();
            mainModel.TryLoadCommandLine(e.Args);

            MainWindow mainWindow = new MainWindow()
            {
                DataContext = mainModel
            };
            mainWindow.Show();
        }
    }
}
