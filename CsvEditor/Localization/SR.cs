using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace CsvEditor
{
    internal static class SR
    {
        #region Variables
        private static CultureInfo culture = CultureInfo.CurrentCulture;
        private static Dictionary<string, string> cache = new Dictionary<string, string>();
        #endregion

        #region Methods
        internal static string GetString(string name)
        {
            return GetString(SR.Culture, name);
        }

        internal static string GetString(CultureInfo culture, string name)
        {
            string key = name + (culture != null ? culture.Name : "");

            if (cache.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                var res = Application.Current.FindResource(name) as string;
                if (!string.IsNullOrEmpty(res))
                {
                    cache.Add(key, res);
                }
                return res != null ? res : name;
            }
        }

        internal static string GetString(string name, params object[] args)
        {
            return GetString(SR.Culture, name, args);
        }

        internal static string GetString(CultureInfo culture, string name, params object[] args)
        {
            var res = GetString(culture, name);
            if (args != null && args.Length > 0)
            {
                return string.Format(culture ?? CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }
        #endregion

        #region Properties
        public static CultureInfo Culture
        {
            get => culture;
            set { culture = value; }
        }

        // Title
        public static string AppName { get => GetString(Keys.AppName); }
        public static string MainWindowTitle { get => GetString(Keys.MainWindowTitle); }
        public static string SettingsWindowTitle { get => GetString(Keys.SettingsWindowTitle); }
        public static string AboutWindowTitle { get => GetString(Keys.AboutWindowTitle); }

        // Label
        public static string OK { get => GetString(Keys.OK); }
        public static string Cancel { get => GetString(Keys.Cancel); }
        public static string General { get => GetString(Keys.General); }
        public static string DefaultDelimiter { get => GetString(Keys.DefaultDelimiter); }
        public static string DefaultEncoding { get => GetString(Keys.DefaultEncoding); }
        public static string WithBOM { get => GetString(Keys.WithBOM); }
        public static string UseDefaultEncoding { get => GetString(Keys.UseDefaultEncoding); }
        public static string EditorFont { get => GetString(Keys.EditorFont); }
        public static string EditorFontSize { get => GetString(Keys.EditorFontSize); }
        public static string FileAssociations { get => GetString(Keys.FileAssociations); }
        public static string AssociateCSVFile { get => GetString(Keys.AssociateCSVFile); }
        public static string AssociateTSVFile { get => GetString(Keys.AssociateTSVFile); }
        public static string RemoveAllAssociations { get => GetString(Keys.RemoveAllAssociations); }

        public static string OnStartUpOpen { get => GetString(Keys.OnStartUpOpen); }
        public static string StartUpBlank { get => GetString(Keys.StartUpBlank); }
        public static string OpenLastFile { get => GetString(Keys.OpenLastFile); }
        public static string CreateNewFile { get => GetString(Keys.CreateNewFile); }

        // Menu
        public static string FileMenuItem { get => GetString(Keys.FileMenuItem); }
        public static string NewMenuItem { get => GetString(Keys.NewMenuItem); }
        public static string OpenMenuItem { get => GetString(Keys.OpenMenuItem); }
        public static string RecentFilesMenuItem { get => GetString(Keys.RecentFilesMenuItem); }
        public static string ClearListMenuItem { get => GetString(Keys.ClearListMenuItem); }
        public static string SaveMenuItem { get => GetString(Keys.SaveMenuItem); }
        public static string SaveAsMenuItem { get => GetString(Keys.SaveAsMenuItem); }
        public static string ExportMenuItem { get => GetString(Keys.ExportMenuItem); }
        public static string PrintMenuItem { get => GetString(Keys.PrintMenuItem); }
        public static string PrintPreviewMenuItem { get => GetString(Keys.PrintPreviewMenuItem); }
        public static string ExitMenuItem { get => GetString(Keys.ExitMenuItem); }

        public static string EditMenuItem { get => GetString(Keys.EditMenuItem); }
        public static string UndoMenuItem { get => GetString(Keys.UndoMenuItem); }
        public static string RedoMenuItem { get => GetString(Keys.RedoMenuItem); }
        public static string CutMenuItem { get => GetString(Keys.CutMenuItem); }
        public static string CopyMenuItem { get => GetString(Keys.CopyMenuItem); }
        public static string PasteMenuItem { get => GetString(Keys.PasteMenuItem); }
        public static string DeleteMenuItem { get => GetString(Keys.DeleteMenuItem); }
        public static string FindMenuItem { get => GetString(Keys.FindMenuItem); }
        public static string ReplaceMenuItem { get => GetString(Keys.ReplaceMenuItem); }
        public static string GoToMenuItem { get => GetString(Keys.GoToMenuItem); }
        public static string SelectAllMenuItem { get => GetString(Keys.SelectAllMenuItem); }

        public static string ViewMenuItem { get => GetString(Keys.ViewMenuItem); }
        public static string HasHeaderMenuItem { get => GetString(Keys.HasHeaderMenuItem); }
        public static string ToolbarMenuItem { get => GetString(Keys.ToolbarMenuItem); }
        public static string StatusbarMenuItem { get => GetString(Keys.StatusbarMenuItem); }
        public static string SettingsMenuItem { get => GetString(Keys.SettingsMenuItem); }

        public static string HelpMenuItem { get => GetString(Keys.HelpMenuItem); }
        public static string AboutMenuItem { get => GetString(Keys.AboutMenuItem); }

        // Tooltip
        public static string NewFileTooltip { get => GetString(Keys.NewFileTooltip); }
        public static string OpenFileTooltip { get => GetString(Keys.OpenFileTooltip); }
        public static string SaveFileTooltip { get => GetString(Keys.SaveFileTooltip); }
        public static string PrintTooltip { get => GetString(Keys.PrintTooltip); }
        public static string CutTooltip { get => GetString(Keys.CutTooltip); }
        public static string CopyTooltip { get => GetString(Keys.CopyTooltip); }
        public static string PasteTooltip { get => GetString(Keys.PasteTooltip); }
        public static string HasHeaderTooltip { get => GetString(Keys.HasHeaderTooltip); }
        public static string AboutTooltip { get => GetString(Keys.AboutTooltip); }
        
        public static string CloseTooltip { get => GetString(Keys.CloseTooltip); }

        public static string DelimiterTooltip { get => GetString(Keys.DelimiterTooltip); }
        public static string EncodingTooltip { get => GetString(Keys.EncodingTooltip); }

        public static string BarCloseTooltip { get => GetString(Keys.BarCloseTooltip); }
        public static string UpButtonTooltip { get => GetString(Keys.UpButtonTooltip); }
        public static string DownButtonTooltip { get => GetString(Keys.DownButtonTooltip); }
        public static string SelectionButtonTooltip { get => GetString(Keys.SelectionButtonTooltip); }
        public static string ReplaceButtonTooltip { get => GetString(Keys.ReplaceButtonTooltip); }
        public static string ReplaceAllButtonTooltip { get => GetString(Keys.ReplaceAllButtonTooltip); }
        public static string CaseButtonTooltip { get => GetString(Keys.CaseButtonTooltip); }
        public static string WholeWordButtonTooltip { get => GetString(Keys.WholeWordButtonTooltip); }
        public static string RegexButtonTooltip { get => GetString(Keys.RegexButtonTooltip); }
        public static string PreserveCaseButtonTooltip { get => GetString(Keys.PreserveCaseButtonTooltip); }

        public static string XyButtonTooltip { get => GetString(Keys.XyButtonTooltip); }
        public static string AcceptButtonTooltip { get => GetString(Keys.AcceptButtonTooltip); }

        // Message
        public static string MessageLoading { get => GetString(Keys.MessageLoading); }
        public static string MessageReady { get => GetString(Keys.MessageReady); }
        public static string MessageSaving { get => GetString(Keys.MessageSaving); }
        public static string NoResults { get => GetString(Keys.NoResults); }
        public static string CurrentLocation { get => GetString(Keys.CurrentLocation); }

        public static string MessageNeedSave { get => GetString(Keys.MessageNeedSave); }
        public static string MessageSave { get => GetString(Keys.MessageSave); }

        // About
        public static string About { get => GetString(Keys.About); }
        public static string Credits { get => GetString(Keys.Credits); }
        public static string License { get => GetString(Keys.License); }
        public static string DevelopBy { get => GetString(Keys.DevelopBy); }

        public static string AboutText { get => GetString(Keys.AboutText); }
        public static string LicenseText { get => GetString(Keys.LicenseText); }
        #endregion

        #region Nested Types
        public static class Keys
        {
            // Title
            public const string AppName = "AppName";
            public const string MainWindowTitle = "MainWindowTitle";
            public const string SettingsWindowTitle = "SettingsWindowTitle";
            public const string AboutWindowTitle = "AboutWindowTitle";

            // Label
            public const string OK = "OK";
            public const string Cancel = "Cancel";
            public const string General = "General";
            public const string DefaultDelimiter = "DefaultDelimiter";
            public const string DefaultEncoding = "DefaultEncoding";
            public const string WithBOM = "WithBOM";
            public const string UseDefaultEncoding = "UseDefaultEncoding";
            public const string EditorFont = "EditorFont";
            public const string EditorFontSize = "EditorFontSize";
            public const string FileAssociations = "FileAssociations";
            public const string AssociateCSVFile = "AssociateCSVFile";
            public const string AssociateTSVFile = "AssociateTSVFile";
            public const string RemoveAllAssociations = "RemoveAllAssociations";

            public const string OnStartUpOpen = "OnStartUpOpen";
            public const string StartUpBlank = "StartUpBlank";
            public const string OpenLastFile = "OpenLastFile";
            public const string CreateNewFile = "CreateNewFile";

            // Menu
            public const string FileMenuItem = "FileMenuItem";
            public const string NewMenuItem = "NewMenuItem";
            public const string OpenMenuItem = "OpenMenuItem";
            public const string RecentFilesMenuItem = "RecentFilesMenuItem";
            public const string ClearListMenuItem = "ClearListMenuItem";
            public const string SaveMenuItem = "SaveMenuItem";
            public const string SaveAsMenuItem = "SaveAsMenuItem";
            public const string ExportMenuItem = "ExportMenuItem";
            public const string PrintMenuItem = "PrintMenuItem";
            public const string PrintPreviewMenuItem = "PrintPreviewMenuItem";
            public const string ExitMenuItem = "ExitMenuItem";

            public const string EditMenuItem = "EditMenuItem";
            public const string UndoMenuItem = "UndoMenuItem";
            public const string RedoMenuItem = "RedoMenuItem";
            public const string CutMenuItem = "CutMenuItem";
            public const string CopyMenuItem = "CopyMenuItem";
            public const string PasteMenuItem = "PasteMenuItem";
            public const string DeleteMenuItem = "DeleteMenuItem";
            public const string FindMenuItem = "FindMenuItem";
            public const string ReplaceMenuItem = "ReplaceMenuItem";
            public const string GoToMenuItem = "GoToMenuItem";
            public const string SelectAllMenuItem = "SelectAllMenuItem";

            public const string ViewMenuItem = "ViewMenuItem";
            public const string HasHeaderMenuItem = "HasHeaderMenuItem";
            public const string ToolbarMenuItem = "ToolbarMenuItem";
            public const string StatusbarMenuItem = "StatusbarMenuItem";
            public const string SettingsMenuItem = "SettingsMenuItem";

            public const string HelpMenuItem = "HelpMenuItem";
            public const string AboutMenuItem = "AboutMenuItem";

            // Tooltip
            public const string NewFileTooltip = "NewFileTooltip";
            public const string OpenFileTooltip = "OpenFileTooltip";
            public const string SaveFileTooltip = "SaveFileTooltip";
            public const string PrintTooltip = "PrintTooltip";
            public const string CutTooltip = "CutTooltip";
            public const string CopyTooltip = "CopyTooltip";
            public const string PasteTooltip = "PasteTooltip";
            public const string HasHeaderTooltip = "HasHeaderTooltip";
            public const string AboutTooltip = "AboutTooltip";
            public const string CloseTooltip = "CloseTooltip";

            public const string DelimiterTooltip = "DelimiterTooltip";
            public const string EncodingTooltip = "EncodingTooltip";

            public const string BarCloseTooltip = "BarCloseTooltip";
            public const string UpButtonTooltip = "UpButtonTooltip";
            public const string DownButtonTooltip = "DownButtonTooltip";
            public const string SelectionButtonTooltip = "SelectionButtonTooltip";
            public const string ReplaceButtonTooltip = "ReplaceButtonTooltip";
            public const string ReplaceAllButtonTooltip = "ReplaceAllButtonTooltip";
            public const string CaseButtonTooltip = "CaseButtonTooltip";
            public const string WholeWordButtonTooltip = "WholeWordButtonTooltip";
            public const string RegexButtonTooltip = "RegexButtonTooltip";
            public const string PreserveCaseButtonTooltip = "PreserveCaseButtonTooltip";

            public const string XyButtonTooltip = "XyButtonTooltip";
            public const string AcceptButtonTooltip = "AcceptButtonTooltip";

            // Message
            public const string MessageLoading = "MessageLoading";
            public const string MessageSaving = "MessageSaving";
            public const string MessageReady = "MessageReady";
            public const string NoResults = "NoResults";
            public const string CurrentLocation = "CurrentLocation";

            public const string MessageNeedSave = "MessageNeedSave";
            public const string MessageSave = "MessageSave";

            // About
            public const string About = "About";
            public const string Credits = "Credits";
            public const string License = "License";
            public const string DevelopBy = "DevelopBy";
            public const string LicenseText = "LicenseText";
            public const string AboutText = "AboutText";
        }
        #endregion
    }
}
