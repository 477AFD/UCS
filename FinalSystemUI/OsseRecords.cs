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
    /// <summary>OSSE > Records: find any violation, add a record (all four types) or delete a record.</summary>
    public partial class OsseRecords : UserControl
    {
        private int _filterIndex;       // 0 all, 1 No ID, 2 Invalid lanyard, 3 No uniform / attire
        private Button[] _filters;

        public OsseRecords()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.SetUserLabels(label11, label12);

            Ui.StyleGrid(dataGridView1);
            Ui.SetColumns(dataGridView1, "ID", "Student ID", "Student", "Violation", "Date", "Recorded by", "Fine", "Status");
            Ui.SetColumnWeights(dataGridView1, 35, 70, 110, 110, 125, 95, 65, 85);

            _filters = new Button[] { button1, button2, button3, button4 };
            button1.Click += delegate { SetFilter(0); };
            button2.Click += delegate { SetFilter(1); };
            button3.Click += delegate { SetFilter(2); };
            button4.Click += delegate { SetFilter(3); };
            button5.Click += delegate { AddRecord(); };
            textBox1.TextChanged += delegate { LoadData(); };
            Ui.StylePrimary(button5);

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
                Ui.CardValue(gboxUnpaid, (c.NoUniform + c.InappropriateAttire).ToString(), Ui.Red, 18, 42);
                Ui.CardValue(groupBox2, c.InvalidLanyard.ToString(), Color.FromArgb(29, 78, 216), 18, 42);
                Ui.CardValue(groupBox3, c.NoId.ToString(), Color.FromArgb(109, 40, 217), 18, 42);
                Ui.CardValue(groupBox4, c.ForClearance.ToString(), Ui.Amber, 18, 42);

                ViolationFilter f = new ViolationFilter();
                f.Search = textBox1.Text;
                if (_filterIndex == 1) f.TypeIds = new int[] { ViolationTypeIds.NoId };
                if (_filterIndex == 2) f.TypeIds = new int[] { ViolationTypeIds.InvalidLanyard };
                if (_filterIndex == 3) f.TypeIds = new int[] { ViolationTypeIds.NoUniform, ViolationTypeIds.InappropriateAttire };

                dataGridView1.Rows.Clear();
                foreach (ViolationRow v in RecordService.AllRecords(f))
                {
                    Ui.AddRow(dataGridView1, v, 7,
                        v.ViolationId, v.StudentNumber, v.StudentName, v.TypeName, Ui.DateText(v.RecordedAt),
                        v.RecordedByName ?? v.RecordedByRole, v.FineText, v.StatusText);
                }
                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void AddRecord()
        {
            int[] all = new int[]
            {
                ViolationTypeIds.NoUniform, ViolationTypeIds.InappropriateAttire,
                ViolationTypeIds.InvalidLanyard, ViolationTypeIds.NoId
            };
            try
            {
                using (RecordEditorForm dlg = new RecordEditorForm("Add record", all, null))
                {
                    if (dlg.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        RecordService.AddRecord(dlg.StudentNumber, dlg.TypeId, dlg.Remarks, dlg.FineAmount);
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void DeleteRecord()
        {
            ViolationRow v = Ui.SelectedTag<ViolationRow>(dataGridView1);
            if (v == null)
            {
                Ui.Warn("Please select a record first.");
                return;
            }
            if (!Ui.Confirm("Delete this record?\n\n" + v.StudentName + " - " + v.TypeName + " - " + Ui.DateText(v.RecordedAt) +
                            "\n\nThis cannot be undone."))
                return;
            try
            {
                RecordService.DeleteRecord(v.ViolationId);
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

        private void btnUser_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseUsers());
        }

        private void btnRecords_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseRecords());
        }

        private void btnClearance_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseClearance1());
        }

        private void btnAuditLog_Click(object sender, EventArgs e)
        {
            AuditLogForm.Open(this.FindForm());
        }

        private void btnSqlQueries_Click(object sender, EventArgs e)
        {
            QueryViewerForm.Open(this.FindForm());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DeleteRecord();
        }
    }
}
