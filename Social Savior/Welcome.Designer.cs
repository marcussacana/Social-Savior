namespace Social_Savior {
    partial class Welcome {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent() {
            this.ThemeContainer = new MangaUnhost.iTalk_ThemeContainer();
            this.iTalk_ControlBox1 = new MangaUnhost.iTalk_ControlBox();
            this.iTalk_Label2 = new MangaUnhost.iTalk_Label();
            this.iTalk_Label1 = new MangaUnhost.iTalk_Label();
            this.PassTB = new MangaUnhost.iTalk_TextBox_Big();
            this.EmailTB = new MangaUnhost.iTalk_TextBox_Big();
            this.LoginBNT = new MangaUnhost.iTalk_Button_2();
            this.ThemeContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // ThemeContainer
            // 
            this.ThemeContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.ThemeContainer.Controls.Add(this.iTalk_ControlBox1);
            this.ThemeContainer.Controls.Add(this.iTalk_Label2);
            this.ThemeContainer.Controls.Add(this.iTalk_Label1);
            this.ThemeContainer.Controls.Add(this.PassTB);
            this.ThemeContainer.Controls.Add(this.EmailTB);
            this.ThemeContainer.Controls.Add(this.LoginBNT);
            this.ThemeContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ThemeContainer.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.ThemeContainer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.ThemeContainer.Location = new System.Drawing.Point(0, 0);
            this.ThemeContainer.Name = "ThemeContainer";
            this.ThemeContainer.Padding = new System.Windows.Forms.Padding(3, 28, 3, 28);
            this.ThemeContainer.Sizable = false;
            this.ThemeContainer.Size = new System.Drawing.Size(509, 276);
            this.ThemeContainer.SmartBounds = false;
            this.ThemeContainer.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ThemeContainer.TabIndex = 0;
            this.ThemeContainer.Text = "Welcome";
            // 
            // iTalk_ControlBox1
            // 
            this.iTalk_ControlBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iTalk_ControlBox1.BackColor = System.Drawing.Color.Transparent;
            this.iTalk_ControlBox1.Location = new System.Drawing.Point(428, -1);
            this.iTalk_ControlBox1.Name = "iTalk_ControlBox1";
            this.iTalk_ControlBox1.Size = new System.Drawing.Size(77, 19);
            this.iTalk_ControlBox1.TabIndex = 5;
            this.iTalk_ControlBox1.Text = "iTalk_ControlBox1";
            // 
            // iTalk_Label2
            // 
            this.iTalk_Label2.BackColor = System.Drawing.Color.Transparent;
            this.iTalk_Label2.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.iTalk_Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.iTalk_Label2.Location = new System.Drawing.Point(9, 76);
            this.iTalk_Label2.Name = "iTalk_Label2";
            this.iTalk_Label2.Size = new System.Drawing.Size(491, 23);
            this.iTalk_Label2.TabIndex = 4;
            this.iTalk_Label2.Text = "The Ultimate Social Picture Backup";
            this.iTalk_Label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // iTalk_Label1
            // 
            this.iTalk_Label1.BackColor = System.Drawing.Color.Transparent;
            this.iTalk_Label1.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.iTalk_Label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.iTalk_Label1.Location = new System.Drawing.Point(12, 28);
            this.iTalk_Label1.Name = "iTalk_Label1";
            this.iTalk_Label1.Size = new System.Drawing.Size(485, 48);
            this.iTalk_Label1.TabIndex = 3;
            this.iTalk_Label1.Text = "Social Savior\r\n";
            this.iTalk_Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // PassTB
            // 
            this.PassTB.BackColor = System.Drawing.Color.Transparent;
            this.PassTB.Font = new System.Drawing.Font("Tahoma", 11F);
            this.PassTB.ForeColor = System.Drawing.Color.DimGray;
            this.PassTB.Image = null;
            this.PassTB.Location = new System.Drawing.Point(12, 149);
            this.PassTB.MaxLength = 32767;
            this.PassTB.Multiline = false;
            this.PassTB.Name = "PassTB";
            this.PassTB.ReadOnly = false;
            this.PassTB.Size = new System.Drawing.Size(485, 41);
            this.PassTB.TabIndex = 2;
            this.PassTB.Text = "Sample123";
            this.PassTB.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.PassTB.UseSystemPasswordChar = true;
            this.PassTB.TextChanged += new System.EventHandler(this.PassChanged);
            this.PassTB.Enter += new System.EventHandler(this.OnPassFocused);
            this.PassTB.Leave += new System.EventHandler(this.OnPassDefocused);
            // 
            // EmailTB
            // 
            this.EmailTB.BackColor = System.Drawing.Color.Transparent;
            this.EmailTB.Font = new System.Drawing.Font("Tahoma", 11F);
            this.EmailTB.ForeColor = System.Drawing.Color.DimGray;
            this.EmailTB.Image = null;
            this.EmailTB.Location = new System.Drawing.Point(12, 102);
            this.EmailTB.MaxLength = 32767;
            this.EmailTB.Multiline = false;
            this.EmailTB.Name = "EmailTB";
            this.EmailTB.ReadOnly = false;
            this.EmailTB.Size = new System.Drawing.Size(485, 41);
            this.EmailTB.TabIndex = 1;
            this.EmailTB.Text = "Your.Email@Sample.com";
            this.EmailTB.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            this.EmailTB.UseSystemPasswordChar = false;
            this.EmailTB.TextChanged += new System.EventHandler(this.EmailChanged);
            this.EmailTB.Enter += new System.EventHandler(this.OnLoginFocused);
            this.EmailTB.Leave += new System.EventHandler(this.OnLoginDefocused);
            // 
            // LoginBNT
            // 
            this.LoginBNT.BackColor = System.Drawing.Color.Transparent;
            this.LoginBNT.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.LoginBNT.ForeColor = System.Drawing.Color.White;
            this.LoginBNT.Image = null;
            this.LoginBNT.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoginBNT.Location = new System.Drawing.Point(12, 196);
            this.LoginBNT.Name = "LoginBNT";
            this.LoginBNT.Size = new System.Drawing.Size(488, 40);
            this.LoginBNT.TabIndex = 0;
            this.LoginBNT.Text = "Login with Facebook";
            this.LoginBNT.TextAlignment = System.Drawing.StringAlignment.Center;
            this.LoginBNT.Click += new System.EventHandler(this.LoginClicked);
            // 
            // Welcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 276);
            this.Controls.Add(this.ThemeContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Welcome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.ThemeContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MangaUnhost.iTalk_ThemeContainer ThemeContainer;
        private MangaUnhost.iTalk_Label iTalk_Label2;
        private MangaUnhost.iTalk_Label iTalk_Label1;
        private MangaUnhost.iTalk_TextBox_Big PassTB;
        private MangaUnhost.iTalk_TextBox_Big EmailTB;
        private MangaUnhost.iTalk_Button_2 LoginBNT;
        private MangaUnhost.iTalk_ControlBox iTalk_ControlBox1;
    }
}

