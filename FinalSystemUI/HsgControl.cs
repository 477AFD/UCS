using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
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
    /// <summary>
    /// HSG > Records. Shows the records, and lets HSG edit a record (first 12 hours only),
    /// set a fine, or mark a fine as paid.
    /// </summary>
    public partial class HsgControl : UserControl
    {
        private int _filterIndex;               // 0 all, 1 no uniform, 2 inappropriate attire, 3 invalid lanyard, 4 no-ID
        private int _editHours = 12;
        private Button[] _filters;

        public HsgControl()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.SetUserLabels(label11, label12);

            Ui.StyleGrid(dgvOsseRecords);
            Ui.SetColumns(dgvOsseRecords, "ID", "Student ID", "Student", "Violation", "Date", "Recorded by", "Fine", "Status", "Edit window");
            Ui.SetColumnWeights(dgvOsseRecords, 35, 70, 105, 105, 120, 90, 60, 75, 100);

            _filters = new Button[] { button1, button2, button3, button4, btnNoIdFilter };
            button2.Click += delegate { SetFilter(1); };
            button3.Click += delegate { SetFilter(2); };
            button4.Click += delegate { SetFilter(3); };
            btnNoIdFilter.Click += delegate { SetFilter(4); };
            button5.Click += btnAddRecord_Click;
            Ui.StylePrimary(button5);
            txtsearch.TextChanged += delegate { LoadData(); };

            dgvOsseRecords.SelectionChanged += delegate { UpdateButtons(); };

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
                _editHours = RecordService.EditWindowHours;
                lblEditHint.Text = "Edit unlocks for " + _editHours + " hours after a record is made.";

                RecordCounts c = RecordService.Counts();
                Ui.CardValue(gboxUnpaid, c.TotalRecords.ToString(), Color.Black, 20, 50);
                Ui.CardNote(gboxUnpaid, "this semester", 92);
                Ui.CardValue(groupBox2, Ui.Money(c.UnpaidFines), Ui.Red, 20, 50);
                Ui.CardValue(groupBox3, Ui.Money(c.CollectedFines), Ui.DarkGreen, 20, 50);

                ViolationFilter f = new ViolationFilter();
                f.Search = txtsearch.Text;
                if (_filterIndex > 0) f.TypeIds = new int[] { _filterIndex };

                dgvOsseRecords.Rows.Clear();
                foreach (ViolationRow v in RecordService.AllRecords(f))
                {
                    int i = Ui.AddRow(dgvOsseRecords, v, 7,
                        v.ViolationId, v.StudentNumber, v.StudentName, v.TypeName, Ui.DateText(v.RecordedAt),
                        v.RecordedByName ?? v.RecordedByRole, v.FineText, v.StatusText, v.EditWindowText(_editHours));
                    dgvOsseRecords.Rows[i].Cells[8].Style.ForeColor = v.CanEdit(_editHours) ? Ui.Amber : Color.Gray;
                }
                dgvOsseRecords.ClearSelection();
                UpdateButtons();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        /// <summary>Each button works only when the selected record allows it.</summary>
        private void UpdateButtons()
        {
            ViolationRow v = Ui.SelectedTag<ViolationRow>(dgvOsseRecords);
            btnEditRecord.Enabled = v != null && v.TypeId != ViolationTypeIds.NoId && v.CanEdit(_editHours);
            btnSetFine.Enabled = v != null && v.IsFinable && !v.IsPaid;
            btnMarkPaid.Enabled = v != null && v.IsFinable && !v.IsPaid;
        }

        private void btnEditRecord_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void btnSetFine_Click(object sender, EventArgs e)
        {
            SetFineSelected();
        }

        private void btnMarkPaid_Click(object sender, EventArgs e)
        {
            MarkPaidSelected();
        }

        private ViolationRow Selected()
        {
            ViolationRow v = Ui.SelectedTag<ViolationRow>(dgvOsseRecords);
            if (v == null) Ui.Warn("Please select a record first.");
            return v;
        }

        private void EditSelected()
        {
            ViolationRow v = Selected();
            if (v == null) return;

            if (v.TypeId == ViolationTypeIds.NoId)
            {
                Ui.Warn("HSG accounts cannot change No-ID records. OSSE handles those.");
                return;
            }
            if (!v.CanEdit(_editHours))
            {
                Ui.Warn("This record is locked. HSG can edit a record only within " + _editHours + " hours after it was made.");
                return;
            }

            int[] allowed = new int[] { ViolationTypeIds.NoUniform, ViolationTypeIds.InappropriateAttire, ViolationTypeIds.InvalidLanyard };
            try
            {
                using (RecordEditorForm dlg = new RecordEditorForm("Edit record", allowed, v))
                {
                    if (dlg.ShowDialog(this.FindForm()) == DialogResult.OK)
                    {
                        RecordService.EditRecord(v.ViolationId, dlg.TypeId, dlg.FineAmount, dlg.Remarks);
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
                LoadData();
            }
        }

        private void SetFineSelected()
        {
            ViolationRow v = Selected();
            if (v == null) return;

            if (!v.IsFinable)
            {
                Ui.Warn("Lanyard and no-ID records are not fined.");
                return;
            }
            if (v.IsPaid)
            {
                Ui.Warn("This fine is already paid.");
                return;
            }

            string current = v.FineAmount.HasValue ? v.FineAmount.Value.ToString("0.00", CultureInfo.InvariantCulture) : "";
            string text = InputBox.Ask("Set fine", "Fine amount (PHP) for " + v.StudentName, current, false);
            if (text == null) return;

            decimal amount;
            text = text.Replace("\u20B1", "").Replace(",", "").Trim();
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) || amount < 0)
            {
                Ui.Warn("Please enter a valid amount, for example 100.00.");
                return;
            }

            try
            {
                RecordService.SetFine(v.ViolationId, amount);
                LoadData();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void MarkPaidSelected()
        {
            ViolationRow v = Selected();
            if (v == null) return;

            if (!v.IsFinable)
            {
                Ui.Warn("Lanyard and no-ID records are not fined.");
                return;
            }
            if (v.IsPaid)
            {
                Ui.Warn("This fine is already paid.");
                return;
            }
            if (!Ui.Confirm("Mark the fine of " + v.StudentName + " (" + v.FineText + ") as PAID?")) return;

            try
            {
                RecordService.MarkPaid(v.ViolationId);
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

        private void btnAddRecord_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new HsgAddRecords());
        }

        private void btnRecords_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new HsgControl());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetFilter(0);
        }
    }
}
