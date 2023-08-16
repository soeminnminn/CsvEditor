using System;
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

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

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
