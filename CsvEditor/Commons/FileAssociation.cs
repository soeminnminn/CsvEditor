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
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
        public int IconIndex { get; set; } = 0;
    }

    public class FileAssociations
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static void EnsureAssociationsSet()
        {
            var filePath = Process.GetCurrentProcess().MainModule.FileName;
            EnsureAssociationsSet(
                new FileAssociation
                {
                    Extension = ".csv",
                    ProgId = "CSVEditor.csv",
                    FileTypeDescription = "CSV File",
                    ExecutableFilePath = filePath,
                    IconIndex = 1
                },
                new FileAssociation
                {
                    Extension = ".tsv",
                    ProgId = "CSVEditor.tsv",
                    FileTypeDescription = "TSV File",
                    ExecutableFilePath = filePath,
                    IconIndex = 2
                });
        }

        public static void EnsureAssociationsSet(params FileAssociation[] associations)
        {
            bool madeChanges = false;
            foreach (var association in associations)
            {
                madeChanges |= SetAssociation(
                    association.Extension,
                    association.ProgId,
                    association.FileTypeDescription,
                    association.ExecutableFilePath,
                    association.IconIndex);
            }

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static bool HasAdminPrivileges()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath, int iconIndex = 0)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{extension}", progId);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}", fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\DefaultIcon", $"\"{applicationFilePath}\",{iconIndex}");
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", $"\"{applicationFilePath}\" \"%1\"");
            return madeChanges;
        }

        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }

            return false;
        }
    }
}
