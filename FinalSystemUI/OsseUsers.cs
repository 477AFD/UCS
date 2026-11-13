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
    /// <summary>OSSE > User: manage student accounts and HSG accounts.</summary>
    public partial class OsseUsers : UserControl
    {
        private string _kind = "All";       // "All", "Student" or "HSG"
        private Button[] _filters;
        private Button btnReset;

        public OsseUsers()
        {
            InitializeComponent();
            if (Ui.InDesigner) return;

            Ui.SetUserLabels(label11, label12);

            Ui.StyleGrid(dataGridView1);
            Ui.SetColumns(dataGridView1, "Type", "ID number", "Name", "Email", "Status");
            Ui.SetColumnWeights(dataGridView1, 60, 80, 140, 180, 70);

            _filters = new Button[] { button1, button2, button3 };
            button1.Click += delegate { SetFilter("All"); };
            button2.Click += delegate { SetFilter("Student"); };
            button3.Click += delegate { SetFilter("HSG"); };
            button5.Click += delegate { AddUser(); };
            button4.Click += delegate { ToggleSelected(); };
            Ui.StylePrimary(button5);

            btnReset = Ui.MakeButton("Reset password", new Point(282, 748), new Size(140, 28), false);
            btnReset.Click += delegate { ResetPassword(); };
            Controls.Add(btnReset);

            SetFilter("All");
        }

        private void SetFilter(string kind)
        {
            _kind = kind;
            Ui.SetActiveFilter(kind == "All" ? button1 : (kind == "Student" ? button2 : button3), _filters);
            LoadData();
        }

        private void LoadData()
        {
            if (_filters == null) return;
            try
            {
                lblSem.Text = RecordService.SemesterName();

                UserCounts c = UserService.Counts();
                Ui.CardValue(gboxUnpaid, c.Students.ToString(), Color.Black, 18, 42);
                Ui.CardValue(groupBox2, c.Hsg.ToString(), Color.Black, 18, 42);
                Ui.CardValue(groupBox3, c.Active.ToString(), Ui.DarkGreen, 18, 42);
                Ui.CardValue(groupBox4, c.Disabled.ToString(), Ui.Red, 18, 42);

                dataGridView1.Rows.Clear();
                foreach (UserListItem u in UserService.List(_kind))
                {
                    Ui.AddRow(dataGridView1, u, 4, u.Kind, u.Number, u.FullName, u.Email, u.IsActive ? "Active" : "Disabled");
                }
                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private UserListItem Selected()
        {
            UserListItem u = Ui.SelectedTag<UserListItem>(dataGridView1);
            if (u == null) Ui.Warn("Please select an account first.");
            return u;
        }

        private void AddUser()
        {
            try
            {
                using (UserEditorForm dlg = new UserEditorForm())
                {
                    if (dlg.ShowDialog(this.FindForm()) != DialogResult.OK) return;

                    string who = dlg.Kind == "Student" ? "student" : "HSG";
                    string used;
                    if (dlg.Kind == "Student")
                        used = UserService.AddStudent(dlg.Number, dlg.FullName, dlg.Email, dlg.Password);
                    else
                        used = UserService.AddHsg(dlg.FullName, dlg.Email, dlg.IsStudentFlag, dlg.Password);

                    if (string.IsNullOrEmpty(dlg.Password))
                        Ui.Info("The " + who + " account was created.\n\nTemporary password: " + used +
                                "\n\nGive it to " + dlg.FullName + ". It is not shown again.");
                    else
                        Ui.Info("The " + who + " account was created.");
                }
                LoadData();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void ToggleSelected()
        {
            UserListItem u = Selected();
            if (u == null) return;

            string action = u.IsActive ? "disable" : "enable";
            if (!Ui.Confirm("Do you want to " + action + " the account of " + u.FullName + "?")) return;
            try
            {
                UserService.SetActive(u.Kind, u.Id, !u.IsActive);
                LoadData();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            UserListItem u = Selected();
            if (u == null) return;

            if (!Ui.Confirm("Delete the account of " + u.FullName + " (" + u.Kind + ")?\n\nThis cannot be undone. " +
                            "An account that already has records cannot be deleted. Disable it instead."))
                return;
            try
            {
                UserService.DeleteUser(u.Kind, u.Id);
                LoadData();
            }
            catch (Exception ex)
            {
                Ui.ShowError(ex);
            }
        }

        private void ResetPassword()
        {
            UserListItem u = Selected();
            if (u == null) return;

            string text = InputBox.Ask("Reset password", "New password for " + u.FullName + " (at least 8 characters)", "", true);
            if (text == null) return;
            try
            {
                UserService.ResetPassword(u.Kind, u.Id, text);
                Ui.Info("The password was changed.");
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

        private void button6_Click(object sender, EventArgs e)
        {
            (this.FindForm() as MainForm)?.ShowScreen(new OsseClearance1());
        }

        private void btnUser_Click(object sender, EventArgs e)
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
    }
}
