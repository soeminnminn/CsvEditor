using System;

namespace CsvEditor.Plugin
{
    internal class Manager : AssmLoader<IExportPlugin>
    {
        #region Variables
        internal const string c_prefix = "Plugin.";
        #endregion

        #region Constructor
        public Manager()
            : base(AppDomain.CurrentDomain.BaseDirectory)
        {
            string systemPath = AppDomain.CurrentDomain.BaseDirectory;
            Load(systemPath, $"{c_prefix}*.dll");
        }
        #endregion

        #region Static
        private static Manager instance = null;

        public static Manager Plugins
        {
            get
            {
                if (instance == null)
                    instance = new Manager();
                return instance;
            }
        }

        public static void SendOnPluginLoaded(AssmInfo<IExportPlugin> info)
        {
            if (info != null && info.Module != null)
            {
                info.Module.OnPluginLoaded();
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        #endregion
    }
}
