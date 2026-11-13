namespace FinalSystemUI
{
    partial class DashboardControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dgvDashboard = new System.Windows.Forms.DataGridView();
            this.lblSem = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.gBoxStanding = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.gBoxFines = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.gBoxViolation = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDashboard)).BeginInit();
            this.gBoxStanding.SuspendLayout();
            this.gBoxFines.SuspendLayout();
            this.gBoxViolation.SuspendLayout();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Gray;
            this.label6.Location = new System.Drawing.Point(288, 751);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(539, 21);
            this.label6.TabIndex = 23;
            this.label6.Text = "Lanyard and No-ID records aren\'t fined. They are settled with OSSE during Clearan" +
    "ce ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Green;
            this.label5.Location = new System.Drawing.Point(288, 425);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(124, 15);
            this.label5.TabIndex = 17;
            this.label5.Text = "VIOLATION HISTORY";
            // 
            // dgvDashboard
            // 
            this.dgvDashboard.AllowUserToAddRows = false;
            this.dgvDashboard.AllowUserToDeleteRows = false;
            this.dgvDashboard.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dgvDashboard.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDashboard.Location = new System.Drawing.Point(288, 443);
            this.dgvDashboard.Name = "dgvDashboard";
            this.dgvDashboard.ReadOnly = true;
            this.dgvDashboard.RowHeadersWidth = 51;
            this.dgvDashboard.RowTemplate.Height = 24;
            this.dgvDashboard.Size = new System.Drawing.Size(616, 304);
            this.dgvDashboard.TabIndex = 25;
            // 
            // lblSem
            // 
            this.lblSem.BackColor = System.Drawing.Color.Transparent;
            this.lblSem.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblSem.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSem.ForeColor = System.Drawing.Color.Green;
            this.lblSem.Location = new System.Drawing.Point(984, 1);
            this.lblSem.Name = "lblSem";
            this.lblSem.Size = new System.Drawing.Size(236, 27);
            this.lblSem.TabIndex = 24;
            this.lblSem.Text = "1st Semester, 2026-2027";
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(285, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(317, 21);
            this.label4.TabIndex = 22;
            this.label4.Text = "Your Violations, Fines and Standing at a glace";
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.ForestGreen;
            this.label3.Location = new System.Drawing.Point(282, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(249, 47);
            this.label3.TabIndex = 21;
            this.label3.Text = "HELLO, JUAN";
            // 
            // gBoxStanding
            // 
            this.gBoxStanding.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gBoxStanding.Controls.Add(this.label9);
            this.gBoxStanding.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gBoxStanding.Location = new System.Drawing.Point(917, 197);
            this.gBoxStanding.Name = "gBoxStanding";
            this.gBoxStanding.Size = new System.Drawing.Size(266, 148);
            this.gBoxStanding.TabIndex = 20;
            this.gBoxStanding.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.OrangeRed;
            this.label9.Location = new System.Drawing.Point(8, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "STANDING";
            // 
            // gBoxFines
            // 
            this.gBoxFines.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gBoxFines.Controls.Add(this.label8);
            this.gBoxFines.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gBoxFines.Location = new System.Drawing.Point(594, 197);
            this.gBoxFines.Name = "gBoxFines";
            this.gBoxFines.Size = new System.Drawing.Size(266, 148);
            this.gBoxFines.TabIndex = 19;
            this.gBoxFines.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Green;
            this.label8.Location = new System.Drawing.Point(11, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 15);
            this.label8.TabIndex = 1;
            this.label8.Text = "MY FINES";
            // 
            // gBoxViolation
            // 
            this.gBoxViolation.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gBoxViolation.Controls.Add(this.label7);
            this.gBoxViolation.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gBoxViolation.Location = new System.Drawing.Point(282, 197);
            this.gBoxViolation.Name = "gBoxViolation";
            this.gBoxViolation.Size = new System.Drawing.Size(266, 148);
            this.gBoxViolation.TabIndex = 18;
            this.gBoxViolation.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Red;
            this.label7.Location = new System.Drawing.Point(26, 21);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "VIOLATIONS";
            // 
            // DashboardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dgvDashboard);
            this.Controls.Add(this.lblSem);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.gBoxStanding);
            this.Controls.Add(this.gBoxFines);
            this.Controls.Add(this.gBoxViolation);
            this.Name = "DashboardControl";
            this.Size = new System.Drawing.Size(1225, 785);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDashboard)).EndInit();
            this.gBoxStanding.ResumeLayout(false);
            this.gBoxStanding.PerformLayout();
            this.gBoxFines.ResumeLayout(false);
            this.gBoxFines.PerformLayout();
            this.gBoxViolation.ResumeLayout(false);
            this.gBoxViolation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgvDashboard;
        private System.Windows.Forms.Label lblSem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gBoxStanding;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox gBoxFines;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox gBoxViolation;
        private System.Windows.Forms.Label label7;
    }
}
