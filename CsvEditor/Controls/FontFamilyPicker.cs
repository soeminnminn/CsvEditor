using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace CsvEditor.Controls
{
    public class FontFamilyPicker : ComboBox
    {
        #region Variables
        private readonly ObservableCollection<FontFamily> itemsSource = new ObservableCollection<FontFamily>();

        private bool _isListValid = false;
        #endregion

        #region Constructors
        public FontFamilyPicker()
            : base()
        {
            ItemsSource = itemsSource;
        }
        #endregion

        #region Methods
        protected override void OnInitialized(EventArgs e)
        {
            if (!_isListValid)
            {
                UpdateItemsSource();
                _isListValid = true;
            }

            base.OnInitialized(e);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FontFamilyPickerItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FontFamilyPickerItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as FontFamilyPickerItem;
            if (container == null)
            {
                throw new InvalidOperationException("Container not created.");
            }

            var obj = item as FontFamily;
            if (obj == null)
            {
                throw new InvalidOperationException("Item not specified.");
            }

            container.FontFamily = obj;
            container.Content = GetDisplayName(obj.FamilyNames);
        }

        private void UpdateItemsSource()
        {
            var fontFamilies = Fonts.SystemFontFamilies.Where(x => !IsSymbolFont(x)).ToList();
            fontFamilies.Sort(new FontFamilyComparer());

            itemsSource.Clear();
            foreach (var item in fontFamilies)
            {
                itemsSource.Add(item);
            }
        }

        private static string GetDisplayName(LanguageSpecificStringDictionary nameDictionary)
        {
            // Look up the display name based on the UI culture, which is the same culture
            // used for resource loading.
            var userLanguage = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);

            // Look for an exact match.
            string name;
            if (nameDictionary.TryGetValue(userLanguage, out name))
            {
                return name;
            }

            // No exact match; return the name for the most closely related language.
            var bestRelatedness = -1;
            var bestName = string.Empty;

            foreach (var pair in nameDictionary)
            {
                var relatedness = GetRelatedness(pair.Key, userLanguage);
                if (relatedness > bestRelatedness)
                {
                    bestRelatedness = relatedness;
                    bestName = pair.Value;
                }
            }

            return bestName;
        }

        private static int GetRelatedness(XmlLanguage keyLang, XmlLanguage userLang)
        {
            try
            {
                // Get equivalent cultures.
                var keyCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(keyLang.IetfLanguageTag);
                var userCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(userLang.IetfLanguageTag);
                if (!userCulture.IsNeutralCulture)
                {
                    userCulture = userCulture.Parent;
                }

                // If the key is a prefix or parent of the user language it's a good match.
                if (IsPrefixOf(keyLang.IetfLanguageTag, userLang.IetfLanguageTag) || userCulture.Equals(keyCulture))
                {
                    return 2;
                }

                // If the key and user language share a common prefix or parent neutral culture, it's a reasonable match.
                if (IsPrefixOf(TrimSuffix(userLang.IetfLanguageTag), keyLang.IetfLanguageTag) ||
                    userCulture.Equals(keyCulture.Parent))
                {
                    return 1;
                }
            }
            catch (ArgumentException)
            {
                // Language tag with no corresponding CultureInfo.
            }

            // They're unrelated languages.
            return 0;
        }

        private static string TrimSuffix(string tag)
        {
            var i = tag.LastIndexOf('-');
            if (i > 0)
            {
                return tag.Substring(0, i);
            }
            return tag;
        }

        private static bool IsPrefixOf(string prefix, string tag)
        {
            return prefix.Length < tag.Length &&
                   tag[prefix.Length] == '-' &&
                   string.CompareOrdinal(prefix, 0, tag, 0, prefix.Length) == 0;
        }

        private static bool IsSymbolFont(FontFamily fontFamily)
        {
            foreach (var typeface in fontFamily.GetTypefaces())
            {
                if (typeface.TryGetGlyphTypeface(out GlyphTypeface face))
                {
                    return face.Symbol;
                }
            }
            return false;
        }
        #endregion

        #region Nested Types
        private class FontFamilyComparer : IComparer<FontFamily>, IEqualityComparer<FontFamily>
        {
            #region Variables
            private static FontFamilyComparer _default = new FontFamilyComparer();
            #endregion

            #region Constructor
            public FontFamilyComparer()
            { }
            #endregion

            #region Properties
            public static FontFamilyComparer Default
            {
                get => _default;
            }
            #endregion

            #region Methods
            public int Compare(FontFamily x, FontFamily y)
            {
                if (x == null && y == null) return 0;
                if (x == null && y != null) return -1;
                if (x != null && y == null) return 1;

                return string.Compare(x.Source, y.Source);
            }

            public bool Equals(FontFamily x, FontFamily y)
            {
                return Compare(x, y) == 0;
            }

            public int GetHashCode(FontFamily obj)
            {
                if (obj != null) return obj.Source.GetHashCode();
                return -1;
            }
            #endregion
        }
        #endregion
    }

    public class FontFamilyPickerItem : ComboBoxItem
    {
        #region Constructors
        public FontFamilyPickerItem()
            : base()
        { }
        #endregion
    }
}
