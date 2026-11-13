using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;

namespace FinalSystemUI
{
    /// <summary>The one and only main window. Every screen is a UserControl shown inside it.</summary>
    public partial class MainForm : Form
    {
        private LoginControl _login;

        public MainForm()
        {
            InitializeComponent();

            this.Text = "Student Uniform Compliance Monitoring System";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            ShowLogin();
        }

        public void ShowLogin()
        {
            Session.End();
            _login = new LoginControl();
            _login.LoginSucceeded += role => GoToDashboard(role);
            ShowScreen(_login);
        }

        public void ShowScreen(UserControl screen)
        {
            // remove (and free) the screen that is showing now
            foreach (Control old in this.Controls.Cast<Control>().ToList())
            {
                this.Controls.Remove(old);
                if (old != _login) old.Dispose();
            }
            if (_login != null && screen != _login)
            {
                _login.Dispose();
                _login = null;
            }

            screen.Location = new Point(0, 0);
            this.Controls.Add(screen);
            this.ClientSize = screen.Size;
        }

        private void GoToDashboard(string role)
        {
            AppUser me = Session.Current;
            if (me == null) return;

            // The screen is chosen from the logged-in account, not from what the user typed.
            switch (me.Role)
            {
                case UserRole.Student:
                    ShowScreen(new StudentControl());
                    break;
                case UserRole.Hsg:
                    ShowScreen(new HsgControl());
                    break;
                case UserRole.Osse:
                    ShowScreen(new OsseRecords());
                    break;
            }
        }
    }
}
