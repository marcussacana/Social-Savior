namespace Social_Savior {
    partial class Update {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.iTalk_ThemeContainer1 = new MangaUnhost.iTalk_ThemeContainer();
            this.lblStatus = new System.Windows.Forms.Label();
            this.iTalk_ProgressIndicator1 = new MangaUnhost.iTalk_ProgressIndicator();
            this.label2 = new System.Windows.Forms.Label();
            this.iTalk_ThemeContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // iTalk_ThemeContainer1
            // 
            this.iTalk_ThemeContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.iTalk_ThemeContainer1.Controls.Add(this.label2);
            this.iTalk_ThemeContainer1.Controls.Add(this.iTalk_ProgressIndicator1);
            this.iTalk_ThemeContainer1.Controls.Add(this.lblStatus);
            this.iTalk_ThemeContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iTalk_ThemeContainer1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.iTalk_ThemeContainer1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.iTalk_ThemeContainer1.Location = new System.Drawing.Point(0, 0);
            this.iTalk_ThemeContainer1.Name = "iTalk_ThemeContainer1";
            this.iTalk_ThemeContainer1.Padding = new System.Windows.Forms.Padding(3, 28, 3, 28);
            this.iTalk_ThemeContainer1.Sizable = true;
            this.iTalk_ThemeContainer1.Size = new System.Drawing.Size(284, 204);
            this.iTalk_ThemeContainer1.SmartBounds = false;
            this.iTalk_ThemeContainer1.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.iTalk_ThemeContainer1.TabIndex = 0;
            this.iTalk_ThemeContainer1.Text = "Social Savior";
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.lblStatus.Location = new System.Drawing.Point(3, 28);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(278, 32);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Searching Updates...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // iTalk_ProgressIndicator1
            // 
            this.iTalk_ProgressIndicator1.Location = new System.Drawing.Point(96, 63);
            this.iTalk_ProgressIndicator1.MinimumSize = new System.Drawing.Size(80, 80);
            this.iTalk_ProgressIndicator1.Name = "iTalk_ProgressIndicator1";
            this.iTalk_ProgressIndicator1.P_AnimationColor = System.Drawing.Color.DimGray;
            this.iTalk_ProgressIndicator1.P_AnimationSpeed = 100;
            this.iTalk_ProgressIndicator1.P_BaseColor = System.Drawing.Color.DarkGray;
            this.iTalk_ProgressIndicator1.Size = new System.Drawing.Size(80, 80);
            this.iTalk_ProgressIndicator1.TabIndex = 1;
            this.iTalk_ProgressIndicator1.Text = "iTalk_ProgressIndicator1";
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.label2.Location = new System.Drawing.Point(3, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(278, 32);
            this.label2.TabIndex = 2;
            this.label2.Text = "Please Wait...";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Update
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 204);
            this.Controls.Add(this.iTalk_ThemeContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Update";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Social Savior";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.iTalk_ThemeContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MangaUnhost.iTalk_ThemeContainer iTalk_ThemeContainer1;
        private MangaUnhost.iTalk_ProgressIndicator iTalk_ProgressIndicator1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label2;
    }
}