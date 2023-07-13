using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using CsvEditor.Commands;
using CsvEditor.Models;
using CsvEditor.Observable;

namespace CsvEditor.ViewModels
{
    public class SettingsViewModel : ObservableObject, IDisposable
    {
        #region Variables
        private readonly ConfigModel config;

        private DelimiterItem delimiter = DelimiterItem.Default;
        private EncodingInfo encoding = null;
        private FontPickerItem font = FontPickerItem.Deafult;

        private readonly ICommand pickFontCommand;
        #endregion

        #region Constructor
        public SettingsViewModel(ConfigModel config)
        {
            this.config = config;

            var currentEncoding = config.DefaultEncoding;
            encoding = Encodings.FirstOrDefault(x => x.CodePage == currentEncoding.CodePage);

            delimiter = Delimiters.FirstOrDefault(x => x.Equals(config.DefaultDelimiter));

            pickFontCommand = new Command(PickFont_Executed);
        }
        #endregion

        #region Properties
        public DelimiterItem[] Delimiters
        {
            get => DelimiterItem.Delimiters;
        }

        public DelimiterItem Delimiter
        {
            get => delimiter;
            set { SetProperty(ref delimiter, value); }
        }

        public EncodingInfo[] Encodings
        {
            get => System.Text.Encoding.GetEncodings();
        }

        public EncodingInfo Encoding
        {
            get => encoding;
            set { SetProperty(ref encoding, value); }
        }

        public FontPickerItem EditorFont
        {
            get => font;
            private set { SetProperty(ref font, value); }
        }

        public ICommand PickFontCommand
        {
            get => pickFontCommand;
        }
        #endregion

        #region Methods
        private void PickFont_Executed()
        {
            var dialog = new System.Windows.Forms.FontDialog()
            {
                Font = EditorFont.Font,
            };
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var chooseFont = dialog.Font;
                if (chooseFont != null)
                {
                    EditorFont = new FontPickerItem(chooseFont);
                }
            }
        }

        public void Save()
        {
            config.DefaultDelimiter = Delimiter.Delimiter;
            config.DefaultEncoding = Encoding.GetEncoding();
        }

        public void Dispose()
        {
        }
        #endregion
    }
}
