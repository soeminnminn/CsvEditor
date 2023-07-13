using System;

namespace CsvEditor.Models
{
    public class DelimiterItem
    {
        #region Variables
        public static readonly DelimiterItem[] Delimiters = new DelimiterItem[]
        {
            new DelimiterItem(",", "Comma"),
            new DelimiterItem(";", "Semicolon"),
            new DelimiterItem("|", "Vertical Line"),
            new DelimiterItem("\t", "Tab"),
        };

        public static readonly DelimiterItem Default = new DelimiterItem(",", "Comma");
        #endregion

        #region Constructor
        public DelimiterItem(string delimiter, string name)
        {
            Delimiter = delimiter;
            Name = name;
        }
        #endregion

        #region Properties
        public string Delimiter { get; }
        public string Name { get; }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{Name} ({Delimiter.Replace("\t", "\\t")})";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is string str)
                return str == Delimiter;
            else if (obj is DelimiterItem item)
                return item.Delimiter == Delimiter;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Delimiter.GetHashCode();
        }
        #endregion
    }
}
