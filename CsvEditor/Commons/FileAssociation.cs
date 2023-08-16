using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace CsvEditor
{
    /*
     * Icon Sizes
     * 256x256,32bit
     * 64x64,32bit
     * 48x48,32bit
     * 40x40,32bit
     * 32x32,32bit
     * 24x24,32bit
     * 20x20,32bit
     * 16x16,32bit
     */
    public class FileAssociation
    {
        public string Extension { get; set; }
        public string ProgId { get; set; }
        public string[] ContentTypes { get; set; }
        public string PerceivedType { get; set; }
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
        public int IconIndex { get; set; } = 0;
    }

    public static class FileAssociations
    {
        #region Variables
        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        private static FileAssociation csvAssociation = null;
        private static FileAssociation tsvAssociation = null;
        #endregion

        #region Native Methods
        // needed so that Explorer windows get refreshed after the registry is updated
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
        #endregion

        #region Properties
        public static FileAssociation CsvAssociation
        {
            get
            {
                if (csvAssociation == null)
                {
                    var filePath = Process.GetCurrentProcess().MainModule.FileName;
                    csvAssociation = new FileAssociation
                    {
                        Extension = ".csv",
                        ProgId = "CSVEditor.csv",
                        ContentTypes = new string[] { 
                            "application/csv", "application/excel", "application/vnd.ms-excel", 
                            "application/vnd.msexcel", "text/anytext", "text/comma-separated-values" 
                        },
                        PerceivedType = "text",
                        FileTypeDescription = "CSV File",
                        ExecutableFilePath = filePath,
                        IconIndex = 1
                    };
                }
                return csvAssociation;
            }
        }

        public static FileAssociation TsvAssociation
        {
            get
            {
                if (tsvAssociation == null)
                {
                    var filePath = Process.GetCurrentProcess().MainModule.FileName;
                    tsvAssociation = new FileAssociation
                    {
                        Extension = ".tsv",
                        ProgId = "CSVEditor.tsv",
                        ContentTypes = new string[] { "text/tab-separated-values" },
                        PerceivedType = "text",
                        FileTypeDescription = "TSV File",
                        ExecutableFilePath = filePath,
                        IconIndex = 2
                    };
                }
                return tsvAssociation;
            }
        }
        #endregion

        #region Methods
        public static bool IsAssociated(this FileAssociation association)
        {
            return IsAssociated(association.Extension, association.ProgId);
        }

        public static bool EnsureSet(this FileAssociation association)
        {
            bool madeChanges = SetAssociation(association);
            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
            return madeChanges;
        }

        public static bool EnsureAssociationsSet(params FileAssociation[] associations)
        {
            bool madeChanges = false;
            foreach (var association in associations)
            {
                madeChanges |= SetAssociation(association);
            }

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }

            return madeChanges;
        }

        public static bool RemoveAllAssociations()
        {
            bool madeChanges = false;
            var associations = new FileAssociation[] { CsvAssociation, TsvAssociation };

            foreach (var association in  associations)
            {
                madeChanges |= RemoveAssociation(association);
                madeChanges |= RemoveFileTypeExtension(association);
            }

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }

            return madeChanges;
        }

        public static bool HasAdminPrivileges()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static bool SetAssociation(FileAssociation association)
        {
            string extension = association.Extension;
            string progId = association.ProgId;
            string fileTypeDescription = association.FileTypeDescription;
            string applicationFilePath = association.ExecutableFilePath;
            int iconIndex = association.IconIndex;

            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{extension}", progId);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}", fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\DefaultIcon", $"\"{applicationFilePath}\",{iconIndex}");
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", $"\"{applicationFilePath}\" \"%1\"");

            if (association.ContentTypes != null && association.ContentTypes.Length > 0)
            {
                string contentType = association.ContentTypes[0];
                SetKeyValue($@"Software\Classes\{extension}", "Content Type", contentType);
            }

            if (!string.IsNullOrEmpty(association.PerceivedType))
            {
                string perceivedType = association.PerceivedType;
                SetKeyValue($@"Software\Classes\{extension}", "PerceivedType", perceivedType);
            }

            return madeChanges;
        }

        private static bool IsAssociated(string extension, string progId)
        {
            var value = GetKeyDefaultValue($@"Software\Classes\{extension}");
            if (string.IsNullOrEmpty(value)) return false;
            if (value == progId)
            {
                RegistryKey key = null;
                try
                {
                    key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{progId}", false);
                    return key != null;
                }
                finally
                {
                    if (key != null) key.Close();
                }
            }
            return false;
        }

        private static bool RemoveAssociation(FileAssociation association)
        {
            string progId = association.ProgId;
            string keyPath = $@"Software\Classes\{progId}";

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(keyPath, true);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static bool RemoveFileTypeExtension(FileAssociation association)
        {
            string extension = association.Extension;
            string keyPath = $@"Software\Classes\{extension}";

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(keyPath, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.CreateSubKey(keyPath);
                if (key != null && key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }
            finally
            {
                if (key != null) key.Close();
            }

            return false;
        }

        private static bool SetKeyValue(string keyPath, string keyName, string value)
        {
            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(keyPath, true);
                if (key != null && key.GetValue(keyName) as string != value)
                {
                    key.SetValue(keyName, value);
                    return true;
                }
            }
            finally
            {
                if (key != null) key.Close();
            }

            return false;
        }

        private static string GetKeyDefaultValue(string keyPath)
        {
            RegistryKey key = null;
            string value = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(keyPath, false);
                if (key != null)
                {
                    value = key.GetValue(null) as string;
                }
            }
            finally
            {
                if (key != null) key.Close();
            }
            return value;
        }
        #endregion
    }
}
