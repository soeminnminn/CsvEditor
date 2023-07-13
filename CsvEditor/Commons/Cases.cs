using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CsvEditor
{
    public static class Cases
    {
        #region Methods
        private static string Up(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.ToUpperInvariant();
        }

        private static string Low(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.ToLowerInvariant();
        }

        private static string Cap(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Length < 2) return Up(s);
            return Up(s[0].ToString()) + s.Substring(1);
        }

        private static string Decap(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Length < 2) return Low(s);
            return Low(s[0].ToString()) + s.Substring(1);
        }

        private static string Deapostrophe(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return RegExps.apostrophe.Replace(s, "");
        }

        private static string Fill(string s, string fill, bool deapostrophe = false)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var val = s;
            if (!string.IsNullOrEmpty(fill))
            {
                val = RegExps.fill.Replace(val, match => 
                {
                    var next = match.Groups[1].Value;
                    return !string.IsNullOrEmpty(next) ? fill + next : "";
                });
            }
            if (deapostrophe)
            {
                val = Cases.Deapostrophe(val);
            }
            return val;
        }

        private static string Prep(string s, bool isFill = false, bool pascal = false, bool upper = false)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            var val = s;
            if (!upper && RegExps.upper.IsMatch(val))
            {
                val = Low(val);
            }
            if (!isFill && !RegExps.hole.IsMatch(val))
            {
                var holey = Fill(val, " ");
                if (RegExps.hole.IsMatch(holey))
                {
                    val = holey;
                }
            }
            if (!pascal && !RegExps.room.IsMatch(s))
            {
                val = RegExps.relax.Replace(val, match => 
                {
                    var before = match.Groups[1].Value;
                    var acronym = match.Groups[2].Value;
                    var caps = match.Groups[3].Value;

                    return before + " " + (!string.IsNullOrEmpty(acronym) ? acronym + " " : "") + caps;
                });
            }
            return val;
        }

        public static CaseTypes Of(string s)
        {
            if (string.IsNullOrEmpty(s)) return CaseTypes.none;

            if (ToLower(s) == s)
                return CaseTypes.lower;
            else if (ToSnake(s) == s)
                return CaseTypes.snake;
            else if (ToConstant(s) == s)
                return CaseTypes.constant;
            else if (ToCamel(s) == s)
                return CaseTypes.camel;
            else if (ToKebab(s) == s)
                return CaseTypes.kebab;
            else if (ToUpper(s) == s)
                return CaseTypes.upper;
            else if (ToCapital(s) == s)
                return CaseTypes.capital;
            else if (ToHeader(s) == s)
                return CaseTypes.header;
            else if (ToPascal(s) == s)
                return CaseTypes.pascal;
            else if (ToTitle(s) == s)
                return CaseTypes.title;
            else if (ToSentence(s) == s)
                return CaseTypes.sentence;

            return CaseTypes.none;
        }

        public static string Flip(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return Regex.Replace(s, @"\w", match => 
            {
                var l = match.Value;
                return l == Up(l) ? Low(l) : Up(l);
            });
        }

        public static string ToLower(string s, string fill = null, bool deapostrophe = false)
        {
            return Cases.Fill(Low(Prep(s, !string.IsNullOrEmpty(fill))), fill, deapostrophe);
        }

        public static string ToSnake(string s)
        {
            return Cases.ToLower(s, "_", true);
        }

        public static string ToConstant(string s)
        {
            return Cases.ToUpper(s, "_", true);
        }

        public static string ToCamel(string s)
        {
            return Cases.Decap(ToPascal(s));
        }

        public static string ToKebab(string s)
        {
            return Cases.ToLower(s, "-", true);
        }

        public static string ToUpper(string s, string fill = null, bool deapostrophe = false)
        {
            return Cases.Fill(Up(Prep(s, !string.IsNullOrEmpty(fill), false, true)), fill, deapostrophe);
        }

        public static string ToCapital(string s, string fill = null, bool deapostrophe = false)
        {
            return Cases.Fill(RegExps.capitalize.Replace(Prep(s, false, true), match => 
            {
                var border = match.Groups[1].Value;
                var letter = match.Groups[2].Value;

                return border + Up(letter);
            }), fill, deapostrophe);
        }

        public static string ToHeader(string s)
        {
            return Cases.ToCapital(s, "-", true);
        }

        public static string ToPascal(string s)
        {
            return Cases.Fill(RegExps.pascal.Replace(Prep(s, false, true), match => {
                var letter = match.Groups[2].Value;
                return Up(letter);
            }), "", true);
        }

        public static string ToTitle(string s)
        {
            var val = ToCapital(s);
            return RegExps.improper.Replace(val, match => 
            {
                var i = match.Index;
                var small = match.Value;
                return i > 0 && i < val.LastIndexOf(' ') ? Low(small) : small;
            });
        }

        public static string ToSentence(string s, string[] names = null, string[] abbreviations = null)
        {
            var val = Cases.ToLower(s);
            val = RegExps.sentence.Replace(val, match => 
            {
                var prelude = match.Groups[1].Value;
                var letter = match.Groups[2].Value;
                return prelude + Up(letter);
            });

            if (names != null)
            {
                foreach(var name in names)
                {
                    var re = new Regex("\\b" + ToLower(name) + "\\b");
                    val = re.Replace(val, match => 
                    {
                        return Cap(match.Value);
                    });
                }
            }

            if (abbreviations != null)
            {
                foreach(var abbr in abbreviations)
                {
                    var re = new Regex("\\b" + ToLower(abbr) + "\\. +)(\\w)");
                    val = re.Replace(val, match =>
                    {
                        var abbrAndSpace = match.Groups[1].Value;
                        var letter = match.Groups[2].Value;
                        return abbrAndSpace + Low(letter);
                    });
                }
            }

            return val;
        }
        
        public static string ToCase(string s, CaseTypes caseType)
        {
            switch (caseType)
            {
                case CaseTypes.lower:
                    return ToLower(s);
                case CaseTypes.snake:
                    return ToSnake(s);
                case CaseTypes.constant:
                    return ToConstant(s);
                case CaseTypes.camel:
                    return ToCamel(s);
                case CaseTypes.kebab:
                    return ToKebab(s);
                case CaseTypes.upper:
                    return ToUpper(s);
                case CaseTypes.capital:
                    return ToCapital(s);
                case CaseTypes.header:
                    return ToHeader(s);
                case CaseTypes.pascal:
                    return ToPascal(s);
                case CaseTypes.title:
                    return ToTitle(s);
                case CaseTypes.sentence:
                    return ToSentence(s);
                default:
                    break;
            }
            return s;
        }
        #endregion

        #region Nested Types
        public enum CaseTypes
        {
            none,
            lower,
            snake,
            constant,
            camel,
            kebab,
            upper,
            capital,
            header,
            pascal,
            title,
            sentence
        }

        private static class RegExps
        {
            private static string symbols = @"\u0020-\u0026,\u0028-\u002F,\u003A-\u0040,\u005B-\u0060,\u007B-\u007E,\u00A0-\u00BF,\u00D7,\u00F7".Replace(",", "");
            private static string lowers = @"a-z,\u00DF-\u00F6,\u00F8-\u00FF".Replace(",", "");
            private static string uppers = @"A-Z,\u00C0-\u00D6,\u00D8-\u00DE".Replace(",", "");
            private static string impropers = @"A|An|And|As|At|But|By|En|For|If|In|Of|On|Or|The|To|Vs?\\.?|Via";

            public static Regex capitalize = new Regex("(^|[" + symbols + "])([" + lowers + "])");
            public static Regex pascal = new Regex("(^|[" + symbols + "])+([" + lowers + uppers + "])");
            public static Regex fill = new Regex("[" + symbols + "]+(.|$)");
            public static Regex sentence = new Regex(@"(^\s*|[\?\!\.]+\""?\s+\""?|,\s+\"")([" + lowers + "])");
            public static Regex improper = new Regex(@"\b(" + impropers + @")\b");
            public static Regex relax = new Regex("([^" + uppers + "])([" + uppers + "]*)([" + uppers + "])(?=[^" + uppers + "]|$)");
            public static Regex upper = new Regex("^[^" + lowers + "]+$");
            public static Regex hole = new Regex(@"[^\s]\s[^\s]");
            public static Regex apostrophe = new Regex(@"'");
            public static Regex room = new Regex("[" + symbols + "]");
        }
        #endregion
    }
}
