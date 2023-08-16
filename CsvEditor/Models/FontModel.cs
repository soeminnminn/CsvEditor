using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace CsvEditor.Models
{
    public class FontModel
    {
        #region Variables
        private static FontModel _default = null;
        private string familyName = null;
        private double size = 0;
        #endregion

        #region Constructor
        public FontModel(FontFamily family, double size)
        {
            Family = family;
            this.size = size;
        }

        public FontModel(string family, double size)
        {
            if (!string.IsNullOrEmpty(family))
                Family = new FontFamily(family);
            else
                Family = Default.Family;

            this.size = size;
        }

        public FontModel(System.Drawing.Font font)
        {
            Family = new FontFamily(font.FontFamily.Name);
            size = font.Size;
        }
        #endregion

        #region Properties
        public static FontModel Default
        {
            get
            {
                if (_default == null)
                {
                    var fontFamily = TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue as FontFamily;
                    var fontSize = (double)TextBlock.FontSizeProperty.DefaultMetadata.DefaultValue;

                    _default = new FontModel(fontFamily, fontSize);
                }
                return _default;
            }
        }

        public FontFamily Family { get; }

        public double Size 
        {
            get => Math.Round(size, 1);
        }

        public string Name
        {
            get => Family.Source;
        }

        public string FamilyName
        {
            get
            {
                if (string.IsNullOrEmpty(familyName))
                    familyName = GetDisplayName(Family.FamilyNames);
                return familyName;
            }
        }

        public System.Drawing.Font Font
        {
            get
            {
                try
                {
                    return new System.Drawing.Font(FamilyName, (float)PixelsToPoints(Size));
                }
                catch(Exception)
                { }

                return null;
            }
        }
        #endregion

        #region Methods
        public static double PointsToPixels(double value) => value * (96.0 / 72.0);

        public static double PixelsToPoints(double value) => value * (72.0 / 96.0);

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

        private static bool IsPrefixOf(string prefix, string tag) => prefix.Length < tag.Length &&
                   tag[prefix.Length] == '-' &&
                   string.CompareOrdinal(prefix, 0, tag, 0, prefix.Length) == 0;

        public override string ToString()
        {
            return FamilyName;
        }
        #endregion
    }
}
