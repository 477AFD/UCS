using System.Drawing;
using System.Windows.Forms;

namespace FinalSystemUI.Dialogs
{
    /// <summary>"Setting up the database..." window shown while the program starts.</summary>
    public class SplashForm : Form
    {
        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 110);
            BackColor = Color.Green;
            ShowInTaskbar = false;

            Label title = new Label();
            title.Text = "Uniform Compliance Monitoring System";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            title.AutoSize = false;
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.SetBounds(0, 22, 420, 30);

            Label sub = new Label();
            sub.Text = "Connecting to the database. The first start can take a little longer...";
            sub.ForeColor = Color.FromArgb(232, 212, 138);
            sub.AutoSize = false;
            sub.TextAlign = ContentAlignment.MiddleCenter;
            sub.SetBounds(0, 62, 420, 24);

            Controls.Add(title);
            Controls.Add(sub);
        }
    }
}
