using System;
using System.Windows.Forms;
using FinalSystemUI.Database;
using FinalSystemUI.Dialogs;

namespace FinalSystemUI
{
    internal static class Program
    {
        /// <summary>The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitResult init;
            SplashForm splash = new SplashForm();
            splash.Show();
            splash.Refresh();
            try
            {
                init = DatabaseInitializer.EnsureReady();
            }
            catch (Exception ex)
            {
                splash.Close();
                MessageBox.Show(
                    "The program could not open the database.\n\n" + ex.Message +
                    "\n\nWhat to check:\n" +
                    "1. SQL Server (or LocalDB) is installed and running.\n" +
                    "2. The connection string named 'UniformDb' in App.config is correct.",
                    "Database problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            splash.Close();

            if (init.SeededSampleData)
            {
                MessageBox.Show(
                    "This is the first start, so the database was created with sample data.\n\n" +
                    "Database file (service-based):\n" + init.DatabaseFile +
                    "\n\nSample accounts (log in with email and password):\n" +
                    "Student:  juan@minsu.edu.ph  /  " + DatabaseInitializer.SampleStudentPassword + "\n" +
                    "HSG:  hsg@minsu.edu.ph  /  " + DatabaseInitializer.SampleHsgPassword + "\n" +
                    "OSSE:  osse@minsu.edu.ph  /  " + DatabaseInitializer.SampleOssePassword,
                    "Sample accounts ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Application.Run(new MainForm());
        }
    }
}
