using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace FinalSystemUI.Helpers
{
    /// <summary>Small helpers that every screen uses (grids, cards, messages).</summary>
    public static class Ui
    {
        public static readonly Color Green = Color.Green;
        public static readonly Color DarkGreen = Color.FromArgb(11, 93, 51);
        public static readonly Color Red = Color.FromArgb(180, 35, 24);
        public static readonly Color Amber = Color.FromArgb(184, 134, 11);
        public static readonly Color Gray = Color.FromArgb(100, 110, 105);

        // ------------------------------------------------------------ text

        public static string Money(decimal amount)
        {
            return "\u20B1" + amount.ToString("N2", CultureInfo.InvariantCulture);
        }

        public static string DateText(DateTime d)
        {
            return d.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture);
        }

        // ------------------------------------------------------------ messages

        public static void Info(string text)
        {
            MessageBox.Show(text, "Uniform Compliance", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warn(string text)
        {
            MessageBox.Show(text, "Uniform Compliance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>Shows a friendly message for any error.</summary>
        public static void ShowError(Exception ex)
        {
            if (ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is ArgumentException)
            {
                Warn(ex.Message);
            }
            else if (ex is SqlException)
            {
                MessageBox.Show("The program could not use the database.\n\n" + ex.Message +
                    "\n\nCheck that SQL Server is running and that the connection string in App.config is correct.",
                    "Database problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Something went wrong.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------ busy cursor

        private class BusyScope : IDisposable
        {
            private readonly Control _c;
            public BusyScope(Control c)
            {
                _c = c;
                if (_c != null) _c.UseWaitCursor = true;
            }
            public void Dispose()
            {
                if (_c != null) _c.UseWaitCursor = false;
            }
        }

        /// <summary>using (Ui.Busy(this)) { ...slow work... }</summary>
        public static IDisposable Busy(Control c)
        {
            return new BusyScope(c);
        }

        // ------------------------------------------------------------ grids

        public static void StyleGrid(DataGridView g)
        {
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 32;
            g.ColumnHeadersDefaultCellStyle.BackColor = Green;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Green;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 240, 225);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 248);
            g.RowTemplate.Height = 28;
        }

        public static void SetColumns(DataGridView g, params string[] headers)
        {
            g.Columns.Clear();
            foreach (string h in headers)
            {
                int i = g.Columns.Add("col" + h.Replace(" ", ""), h);
                g.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        public static void SetColumnWeights(DataGridView g, params float[] weights)
        {
            for (int i = 0; i < weights.Length && i < g.Columns.Count; i++)
                g.Columns[i].FillWeight = weights[i];
        }

        public static T SelectedTag<T>(DataGridView g) where T : class
        {
            if (g.SelectedRows.Count == 0) return null;
            return g.SelectedRows[0].Tag as T;
        }

        /// <summary>Colors the text of one cell, for example the Status column.</summary>
        public static void ColorStatus(DataGridViewCell cell, string status)
        {
            switch (status)
            {
                case "Paid":
                case "Cleared":
                case "Active":
                    cell.Style.ForeColor = Color.FromArgb(11, 120, 50);
                    break;
                case "Unpaid":
                case "Disabled":
                    cell.Style.ForeColor = Red;
                    break;
                case "For clearance":
                    cell.Style.ForeColor = Amber;
                    break;
            }
            cell.Style.Font = new Font(cell.DataGridView.Font, FontStyle.Bold);
        }

        // ------------------------------------------------------------ cards (the gray group boxes)

        /// <summary>Puts (or updates) a big number inside a stat card.</summary>
        public static void CardValue(GroupBox box, string text, Color color, float size, int y)
        {
            Label l = FindLabel(box, "lblValue");
            if (l == null)
            {
                l = new Label();
                l.Name = "lblValue";
                l.AutoSize = true;
                l.BackColor = Color.Transparent;
                box.Controls.Add(l);
            }
            l.Font = new Font("Segoe UI", size, FontStyle.Bold);
            l.ForeColor = color;
            l.Location = new Point(24, y);
            l.Text = text;
        }

        /// <summary>Puts (or updates) a small gray line inside a stat card.</summary>
        public static void CardNote(GroupBox box, string text, int y)
        {
            Label l = FindLabel(box, "lblNote");
            if (l == null)
            {
                l = new Label();
                l.Name = "lblNote";
                l.AutoSize = true;
                l.BackColor = Color.Transparent;
                l.ForeColor = Gray;
                l.Font = new Font("Segoe UI", 8.5f);
                box.Controls.Add(l);
            }
            l.Location = new Point(26, y);
            l.Text = text;
        }

        private static Label FindLabel(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c as Label;
            }
            return null;
        }

        // ------------------------------------------------------------ filter buttons (All, No uniform, ...)

        public static void SetActiveFilter(Button active, params Button[] all)
        {
            foreach (Button b in all)
            {
                bool on = (b == active);
                b.FlatStyle = FlatStyle.Flat;
                b.UseVisualStyleBackColor = false;
                b.BackColor = on ? Green : Color.White;
                b.ForeColor = on ? Color.White : Color.Black;
                b.FlatAppearance.BorderColor = on ? Green : Color.Silver;
            }
        }

        /// <summary>Makes a normal action button look like the green primary button.</summary>
        public static void StylePrimary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.BackColor = Green;
            b.ForeColor = Color.White;
            b.FlatAppearance.BorderColor = Green;
        }

        public static Button MakeButton(string text, Point location, Size size, bool primary)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = location;
            b.Size = size;
            b.Cursor = Cursors.Hand;
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.BackColor = primary ? Green : Color.White;
            b.ForeColor = primary ? Color.White : Color.Black;
            b.FlatAppearance.BorderColor = primary ? Green : Color.Silver;
            return b;
        }

        public static Label MakeLabel(string text, Point location, float size, bool bold, Color color)
        {
            Label l = new Label();
            l.AutoSize = true;
            l.Text = text;
            l.Location = location;
            l.Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
            l.ForeColor = color;
            l.BackColor = Color.Transparent;
            return l;
        }

        // ------------------------------------------------------------ misc

        /// <summary>True when Visual Studio is showing the screen in the Designer (do not load data then).</summary>
        public static bool InDesigner
        {
            get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
        }

        /// <summary>Adds one row to a grid, remembers the record in Row.Tag, and colors the Status cell.</summary>
        public static int AddRow(DataGridView g, object tag, int statusColumn, params object[] cells)
        {
            int i = g.Rows.Add(cells);
            g.Rows[i].Tag = tag;
            if (statusColumn >= 0 && statusColumn < cells.Length)
                ColorStatus(g.Rows[i].Cells[statusColumn], Convert.ToString(cells[statusColumn]));
            return i;
        }

        /// <summary>
        /// Shows the logged-in user's name and role in the sidebar, centered above the Log out button,
        /// with a clear gap so they do not touch.
        /// </summary>
        public static void SetUserLabels(Label nameLabel, Label roleLabel)
        {
            if (Helpers.Session.Current == null) return;

            Control sidebar = nameLabel.Parent;
            int width = sidebar != null ? sidebar.ClientSize.Width : 250;

            Button logout = null;
            if (sidebar != null)
            {
                foreach (Control c in sidebar.Controls)
                {
                    Button b = c as Button;
                    if (b != null && b.Text.Replace(" ", "").ToLowerInvariant() == "logout") logout = b;
                }
            }
            if (logout != null) logout.Left = (width - logout.Width) / 2;

            int bottom = logout != null ? logout.Top : nameLabel.Top + 62;

            roleLabel.AutoSize = false;
            roleLabel.TextAlign = ContentAlignment.MiddleCenter;
            roleLabel.SetBounds(10, bottom - roleLabel.Height - 12, width - 20, roleLabel.Height);
            roleLabel.Text = Helpers.Session.Current.RoleText;

            nameLabel.AutoSize = false;
            nameLabel.AutoEllipsis = true;
            nameLabel.TextAlign = ContentAlignment.MiddleCenter;
            nameLabel.SetBounds(10, roleLabel.Top - nameLabel.Height + 4, width - 20, nameLabel.Height);
            nameLabel.Text = Helpers.Session.Current.FullName;
        }

        public static string FirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            return fullName.Trim().Split(' ')[0];
        }
    }
}
