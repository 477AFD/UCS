using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FinalSystemUI.Database;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;

namespace FinalSystemUI.Dialogs
{
    /// <summary>
    /// OSSE > SQL Queries. Shows ready-made SQL queries (SELECT, JOIN, GROUP BY, subqueries, views, stored procedure)
    /// and runs them on the real database. The SQL text can be read and copied, but not changed:
    /// only the queries listed in Database/SavedQueries.cs can be run, so nothing can be damaged from here.
    /// </summary>
    public class QueryViewerForm : Form
    {
        private ListBox lst;
        private Label lblDesc;
        private TextBox txtSql;
        private Label lblResult;
        private DataGridView dgv;

        /// <summary>Opens the window. Only OSSE is allowed.</summary>
        public static void Open(IWin32Window owner)
        {
            Session.Require(UserRole.Osse);
            using (QueryViewerForm f = new QueryViewerForm())
            {
                f.ShowDialog(owner);
            }
        }

        public QueryViewerForm()
        {
            Text = "SQL Queries";
            Font = new Font("Segoe UI", 9.5f);
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            // ---- header
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 64;
            header.BackColor = Ui.Green;

            Label title = Ui.MakeLabel("SQL Queries", new Point(16, 8), 13f, true, Color.White);
            Label info = Ui.MakeLabel(ConnectionText(), new Point(18, 38), 8.5f, false, Color.FromArgb(232, 212, 138));
            header.Controls.Add(title);
            header.Controls.Add(info);

            // ---- left: list of queries
            lst = new ListBox();
            lst.Dock = DockStyle.Left;
            lst.Width = 380;
            lst.IntegralHeight = false;
            lst.BorderStyle = BorderStyle.None;
            lst.BackColor = Color.FromArgb(245, 247, 245);
            string lastGroup = null;
            foreach (SavedQuery q in SavedQueries.All)
            {
                if (q.Group != lastGroup)
                {
                    lastGroup = q.Group;
                }
                lst.Items.Add("Q" + q.Number.ToString("00") + "   " + q.Title);
            }
            lst.SelectedIndexChanged += delegate { ShowSelected(); };
            lst.DoubleClick += delegate { RunSelected(); };

            // ---- right: description, SQL text, buttons, result grid
            Panel right = new Panel();
            right.Dock = DockStyle.Fill;
            right.Padding = new Padding(14, 10, 14, 12);

            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            Ui.StyleGrid(dgv);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            Panel bar = new Panel();
            bar.Dock = DockStyle.Top;
            bar.Height = 46;
            Button btnRun = Ui.MakeButton("Run query", new Point(0, 7), new Size(120, 32), true);
            Button btnCopy = Ui.MakeButton("Copy SQL", new Point(128, 7), new Size(110, 32), false);
            lblResult = Ui.MakeLabel("", new Point(256, 14), 9.5f, true, Ui.DarkGreen);
            btnRun.Click += delegate { RunSelected(); };
            btnCopy.Click += delegate
            {
                if (txtSql.Text.Length > 0)
                {
                    Clipboard.SetText(txtSql.Text);
                    lblResult.ForeColor = Ui.Gray;
                    lblResult.Text = "The SQL was copied.";
                }
            };
            bar.Controls.Add(btnRun);
            bar.Controls.Add(btnCopy);
            bar.Controls.Add(lblResult);

            txtSql = new TextBox();
            txtSql.Dock = DockStyle.Top;
            txtSql.Height = 210;
            txtSql.Multiline = true;
            txtSql.ReadOnly = true;
            txtSql.ScrollBars = ScrollBars.Both;
            txtSql.WordWrap = false;
            txtSql.Font = new Font("Consolas", 10.5f);
            txtSql.BackColor = Color.White;

            lblDesc = new Label();
            lblDesc.Dock = DockStyle.Top;
            lblDesc.Height = 52;
            lblDesc.AutoSize = false;
            lblDesc.Font = new Font("Segoe UI", 9.5f);

            // Fill first, then the top items from the bottom up (the last one added is the highest).
            right.Controls.Add(dgv);
            right.Controls.Add(bar);
            right.Controls.Add(txtSql);
            right.Controls.Add(lblDesc);

            Controls.Add(right);
            Controls.Add(lst);
            Controls.Add(header);

            Shown += delegate
            {
                lst.SelectedIndex = 0;
                RunSelected();
            };
        }

        private static string ConnectionText()
        {
            try
            {
                SqlConnectionStringBuilder b = new SqlConnectionStringBuilder(Db.ConnectionString);
                return "Server: " + b.DataSource + "     Database file: " + DataPath.MdfPath;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private SavedQuery Selected()
        {
            int i = lst.SelectedIndex;
            return (i >= 0 && i < SavedQueries.All.Count) ? SavedQueries.All[i] : null;
        }

        private void ShowSelected()
        {
            SavedQuery q = Selected();
            if (q == null) return;
            lblDesc.Text = q.Group + "\r\n" + q.Description;
            txtSql.Text = q.Sql.Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private void RunSelected()
        {
            SavedQuery q = Selected();
            if (q == null) return;

            try
            {
                DataTable table = new DataTable();
                Stopwatch watch = Stopwatch.StartNew();
                using (Ui.Busy(this))
                {
                    using (SqlConnection c = Db.Open())
                    using (SqlDataAdapter adapter = new SqlDataAdapter(q.Sql, c))
                    {
                        adapter.Fill(table);
                    }
                }
                watch.Stop();

                dgv.DataSource = table;
                dgv.ClearSelection();
                lblResult.ForeColor = Ui.DarkGreen;
                lblResult.Text = table.Rows.Count + " row(s)  |  " + watch.ElapsedMilliseconds + " ms";
            }
            catch (Exception ex)
            {
                dgv.DataSource = null;
                lblResult.ForeColor = Ui.Red;
                lblResult.Text = "The query failed.";
                Ui.ShowError(ex);
            }
        }
    }
}
