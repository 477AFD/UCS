using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FinalSystemUI.Dialogs;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Services;

namespace FinalSystemUI
{
    /// <summary>OSSE > Clearance: settle No-ID and invalid-lanyard records, and see the clearance history.</summary>
    public partial class OsseClearance1 : UserControl
    {
        private int _filterIndex;       // 0 = for clearance, 1 = cleared, 2 = all
        private Button[] _filters;

        public OsseClearance1()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.SetUserLabels(label11, label12);

            Ui.StyleGrid(dataGridView1);
            Ui.SetColumns(dataGridView1, "ID", "Student ID", "Student", "Violation", "Date", "Status", "Cleared on");
            Ui.SetColumnWeights(dataGridView1, 35, 75, 120, 115, 125, 85, 125);

            // the screen was designed with an "Add user" button; here it is reused
            button5.Text = "Mark cleared";
            button5.Size = new Size(130, 30);
            Ui.StylePrimary(button5);

            _filters = new Button[] { button1, button2, button3 };
            button1.Click += delegate { SetFilter(0); };
            button2.Click += delegate { SetFilter(1); };
            // button3 ("All") is connected to button3_Click by the designer
            button5.Click += delegate { MarkCleared(); };

            SetFilter(0);
        }

        private void SetFilter(int index)
        {
            _filterIndex = index;
            Ui.SetActiveFilter(_filters[index], _filters);
            LoadData();
        }

        private void LoadData()
        {
            if (_filters == null) return;
            try
            {
                lblSem.Text = RecordService.SemesterName();

                RecordCounts c = RecordService.Counts();
                Ui.CardValue(gboxUnpaid, c.ForClearance.ToString(), Ui.Amber, 18, 42);
                Ui.CardValue(groupBox2, c.NoId.ToString(), Color.FromArgb(109, 40, 217), 18, 42);
                Ui.CardValue(groupBox3, c.InvalidLanyard.ToString(), Color.FromArgb(29, 78, 216), 18, 42);
                Ui.CardValue(groupBox4, c.ClearedToday.ToString(), Ui.DarkGreen, 18, 42);

                ViolationFilter f = new ViolationFilter();
                f.OnlyNeedsClearance = true;
                if (_filterIndex == 0) f.Clearance = 1;
                if (_filterIndex == 1) f.Clearance = 2;

                dataGridView1.Rows.Clear();
                foreach (ViolationRow v in RecordService.AllRecords(f))
                {
                    Ui.AddRow(dataGridView1, v, 5,
                        v.ViolationId, v.StudentNumber, v.StudentName, v.TypeName, Ui.DateText(v.RecordedAt),
                        v.StatusText, v.ClearedAt.HasValue ? Ui.DateText(v.ClearedAt.Value) : "-");
                }
                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void MarkCleared()
        {
            ViolationRow v = Ui.SelectedTag<ViolationRow>(dataGridView1);
            if (v == null)
            {
                Ui.Warn("Please select a record first.");
                return;
            }
            if (v.IsCleared)
            {
                Ui.Warn("This record is already cleared.");
                return;
            }

            string remarks = InputBox.Ask("Clearance", "Remarks for " + v.StudentName + " (optional)", "", false);
            if (remarks == null) return;
            try
            {
                RecordService.ClearRecord(v.ViolationId, remarks);
                LoadData();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void btnOsseLogout_Click(object sender, EventArgs e)
        {
            MainForm main = this.FindForm() as MainForm;
            main?.ShowLogin();
        }

        private void btnRecords_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseRecords());
        }

        private void btnClearance_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseClearance1());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetFilter(2);
        }

        private void btnUser_Click_1(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseUsers());
        }

        private void btnAuditLog_Click(object sender, EventArgs e)
        {
            AuditLogForm.Open(this.FindForm());
        }

        private void btnSqlQueries_Click(object sender, EventArgs e)
        {
            QueryViewerForm.Open(this.FindForm());
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }
}
