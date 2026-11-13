using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using FinalSystemUI.Helpers;
using FinalSystemUI.Models;
using FinalSystemUI.Services;

namespace FinalSystemUI.Dialogs
{
    /// <summary>
    /// Pop-up used to ADD a record (OSSE) or to EDIT a record (HSG, first 12 hours only).
    /// It is built in code, so there is no designer file.
    /// </summary>
    public class RecordEditorForm : Form
    {
        private readonly bool _isAdd;
        private readonly int[] _typeIds;
        private TextBox txtStudent;
        private ComboBox cboType;
        private TextBox txtFine;
        private TextBox txtRemarks;
        private Label lblStudentName;

        // results
        public string StudentNumber { get; private set; }
        public int TypeId { get; private set; }
        public decimal? FineAmount { get; private set; }
        public string Remarks { get; private set; }

        /// <param name="allowedTypeIds">The violation types this user may choose.</param>
        /// <param name="existing">null when adding; the record when editing.</param>
        public RecordEditorForm(string title, int[] allowedTypeIds, ViolationRow existing)
        {
            _isAdd = (existing == null);
            _typeIds = allowedTypeIds;

            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, 340);
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(Ui.MakeLabel("Student ID number", new Point(16, 14), 9.5f, true, Color.Black));
            txtStudent = new TextBox();
            txtStudent.Location = new Point(16, 38);
            txtStudent.Size = new Size(200, 26);
            Controls.Add(txtStudent);

            lblStudentName = Ui.MakeLabel("", new Point(226, 41), 9.5f, false, Ui.Gray);
            lblStudentName.AutoSize = false;
            lblStudentName.AutoEllipsis = true;
            lblStudentName.Size = new Size(92, 20);
            Controls.Add(lblStudentName);

            if (_isAdd)
            {
                Button newStudent = Ui.MakeButton("New student", new Point(322, 36), new Size(92, 28), false);
                newStudent.Click += delegate { AddNewStudent(); };
                Controls.Add(newStudent);
            }

            Controls.Add(Ui.MakeLabel("Violation", new Point(16, 78), 9.5f, true, Color.Black));
            cboType = new ComboBox();
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.Location = new Point(16, 102);
            cboType.Size = new Size(260, 26);
            foreach (int id in _typeIds) cboType.Items.Add(ViolationTypeIds.NameOf(id));
            Controls.Add(cboType);

            Controls.Add(Ui.MakeLabel("Fine (PHP)", new Point(16, 142), 9.5f, true, Color.Black));
            txtFine = new TextBox();
            txtFine.Location = new Point(16, 166);
            txtFine.Size = new Size(120, 26);
            Controls.Add(txtFine);

            Controls.Add(Ui.MakeLabel("Remarks (optional)", new Point(16, 206), 9.5f, true, Color.Black));
            txtRemarks = new TextBox();
            txtRemarks.Location = new Point(16, 230);
            txtRemarks.Size = new Size(398, 50);
            txtRemarks.Multiline = true;
            txtRemarks.MaxLength = 300;
            Controls.Add(txtRemarks);

            Button ok = Ui.MakeButton("Save", new Point(234, 294), new Size(86, 32), true);
            Button cancel = Ui.MakeButton("Cancel", new Point(328, 294), new Size(86, 32), false);
            ok.Click += OnOk;
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;

            if (_isAdd)
            {
                txtStudent.Leave += delegate { ShowStudentName(); };
                cboType.SelectedIndex = 0;
            }
            else
            {
                txtStudent.Text = existing.StudentNumber;
                txtStudent.ReadOnly = true;
                lblStudentName.Text = existing.StudentName;
                int index = Array.IndexOf(_typeIds, existing.TypeId);
                cboType.SelectedIndex = index >= 0 ? index : 0;
                txtRemarks.Text = existing.Remarks ?? "";
                if (existing.FineAmount.HasValue)
                    txtFine.Text = existing.FineAmount.Value.ToString("0.00", CultureInfo.InvariantCulture);
            }

            cboType.SelectedIndexChanged += delegate { OnTypeChanged(); };
            OnTypeChanged();
        }

        private void AddNewStudent()
        {
            try
            {
                using (StudentEditorForm dlg = new StudentEditorForm(txtStudent.Text))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    string used = UserService.AddStudent(dlg.Number, dlg.FullName, dlg.Email, dlg.Password);
                    txtStudent.Text = dlg.Number;
                    ShowStudentName();
                    if (string.IsNullOrEmpty(dlg.Password))
                        Ui.Info("The student was added.\n\nTemporary password: " + used +
                                "\n\nGive it to the student. It is not shown again.");
                    else
                        Ui.Info("The student was added.");
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private int SelectedTypeId()
        {
            return cboType.SelectedIndex >= 0 ? _typeIds[cboType.SelectedIndex] : 0;
        }

        private void ShowStudentName()
        {
            try
            {
                StudentRow s = RecordService.FindStudent(txtStudent.Text);
                lblStudentName.Text = s == null ? "Student not found" : s.FullName;
                lblStudentName.ForeColor = s == null ? Ui.Red : Ui.DarkGreen;
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void OnTypeChanged()
        {
            int typeId = SelectedTypeId();
            bool finable = ViolationTypeIds.IsFinable(typeId);
            txtFine.Enabled = finable;
            if (!finable)
            {
                txtFine.Text = "";
            }
            else if (string.IsNullOrWhiteSpace(txtFine.Text))
            {
                try
                {
                    txtFine.Text = RecordService.DefaultFine(typeId).ToString("0.00", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    txtFine.Text = "";
                }
            }
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (_isAdd && string.IsNullOrWhiteSpace(txtStudent.Text))
            {
                Ui.Warn("Please enter the student's ID number.");
                return;
            }
            int typeId = SelectedTypeId();
            if (typeId == 0)
            {
                Ui.Warn("Please choose a violation type.");
                return;
            }

            decimal? fine = null;
            if (ViolationTypeIds.IsFinable(typeId))
            {
                string text = txtFine.Text.Replace("\u20B1", "").Replace(",", "").Trim();
                decimal value;
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) || value < 0)
                {
                    Ui.Warn("Please enter a valid fine amount, for example 100.00.");
                    return;
                }
                fine = value;
            }

            StudentNumber = txtStudent.Text.Trim();
            TypeId = typeId;
            FineAmount = fine;
            Remarks = txtRemarks.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
