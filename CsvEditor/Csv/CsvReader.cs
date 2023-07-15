using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CsvEditor.Csv
{
    public class CsvReader : IDisposable
    {
        #region Variables
        public static readonly string[] DELIMITERS = new string[] { ",", ";", "|", "\t" };
        public static readonly string CommentCharacter = "#";

        private readonly TextReader rdr;
        private readonly TextFieldParser parser;

        private int fieldsCount = 0;
        private string[] current = null;
        #endregion

        #region Constructors
        public CsvReader(string text)
            : this(text, DetectDelimiter(text))
        {
        }

        public CsvReader(string text, string delimiter, bool detectDelimiter = false)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be empty.");

            if (string.IsNullOrEmpty(delimiter))
                throw new ArgumentException("Delimiter cannot be empty.");

            if (detectDelimiter)
                Delimiter = DetectDelimiter(text, delimiter);
            else
                Delimiter = delimiter;

            rdr = new StringReader(text);

            parser = new TextFieldParser(rdr);
            parser.SetDelimiters(Delimiter);
            parser.SetCommentTokens(CommentCharacter);
        }

        public CsvReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentException("reader cannot be null.");

            var text = reader.ReadToEnd();
            string detected = DetectDelimiter(text);
            Delimiter = detected;

            rdr = new StringReader(text);
            parser = new TextFieldParser(rdr);
            parser.SetDelimiters(Delimiter);
            parser.SetCommentTokens(CommentCharacter);
        }

        public CsvReader(TextReader reader, string delimiter, bool detectDelimiter = false)
        {
            if (reader == null)
                throw new ArgumentException("reader cannot be null.");

            if (string.IsNullOrEmpty(delimiter))
                throw new ArgumentException("Delimiter cannot be empty.");

            if (detectDelimiter)
            {
                var text = reader.ReadToEnd();
                Delimiter = DetectDelimiter(text, delimiter);
                rdr = new StringReader(text);
            }
            else
            {
                Delimiter = delimiter;
                rdr = reader;
            }
            
            parser = new TextFieldParser(rdr);
            parser.SetDelimiters(Delimiter);
            parser.SetCommentTokens(CommentCharacter);
        }
        #endregion

        #region Properties
        public string Delimiter { get; private set; }

        /// <summary>
        /// If true start/end spaces are excluded from field values (except values in quotes). True by default.
        /// </summary>
        public bool TrimFields { get; set; } = true;

        public int FieldsCount
        {
            get => fieldsCount;
        }

        public string this[int idx]
        {
            get
            {
                if (idx < fieldsCount && current != null)
                {
                    return current[idx];
                }
                return null;
            }
        }
        #endregion

        #region Methods
        public static string DetectDelimiter(string csvText, string defaultDelimiter = ",")
        {
            var text = Regex.Replace(csvText, $"\".*?\"", string.Empty, RegexOptions.Singleline);
            text = Regex.Replace(text, @"^#[^\n]+", string.Empty, RegexOptions.Singleline);

            var newLine = "\r\n|\r|\n";

            var lineDelimiterCounts = new List<Dictionary<string, int>>();
            while (text.Length > 0)
            {
                // Since all escaped text has been removed, we can reliably read line by line.
                var match = Regex.Match(text, newLine);
                var line = match.Success ? text.Substring(0, match.Index + match.Length) : text;
                if (string.IsNullOrWhiteSpace(line))
                {
                    text = text.Substring(match.Index + match.Length);
                    continue;
                }

                var delimiterCounts = new Dictionary<string, int>();
                foreach (var delimiter in DELIMITERS)
                {
                    // Escape regex special chars to use as regex pattern.
                    var pattern = Regex.Replace(delimiter, @"([.$^{\[(|)*+?\\])", "\\$1");
                    int count = Regex.Matches(line, pattern).Count;
                    delimiterCounts[delimiter] = count;
                }

                lineDelimiterCounts.Add(delimiterCounts);

                text = match.Success ? text.Substring(match.Index + match.Length) : string.Empty;
            }

            if (lineDelimiterCounts.Count > 1)
            {
                // The last line isn't complete and can't be used to reliably detect a delimiter.
                lineDelimiterCounts.Remove(lineDelimiterCounts.Last());
            }

            // Rank only the delimiters that appear on every line.
            var delimiters =
            (
                from counts in lineDelimiterCounts
                from count in counts
                group count by count.Key into g
                where g.All(x => x.Value > 0)
                let sum = g.Sum(x => x.Value)
                orderby sum descending
                select new
                {
                    Delimiter = g.Key,
                    Count = sum
                }
            ).ToList();

            string result = delimiters.Select(x => x.Delimiter).FirstOrDefault();
            return string.IsNullOrEmpty(result) ? defaultDelimiter : result;
        }

        public bool Read()
        {
            if (parser.EndOfData) return false;

            current = parser.ReadFields();
            if (current != null)
            {
                fieldsCount = Math.Max(fieldsCount, current.Length);
                if (TrimFields)
                    current = current.Select(x => x.Trim()).ToArray();
            }

            return true;
        }

        public string[] ToArray()
        {
            return current;
        }

        public void Dispose()
        {
            if (rdr != null)
            {
                rdr.Dispose();
            }
        }
        #endregion
    }
}
