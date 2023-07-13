using System;

namespace CsvEditor.ViewModels
{
    public partial class MainViewModel : IUiModel
    {
        private bool showToolbar = true;
        private bool showStatusbar = true;

        public bool ShowToolbar
        {
            get => showToolbar;
            set 
            { 
                SetProperty(ref showToolbar, value, nameof(ShowToolbar), () => 
                {
                    if (config.IsLoaded)
                        config.ShowToolbar = value;
                }); 
            }
        }

        public bool ShowStatusbar
        {
            get => showStatusbar;
            set 
            { 
                SetProperty(ref showStatusbar, value, nameof(ShowStatusbar), () => 
                {
                    if (config.IsLoaded)
                        config.ShowStatusbar = value;
                }); 
            }
        }
    }
}
