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
    /// <summary>Student > Dashboard: totals, current standing and the latest violations.</summary>
    public partial class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.StyleGrid(dgvDashboard);
            Ui.SetColumns(dgvDashboard, "Date", "Violation", "Recorded by", "Fine", "Status");
            Ui.SetColumnWeights(dgvDashboard, 130, 120, 110, 70, 90);
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                AppUser me = Session.Current;
                label3.Text = "HELLO, " + Ui.FirstName(me.FullName).ToUpper();
                lblSem.Text = RecordService.SemesterName();

                StudentSummary s = RecordService.MySummary();

                Ui.CardValue(gBoxViolation, s.TotalViolations.ToString(), Ui.Red, 24, 48);
                Ui.CardNote(gBoxViolation,
                    s.UniformViolations + " uniform, " + (s.LanyardViolations + s.NoIdViolations) + " ID / lanyard", 104);

                Ui.CardValue(gBoxFines, Ui.Money(s.UnpaidFines), Ui.DarkGreen, 22, 48);
                Ui.CardNote(gBoxFines, "unpaid of " + Ui.Money(s.TotalFines) + " this semester", 104);

                Color standingColor = s.Standing == "Good" ? Ui.DarkGreen : (s.Standing == "Warning" ? Ui.Amber : Ui.Red);
                Ui.CardValue(gBoxStanding, s.Standing, standingColor, 22, 48);
                Ui.CardNote(gBoxStanding, s.TotalViolations + " of " + s.Threshold + " before clearance hold", 104);

                string note = "Lanyard and No-ID records aren't fined. They are settled with OSSE during Clearance.";
                if (s.PendingClearance > 0)
                    note += "  You have " + s.PendingClearance + " record(s) waiting for clearance.";
                label6.Text = note;
                label6.AutoSize = false;
                label6.Size = new Size(616, 21);

                dgvDashboard.Rows.Clear();
                List<ViolationRow> rows = RecordService.MyViolations(new ViolationFilter());
                foreach (ViolationRow v in rows.Take(10))
                {
                    Ui.AddRow(dgvDashboard, v, 4,
                        Ui.DateText(v.RecordedAt), v.TypeName, v.RecordedByName ?? v.RecordedByRole, v.FineText, v.StatusText);
                }
                dgvDashboard.ClearSelection();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }
    }
}
