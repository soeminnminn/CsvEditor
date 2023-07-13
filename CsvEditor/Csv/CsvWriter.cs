/*
 * NReco CSV library (https://github.com/nreco/csv/)
 * Copyright 2017-2018 Vitaliy Fedorchenko
 * Distributed under the MIT license
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;

namespace CsvEditor.Csv
{
    /// <summary>
    /// Fast and efficient implementation of CSV writer.
    /// </summary>
    /// <remarks>API is similar to CSVHelper CsvWriter class</remarks>
    public class CsvWriter : IDisposable
    {
        #region Variables
        private readonly TextWriter wr;

        private char[] quoteRequiredChars;
        private bool checkDelimForQuote = false;
        private string quoteString = "\"";
        private string doubleQuoteString = "\"\"";
        private int recordFieldCount = 0;
        #endregion

        #region Properties
        public string Delimiter { get; private set; }

        public string QuoteString
        {
            get
            {
                return quoteString;
            }
            set
            {
                quoteString = value;
                doubleQuoteString = value + value;
            }
        }

        public bool QuoteAllFields { get; set; } = false;

        public bool Trim { get; set; } = false;
        #endregion

        #region Constructors
        public CsvWriter(TextWriter wr) 
            : this(wr, ",") 
        { }

        public CsvWriter(TextWriter wr, string delimiter)
        {
            this.wr = wr;
            Delimiter = delimiter;
            checkDelimForQuote = delimiter.Length > 1;
            quoteRequiredChars = checkDelimForQuote ? new[] { '\r', '\n' } : new[] { '\r', '\n', delimiter[0] };
        }
        #endregion

        #region Methods
        public void WriteField(string field)
        {
            var shouldQuote = QuoteAllFields;

            field = field ?? string.Empty;

            if (field.Length > 0 && Trim)
            {
                field = field.Trim();
            }

            if (field.Length > 0)
            {
                if (shouldQuote // Quote all fields
                    || field.Contains(quoteString) // Contains quote
                    || field[0] == ' ' // Starts with a space
                    || field[field.Length - 1] == ' ' // Ends with a space
                    || field.IndexOfAny(quoteRequiredChars) > -1 // Contains chars that require quotes
                    || (checkDelimForQuote && field.Contains(Delimiter)) // Contains delimiter
                )
                {
                    shouldQuote = true;
                }
            }

            // All quotes must be doubled.       
            if (shouldQuote && field.Length > 0)
            {
                field = field.Replace(quoteString, doubleQuoteString);
            }

            if (shouldQuote)
            {
                field = quoteString + field + quoteString;
            }
            if (recordFieldCount > 0)
            {
                wr.Write(Delimiter);
            }
            if (field.Length > 0)
                wr.Write(field);
            recordFieldCount++;
        }

        public void WriteRow(string[] row)
        {
            if (row == null || row.Length == 0) return;

            foreach(var field in row)
            {
                WriteField(field);
            }
            NextRecord();
        }

        public void NextRecord()
        {
            wr.WriteLine();
            recordFieldCount = 0;
        }

        public void Dispose()
        {
            wr.Dispose();
        }
        #endregion
    }
}
