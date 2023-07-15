using System.Windows;
using CsvEditor.ViewModels;

namespace CsvEditor.Views
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel model)
                model.Save();

            DialogResult = true;
            Close();
        }
    }
}
