namespace FinalSystemUI
{
    partial class ViolationHistory
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
            this.label3 = new System.Windows.Forms.Label();
            this.dgvViolationHis = new System.Windows.Forms.DataGridView();
            this.label4 = new System.Windows.Forms.Label();
            this.gBoxCurrentStand = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSem = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnAll = new System.Windows.Forms.Button();
            this.btnLanyard = new System.Windows.Forms.Button();
            this.btnNounif = new System.Windows.Forms.Button();
            this.btnNoid = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvViolationHis)).BeginInit();
            this.gBoxCurrentStand.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.ForestGreen;
            this.label3.Location = new System.Drawing.Point(274, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(290, 47);
            this.label3.TabIndex = 14;
            this.label3.Text = "VIOLATION HISTORY";
            // 
            // dgvViolationHis
            // 
            this.dgvViolationHis.AllowUserToAddRows = false;
            this.dgvViolationHis.AllowUserToDeleteRows = false;
            this.dgvViolationHis.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dgvViolationHis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvViolationHis.Location = new System.Drawing.Point(270, 220);
            this.dgvViolationHis.Name = "dgvViolationHis";
            this.dgvViolationHis.ReadOnly = true;
            this.dgvViolationHis.RowHeadersWidth = 51;
            this.dgvViolationHis.RowTemplate.Height = 24;
            this.dgvViolationHis.Size = new System.Drawing.Size(541, 312);
            this.dgvViolationHis.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(277, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(323, 21);
            this.label4.TabIndex = 16;
            this.label4.Text = "Dates when you violated uniform and ID-Lanyard compliance";
            // 
            // gBoxCurrentStand
            // 
            this.gBoxCurrentStand.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gBoxCurrentStand.Controls.Add(this.label7);
            this.gBoxCurrentStand.Location = new System.Drawing.Point(883, 211);
            this.gBoxCurrentStand.Name = "gBoxCurrentStand";
            this.gBoxCurrentStand.Size = new System.Drawing.Size(280, 321);
            this.gBoxCurrentStand.TabIndex = 17;
            this.gBoxCurrentStand.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Green;
            this.label7.Location = new System.Drawing.Point(20, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 15);
            this.label7.TabIndex = 1;
            this.label7.Text = "CURRENT STANDING";
            // 
            // lblSem
            // 
            this.lblSem.BackColor = System.Drawing.Color.Transparent;
            this.lblSem.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblSem.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSem.ForeColor = System.Drawing.Color.Green;
            this.lblSem.Location = new System.Drawing.Point(986, 0);
            this.lblSem.Name = "lblSem";
            this.lblSem.Size = new System.Drawing.Size(236, 27);
            this.lblSem.TabIndex = 18;
            this.lblSem.Text = "1st Semester, 2026-2027";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label5.Location = new System.Drawing.Point(277, 199);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 17);
            this.label5.TabIndex = 19;
            this.label5.Text = "TIMELINE";
            // 
            // btnAll
            // 
            this.btnAll.BackColor = System.Drawing.Color.Green;
            this.btnAll.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAll.ForeColor = System.Drawing.SystemColors.Control;
            this.btnAll.Location = new System.Drawing.Point(442, 184);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(75, 31);
            this.btnAll.TabIndex = 20;
            this.btnAll.Text = "All";
            this.btnAll.UseVisualStyleBackColor = false;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnLanyard
            // 
            this.btnLanyard.BackColor = System.Drawing.Color.Green;
            this.btnLanyard.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLanyard.ForeColor = System.Drawing.SystemColors.Control;
            this.btnLanyard.Location = new System.Drawing.Point(633, 185);
            this.btnLanyard.Name = "btnLanyard";
            this.btnLanyard.Size = new System.Drawing.Size(81, 31);
            this.btnLanyard.TabIndex = 21;
            this.btnLanyard.Text = "Lanyard";
            this.btnLanyard.UseVisualStyleBackColor = false;
            // 
            // btnNounif
            // 
            this.btnNounif.BackColor = System.Drawing.Color.Green;
            this.btnNounif.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNounif.ForeColor = System.Drawing.SystemColors.Control;
            this.btnNounif.Location = new System.Drawing.Point(532, 185);
            this.btnNounif.Name = "btnNounif";
            this.btnNounif.Size = new System.Drawing.Size(91, 31);
            this.btnNounif.TabIndex = 22;
            this.btnNounif.Text = "No Uniform";
            this.btnNounif.UseVisualStyleBackColor = false;
            // 
            // btnNoid
            // 
            this.btnNoid.BackColor = System.Drawing.Color.Green;
            this.btnNoid.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNoid.ForeColor = System.Drawing.SystemColors.Control;
            this.btnNoid.Location = new System.Drawing.Point(724, 185);
            this.btnNoid.Name = "btnNoid";
            this.btnNoid.Size = new System.Drawing.Size(75, 31);
            this.btnNoid.TabIndex = 23;
            this.btnNoid.Text = "No-ID";
            this.btnNoid.UseVisualStyleBackColor = false;
            // 
            // ViolationHistory
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.btnNoid);
            this.Controls.Add(this.btnNounif);
            this.Controls.Add(this.btnLanyard);
            this.Controls.Add(this.btnAll);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblSem);
            this.Controls.Add(this.gBoxCurrentStand);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgvViolationHis);
            this.Controls.Add(this.label3);
            this.Name = "ViolationHistory";
            this.Size = new System.Drawing.Size(1225, 785);
            ((System.ComponentModel.ISupportInitialize)(this.dgvViolationHis)).EndInit();
            this.gBoxCurrentStand.ResumeLayout(false);
            this.gBoxCurrentStand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgvViolationHis;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox gBoxCurrentStand;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblSem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.Button btnLanyard;
        private System.Windows.Forms.Button btnNounif;
        private System.Windows.Forms.Button btnNoid;
    }
}
