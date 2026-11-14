using System;
using System.Configuration;
using System.IO;

namespace FinalSystemUI.Database
{
    /// <summary>
    /// Where the service-based database file (UniformComplianceDB.mdf) is kept.
    ///
    /// The connection string in App.config says  AttachDbFilename=|DataDirectory|\UniformComplianceDB.mdf
    /// "|DataDirectory|" is replaced by the folder chosen here:
    ///   1. the folder in App.config  (appSettings key "DataFolder"), if there is one
    ///   2. the App_Data folder of the project, when you run the program from Visual Studio
    ///   3. otherwise  %LocalAppData%\UniformCompliance\Data
    /// </summary>
    public static class DataPath
    {
        public const string FileName = "UniformComplianceDB.mdf";
        public const string LogFileName = "UniformComplianceDB_log.ldf";

        private static string _folder;
        private static readonly object Gate = new();

        public static string Folder
        {
            get
            {
                Init();
                return _folder;
            }
        }

        public static string MdfPath
        {
            get { return Path.Combine(Folder, FileName); }
        }

        public static string LdfPath
        {
            get { return Path.Combine(Folder, LogFileName); }
        }

        /// <summary>Chooses the folder and tells SQL Server where |DataDirectory| is. Safe to call many times.</summary>
        public static void Init()
        {
            lock (Gate)
            {
                if (_folder != null) return;

                string folder = null;

                string custom = ConfigurationManager.AppSettings["DataFolder"];
                if (!string.IsNullOrWhiteSpace(custom))
                {
                    folder = Environment.ExpandEnvironmentVariables(custom.Trim());
                }
                else
                {
                    // Started from Visual Studio: bin\Debug\ -> go up until the folder that has the .csproj
                    DirectoryInfo d = new(AppDomain.CurrentDomain.BaseDirectory);
                    for (int i = 0; i < 5 && d != null; i++)
                    {
                        if (d.GetFiles("*.csproj").Length > 0)
                        {
                            folder = Path.Combine(d.FullName, "App_Data");
                            break;
                        }
                        d = d.Parent;
                    }
                    folder ??= Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "UniformCompliance", "Data");
                }

                Directory.CreateDirectory(folder);
                AppDomain.CurrentDomain.SetData("DataDirectory", folder);
                _folder = folder;
            }
        }
    }
}
