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
    /// HSG > Add record. Steps: 1) type the student ID and press Search, 2) choose the violation,
    /// 3) check the fine, 4) Save. The date, time and "recorded by" are filled in by the system.
    /// </summary>
    public partial class HsgAddRecords : UserControl
    {
        private StudentRow _student;
        private int _typeId;                // 1, 2, 3 or 4 (0 = nothing chosen yet)
        private TextBox txtRemarks;
        private Button btnNewStudent;

        public HsgAddRecords()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.SetUserLabels(label11, label12);
            label10.Text = "Record a uniform, ID lanyard or No-ID violation.";
            lblSem.Text = SafeSemester();

            // student info grid (one row)
            Ui.StyleGrid(dataGridView1);
            dataGridView1.Size = new Size(510, 66);
            dataGridView1.ColumnHeadersHeight = 28;
            dataGridView1.RowTemplate.Height = 28;
            Ui.SetColumns(dataGridView1, "ID number", "Name", "Email", "Violations", "Standing");
            Ui.SetColumnWeights(dataGridView1, 70, 110, 130, 60, 80);

            // recorded by + date (filled by the system)
            textBox2.Text = Session.Current.FullName;
            textBox2.ReadOnly = true;
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "MMM d, yyyy  h:mm tt";
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker1.Enabled = false;

            // remarks (added in code)
            groupBox2.Controls.Add(Ui.MakeLabel("Remarks (optional)", new Point(15, 486), 10f, true, Color.Black));
            txtRemarks = new TextBox();
            txtRemarks.Location = new Point(15, 510);
            txtRemarks.Size = new Size(616, 28);
            txtRemarks.MaxLength = 300;
            groupBox2.Controls.Add(txtRemarks);

            // "New student" button next to the search box
            textBox1.Width = 384;
            btnNewStudent = Ui.MakeButton("+ New student", new Point(textBox1.Right + 8, button3.Top), new Size(122, button3.Height), false);
            btnNewStudent.Click += delegate { AddNewStudent(textBox1.Text); };
            groupBox2.Controls.Add(btnNewStudent);

            // buttons
            button3.Click += delegate { SearchStudent(); };
            textBox1.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SearchStudent();
                }
            };
            button4.Click += delegate { SelectType(ViolationTypeIds.NoUniform); };
            button5.Click += delegate { SelectType(ViolationTypeIds.InappropriateAttire); };
            btnNoId.Click += delegate { SelectType(ViolationTypeIds.NoId); };
            button2.Click += delegate { ResetForm(); };
            Ui.StylePrimary(button1);

            ResetForm();
        }

        private string SafeSemester()
        {
            try { return RecordService.SemesterName(); }
            catch (Exception ex) { Ui.ShowError(ex); return ""; }
        }

        private void ResetForm()
        {
            _student = null;
            _typeId = 0;
            textBox1.Clear();
            textBox3.Clear();
            textBox3.Enabled = false;
            txtRemarks.Clear();
            dataGridView1.Rows.Clear();
            dateTimePicker1.Value = DateTime.Now;
            Ui.SetActiveFilter(null, button4, button5, button6, btnNoId);
        }

        private void SearchStudent()
        {
            try
            {
                StudentRow s = RecordService.FindStudent(textBox1.Text);
                dataGridView1.Rows.Clear();
                _student = null;

                if (s == null)
                {
                    if (!string.IsNullOrWhiteSpace(textBox1.Text) &&
                        Ui.Confirm("No student was found with that ID number.\n\nDo you want to add this student now?"))
                    {
                        AddNewStudent(textBox1.Text);
                    }
                    return;
                }
                if (!s.IsActive)
                {
                    Ui.Warn("This student account is disabled.");
                    return;
                }

                StudentSummary sum = RecordService.SummaryFor(s.StudentId);
                dataGridView1.Rows.Add(s.StudentNumber, s.FullName, s.Email, sum.TotalViolations, sum.Standing);
                dataGridView1.ClearSelection();
                _student = s;
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        /// <summary>Registers a new student, then searches for them so a record can be added right away.</summary>
        private void AddNewStudent(string suggestedNumber)
        {
            try
            {
                using (StudentEditorForm dlg = new StudentEditorForm(suggestedNumber))
                {
                    if (dlg.ShowDialog(this.FindForm()) != DialogResult.OK) return;

                    string used = UserService.AddStudent(dlg.Number, dlg.FullName, dlg.Email, dlg.Password);
                    textBox1.Text = dlg.Number;
                    if (string.IsNullOrEmpty(dlg.Password))
                        Ui.Info("The student was added.\n\nTemporary password: " + used +
                                "\n\nGive it to the student. It is not shown again.");
                    else
                        Ui.Info("The student was added.");
                    SearchStudent();
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void SelectType(int typeId)
        {
            _typeId = typeId;
            Button active = typeId == ViolationTypeIds.NoUniform ? button4
                          : (typeId == ViolationTypeIds.InappropriateAttire ? button5
                          : (typeId == ViolationTypeIds.InvalidLanyard ? button6 : btnNoId));
            Ui.SetActiveFilter(active, button4, button5, button6, btnNoId);

            bool finable = ViolationTypeIds.IsFinable(typeId);
            textBox3.Enabled = finable;
            if (finable)
            {
                try
                {
                    textBox3.Text = RecordService.DefaultFine(typeId).ToString("0.00", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    Ui.ShowError(ex);
                }
            }
            else
            {
                textBox3.Text = "";
            }
        }

        private void SaveRecord()
        {
            if (_student == null)
            {
                Ui.Warn("Please search for the student first.");
                return;
            }
            if (_typeId == 0)
            {
                Ui.Warn("Please choose the violation.");
                return;
            }

            decimal? fine = null;
            if (ViolationTypeIds.IsFinable(_typeId))
            {
                string text = textBox3.Text.Replace("\u20B1", "").Replace(",", "").Trim();
                decimal value;
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) || value < 0)
                {
                    Ui.Warn("Please enter a valid fine amount, for example 100.00.");
                    return;
                }
                fine = value;
            }

            try
            {
                using (Ui.Busy(this))
                {
                    RecordService.AddRecord(_student.StudentNumber, _typeId, txtRemarks.Text, fine);
                }
                Ui.Info("The record for " + _student.FullName + " was saved.");
                ResetForm();
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
            (this.FindForm() as MainForm)?.ShowScreen(new HsgControl());
        }

        private void btnAddRecord_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new HsgAddRecords());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveRecord();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SelectType(ViolationTypeIds.InvalidLanyard);
        }
    }
}
