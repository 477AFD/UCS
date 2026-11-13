using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Services;

namespace FinalSystemUI
{
    /// <summary>
    /// Student > Receipt: pick one of your PAID fines and see its receipt. The receipt can be
    /// previewed and printed. A student only ever sees their own paid fines (RecordService checks this).
    /// </summary>
    public partial class Receipt : UserControl
    {
        private ComboBox cboPaid;
        private Button btnPrint;
        private List<ViolationRow> _paid = new List<ViolationRow>();
        private ViolationRow _current;

        public Receipt()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            label4.Text = "Receipts for the fines you have paid.";

            // the receipt "paper" sits in the middle of the content area (the sidebar is 250 px wide)
            groupBox2.Left = 250 + (Width - 250 - groupBox2.Width) / 2;
            groupBox2.Top = 135;

            // receipt history (dropdown) + print button, lined up with the edges of the receipt card
            int printWidth = 120;
            cboPaid = new ComboBox();
            cboPaid.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPaid.Font = new Font("Segoe UI", 9.5f);
            cboPaid.Location = new Point(groupBox2.Left, 95);
            cboPaid.Width = groupBox2.Width - printWidth - 8;
            cboPaid.DropDownWidth = groupBox2.Width;
            cboPaid.SelectedIndexChanged += delegate { ShowSelected(); };
            Controls.Add(cboPaid);

            btnPrint = Ui.MakeButton("Print", new Point(groupBox2.Right - printWidth, 95), new Size(printWidth, cboPaid.Height), true);
            btnPrint.Click += delegate { PrintReceipt(); };
            Controls.Add(btnPrint);

            cboPaid.BringToFront();
            btnPrint.BringToFront();

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                lblSem.Text = RecordService.SemesterName();

                ViolationFilter f = new ViolationFilter();
                f.OnlyFined = true;
                f.FineState = 2;        // paid only
                _paid = RecordService.MyViolations(f)
                    .Where(v => v.IsPaid)
                    .OrderByDescending(v => v.PaidAt ?? v.RecordedAt)
                    .ToList();

                cboPaid.Items.Clear();
                foreach (ViolationRow v in _paid)
                    cboPaid.Items.Add(ReceiptNo(v) + " - " + v.TypeName);

                bool any = _paid.Count > 0;
                cboPaid.Enabled = any;
                btnPrint.Enabled = any;

                if (any) cboPaid.SelectedIndex = 0;
                else ShowEmpty();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        /// <summary>Receipt number made from the payment date and the record number, e.g. OR-20261112-00007.</summary>
        private static string ReceiptNo(ViolationRow v)
        {
            DateTime d = v.PaidAt ?? v.RecordedAt;
            return "OR-" + d.ToString("yyyyMMdd") + "-" + v.ViolationId.ToString("D5");
        }

        /// <summary>The rows printed on the receipt (caption, value).</summary>
        private static List<string[]> BuildLines(ViolationRow v)
        {
            AppUser me = Session.Current;
            DateTime paidAt = v.PaidAt ?? v.RecordedAt;
            List<string[]> rows = new List<string[]>();
            rows.Add(new string[] { "Receipt no.", ReceiptNo(v) });
            rows.Add(new string[] { "Date paid", Ui.DateText(paidAt) });
            rows.Add(new string[] { "Student", me.FullName ?? "" });
            rows.Add(new string[] { "ID number", me.StudentNumber ?? "" });
            rows.Add(new string[] { "Violation", v.TypeName });
            rows.Add(new string[] { "Date recorded", Ui.DateText(v.RecordedAt) });
            rows.Add(new string[] { "Semester", RecordService.SemesterName() });
            return rows;
        }

        private void ClearDynamic()
        {
            foreach (Control c in groupBox2.Controls.Cast<Control>().ToList())
            {
                if (c.Name.StartsWith("dyn"))
                {
                    groupBox2.Controls.Remove(c);
                    c.Dispose();
                }
            }
        }

        private void ShowEmpty()
        {
            _current = null;
            ClearDynamic();

            Label l = Ui.MakeLabel("No paid fines yet.\n\nA receipt will appear here after one of your fines is marked as paid.",
                new Point(30, 160), 10f, false, Ui.Gray);
            l.Name = "dynEmpty";
            l.AutoSize = false;
            l.Size = new Size(groupBox2.Width - 60, 120);
            l.TextAlign = ContentAlignment.TopCenter;
            groupBox2.Controls.Add(l);
        }

        private void ShowSelected()
        {
            if (cboPaid.SelectedIndex < 0 || cboPaid.SelectedIndex >= _paid.Count) return;
            try
            {
                _current = _paid[cboPaid.SelectedIndex];
                ClearDynamic();

                int left = 24;
                int valueLeft = 150;
                int y = 80;
                foreach (string[] row in BuildLines(_current))
                {
                    Label cap = Ui.MakeLabel(row[0], new Point(left, y), 9f, false, Ui.Gray);
                    cap.Name = "dynCap" + y;
                    groupBox2.Controls.Add(cap);

                    Label val = Ui.MakeLabel(row[1], new Point(valueLeft, y - 2), 9.5f, true, Color.Black);
                    val.Name = "dynVal" + y;
                    val.AutoSize = false;
                    val.AutoEllipsis = true;
                    val.Size = new Size(groupBox2.Width - valueLeft - 20, 24);
                    groupBox2.Controls.Add(val);
                    y += 32;
                }

                Panel line = new Panel();
                line.Name = "dynLine";
                line.BackColor = Color.Silver;
                line.Location = new Point(left, y + 6);
                line.Size = new Size(groupBox2.Width - 2 * left, 1);
                groupBox2.Controls.Add(line);

                Label capAmount = Ui.MakeLabel("Amount paid", new Point(left, y + 22), 9f, false, Ui.Gray);
                capAmount.Name = "dynCapAmount";
                groupBox2.Controls.Add(capAmount);

                Label amount = Ui.MakeLabel(Ui.Money(_current.FineAmount ?? 0m), new Point(left, y + 44), 22f, true, Ui.DarkGreen);
                amount.Name = "dynAmount";
                groupBox2.Controls.Add(amount);

                Label paidMark = Ui.MakeLabel("PAID", new Point(groupBox2.Width - 110, y + 48), 16f, true, Ui.DarkGreen);
                paidMark.Name = "dynPaid";
                groupBox2.Controls.Add(paidMark);

                Label note = Ui.MakeLabel("This receipt was generated by the Student Uniform Compliance Monitoring System.",
                    new Point(left, 0), 8.5f, false, Ui.Gray);
                note.Name = "dynNote";
                note.AutoSize = false;
                note.Size = new Size(groupBox2.Width - 2 * left, 36);
                note.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                note.Top = groupBox2.ClientSize.Height - note.Height - 14;
                groupBox2.Controls.Add(note);
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        // ------------------------------------------------------------ printing

        private void PrintReceipt()
        {
            if (_current == null) return;
            try
            {
                using (PrintDocument doc = new PrintDocument())
                using (PrintPreviewDialog dlg = new PrintPreviewDialog())
                {
                    doc.DocumentName = ReceiptNo(_current);
                    doc.PrintPage += DrawPage;
                    dlg.Document = doc;
                    dlg.Width = 900;
                    dlg.Height = 700;
                    dlg.ShowDialog(this.FindForm());
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void DrawPage(object sender, PrintPageEventArgs e)
        {
            ViolationRow v = _current;
            Graphics g = e.Graphics;
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float width = Math.Min(e.MarginBounds.Width, 460f);

            using (Font title = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (Font sub = new Font("Segoe UI", 9f))
            using (Font cap = new Font("Segoe UI", 9.5f))
            using (Font val = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (Font big = new Font("Segoe UI", 20f, FontStyle.Bold))
            using (Pen pen = new Pen(Color.Black))
            {
                g.DrawString("Mindoro State University", title, Brushes.Black, x, y);
                y += 32;
                g.DrawString("Student Uniform Compliance Monitoring System", sub, Brushes.DimGray, x, y);
                y += 28;
                g.DrawString("PAYMENT RECEIPT", val, Brushes.Black, x, y);
                y += 28;
                g.DrawLine(pen, x, y, x + width, y);
                y += 14;

                foreach (string[] row in BuildLines(v))
                {
                    g.DrawString(row[0], cap, Brushes.DimGray, x, y);
                    g.DrawString(row[1], val, Brushes.Black, x + 140, y - 1);
                    y += 26;
                }

                y += 6;
                g.DrawLine(pen, x, y, x + width, y);
                y += 14;
                g.DrawString("Amount paid", cap, Brushes.DimGray, x, y);
                y += 22;
                g.DrawString(Ui.Money(v.FineAmount ?? 0m), big, Brushes.Black, x, y);
                g.DrawString("PAID", title, Brushes.Black, x + width - 70, y + 6);
                y += 56;
                g.DrawString("This receipt was generated by the system.", sub, Brushes.DimGray, x, y);
            }
            e.HasMorePages = false;
        }
    }
}
