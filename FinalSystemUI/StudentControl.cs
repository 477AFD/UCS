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

namespace FinalSystemUI
{
    /// <summary>The student's window: a sidebar plus a content area that shows one page at a time.</summary>
    public partial class StudentControl : UserControl
    {
        public StudentControl()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.SetUserLabels(label11, label12);
            Open("dashboard");
        }

        /// <summary>page: "dashboard", "violations", "fines" or "receipt".</summary>
        public void Open(string page)
        {
            UserControl next;
            switch (page)
            {
                case "violations": next = new ViolationHistory(); break;
                case "fines": next = new MyFines(); break;
                case "receipt": next = new Receipt(); break;
                default: next = new DashboardControl(); break;
            }
            LoadControl(next);
        }

        private void LoadControl(UserControl control)
        {
            foreach (Control old in pnlContent.Controls.Cast<Control>().ToList())
            {
                pnlContent.Controls.Remove(old);
                old.Dispose();
            }
            control.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(control);
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            Open("dashboard");
        }

        private void btnViolation_Click(object sender, EventArgs e)
        {
            Open("violations");
        }

        private void btnFines_Click(object sender, EventArgs e)
        {
            Open("fines");
        }

        private void btnReceipt_Click(object sender, EventArgs e)
        {
            Open("receipt");
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            MainForm main = this.FindForm() as MainForm;
            main?.ShowLogin();
        }
    }
}
