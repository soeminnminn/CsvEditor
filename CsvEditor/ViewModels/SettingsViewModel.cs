using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CsvEditor.Models;
using CsvEditor.Observable;

namespace CsvEditor.ViewModels
{
    public class SettingsViewModel : ObservableObject, IDisposable
    {
        #region Variables
        private static readonly double[] CommonlyUsedFontSizes =
        {
            3.0, 4.0, 5.0, 6.0, 6.5,
            7.0, 7.5, 8.0, 8.5, 9.0,
            9.5, 10.0, 10.5, 11.0, 11.5,
            12.0, 12.5, 13.0, 13.5, 14.0,
            15.0, 16.0, 17.0, 18.0, 19.0,
            20.0, 22.0, 24.0, 26.0, 28.0, 30.0, 32.0, 34.0, 36.0, 38.0,
            40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
            80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
        };

        private readonly ConfigModel config;

        private DelimiterModel delimiter = DelimiterModel.Default;

        private EncodingModel encoding = null;
        private bool useEncodingWithBom = false;
        private bool useDefaultEncoding = false;

        private FontModel font = FontModel.Default;
        private object fontFamily = null;
        private object fontSize = null;
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
            {
                font = new FontModel(config.EditorFontFamily, config.EditorFontSize);
            }

            fontFamily = font.Family;
            fontSize = font.Size;
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

        public double[] FontSizes
        {
            get => CommonlyUsedFontSizes;
        }

        public object EditorFontFamily
        {
            get => fontFamily;
            set { SetProperty(ref fontFamily, value); }
        }

        public object EditorFontSize
        {
            get => fontSize;
            set { SetProperty(ref fontSize, value); }
        }
        #endregion

        #region Methods
        public void Save()
        {
            config.DefaultDelimiter = delimiter.Delimiter;

            config.DefaultEncoding = encoding.Encoding;
            config.UseEncodingWithBom = useEncodingWithBom;
            config.UseDefaultEncoding = useDefaultEncoding;
            
            if (fontFamily is FontFamily family)
                config.EditorFontFamily = family.Source;

            if (fontSize is double size)
                config.EditorFontSize = size;
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
