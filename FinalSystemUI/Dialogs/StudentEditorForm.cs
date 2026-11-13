using System;
using System.Drawing;
using System.Windows.Forms;
using FinalSystemUI.Helpers;

namespace FinalSystemUI.Dialogs
{
    /// <summary>
    /// Pop-up to register a NEW student. HSG and OSSE both use it.
    /// If the password is left empty, the system makes a temporary password.
    /// </summary>
    public class StudentEditorForm : Form
    {
        private TextBox txtNumber;
        private TextBox txtName;
        private TextBox txtEmail;
        private TextBox txtPassword;

        public string Number { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }      // empty = make a temporary one

        public StudentEditorForm(string suggestedNumber)
        {
            Text = "Add student";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, 350);
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(Ui.MakeLabel("Student ID number", new Point(16, 14), 9.5f, true, Color.Black));
            txtNumber = new TextBox();
            txtNumber.Location = new Point(16, 38);
            txtNumber.Size = new Size(200, 26);
            txtNumber.MaxLength = 20;
            txtNumber.Text = suggestedNumber ?? "";
            Controls.Add(txtNumber);

            Controls.Add(Ui.MakeLabel("Full name", new Point(16, 78), 9.5f, true, Color.Black));
            txtName = new TextBox();
            txtName.Location = new Point(16, 102);
            txtName.Size = new Size(398, 26);
            txtName.MaxLength = 120;
            Controls.Add(txtName);

            Controls.Add(Ui.MakeLabel("Email (the student logs in with this)", new Point(16, 142), 9.5f, true, Color.Black));
            txtEmail = new TextBox();
            txtEmail.Location = new Point(16, 166);
            txtEmail.Size = new Size(398, 26);
            txtEmail.MaxLength = 150;
            Controls.Add(txtEmail);

            Controls.Add(Ui.MakeLabel("Password (leave empty to create one automatically)", new Point(16, 206), 9.5f, true, Color.Black));
            txtPassword = new TextBox();
            txtPassword.Location = new Point(16, 230);
            txtPassword.Size = new Size(260, 26);
            txtPassword.UseSystemPasswordChar = true;
            Controls.Add(txtPassword);

            Controls.Add(Ui.MakeLabel("A temporary password is shown once after saving. Give it to the student.",
                new Point(16, 262), 8.5f, false, Ui.Gray));

            Button ok = Ui.MakeButton("Save", new Point(234, 304), new Size(86, 32), true);
            Button cancel = Ui.MakeButton("Cancel", new Point(328, 304), new Size(86, 32), false);
            ok.Click += OnOk;
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;
        }

        private void OnOk(object sender, EventArgs e)
        {
            Number = txtNumber.Text.Trim();
            FullName = txtName.Text.Trim();
            Email = txtEmail.Text.Trim();
            Password = txtPassword.Text;

            if (Number.Length == 0 || FullName.Length == 0 || Email.Length == 0)
            {
                Ui.Warn("Please enter the ID number, full name, and email.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
