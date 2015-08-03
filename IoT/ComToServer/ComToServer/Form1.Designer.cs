namespace ComToServer
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.FtpConnect = new System.Windows.Forms.Button();
            this.FTPLogin = new System.Windows.Forms.TextBox();
            this.FTPPass = new System.Windows.Forms.TextBox();
            this.ComConnect = new System.Windows.Forms.Button();
            this.COMPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.FTPCon = new System.Windows.Forms.Label();
            this.COMCon = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label6 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queueLengthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FtpConnect
            // 
            this.FtpConnect.Location = new System.Drawing.Point(488, 258);
            this.FtpConnect.Name = "FtpConnect";
            this.FtpConnect.Size = new System.Drawing.Size(99, 27);
            this.FtpConnect.TabIndex = 0;
            this.FtpConnect.Text = "Connect";
            this.FtpConnect.UseVisualStyleBackColor = true;
            this.FtpConnect.Click += new System.EventHandler(this.FtpConnect_Click);
            // 
            // FTPLogin
            // 
            this.FTPLogin.Location = new System.Drawing.Point(473, 135);
            this.FTPLogin.Name = "FTPLogin";
            this.FTPLogin.Size = new System.Drawing.Size(132, 21);
            this.FTPLogin.TabIndex = 1;
            // 
            // FTPPass
            // 
            this.FTPPass.Location = new System.Drawing.Point(473, 191);
            this.FTPPass.Name = "FTPPass";
            this.FTPPass.Size = new System.Drawing.Size(132, 21);
            this.FTPPass.TabIndex = 2;
            // 
            // ComConnect
            // 
            this.ComConnect.Enabled = false;
            this.ComConnect.Location = new System.Drawing.Point(42, 270);
            this.ComConnect.Name = "ComConnect";
            this.ComConnect.Size = new System.Drawing.Size(99, 27);
            this.ComConnect.TabIndex = 3;
            this.ComConnect.Text = "Connect";
            this.ComConnect.UseVisualStyleBackColor = true;
            this.ComConnect.Click += new System.EventHandler(this.ComConnect_Click);
            // 
            // COMPort
            // 
            this.COMPort.Location = new System.Drawing.Point(30, 157);
            this.COMPort.Name = "COMPort";
            this.COMPort.Size = new System.Drawing.Size(132, 21);
            this.COMPort.TabIndex = 4;
            this.COMPort.Text = "COM1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(25, 118);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Port number";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(470, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Login";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(470, 168);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(512, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "FTP";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(73, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 17);
            this.label5.TabIndex = 9;
            this.label5.Text = "COM";
            // 
            // FTPCon
            // 
            this.FTPCon.AutoSize = true;
            this.FTPCon.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FTPCon.Location = new System.Drawing.Point(485, 328);
            this.FTPCon.Name = "FTPCon";
            this.FTPCon.Size = new System.Drawing.Size(106, 17);
            this.FTPCon.TabIndex = 10;
            this.FTPCon.Text = "Disconnected";
            // 
            // COMCon
            // 
            this.COMCon.AutoSize = true;
            this.COMCon.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.COMCon.Location = new System.Drawing.Point(39, 325);
            this.COMCon.Name = "COMCon";
            this.COMCon.Size = new System.Drawing.Size(106, 17);
            this.COMCon.TabIndex = 11;
            this.COMCon.Text = "Disconnected";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(214, 160);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(193, 23);
            this.progressBar1.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(283, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 17);
            this.label6.TabIndex = 14;
            this.label6.Text = "Queue";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(632, 24);
            this.menuStrip1.TabIndex = 15;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.queueLengthToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // queueLengthToolStripMenuItem
            // 
            this.queueLengthToolStripMenuItem.Name = "queueLengthToolStripMenuItem";
            this.queueLengthToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.queueLengthToolStripMenuItem.Text = "Queue length";
            this.queueLengthToolStripMenuItem.Click += new System.EventHandler(this.queueLengthToolStripMenuItem_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(30, 191);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(132, 21);
            this.textBox1.TabIndex = 16;
            this.textBox1.Text = "19200";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.comboBox1.Items.AddRange(new object[] {
            "Disable",
            "Hardware"});
            this.comboBox1.Location = new System.Drawing.Point(30, 219);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(132, 23);
            this.comboBox1.TabIndex = 17;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 369);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.COMCon);
            this.Controls.Add(this.FTPCon);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.COMPort);
            this.Controls.Add(this.ComConnect);
            this.Controls.Add(this.FTPPass);
            this.Controls.Add(this.FTPLogin);
            this.Controls.Add(this.FtpConnect);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "ComToServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button FtpConnect;
        private System.Windows.Forms.TextBox FTPLogin;
        private System.Windows.Forms.TextBox FTPPass;
        private System.Windows.Forms.Button ComConnect;
        private System.Windows.Forms.TextBox COMPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label FTPCon;
        private System.Windows.Forms.Label COMCon;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem queueLengthToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

