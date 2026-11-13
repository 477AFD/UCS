using System;
using System.Drawing;
using System.Windows.Forms;
using FinalSystemUI.Helpers;

namespace FinalSystemUI.Dialogs
{
    /// <summary>Pop-up used by OSSE to add a Student account or an HSG account (both log in with email + password).</summary>
    public class UserEditorForm : Form
    {
        private ComboBox cboKind;
        private TextBox txtNumber;
        private TextBox txtName;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private CheckBox chkIsStudent;
        private Label lblNumber;

        public string Kind { get; private set; }          // "Student" or "HSG"
        public string Number { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public bool IsStudentFlag { get; private set; }

        public UserEditorForm()
        {
            Text = "Add user";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, 430);
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(Ui.MakeLabel("Account type", new Point(16, 14), 9.5f, true, Color.Black));
            cboKind = new ComboBox();
            cboKind.DropDownStyle = ComboBoxStyle.DropDownList;
            cboKind.Items.AddRange(new object[] { "Student", "HSG" });
            cboKind.Location = new Point(16, 38);
            cboKind.Size = new Size(200, 26);
            Controls.Add(cboKind);

            lblNumber = Ui.MakeLabel("Student ID number", new Point(16, 78), 9.5f, true, Color.Black);
            Controls.Add(lblNumber);
            txtNumber = new TextBox();
            txtNumber.Location = new Point(16, 102);
            txtNumber.Size = new Size(200, 26);
            txtNumber.MaxLength = 20;
            Controls.Add(txtNumber);

            Controls.Add(Ui.MakeLabel("Full name", new Point(16, 142), 9.5f, true, Color.Black));
            txtName = new TextBox();
            txtName.Location = new Point(16, 166);
            txtName.Size = new Size(398, 26);
            txtName.MaxLength = 120;
            Controls.Add(txtName);

            Controls.Add(Ui.MakeLabel("Email", new Point(16, 206), 9.5f, true, Color.Black));
            txtEmail = new TextBox();
            txtEmail.Location = new Point(16, 230);
            txtEmail.Size = new Size(398, 26);
            txtEmail.MaxLength = 150;
            Controls.Add(txtEmail);

            Controls.Add(Ui.MakeLabel("Password (leave empty to create one automatically)",
                new Point(16, 270), 9.5f, true, Color.Black));
            txtPassword = new TextBox();
            txtPassword.Location = new Point(16, 294);
            txtPassword.Size = new Size(260, 26);
            txtPassword.UseSystemPasswordChar = true;
            Controls.Add(txtPassword);

            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "Show password";
            chkShowPassword.Location = new Point(288, 296);
            chkShowPassword.AutoSize = true;
            chkShowPassword.CheckedChanged += delegate { txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked; };
            Controls.Add(chkShowPassword);

            chkIsStudent = new CheckBox();
            chkIsStudent.Text = "This HSG officer is also a student";
            chkIsStudent.Location = new Point(16, 338);
            chkIsStudent.AutoSize = true;
            chkIsStudent.Checked = true;
            Controls.Add(chkIsStudent);

            Button ok = Ui.MakeButton("Save", new Point(234, 380), new Size(86, 32), true);
            Button cancel = Ui.MakeButton("Cancel", new Point(328, 380), new Size(86, 32), false);
            ok.Click += OnOk;
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;

            cboKind.SelectedIndexChanged += delegate { ApplyKind(); };
            cboKind.SelectedIndex = 0;
        }

        private void ApplyKind()
        {
            bool student = (cboKind.SelectedItem as string) == "Student";
            lblNumber.Visible = student;
            txtNumber.Visible = student;
            chkIsStudent.Visible = !student;
        }

        private void OnOk(object sender, EventArgs e)
        {
            Kind = cboKind.SelectedItem as string;
            Number = txtNumber.Text.Trim();
            FullName = txtName.Text.Trim();
            Email = txtEmail.Text.Trim();
            Password = txtPassword.Text;
            IsStudentFlag = chkIsStudent.Checked;

            if (FullName.Length == 0 || Email.Length == 0 || (Kind == "Student" && Number.Length == 0))
            {
                Ui.Warn("Please fill in all the fields.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
