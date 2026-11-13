using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Repositories;

namespace FinalSystemUI.Dialogs
{
    /// <summary>
    /// OSSE > Audit Log. Shows who did what and when (add/edit/delete records, fines, clearance, user changes).
    /// The entries are written by the services; this window only reads them.
    /// </summary>
    public class AuditLogForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch;
        private Label lblCount;
        private List<AuditEntry> _all = new List<AuditEntry>();

        /// <summary>Opens the window. Only OSSE is allowed.</summary>
        public static void Open(IWin32Window owner)
        {
            Session.Require(UserRole.Osse);
            using (AuditLogForm f = new AuditLogForm())
            {
                f.ShowDialog(owner);
            }
        }

        public AuditLogForm()
        {
            Text = "Audit Log";
            Font = new Font("Segoe UI", 9.5f);
            ClientSize = new Size(1000, 600);
            MinimumSize = new Size(760, 420);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 64;
            header.BackColor = Ui.Green;
            header.Controls.Add(Ui.MakeLabel("Audit Log", new Point(16, 8), 13f, true, Color.White));
            header.Controls.Add(Ui.MakeLabel("The newest 500 actions of HSG and OSSE accounts.", new Point(18, 38), 8.5f, false, Color.FromArgb(232, 212, 138)));

            Panel bar = new Panel();
            bar.Dock = DockStyle.Top;
            bar.Height = 46;
            bar.Padding = new Padding(14, 0, 14, 0);
            Label lblSearch = Ui.MakeLabel("Search", new Point(14, 14), 9.5f, false, Color.Black);
            txtSearch = new TextBox();
            txtSearch.Location = new Point(70, 10);
            txtSearch.Width = 260;
            Button btnRefresh = Ui.MakeButton("Refresh", new Point(342, 8), new Size(100, 30), true);
            lblCount = Ui.MakeLabel("", new Point(458, 14), 9.5f, true, Ui.DarkGreen);
            txtSearch.TextChanged += delegate { ShowRows(); };
            btnRefresh.Click += delegate { LoadData(); };
            bar.Controls.Add(lblSearch);
            bar.Controls.Add(txtSearch);
            bar.Controls.Add(btnRefresh);
            bar.Controls.Add(lblCount);

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            Ui.StyleGrid(dgv);
            Ui.SetColumns(dgv, "Date and time", "Role", "Name", "Action", "Details");
            Ui.SetColumnWeights(dgv, 120, 55, 120, 110, 220);

            // Fill first, then the top items from the bottom up (the last one added is the highest).
            Controls.Add(dgv);
            Controls.Add(bar);
            Controls.Add(header);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _all = AuditRepository.Recent(500);
                ShowRows();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void ShowRows()
        {
            string key = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            dgv.Rows.Clear();
            int shown = 0;
            foreach (AuditEntry a in _all)
            {
                string text = ((a.Role ?? "") + " " + (a.ActorName ?? "") + " " + (a.Action ?? "") + " " + (a.Details ?? "")).ToLowerInvariant();
                if (key.Length > 0 && !text.Contains(key)) continue;
                dgv.Rows.Add(Ui.DateText(a.At), a.Role, a.ActorName, a.Action, a.Details);
                shown++;
            }
            dgv.ClearSelection();
            lblCount.Text = shown + " entries";
        }
    }
}
