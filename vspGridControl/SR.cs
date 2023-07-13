using System;
using System.Globalization;
using System.Resources;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    internal class SR
    {
        public static string CellDefaultAccessibleName(long rowIndex, int columnIndex)
        {
            return Keys.GetString("CellDefaultAccessibleName", new object[] { rowIndex, columnIndex });
        }

        public static string ColumnHeaderAAName(int columnIndex)
        {
            return Keys.GetString("ColumnHeaderAAName", new object[] { columnIndex });
        }

        public static string ColumnNumber(int num)
        {
            return Keys.GetString("ColumnNumber", new object[] { num });
        }

        public static string ToolTipUrl(string url)
        {
            return Keys.GetString("ToolTipUrl", new object[] { url });
        }

        public static string ClearSelAndSelectCell
        {
            get
            {
                return Keys.GetString("ClearSelAndSelectCell");
            }
        }

        public static string ColumnHeaderAADefaultAction
        {
            get
            {
                return Keys.GetString("ColumnHeaderAADefaultAction");
            }
        }

        public static CultureInfo Culture
        {
            get
            {
                return Keys.Culture;
            }
            set
            {
                Keys.Culture = value;
            }
        }

        public static string EmbeddedEditCancelAAName
        {
            get
            {
                return Keys.GetString("EmbeddedEditCancelAAName");
            }
        }

        public static string EmbeddedEditCancelDefaultAction
        {
            get
            {
                return Keys.GetString("EmbeddedEditCancelDefaultAction");
            }
        }

        public static string EmbeddedEditCommitAAName
        {
            get
            {
                return Keys.GetString("EmbeddedEditCommitAAName");
            }
        }

        public static string EmbeddedEditCommitDefaultAction
        {
            get
            {
                return Keys.GetString("EmbeddedEditCommitDefaultAction");
            }
        }

        public static string GridControlAaName
        {
            get
            {
                return Keys.GetString("GridControlAaName");
            }
        }

        public static string StartEditCell
        {
            get
            {
                return Keys.GetString("StartEditCell");
            }
        }

        internal class Keys
        {
            public const string CellDefaultAccessibleName = "CellDefaultAccessibleName";
            public const string ClearSelAndSelectCell = "ClearSelAndSelectCell";
            public const string ColumnHeaderAADefaultAction = "ColumnHeaderAADefaultAction";
            public const string ColumnHeaderAAName = "ColumnHeaderAAName";
            public const string ColumnNumber = "ColumnNumber";
            public const string EmbeddedEditCancelAAName = "EmbeddedEditCancelAAName";
            public const string EmbeddedEditCancelDefaultAction = "EmbeddedEditCancelDefaultAction";
            public const string EmbeddedEditCommitAAName = "EmbeddedEditCommitAAName";
            public const string EmbeddedEditCommitDefaultAction = "EmbeddedEditCommitDefaultAction";
            public const string GridControlAaName = "GridControlAaName";
            public const string StartEditCell = "StartEditCell";
            public const string ToolTipUrl = "ToolTipUrl";

            private static CultureInfo culture = null;
            private static ResourceManager resourceManager = new ResourceManager("Microsoft.SqlServer.Management.UI.Grid.SR", typeof(SR).Module.Assembly);
            
            public static string GetString(string key)
            {
                return resourceManager.GetString(key, culture);
            }

            public static string GetString(string key, params object[] args)
            {
                return string.Format(resourceManager.GetString(key, culture), args);
            }

            public static CultureInfo Culture
            {
                get
                {
                    return culture;
                }
                set
                {
                    culture = value;
                }
            }
        }
    }
}

