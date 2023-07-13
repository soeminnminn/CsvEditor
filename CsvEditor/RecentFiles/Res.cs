using System;

namespace CsvEditor.RecentFiles
{
    internal static class Res
    {
        internal static string ClearList
        {
            get
            {
                return "Clear list";
            }
        }

        internal static string FilePathNotFullyQualified
        {
            get
            {
                return "{0} is not a fully qualified path.";
            }
        }

        internal static string NotAnExistingDirectory
        {
            get
            {
                return "{0} doesn't refer to an existing directory.";
            }
        }

        internal static string NotInitialized
        {
            get
            {
                return "The MenuItem has not yet been assigned. Call {0} first!";
            }
        }
    }
}
