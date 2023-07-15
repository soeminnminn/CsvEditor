using System;

namespace CsvEditor.Models
{
    public class DelimiterModel
    {
        #region Variables
        public static readonly DelimiterModel[] Delimiters = new DelimiterModel[]
        {
            new DelimiterModel(",", "Comma"),
            new DelimiterModel(";", "Semicolon"),
            new DelimiterModel("|", "Vertical Line"),
            new DelimiterModel("\t", "Tab"),
        };

        public static readonly DelimiterModel Default = new DelimiterModel(",", "Comma");
        #endregion

        #region Constructor
        public DelimiterModel(string delimiter, string name)
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
            else if (obj is DelimiterModel item)
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
