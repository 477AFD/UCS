using System.Drawing;
using System.Windows.Forms;

namespace FinalSystemUI.Dialogs
{
    /// <summary>A tiny pop-up that asks for one line of text.</summary>
    public static class InputBox
    {
        /// <summary>Returns the text, or null if the user pressed Cancel.</summary>
        public static string Ask(string title, string label, string defaultValue, bool password)
        {
            using (Form f = new Form())
            {
                f.Text = title;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ShowInTaskbar = false;
                f.ClientSize = new Size(380, 130);
                f.Font = new Font("Segoe UI", 9.5f);

                Label l = new Label();
                l.Text = label;
                l.AutoSize = false;
                l.Location = new Point(14, 14);
                l.Size = new Size(350, 22);

                TextBox t = new TextBox();
                t.Location = new Point(14, 40);
                t.Size = new Size(350, 26);
                t.Text = defaultValue ?? "";
                t.UseSystemPasswordChar = password;

                Button ok = new Button();
                ok.Text = "OK";
                ok.Location = new Point(194, 84);
                ok.Size = new Size(80, 30);
                ok.DialogResult = DialogResult.OK;

                Button cancel = new Button();
                cancel.Text = "Cancel";
                cancel.Location = new Point(284, 84);
                cancel.Size = new Size(80, 30);
                cancel.DialogResult = DialogResult.Cancel;

                f.Controls.AddRange(new Control[] { l, t, ok, cancel });
                if (password)
                {
                    CheckBox show = new CheckBox();
                    show.Text = "Show password";
                    show.AutoSize = true;
                    show.Location = new Point(14, 90);
                    show.CheckedChanged += delegate { t.UseSystemPasswordChar = !show.Checked; };
                    f.Controls.Add(show);
                }
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                return f.ShowDialog() == DialogResult.OK ? t.Text : null;
            }
        }
    }
}
