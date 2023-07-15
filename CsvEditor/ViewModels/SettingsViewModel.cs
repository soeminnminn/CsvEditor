using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CsvEditor.Commands;
using CsvEditor.Models;
using CsvEditor.Observable;

namespace CsvEditor.ViewModels
{
    public class SettingsViewModel : ObservableObject, IDisposable
    {
        #region Variables
        private readonly ConfigModel config;

        private DelimiterModel delimiter = DelimiterModel.Default;

        private EncodingModel encoding = null;
        private bool useEncodingWithBom = false;
        private bool useDefaultEncoding = false;

        private FontModel font = FontModel.Default;

        private readonly ICommand pickFontCommand;
        #endregion

        #region Constructor
        public SettingsViewModel(ConfigModel config)
        {
            this.config = config;

            var currentEncoding = config.DefaultEncoding;
            encoding = Encodings.FirstOrDefault(x => x.CodePage == currentEncoding.CodePage);
            useEncodingWithBom = config.UseEncodingWithBom;
            useDefaultEncoding = config.UseDefaultEncoding;

            delimiter = Delimiters.FirstOrDefault(x => x.Equals(config.DefaultDelimiter));

            if (!string.IsNullOrEmpty(config.EditorFontFamily))
                font = new FontModel(config.EditorFontFamily, config.EditorFontSize);

            pickFontCommand = new Command(PickFont_Executed);
        }
        #endregion

        #region Properties
        public DelimiterModel[] Delimiters
        {
            get => DelimiterModel.Delimiters;
        }

        public DelimiterModel Delimiter
        {
            get => delimiter;
            set { SetProperty(ref delimiter, value); }
        }

        public EncodingModel[] Encodings
        {
            get => EncodingModel.Encodings;
        }

        public EncodingModel Encoding
        {
            get => encoding;
            set 
            { 
                SetProperty(ref encoding, value, nameof(Encoding), () => 
                {
                    PostPropertyChanged(nameof(EncodingHasBom));
                }); 
            }
        }

        public bool EncodingHasBom
        {
            get
            {
                if (Encoding == null) return false;
                return Encoding.HasBOM;
            }
        }

        public bool UseEncodingWithBom
        {
            get => useEncodingWithBom;
            set { SetProperty(ref useEncodingWithBom, value); }
        }

        public bool UseDefaultEncoding
        {
            get => useDefaultEncoding;
            set { SetProperty(ref useDefaultEncoding, value); }
        }

        public FontModel EditorFont
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
                    EditorFont = new FontModel(chooseFont);
                }
            }
        }

        public void Save()
        {
            config.DefaultDelimiter = delimiter.Delimiter;

            config.DefaultEncoding = encoding.Encoding;
            config.UseEncodingWithBom = useEncodingWithBom;
            config.UseDefaultEncoding = useDefaultEncoding;
            
            config.EditorFontFamily = font.Name;
            config.EditorFontSize = font.Size;
        }

        public void Dispose()
        {
        }
        #endregion

        #region Nested Types
        private class EncodingInfoComparer : IComparer<EncodingInfo>
        {
            public int Compare(EncodingInfo x, EncodingInfo y)
            {
                if (x == null && y == null) return 0;
                if (x == null && y != null) return -1;
                if (x != null && y == null) return 1;

                return string.Compare(x.DisplayName, y.DisplayName);
            }
        }
        #endregion
    }
}
