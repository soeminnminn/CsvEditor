using System;

namespace NestedHtmlWriter
{
    /// <summary>
    /// Utility functions for NestedHtmlWriter
    /// </summary>
    public class NhUtil
    {
        /// <summary>
        /// encode string to defeind reference charaters
        /// </summary>
        /// <param name="s">normal string</param>
        /// <returns>encoded string</returns>
        public static string QuoteText(string s)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '&':
                        sb.Append("&amp;");
                        break;
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
