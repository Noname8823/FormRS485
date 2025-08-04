namespace projectRS485
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ComboBox comboBoxComPort;
        private System.Windows.Forms.ComboBox comboBoxBaudRate;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOut2;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.comboBoxComPort = new System.Windows.Forms.ComboBox();
            this.comboBoxBaudRate = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOut2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.btnOut1 = new System.Windows.Forms.Button();
            this.btnOut2 = new System.Windows.Forms.Button();
            this.btnOut4 = new System.Windows.Forms.Button();
            this.btnOut3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOut1 = new System.Windows.Forms.TextBox();
            this.txtOut4 = new System.Windows.Forms.TextBox();
            this.txtOut3 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtIn4 = new System.Windows.Forms.TextBox();
            this.txtIn3 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtIn2 = new System.Windows.Forms.TextBox();
            this.txtIn1 = new System.Windows.Forms.TextBox();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // comboBoxComPort
            // 
            this.comboBoxComPort.FormattingEnabled = true;
            this.comboBoxComPort.Location = new System.Drawing.Point(20, 30);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(100, 21);
            this.comboBoxComPort.TabIndex = 0;
            this.comboBoxComPort.SelectedIndexChanged += new System.EventHandler(this.comboBoxComPort_SelectedIndexChanged);
            // 
            // comboBoxBaudRate
            // 
            this.comboBoxBaudRate.FormattingEnabled = true;
            this.comboBoxBaudRate.Items.AddRange(new object[] {
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.comboBoxBaudRate.Location = new System.Drawing.Point(140, 30);
            this.comboBoxBaudRate.Name = "comboBoxBaudRate";
            this.comboBoxBaudRate.Size = new System.Drawing.Size(80, 21);
            this.comboBoxBaudRate.TabIndex = 1;
            this.comboBoxBaudRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxBaudRate_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(240, 30);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Làm mới";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(320, 30);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(90, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Kết nối";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(420, 35);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 20);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Chưa kết nối";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(20, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(200, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "Chọn cổng COM và tốc độ Baud:";
            // 
            // txtOut2
            // 
            this.txtOut2.Location = new System.Drawing.Point(169, 178);
            this.txtOut2.Name = "txtOut2";
            this.txtOut2.Size = new System.Drawing.Size(25, 20);
            this.txtOut2.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(48, 145);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(36, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "OUT1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(158, 145);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(36, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "OUT2";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(299, 145);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(36, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "OUT3";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(436, 145);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(36, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "OUT4";
            // 
            // btnOut1
            // 
            this.btnOut1.Location = new System.Drawing.Point(54, 101);
            this.btnOut1.Name = "btnOut1";
            this.btnOut1.Size = new System.Drawing.Size(30, 23);
            this.btnOut1.TabIndex = 21;
            this.btnOut1.Text = "1";
            this.btnOut1.UseVisualStyleBackColor = true;
           // this.btnOut1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnOut2
            // 
            this.btnOut2.Location = new System.Drawing.Point(161, 101);
            this.btnOut2.Name = "btnOut2";
            this.btnOut2.Size = new System.Drawing.Size(33, 23);
            this.btnOut2.TabIndex = 22;
            this.btnOut2.Text = "2";
            this.btnOut2.UseVisualStyleBackColor = true;
           // this.btnOut2.Click += new System.EventHandler(this.btnOut2_Click);
            // 
            // btnOut4
            // 
            this.btnOut4.Location = new System.Drawing.Point(439, 101);
            this.btnOut4.Name = "btnOut4";
            this.btnOut4.Size = new System.Drawing.Size(31, 23);
            this.btnOut4.TabIndex = 23;
            this.btnOut4.Text = "4";
            this.btnOut4.UseVisualStyleBackColor = true;
          //  this.btnOut4.Click += new System.EventHandler(this.btnOut4_Click);
            // 
            // btnOut3
            // 
            this.btnOut3.Location = new System.Drawing.Point(301, 101);
            this.btnOut3.Name = "btnOut3";
            this.btnOut3.Size = new System.Drawing.Size(34, 23);
            this.btnOut3.TabIndex = 24;
            this.btnOut3.Text = "3";
            this.btnOut3.UseVisualStyleBackColor = true;
           // this.btnOut3.Click += new System.EventHandler(this.btnOut3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(207, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Trạng thái OUTPUT";
            // 
            // txtOut1
            // 
            this.txtOut1.Location = new System.Drawing.Point(58, 178);
            this.txtOut1.Name = "txtOut1";
            this.txtOut1.Size = new System.Drawing.Size(26, 20);
            this.txtOut1.TabIndex = 29;
            // 
            // txtOut4
            // 
            this.txtOut4.Location = new System.Drawing.Point(444, 178);
            this.txtOut4.Name = "txtOut4";
            this.txtOut4.Size = new System.Drawing.Size(28, 20);
            this.txtOut4.TabIndex = 30;
            // 
            // txtOut3
            // 
            this.txtOut3.Location = new System.Drawing.Point(310, 178);
            this.txtOut3.Name = "txtOut3";
            this.txtOut3.Size = new System.Drawing.Size(25, 20);
            this.txtOut3.TabIndex = 31;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(369, 326);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "IN3";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(317, 267);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 21);
            this.label1.TabIndex = 8;
            this.label1.Text = "Trạng thái INPUT";
            // 
            // txtIn4
            // 
            this.txtIn4.Location = new System.Drawing.Point(446, 365);
            this.txtIn4.Name = "txtIn4";
            this.txtIn4.Size = new System.Drawing.Size(24, 20);
            this.txtIn4.TabIndex = 26;
            // 
            // txtIn3
            // 
            this.txtIn3.Location = new System.Drawing.Point(369, 365);
            this.txtIn3.Name = "txtIn3";
            this.txtIn3.Size = new System.Drawing.Size(24, 20);
            this.txtIn3.TabIndex = 27;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(446, 326);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "IN4";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(298, 326);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "IN2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(237, 326);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "IN1";
            // 
            // txtIn2
            // 
            this.txtIn2.Location = new System.Drawing.Point(298, 365);
            this.txtIn2.Name = "txtIn2";
            this.txtIn2.Size = new System.Drawing.Size(24, 20);
            this.txtIn2.TabIndex = 25;
            // 
            // txtIn1
            // 
            this.txtIn1.Location = new System.Drawing.Point(237, 365);
            this.txtIn1.Name = "txtIn1";
            this.txtIn1.Size = new System.Drawing.Size(24, 20);
            this.txtIn1.TabIndex = 10;
            // 
            // listBoxLog
            // 
            this.listBoxLog.Location = new System.Drawing.Point(2, 310);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(200, 108);
            this.listBoxLog.TabIndex = 7;
            this.listBoxLog.SelectedIndexChanged += new System.EventHandler(this.listBoxLog_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(540, 440);
            this.Controls.Add(this.txtOut3);
            this.Controls.Add(this.txtOut4);
            this.Controls.Add(this.txtOut1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtIn3);
            this.Controls.Add(this.txtIn4);
            this.Controls.Add(this.txtIn2);
            this.Controls.Add(this.btnOut3);
            this.Controls.Add(this.btnOut4);
            this.Controls.Add(this.btnOut2);
            this.Controls.Add(this.btnOut1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxComPort);
            this.Controls.Add(this.comboBoxBaudRate);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtIn1);
            this.Controls.Add(this.txtOut2);
            this.Controls.Add(this.label3);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RS485 WinForms Demo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnOut1;
        private System.Windows.Forms.Button btnOut2;
        private System.Windows.Forms.Button btnOut4;
        private System.Windows.Forms.Button btnOut3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOut1;
        private System.Windows.Forms.TextBox txtOut4;
        private System.Windows.Forms.TextBox txtOut3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIn4;
        private System.Windows.Forms.TextBox txtIn3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtIn2;
        private System.Windows.Forms.TextBox txtIn1;
        private System.Windows.Forms.ListBox listBoxLog;
    }
}