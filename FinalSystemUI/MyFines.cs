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
using FinalSystemUI.Services;

namespace FinalSystemUI
{
    /// <summary>Student > My fines: totals, what is still unpaid, and every fine.</summary>
    public partial class MyFines : UserControl
    {
        private int _fineState;     // 0 = all, 1 = unpaid, 2 = paid

        public MyFines()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.StyleGrid(dataGridView1);
            Ui.SetColumns(dataGridView1, "Date", "Violation", "Amount", "Status", "Date paid");
            Ui.SetColumnWeights(dataGridView1, 130, 120, 70, 70, 110);

            btnAllFines.Click += delegate { _fineState = 0; LoadData(); };
            btnUnpaid.Click += delegate { _fineState = 1; LoadData(); };
            btnPaid.Click += delegate { _fineState = 2; LoadData(); };

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Ui.SetActiveFilter(_fineState == 0 ? btnAllFines : (_fineState == 1 ? btnUnpaid : btnPaid),
                    btnAllFines, btnUnpaid, btnPaid);
                lblSem.Text = RecordService.SemesterName();

                StudentSummary s = RecordService.MySummary();
                Ui.CardValue(gboxUnpaid, Ui.Money(s.UnpaidFines), s.UnpaidFines > 0 ? Ui.Red : Ui.DarkGreen, 18, 50);
                Ui.CardValue(gBoxTotal, Ui.Money(s.TotalFines), Color.Black, 18, 50);
                Ui.CardValue(gBoxPaid, Ui.Money(s.PaidFines), Ui.DarkGreen, 18, 50);
                Ui.CardValue(gBoxfined, s.FinedCount.ToString(), Color.Black, 18, 50);
                Ui.CardNote(gBoxfined, "uniform / attire", 90);

                ShowAmountBox(s);

                ViolationFilter f = new ViolationFilter();
                f.OnlyFined = true;
                f.FineState = _fineState;

                dataGridView1.Rows.Clear();
                foreach (ViolationRow v in RecordService.MyViolations(f))
                {
                    Ui.AddRow(dataGridView1, v, 3,
                        Ui.DateText(v.RecordedAt), v.TypeName, v.FineText, v.StatusText,
                        v.PaidAt.HasValue ? Ui.DateText(v.PaidAt.Value) : "-");
                }
                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        /// <summary>The "Amount to pay" box: unpaid amount, a bar of paid vs unpaid, and a note.</summary>
        private void ShowAmountBox(StudentSummary s)
        {
            foreach (Control c in gBoxAmount.Controls.Cast<Control>().ToList())
            {
                if (c.Name.StartsWith("dyn"))
                {
                    gBoxAmount.Controls.Remove(c);
                    c.Dispose();
                }
            }

            Ui.CardValue(gBoxAmount, Ui.Money(s.UnpaidFines), s.UnpaidFines > 0 ? Ui.Red : Ui.DarkGreen, 22, 50);

            int margin = 24;
            int barWidth = gBoxAmount.ClientSize.Width - 2 * margin;

            Panel track = new Panel();
            track.Name = "dynTrack";
            track.Location = new Point(margin, 112);
            track.Size = new Size(barWidth, 12);
            track.BackColor = Color.FromArgb(244, 180, 174);
            gBoxAmount.Controls.Add(track);

            if (s.TotalFines > 0)
            {
                Panel paid = new Panel();
                paid.Name = "dynPaid";
                paid.BackColor = Ui.DarkGreen;
                paid.Dock = DockStyle.Left;
                paid.Width = (int)(barWidth * (double)(s.PaidFines / s.TotalFines));
                track.Controls.Add(paid);
            }

            Label l1 = Ui.MakeLabel("Paid  " + Ui.Money(s.PaidFines), new Point(margin, 140), 9f, false, Ui.DarkGreen);
            l1.Name = "dynLegendPaid";
            gBoxAmount.Controls.Add(l1);

            Label l2 = Ui.MakeLabel("Unpaid  " + Ui.Money(s.UnpaidFines), new Point(margin, 166), 9f, false, Ui.Red);
            l2.Name = "dynLegendUnpaid";
            gBoxAmount.Controls.Add(l2);

            Label note = Ui.MakeLabel("Fines come only from no-uniform and inappropriate attire violations.",
                new Point(margin, 0), 8.5f, false, Ui.Gray);
            note.Name = "dynNote";
            note.AutoSize = false;
            note.Size = new Size(barWidth, 40);
            note.Top = gBoxAmount.ClientSize.Height - note.Height - 28;
            gBoxAmount.Controls.Add(note);
        }
    }
}
