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
using FinalSystemUI.Services;

namespace FinalSystemUI
{
    /// <summary>
    /// The login screen. One drop-down chooses the account type (STUDENT, HSG or OSSE).
    /// Every account type logs in with an EMAIL and a PASSWORD. "Show password" shows or hides the password.
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public event Action<string> LoginSucceeded;

        private Label lblStatus;
        private ProgressBar prg;
        private bool _busy;

        public LoginControl()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            BuildExtraControls();

            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRole.SelectedIndexChanged += delegate { ApplyRole(); };
            cboRole.SelectedIndex = 0;      // STUDENT

            // Pressing Enter in the email or password box logs in.
            textBox1.KeyDown += LoginBox_KeyDown;
            textBox2.KeyDown += LoginBox_KeyDown;
        }

        private void BuildExtraControls()
        {
            lblStatus = new Label();
            lblStatus.AutoSize = false;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Location = new Point(40, 350);
            lblStatus.Size = new Size(342, 30);
            lblStatus.Font = new Font("Segoe UI", 9f);
            groupBox2.Controls.Add(lblStatus);

            prg = new ProgressBar();
            prg.Style = ProgressBarStyle.Marquee;
            prg.MarqueeAnimationSpeed = 30;
            prg.Location = new Point(textBox1.Left, 384);
            prg.Size = new Size(186, 10);
            prg.Visible = false;
            groupBox2.Controls.Add(prg);
        }

        private string SelectedRole()
        {
            return cboRole.SelectedItem as string;
        }

        /// <summary>Clears the boxes when another account type is chosen.</summary>
        private void ApplyRole()
        {
            textBox1.Clear();
            textBox2.Clear();
            chkShowPassword.Checked = false;
            SetStatus("", false);
            textBox1.Focus();
        }

        /// <summary>View or hide the password.</summary>
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private async void LoginBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await DoLoginAsync();
        }

        private void SetStatus(string text, bool isError)
        {
            if (IsDisposed || lblStatus == null) return;
            lblStatus.Text = text;
            lblStatus.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            btnLogin.Enabled = !busy;
            cboRole.Enabled = !busy;
            textBox1.Enabled = !busy;
            textBox2.Enabled = !busy;
            chkShowPassword.Enabled = !busy;
            prg.Visible = busy;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await DoLoginAsync();
        }

        private async Task DoLoginAsync()
        {
            if (_busy) return;

            string role = SelectedRole();
            if (role == null)
            {
                Ui.Warn("Please choose an account type.");
                return;
            }

            SetStatus("", false);
            SetBusy(true);
            try
            {
                SetStatus("Checking your account...", false);

                AuthResult result;
                if (role == "STUDENT")
                    result = await AuthService.LoginStudentAsync(textBox1.Text, textBox2.Text);
                else if (role == "HSG")
                    result = await AuthService.LoginHsgAsync(textBox1.Text, textBox2.Text);
                else
                    result = await AuthService.LoginOsseAsync(textBox1.Text, textBox2.Text);

                if (IsDisposed) return;
                if (!result.Success)
                {
                    SetStatus(result.Message, true);
                    return;
                }

                Session.Start(result.User);
                textBox2.Clear();
                Action<string> handler = LoginSucceeded;
                if (handler != null) handler(role);
            }
            catch (Exception ex)
            {
                SetStatus("Could not log in.", true);
                Ui.ShowError(ex);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false);
            }
        }
    }
}
