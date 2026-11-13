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
    /// <summary>Student > Violation history: every violation of this semester, with filters (All, No Uniform, Lanyard, No-ID).</summary>
    public partial class ViolationHistory : UserControl
    {
        private int _mode;      // 0 = all, 1 = no uniform / inappropriate attire, 2 = invalid lanyard, 3 = no ID

        public ViolationHistory()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.StyleGrid(dgvViolationHis);
            Ui.SetColumns(dgvViolationHis, "Date", "Violation", "Recorded by", "Fine", "Status");
            Ui.SetColumnWeights(dgvViolationHis, 130, 120, 110, 70, 90);

            btnNounif.Click += delegate { _mode = 1; LoadData(); };
            btnLanyard.Click += delegate { _mode = 2; LoadData(); };
            btnNoid.Click += delegate { _mode = 3; LoadData(); };

            LoadData();
        }

        /// <summary>The filter button that is switched on now.</summary>
        private Button ActiveButton()
        {
            switch (_mode)
            {
                case 1: return btnNounif;
                case 2: return btnLanyard;
                case 3: return btnNoid;
                default: return btnAll;
            }
        }

        private void LoadData()
        {
            try
            {
                Ui.SetActiveFilter(ActiveButton(), btnAll, btnNounif, btnLanyard, btnNoid);
                lblSem.Text = RecordService.SemesterName();

                ViolationFilter f = new ViolationFilter();
                if (_mode == 1) f.TypeIds = new int[] { ViolationTypeIds.NoUniform, ViolationTypeIds.InappropriateAttire };
                if (_mode == 2) f.TypeIds = new int[] { ViolationTypeIds.InvalidLanyard };
                if (_mode == 3) f.TypeIds = new int[] { ViolationTypeIds.NoId };

                dgvViolationHis.Rows.Clear();
                foreach (ViolationRow v in RecordService.MyViolations(f))
                {
                    Ui.AddRow(dgvViolationHis, v, 4,
                        Ui.DateText(v.RecordedAt), v.TypeName, v.RecordedByName ?? v.RecordedByRole, v.FineText, v.StatusText);
                }
                dgvViolationHis.ClearSelection();

                ShowStanding();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        /// <summary>Fills the "Current standing" box on the right.</summary>
        private void ShowStanding()
        {
            StudentSummary s = RecordService.MySummary();
            Color color = s.Standing == "Good" ? Ui.DarkGreen : (s.Standing == "Warning" ? Ui.Amber : Ui.Red);
            Ui.CardValue(gBoxCurrentStand, s.Standing, color, 22, 46);

            foreach (Control c in gBoxCurrentStand.Controls.Cast<Control>().ToList())
            {
                if (c.Name.StartsWith("dyn"))
                {
                    gBoxCurrentStand.Controls.Remove(c);
                    c.Dispose();
                }
            }

            ProgressBar bar = new ProgressBar();
            bar.Name = "dynBar";
            bar.Style = ProgressBarStyle.Continuous;
            bar.Maximum = Math.Max(1, s.Threshold);
            bar.Value = Math.Min(s.TotalViolations, bar.Maximum);
            bar.Location = new Point(20, 100);
            bar.Size = new Size(gBoxCurrentStand.ClientSize.Width - 40, 14);
            gBoxCurrentStand.Controls.Add(bar);

            AddLine("dynNote", s.TotalViolations + " of " + s.Threshold + " violations before clearance hold", 122, Ui.Gray, false);

            // counts by type
            List<ViolationRow> all = RecordService.MyViolations(new ViolationFilter());
            int y = 156;
            for (int typeId = 1; typeId <= 4; typeId++)
            {
                int count = all.Count(v => v.TypeId == typeId);
                AddLine("dynType" + typeId, ViolationTypeIds.NameOf(typeId) + ":  " + count, y, Color.Black, false);
                y += 25;
            }

            if (s.PendingClearance > 0)
                AddLine("dynPending", "Waiting for clearance:  " + s.PendingClearance, y + 10, Ui.Amber, true);
        }

        private void AddLine(string name, string text, int y, Color color, bool bold)
        {
            Label l = Ui.MakeLabel(text, new Point(20, y), 9f, bold, color);
            l.Name = name;
            gBoxCurrentStand.Controls.Add(l);
        }

        private void btnAll_Click(object sender, EventArgs e)
        {
            _mode = 0;
            LoadData();
        }
    }
}
