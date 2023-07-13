// Microsoft.VisualBasic.FileIO.TextFieldParser
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvEditor.Csv
{
    /// <summary>
    /// Indicates whether text fields are delimited or fixed width.
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        /// Indicates that the fields are delimited.
        /// </summary>
        Delimited,
        /// <summary>
        /// Indicates that the fields are fixed width.
        /// </summary>
        FixedWidth
    }

    public class MalformedLineException : Exception
    {
        public readonly long LineNumber;

        public MalformedLineException(long lineNo)
            : base()
        {
            LineNumber = lineNo;
        }
    }

    /// <summary>
    /// Provides methods and properties for parsing structured text files.
    /// </summary>
    public class TextFieldParser : IDisposable
    {
        #region Variables
        private const RegexOptions REGEX_OPTIONS = RegexOptions.CultureInvariant;

        private const int DEFAULT_BUFFER_LENGTH = 4096;

        private const int DEFAULT_BUILDER_INCREASE = 10;

        private const string BEGINS_WITH_QUOTE = "\\G[{0}]*\"";

        private const string ENDING_QUOTE = "\"[{0}]*";

        private delegate int ChangeBufferFunction();

        private bool m_Disposed;

        private TextReader m_Reader;

        private string[] m_CommentTokens;

        private long m_LineNumber;

        private bool m_EndOfData;

        private string m_ErrorLine;

        private long m_ErrorLineNumber;

        private FieldType m_TextFieldType;

        private int[] m_FieldWidths;

        private string[] m_Delimiters;

        private int[] m_FieldWidthsCopy;

        private string[] m_DelimitersCopy;

        private Regex m_DelimiterRegex;

        private Regex m_DelimiterWithEndCharsRegex;

        private int[] m_WhitespaceCodes;

        private Regex m_BeginQuotesRegex;

        private Regex m_WhiteSpaceRegEx;

        private bool m_TrimWhiteSpace;

        private int m_Position;

        private int m_PeekPosition;

        private int m_CharsRead;

        private bool m_NeedPropertyCheck;

        private char[] m_Buffer;

        private int m_LineLength;

        private bool m_HasFieldsEnclosedInQuotes;

        private string m_SpaceChars;

        private int m_MaxLineSize;

        private int m_MaxBufferSize;

        private bool m_LeaveOpen;
        #endregion

        #region Constructors
        private TextFieldParser()
        {
            m_CommentTokens = new string[0];
            m_LineNumber = 1L;
            m_EndOfData = false;
            m_ErrorLine = "";
            m_ErrorLineNumber = -1L;
            m_TextFieldType = FieldType.Delimited;
            m_WhitespaceCodes = new int[23]
            {
                9, 11, 12, 32, 133, 160, 5760, 8192, 8193, 8194,
                8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8203, 8232,
                8233, 12288, 65279
            };
            m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            m_TrimWhiteSpace = true;
            m_Position = 0;
            m_PeekPosition = 0;
            m_CharsRead = 0;
            m_NeedPropertyCheck = true;
            m_Buffer = new char[4096];
            m_HasFieldsEnclosedInQuotes = true;
            m_MaxLineSize = 10000000;
            m_MaxBufferSize = 10000000;
            m_LeaveOpen = false;
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="path">The complete path of the file to be parsed.</param>
        public TextFieldParser(string path)
            : this()
        {
            InitializeFromPath(path, Encoding.UTF8, detectEncoding: true);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="path">The complete path of the file to be parsed.</param>
        /// <param name="defaultEncoding">The character encoding to use if encoding is not determined from file. Default is System.Text.Encoding.UTF8.</param>
        public TextFieldParser(string path, Encoding defaultEncoding)
            : this()
        {
            InitializeFromPath(path, defaultEncoding, detectEncoding: true);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="path">The complete path of the file to be parsed.</param>
        /// <param name="defaultEncoding">The character encoding to use if encoding is not determined from file. Default is System.Text.Encoding.UTF8.</param>
        /// <param name="detectEncoding">Indicates whether to look for byte order marks at the beginning of the file. Default is True.</param>
        public TextFieldParser(string path, Encoding defaultEncoding, bool detectEncoding)
            : this()
        {
            InitializeFromPath(path, defaultEncoding, detectEncoding);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="stream">The stream to be parsed.</param>
        public TextFieldParser(Stream stream)
            : this()
        {
            InitializeFromStream(stream, Encoding.UTF8, detectEncoding: true);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="stream">The stream to be parsed.</param>
        /// <param name="defaultEncoding">The character encoding to use if encoding is not determined from file. Default is System.Text.Encoding.UTF8.</param>
        public TextFieldParser(Stream stream, Encoding defaultEncoding)
            : this()
        {
            InitializeFromStream(stream, defaultEncoding, detectEncoding: true);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="stream">The stream to be parsed.</param>
        /// <param name="defaultEncoding">The character encoding to use if encoding is not determined from file. Default is System.Text.Encoding.UTF8.</param>
        /// <param name="detectEncoding">Indicates whether to look for byte order marks at the beginning of the file. Default is True.</param>
        public TextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding)
            : this()
        {
            InitializeFromStream(stream, defaultEncoding, detectEncoding);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="stream">The stream to be parsed.</param>
        /// <param name="defaultEncoding">The character encoding to use if encoding is not determined from file. Default is System.Text.Encoding.UTF8.</param>
        /// <param name="detectEncoding">Indicates whether to look for byte order marks at the beginning of the file. Default is True.</param>
        /// <param name="leaveOpen">Indicates whether to leave stream open when the TextFieldParser object is closed. Default is False.</param>
        public TextFieldParser(Stream stream, Encoding defaultEncoding, bool detectEncoding, bool leaveOpen)
            : this()
        {
            InitializeFromStream(stream, defaultEncoding, detectEncoding);
        }

        /// <summary>
        /// Initializes a new instance of the TextFieldParser class.
        /// </summary>
        /// <param name="reader">The System.IO.TextReader stream to be parsed.</param>
        public TextFieldParser(TextReader reader)
            : this()
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            
            m_Reader = reader;
            ReadToBuffer();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Defines comment tokens. A comment token is a string that, when placed at the
        /// beginning of a line, indicates that the line is a comment and should be ignored
        /// by the parser.
        /// </summary>
        public string[] CommentTokens
        {
            get
            {
                return m_CommentTokens;
            }
        }

        /// <summary>
        /// Returns True if there are no non-blank, non-comment lines between the current
        /// cursor position and the end of the file.
        /// </summary>
        public bool EndOfData
        {
            get
            {
                if (m_EndOfData)
                {
                    return m_EndOfData;
                }

                if ((m_Reader == null) | (m_Buffer == null))
                {
                    m_EndOfData = true;
                    return true;
                }

                if (PeekNextDataLine() != null)
                {
                    return false;
                }

                m_EndOfData = true;
                return true;
            }
        }

        /// <summary>
        /// Returns the current line number, or returns -1 if no more characters are available
        /// in the stream.
        /// </summary>
        public long LineNumber
        {
            get
            {
                if (m_LineNumber != -1 && ((m_Reader.Peek() == -1) & (m_Position == m_CharsRead)))
                {
                    CloseReader();
                }

                return m_LineNumber;
            }
        }

        /// <summary>
        /// Returns the line that caused the most recent Microsoft.VisualBasic.FileIO.MalformedLineException
        /// exception.
        /// </summary>
        public string ErrorLine => m_ErrorLine;

        /// <summary>
        /// Returns the number of the line that caused the most recent Microsoft.VisualBasic.FileIO.MalformedLineException
        /// exception.
        /// </summary>
        public long ErrorLineNumber => m_ErrorLineNumber;

        /// <summary>
        /// Indicates whether the file to be parsed is delimited or fixed-width.
        /// </summary>
        public FieldType TextFieldType
        {
            get
            {
                return m_TextFieldType;
            }
            set
            {
                ValidateFieldTypeEnumValue(value, "value");
                m_TextFieldType = value;
                m_NeedPropertyCheck = true;
            }
        }

        /// <summary>
        /// Denotes the width of each column in the text file being parsed.
        /// </summary>
        public int[] FieldWidths
        {
            get
            {
                return m_FieldWidths;
            }
            set
            {
                if (value != null)
                {
                    ValidateFieldWidthsOnInput(value);
                    m_FieldWidthsCopy = (int[])value.Clone();
                }
                else
                {
                    m_FieldWidthsCopy = null;
                }

                m_FieldWidths = value;
                m_NeedPropertyCheck = true;
            }
        }

        /// <summary>
        /// Defines the delimiters for a text file.
        /// </summary>
        public string[] Delimiters
        {
            get
            {
                return m_Delimiters;
            }
            set
            {
                if (value != null)
                {
                    ValidateDelimiters(value);
                    m_DelimitersCopy = (string[])value.Clone();
                }
                else
                {
                    m_DelimitersCopy = null;
                }

                m_Delimiters = value;
                m_NeedPropertyCheck = true;
                m_BeginQuotesRegex = null;
            }
        }

        /// <summary>
        /// Indicates whether leading and trailing white space should be trimmed from field
        /// values.
        /// </summary>
        public bool TrimWhiteSpace
        {
            get
            {
                return m_TrimWhiteSpace;
            }
            set
            {
                m_TrimWhiteSpace = value;
            }
        }

        /// <summary>
        /// Denotes whether fields are enclosed in quotation marks when a delimited file
        /// is being parsed.
        /// </summary>
        public bool HasFieldsEnclosedInQuotes
        {
            get
            {
                return m_HasFieldsEnclosedInQuotes;
            }
            set
            {
                m_HasFieldsEnclosedInQuotes = value;
            }
        }

        private Regex BeginQuotesRegex
        {
            get
            {
                if (m_BeginQuotesRegex == null)
                {
                    string pattern = string.Format(CultureInfo.InvariantCulture, "\\G[{0}]*\"", new object[1] { WhitespacePattern });
                    m_BeginQuotesRegex = new Regex(pattern, RegexOptions.CultureInvariant);
                }

                return m_BeginQuotesRegex;
            }
        }

        private string EndQuotePattern => string.Format(CultureInfo.InvariantCulture, "\"[{0}]*", new object[1] { WhitespacePattern });

        private string WhitespaceCharacters
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                int[] whitespaceCodes = m_WhitespaceCodes;
                for (int i = 0; i < whitespaceCodes.Length; i = checked(i + 1))
                {
                    char c = Convert.ToChar(whitespaceCodes[i]);
                    if (!CharacterIsInDelimiter(c))
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString();
            }
        }

        private string WhitespacePattern
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                int[] whitespaceCodes = m_WhitespaceCodes;
                for (int i = 0; i < whitespaceCodes.Length; i = checked(i + 1))
                {
                    int charCode = whitespaceCodes[i];
                    char testCharacter = Convert.ToChar(charCode);
                    if (!CharacterIsInDelimiter(testCharacter))
                    {
                        stringBuilder.Append("\\u" + charCode.ToString("X4", CultureInfo.InvariantCulture));
                    }
                }

                return stringBuilder.ToString();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the delimiters for the reader to the specified values, and sets the field
        /// type to Delimited.
        /// </summary>
        /// <param name="delimiters">Array of type String.</param>
        public void SetDelimiters(params string[] delimiters)
        {
            Delimiters = delimiters;
        }

        /// <summary>
        /// Sets the comment tokens for the reader to the specified values.
        /// </summary>
        /// <param name="tokens">Array of type String.</param>
        public void SetCommentTokens(params string[] tokens)
        {
            CheckCommentTokensForWhitespace(tokens);
            m_CommentTokens = tokens;
            m_NeedPropertyCheck = true;
        }

        /// <summary>
        /// Sets the delimiters for the reader to the specified values.
        /// </summary>
        /// <param name="fieldWidths">Array of Integer.</param>
        public void SetFieldWidths(params int[] fieldWidths)
        {
            FieldWidths = fieldWidths;
        }

        /// <summary>
        /// Returns the current line as a string and advances the cursor to the next line.
        /// </summary>
        /// <returns>The current line from the file or stream.</returns>
        public string ReadLine()
        {
            if ((m_Reader == null) | (m_Buffer == null))
            {
                return null;
            }

            ChangeBufferFunction changeBuffer = ReadToBuffer;
            string text = ReadNextLine(ref m_Position, changeBuffer);
            if (text == null)
            {
                FinishReading();
                return null;
            }

            checked
            {
                m_LineNumber++;
                return text.TrimEnd('\r', '\n');
            }
        }

        /// <summary>
        /// Reads all fields on the current line, returns them as an array of strings, and
        /// advances the cursor to the next line containing data.
        /// </summary>
        /// <returns>An array of strings that contains field values for the current line.</returns>
        public string[] ReadFields()
        {
            if ((m_Reader == null) | (m_Buffer == null))
            {
                return null;
            }

            ValidateReadyToRead();
            switch(m_TextFieldType)
            {
                case FieldType.FixedWidth: return ParseFixedWidthLine();
                case FieldType.Delimited: return ParseDelimitedLine();
                default: return null;
            }
        }

        /// <summary>
        /// Reads the specified number of characters without advancing the cursor.
        /// </summary>
        /// <param name="numberOfChars">Number of characters to read. Required.</param>
        /// <returns>A string that contains the specified number of characters read.</returns>
        public string PeekChars(int numberOfChars)
        {
            if (numberOfChars <= 0)
            {
                throw new ArgumentException("numberOfChars");
            }

            if ((m_Reader == null) | (m_Buffer == null))
            {
                return null;
            }

            if (m_EndOfData)
            {
                return null;
            }

            string text = PeekNextDataLine();
            if (text == null)
            {
                m_EndOfData = true;
                return null;
            }

            text = text.TrimEnd('\r', '\n');
            if (text.Length < numberOfChars)
            {
                return text;
            }

            return new StringInfo(text).SubstringByTextElements(0, numberOfChars);
        }

        /// <summary>
        /// Reads the remainder of the text file and returns it as a string.
        /// </summary>
        /// <returns>The remaining text from the file or stream.</returns>
        public string ReadToEnd()
        {
            if ((m_Reader == null) | (m_Buffer == null))
            {
                return null;
            }

            StringBuilder stringBuilder = new StringBuilder(m_Buffer.Length);
            stringBuilder.Append(m_Buffer, m_Position, checked(m_CharsRead - m_Position));
            stringBuilder.Append(m_Reader.ReadToEnd());
            FinishReading();
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Closes the current TextFieldParser object.
        /// </summary>
        public void Close()
        {
            CloseReader();
        }

        /// <summary>
        /// Releases resources used by the TextFieldParser object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the Microsoft.VisualBasic.FileIO.TextFieldParser object.
        /// </summary>
        /// <param name="disposing">True releases both managed and unmanaged resources; False releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!m_Disposed)
                {
                    Close();
                }

                m_Disposed = true;
            }
        }

        private void ValidateFieldTypeEnumValue(FieldType value, string paramName)
        {
            if (value < FieldType.Delimited || value > FieldType.FixedWidth)
            {
                throw new InvalidEnumArgumentException(paramName, (int)value, typeof(FieldType));
            }
        }

        private void CloseReader()
        {
            FinishReading();
            if (m_Reader != null)
            {
                if (!m_LeaveOpen)
                {
                    m_Reader.Close();
                }

                m_Reader = null;
            }
        }

        private void FinishReading()
        {
            m_LineNumber = -1L;
            m_EndOfData = true;
            m_Buffer = null;
            m_DelimiterRegex = null;
            m_BeginQuotesRegex = null;
        }

        private void InitializeFromPath(string path, Encoding defaultEncoding, bool detectEncoding)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (defaultEncoding == null)
            {
                throw new ArgumentNullException("defaultEncoding");
            }

            string filePath = ValidatePath(path);
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            m_Reader = new StreamReader(stream, defaultEncoding, detectEncoding);
            ReadToBuffer();
        }

        private void InitializeFromStream(Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("stream");
            }

            if (defaultEncoding == null)
            {
                throw new ArgumentNullException("defaultEncoding");
            }

            m_Reader = new StreamReader(stream, defaultEncoding, detectEncoding);
            ReadToBuffer();
        }

        private string ValidatePath(string path)
        {
            string text = Path.GetFullPath(path);
            if (!File.Exists(text))
            {
                throw new FileNotFoundException();
            }

            return text;
        }

        private bool IgnoreLine(string line)
        {
            if (line == null)
            {
                return false;
            }

            string text = line.Trim();
            if (text.Length == 0)
            {
                return true;
            }

            if (m_CommentTokens != null)
            {
                string[] commentTokens = m_CommentTokens;
                foreach (string token in commentTokens)
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        if (text.StartsWith(token, StringComparison.Ordinal))
                        {
                            return true;
                        }

                        if (line.StartsWith(token, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private int ReadToBuffer()
        {
            m_Position = 0;
            int length = m_Buffer.Length;
            if (length > 4096)
            {
                length = 4096;
                m_Buffer = new char[checked(length - 1 + 1)];
            }

            m_CharsRead = m_Reader.Read(m_Buffer, 0, length);
            return m_CharsRead;
        }

        private int SlideCursorToStartOfBuffer()
        {
            checked
            {
                if (m_Position > 0)
                {
                    int length = m_Buffer.Length;
                    int prevCount = m_CharsRead - m_Position;
                    char[] array = new char[length - 1 + 1];
                    Array.Copy(m_Buffer, m_Position, array, 0, prevCount);
                    int readCount = m_Reader.Read(array, prevCount, length - prevCount);
                    m_CharsRead = prevCount + readCount;
                    m_Position = 0;
                    m_Buffer = array;
                    return readCount;
                }

                return 0;
            }
        }

        private int IncreaseBufferSize()
        {
            m_PeekPosition = m_CharsRead;
            checked
            {
                int acquireSize = m_Buffer.Length + 4096;
                if (acquireSize > m_MaxBufferSize)
                {
                    throw new InvalidOperationException();
                }

                char[] array = new char[acquireSize - 1 + 1];
                Array.Copy(m_Buffer, array, m_Buffer.Length);
                int readCount = m_Reader.Read(array, m_Buffer.Length, 4096);
                m_Buffer = array;
                m_CharsRead += readCount;
                return readCount;
            }
        }

        private string ReadNextDataLine()
        {
            ChangeBufferFunction changeBuffer = ReadToBuffer;
            checked
            {
                string line;
                do
                {
                    line = ReadNextLine(ref m_Position, changeBuffer);
                    m_LineNumber++;
                }
                while (IgnoreLine(line));

                if (line == null)
                {
                    CloseReader();
                }

                return line;
            }
        }

        private string PeekNextDataLine()
        {
            ChangeBufferFunction changeBuffer = IncreaseBufferSize;
            SlideCursorToStartOfBuffer();
            m_PeekPosition = 0;
            string line;
            do
            {
                line = ReadNextLine(ref m_PeekPosition, changeBuffer);
            }
            while (IgnoreLine(line));
            return line;
        }

        private string ReadNextLine(ref int cursor, ChangeBufferFunction changeBuffer)
        {
            if (cursor == m_CharsRead && changeBuffer() == 0)
            {
                return null;
            }

            StringBuilder stringBuilder = null;
            checked
            {
                do
                {
                    int index = cursor;
                    int endIndex = m_CharsRead - 1;
                    for (int i = index; i <= endIndex; i++)
                    {
                        char c = m_Buffer[i];
                        if (!((string.Compare(c.ToString(), "\r", false) == 0) | (string.Compare(c.ToString(), "\n", false) == 0)))
                        {
                            continue;
                        }

                        if (stringBuilder != null)
                        {
                            stringBuilder.Append(m_Buffer, cursor, i - cursor + 1);
                        }
                        else
                        {
                            stringBuilder = new StringBuilder(i + 1);
                            stringBuilder.Append(m_Buffer, cursor, i - cursor + 1);
                        }

                        cursor = i + 1;
                        if (string.Compare(c.ToString(), "\r", false) == 0)
                        {
                            if (cursor < m_CharsRead)
                            {
                                if (string.Compare(m_Buffer[cursor].ToString(), "\n", false) == 0)
                                {
                                    cursor++;
                                    stringBuilder.Append("\n");
                                }
                            }
                            else if (changeBuffer() > 0 && string.Compare(m_Buffer[cursor].ToString(), "\n", false) == 0)
                            {
                                cursor++;
                                stringBuilder.Append("\n");
                            }
                        }

                        return stringBuilder.ToString();
                    }

                    int remainCount = m_CharsRead - cursor;
                    if (stringBuilder == null)
                    {
                        stringBuilder = new StringBuilder(remainCount + 10);
                    }

                    stringBuilder.Append(m_Buffer, cursor, remainCount);
                }
                while (changeBuffer() > 0);
                return stringBuilder.ToString();
            }
        }

        private string[] ParseDelimitedLine()
        {
            string line = ReadNextDataLine();
            if (line == null)
            {
                return null;
            }

            checked
            {
                long endIndex = m_LineNumber - 1;
                int index = 0;
                List<string> list = new List<string>();
                int endOfLineIndex = GetEndOfLineIndex(line);

                while (index <= endOfLineIndex)
                {
                    Match match = null;
                    bool hasFieldsEnclosedInQuotes = false;
                    if (m_HasFieldsEnclosedInQuotes)
                    {
                        match = BeginQuotesRegex.Match(line, index);
                        hasFieldsEnclosedInQuotes = match.Success;
                    }

                    string field;
                    if (hasFieldsEnclosedInQuotes)
                    {
                        index = match.Index + match.Length;
                        QuoteDelimitedFieldBuilder quoteDelimitedFieldBuilder = new QuoteDelimitedFieldBuilder(m_DelimiterWithEndCharsRegex, m_SpaceChars);
                        quoteDelimitedFieldBuilder.BuildField(line, index);

                        if (quoteDelimitedFieldBuilder.MalformedLine)
                        {
                            m_ErrorLine = line.TrimEnd('\r', '\n');
                            m_ErrorLineNumber = endIndex;
                            throw new MalformedLineException(endIndex);
                        }

                        if (quoteDelimitedFieldBuilder.FieldFinished)
                        {
                            field = quoteDelimitedFieldBuilder.Field;
                            index = quoteDelimitedFieldBuilder.Index + quoteDelimitedFieldBuilder.DelimiterLength;
                        }
                        else
                        {
                            do
                            {
                                int length = line.Length;
                                string nextLine = ReadNextDataLine();
                                if (nextLine == null)
                                {
                                    m_ErrorLine = line.TrimEnd('\r', '\n');
                                    m_ErrorLineNumber = endIndex;
                                    throw new MalformedLineException(endIndex);
                                }

                                if (line.Length + nextLine.Length > m_MaxLineSize)
                                {
                                    m_ErrorLine = line.TrimEnd('\r', '\n');
                                    m_ErrorLineNumber = endIndex;
                                    throw new MalformedLineException(endIndex);
                                }

                                line += nextLine;
                                endOfLineIndex = GetEndOfLineIndex(line);
                                quoteDelimitedFieldBuilder.BuildField(line, length);

                                if (quoteDelimitedFieldBuilder.MalformedLine)
                                {
                                    m_ErrorLine = line.TrimEnd('\r', '\n');
                                    m_ErrorLineNumber = endIndex;
                                    throw new MalformedLineException(endIndex);
                                }
                            }

                            while (!quoteDelimitedFieldBuilder.FieldFinished);

                            field = quoteDelimitedFieldBuilder.Field;
                            index = quoteDelimitedFieldBuilder.Index + quoteDelimitedFieldBuilder.DelimiterLength;
                        }

                        if (m_TrimWhiteSpace)
                        {
                            field = field.Trim();
                        }

                        list.Add(field);
                        continue;
                    }

                    Match matchDelimiter = m_DelimiterRegex.Match(line, index);
                    if (matchDelimiter.Success)
                    {
                        field = line.Substring(index, matchDelimiter.Index - index);
                        if (m_TrimWhiteSpace)
                        {
                            field = field.Trim();
                        }

                        list.Add(field);
                        index = matchDelimiter.Index + matchDelimiter.Length;
                        continue;
                    }

                    field = line.Substring(index).TrimEnd('\r', '\n');
                    if (m_TrimWhiteSpace)
                    {
                        field = field.Trim();
                    }

                    list.Add(field);
                    break;
                }

                return list.ToArray();
            }
        }

        private string[] ParseFixedWidthLine()
        {
            string line = ReadNextDataLine();
            if (line == null)
            {
                return null;
            }

            line = line.TrimEnd('\r', '\n');
            StringInfo lineInfo = new StringInfo(line);
            checked
            {
                ValidateFixedWidthLine(lineInfo, m_LineNumber - 1);
                int start = 0;
                int end = m_FieldWidths.Length - 1;
                string[] array = new string[end + 1];
                for (int i = 0; i <= end; i++)
                {
                    array[i] = GetFixedWidthField(lineInfo, start, m_FieldWidths[i]);
                    start += m_FieldWidths[i];
                }

                return array;
            }
        }

        private string GetFixedWidthField(StringInfo line, int index, int fieldLength)
        {
            string text = ((fieldLength > 0) ? line.SubstringByTextElements(index, fieldLength) : ((index < line.LengthInTextElements) ? line.SubstringByTextElements(index).TrimEnd('\r', '\n') : string.Empty));
            if (m_TrimWhiteSpace)
            {
                return text.Trim();
            }

            return text;
        }

        private int GetEndOfLineIndex(string line)
        {
            int length = line.Length;
            if (length == 1)
            {
                return length;
            }

            checked
            {
                if ((string.Compare(line[length - 2].ToString(), "\r", false) == 0) | (string.Compare(line[length - 2].ToString(), "\n", false) == 0))
                {
                    return length - 2;
                }

                if ((string.Compare(line[length - 1].ToString(), "\r", false) == 0) | (string.Compare(line[length - 1].ToString(), "\n", false) == 0))
                {
                    return length - 1;
                }

                return length;
            }
        }

        private void ValidateFixedWidthLine(StringInfo line, long lineNumber)
        {
            if (line.LengthInTextElements < m_LineLength)
            {
                m_ErrorLine = line.String;
                m_ErrorLineNumber = checked(m_LineNumber - 1);
                throw new MalformedLineException(lineNumber);
            }
        }

        private void ValidateFieldWidths()
        {
            if (m_FieldWidths == null)
            {
                throw new InvalidOperationException("TextFieldParser_FieldWidthsNothing");
            }

            if (m_FieldWidths.Length == 0)
            {
                throw new InvalidOperationException();
            }

            checked
            {
                int lastIndex = m_FieldWidths.Length - 1;
                m_LineLength = 0;
                int end = lastIndex - 1;
                for (int i = 0; i <= end; i++)
                {
                    m_LineLength += m_FieldWidths[i];
                }

                if (m_FieldWidths[lastIndex] > 0)
                {
                    m_LineLength += m_FieldWidths[lastIndex];
                }
            }
        }

        private void ValidateFieldWidthsOnInput(int[] widths)
        {
            checked
            {
                int end = widths.Length - 2;
                for (int i = 0; i <= end; i++)
                {
                    if (widths[i] < 1)
                    {
                        throw new ArgumentException("FieldWidths");
                    }
                }
            }
        }

        private void ValidateAndEscapeDelimiters()
        {
            if (m_Delimiters == null)
            {
                throw new ArgumentException("Delimiters");
            }

            if (m_Delimiters.Length == 0)
            {
                throw new ArgumentException("Delimiters");
            }

            int length = m_Delimiters.Length;
            StringBuilder delimiters = new StringBuilder();
            StringBuilder delimiterWithEndChars = new StringBuilder();
            delimiterWithEndChars.Append(EndQuotePattern + "(");
            checked
            {
                int end = length - 1;
                for (int i = 0; i <= end; i++)
                {
                    if (m_Delimiters[i] != null)
                    {
                        if (m_HasFieldsEnclosedInQuotes && m_Delimiters[i].IndexOf('"') > -1)
                        {
                            throw new InvalidOperationException();
                        }

                        string text = Regex.Escape(m_Delimiters[i]);
                        delimiters.Append(text + "|");
                        delimiterWithEndChars.Append(text + "|");
                    }
                }

                m_SpaceChars = WhitespaceCharacters;
                m_DelimiterRegex = new Regex(delimiters.ToString(0, delimiters.Length - 1), RegexOptions.CultureInvariant);
                delimiters.Append("\r|\n");
                m_DelimiterWithEndCharsRegex = new Regex(delimiters.ToString(), RegexOptions.CultureInvariant);
                delimiterWithEndChars.Append("\r|\n)|\"$");
            }
        }

        private void ValidateReadyToRead()
        {
            if (!(m_NeedPropertyCheck | ArrayHasChanged()))
            {
                return;
            }

            switch (m_TextFieldType)
            {
                case FieldType.Delimited:
                    ValidateAndEscapeDelimiters();
                    break;
                case FieldType.FixedWidth:
                    ValidateFieldWidths();
                    break;
            }

            if (m_CommentTokens != null)
            {
                string[] commentTokens = m_CommentTokens;
                foreach (string token in commentTokens)
                {
                    if (string.IsNullOrEmpty(token) && (m_HasFieldsEnclosedInQuotes & (m_TextFieldType == FieldType.Delimited)) && string.Compare(token.Trim(), "\"", StringComparison.Ordinal) == 0)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            m_NeedPropertyCheck = false;
        }

        private void ValidateDelimiters(string[] delimiterArray)
        {
            if (delimiterArray == null)
            {
                return;
            }

            foreach (string delimiter in delimiterArray)
            {
                if (string.IsNullOrEmpty(delimiter))
                {
                    throw new ArgumentException("Delimiters");
                }

                if (delimiter.IndexOfAny(new char[2] { '\r', '\n' }) > -1)
                {
                    throw new ArgumentException("Delimiters");
                }
            }
        }

        private bool ArrayHasChanged()
        {
            int lowerBound;
            checked
            {
                switch (m_TextFieldType)
                {
                    case FieldType.Delimited:
                        {
                            if (m_Delimiters == null)
                            {
                                return false;
                            }

                            lowerBound = m_DelimitersCopy.GetLowerBound(0);
                            int upperBound = m_DelimitersCopy.GetUpperBound(0);
                            for (int j = lowerBound; j <= upperBound; j++)
                            {
                                if (string.Compare(m_Delimiters[j], m_DelimitersCopy[j], false) != 0)
                                {
                                    return true;
                                }
                            }

                            break;
                        }
                    case FieldType.FixedWidth:
                        {
                            if (m_FieldWidths == null)
                            {
                                return false;
                            }

                            lowerBound = m_FieldWidthsCopy.GetLowerBound(0);
                            int upperBound = m_FieldWidthsCopy.GetUpperBound(0);
                            for (int i = lowerBound; i <= upperBound; i++)
                            {
                                if (m_FieldWidths[i] != m_FieldWidthsCopy[i])
                                {
                                    return true;
                                }
                            }

                            break;
                        }
                }

                return false;
            }
        }

        private void CheckCommentTokensForWhitespace(string[] tokens)
        {
            if (tokens == null)
            {
                return;
            }

            foreach (string token in tokens)
            {
                if (m_WhiteSpaceRegEx.IsMatch(token))
                {
                    throw new ArgumentException("CommentTokens");
                }
            }
        }

        private bool CharacterIsInDelimiter(char testCharacter)
        {
            string[] delimiters = m_Delimiters;
            for (int i = 0; i < delimiters.Length; i = checked(i + 1))
            {
                if (delimiters[i].IndexOf(testCharacter) > -1)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Nested Types
        internal class QuoteDelimitedFieldBuilder
        {
            private StringBuilder m_Field;

            private bool m_FieldFinished;

            private int m_Index;

            private int m_DelimiterLength;

            private Regex m_DelimiterRegex;

            private string m_SpaceChars;

            private bool m_MalformedLine;

            public bool FieldFinished => m_FieldFinished;

            public string Field => m_Field.ToString();

            public int Index => m_Index;

            public int DelimiterLength => m_DelimiterLength;

            public bool MalformedLine => m_MalformedLine;

            public QuoteDelimitedFieldBuilder(Regex DelimiterRegex, string SpaceChars)
            {
                m_Field = new StringBuilder();
                m_DelimiterRegex = DelimiterRegex;
                m_SpaceChars = SpaceChars;
            }

            public void BuildField(string Line, int StartAt)
            {
                m_Index = StartAt;
                int length = Line.Length;
                checked
                {
                    while (m_Index < length)
                    {
                        if (Line[m_Index] == '"')
                        {
                            if (m_Index + 1 == length)
                            {
                                m_FieldFinished = true;
                                m_DelimiterLength = 1;
                                m_Index++;
                                break;
                            }
                            if (!((m_Index + 1 < Line.Length) & (Line[m_Index + 1] == '"')))
                            {
                                Match match = m_DelimiterRegex.Match(Line, m_Index + 1);
                                int num = (match.Success ? (match.Index - 1) : (length - 1));
                                int num2 = m_Index + 1;
                                int num3 = num;
                                for (int i = num2; i <= num3; i++)
                                {
                                    if (m_SpaceChars.IndexOf(Line[i]) < 0)
                                    {
                                        m_MalformedLine = true;
                                        return;
                                    }
                                }
                                m_DelimiterLength = 1 + num - m_Index;
                                if (match.Success)
                                {
                                    m_DelimiterLength += match.Length;
                                }
                                m_FieldFinished = true;
                                break;
                            }
                            m_Field.Append('"');
                            m_Index += 2;
                        }
                        else
                        {
                            m_Field.Append(Line[m_Index]);
                            m_Index++;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
