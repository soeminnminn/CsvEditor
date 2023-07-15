using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvEditor.Models
{
    public class EncodingModel
    {
        #region Variables
        private static EncodingModel[] allEncodings = null;

        public static readonly Encoding BigEndianUTF32 = new UTF32Encoding(true, true);

        private static readonly int[] hasBomEncodings = new int[]
            {
                System.Text.Encoding.UTF7.CodePage,
                System.Text.Encoding.UTF8.CodePage,
                System.Text.Encoding.UTF32.CodePage,
                System.Text.Encoding.Unicode.CodePage,
                System.Text.Encoding.BigEndianUnicode.CodePage,
                BigEndianUTF32.CodePage,
            };
        #endregion

        #region Properties
        public static EncodingModel[] Encodings
        {
            get
            {
                if (allEncodings == null)
                {
                    var list = System.Text.Encoding.GetEncodings().ToList();
                    list.Sort(new EncodingInfoComparer());
                    allEncodings = list.Select(x => new EncodingModel(x)).ToArray();
                }

                return allEncodings;
            }
        }

        public Encoding Encoding { get; private set; }

        public EncodingInfo EncodingInfo { get; private set; }

        public int CodePage { get => Encoding.CodePage; }

        public string DisplayName { get => EncodingInfo?.DisplayName ?? Encoding.EncodingName; }

        public bool HasBOM
        {
            get => Array.IndexOf(hasBomEncodings, Encoding.CodePage) > -1;
        }
        
        public byte[] BOM
        {
            get
            {
                if (Encoding.CodePage == System.Text.Encoding.UTF7.CodePage)
                    return BOMS.UTF7;

                else if (Encoding.CodePage == System.Text.Encoding.UTF8.CodePage)
                    return BOMS.UTF8;

                else if (Encoding.CodePage == System.Text.Encoding.UTF32.CodePage)
                    return BOMS.UTF32;

                else if (Encoding.CodePage == System.Text.Encoding.Unicode.CodePage)
                    return BOMS.Unicode;

                else if (Encoding.CodePage == System.Text.Encoding.BigEndianUnicode.CodePage)
                    return BOMS.BigEndianUnicode;

                else if (Encoding.CodePage == BigEndianUTF32.CodePage)
                    return BOMS.BigEndianUTF32;

                return null;
            }
        }
        #endregion

        #region Constructors
        public EncodingModel(Encoding encoding)
        {
            Encoding = encoding;
            EncodingInfo = Encodings.FirstOrDefault(x => x.CodePage == encoding.CodePage)?.EncodingInfo;
        }

        public EncodingModel(EncodingInfo encodingInfo)
        {
            EncodingInfo = encodingInfo;
            Encoding = encodingInfo.GetEncoding();
        }

        public EncodingModel(int codePage)
        {
            var model = Encodings.FirstOrDefault(x => x.CodePage == codePage);
            EncodingInfo = model.EncodingInfo;
            Encoding = model.Encoding;
        }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (obj is System.Text.Encoding enc)
                return Encoding.CodePage == enc.CodePage;
            else if (obj is System.Text.EncodingInfo info)
                return Encoding.CodePage == info.CodePage;
            else if (obj is int i)
                return Encoding.CodePage == i;
            else if (obj is string str)
                return Encoding.EncodingName == str || EncodingInfo?.DisplayName == str;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Encoding.GetHashCode();
        }

        public override string ToString()
        {
            return Encoding.EncodingName;
        }
        #endregion

        #region Nested Types
        public static class BOMS
        {
            public static readonly byte[] BigEndianUnicode = new byte[] { 0xFE, 0xFF };
            public static readonly byte[] Unicode = new byte[] { 0xFF, 0xFE };
            public static readonly byte[] UTF32 = new byte[] { 0xFF, 0xFE, 0, 0 };
            public static readonly byte[] BigEndianUTF32 = new byte[] { 0, 0, 0xFE, 0xFF };
            public static readonly byte[] UTF7 = new byte[] { 0x2B, 0x2F, 0x76 };
            public static readonly byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF };
        }

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

        #region Operators
        public static implicit operator EncodingModel(Encoding encoding)
            => new EncodingModel(encoding);
        public static explicit operator Encoding(EncodingModel encoding)
            => encoding.Encoding;

        public static implicit operator EncodingModel(EncodingInfo encoding)
            => new EncodingModel(encoding);
        public static explicit operator EncodingInfo(EncodingModel encoding)
            => encoding.EncodingInfo;

        public static implicit operator EncodingModel(int codePage)
            => new EncodingModel(codePage);
        public static explicit operator int(EncodingModel encoding)
            => encoding.CodePage;
        #endregion
    }
}
