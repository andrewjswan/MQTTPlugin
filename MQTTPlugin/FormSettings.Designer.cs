namespace MQTTPlugin
{
    partial class FormSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
      this.ok_Button = new System.Windows.Forms.Button();
      this.cancel_Button = new System.Windows.Forms.Button();
      this.checkBox3 = new System.Windows.Forms.CheckBox();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.comboBox2 = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.textBox3 = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.button2 = new System.Windows.Forms.Button();
      this.textBox4 = new System.Windows.Forms.TextBox();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.label4 = new System.Windows.Forms.Label();
      this.password_textBox = new System.Windows.Forms.TextBox();
      this.label13 = new System.Windows.Forms.Label();
      this.user_textBox = new System.Windows.Forms.TextBox();
      this.label12 = new System.Windows.Forms.Label();
      this.port_textBox = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this.host_textBox = new System.Windows.Forms.TextBox();
      this.groupBox6 = new System.Windows.Forms.GroupBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.mediaDuration_textBox = new System.Windows.Forms.TextBox();
      this.button3 = new System.Windows.Forms.Button();
      this.WindowChange_checkBox = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.debug_checkBox = new System.Windows.Forms.CheckBox();
      this.groupBox5.SuspendLayout();
      this.groupBox6.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // ok_Button
      // 
      this.ok_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.ok_Button.Location = new System.Drawing.Point(139, 331);
      this.ok_Button.Name = "ok_Button";
      this.ok_Button.Size = new System.Drawing.Size(75, 23);
      this.ok_Button.TabIndex = 2;
      this.ok_Button.Text = "Save";
      this.ok_Button.UseVisualStyleBackColor = true;
      this.ok_Button.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // cancel_Button
      // 
      this.cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancel_Button.Location = new System.Drawing.Point(220, 331);
      this.cancel_Button.Name = "cancel_Button";
      this.cancel_Button.Size = new System.Drawing.Size(75, 23);
      this.cancel_Button.TabIndex = 2;
      this.cancel_Button.Text = "Cancel";
      this.cancel_Button.UseVisualStyleBackColor = true;
      // 
      // checkBox3
      // 
      this.checkBox3.AutoSize = true;
      this.checkBox3.Location = new System.Drawing.Point(9, 46);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(121, 17);
      this.checkBox3.TabIndex = 28;
      this.checkBox3.Text = "Last part as payload";
      this.checkBox3.UseVisualStyleBackColor = true;
      // 
      // textBox2
      // 
      this.textBox2.Enabled = false;
      this.textBox2.Location = new System.Drawing.Point(8, 19);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(80, 20);
      this.textBox2.TabIndex = 25;
      this.textBox2.Text = "MediaPortal";
      // 
      // comboBox1
      // 
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Items.AddRange(new object[] {
            "Type",
            "Status"});
      this.comboBox1.Location = new System.Drawing.Point(190, 19);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(80, 21);
      this.comboBox1.TabIndex = 24;
      // 
      // comboBox2
      // 
      this.comboBox2.DisplayMember = "1";
      this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox2.FormattingEnabled = true;
      this.comboBox2.Items.AddRange(new object[] {
            "Type",
            "Status"});
      this.comboBox2.Location = new System.Drawing.Point(99, 19);
      this.comboBox2.Name = "comboBox2";
      this.comboBox2.Size = new System.Drawing.Size(80, 21);
      this.comboBox2.TabIndex = 23;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(88, 25);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(12, 15);
      this.label1.TabIndex = 26;
      this.label1.Text = "•";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.Location = new System.Drawing.Point(179, 25);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(12, 15);
      this.label5.TabIndex = 27;
      this.label5.Text = "•";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(6, 25);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(219, 13);
      this.label6.TabIndex = 21;
      this.label6.Text = "Select a path to your EventGhost installation:";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(243, 42);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(27, 20);
      this.button1.TabIndex = 19;
      this.button1.Text = "...";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // textBox3
      // 
      this.textBox3.Location = new System.Drawing.Point(9, 42);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(232, 20);
      this.textBox3.TabIndex = 20;
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(6, 25);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(219, 13);
      this.label7.TabIndex = 21;
      this.label7.Text = "Select a path to your EventGhost installation:";
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(243, 42);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(27, 20);
      this.button2.TabIndex = 19;
      this.button2.Text = "...";
      this.button2.UseVisualStyleBackColor = true;
      // 
      // textBox4
      // 
      this.textBox4.Location = new System.Drawing.Point(9, 42);
      this.textBox4.Name = "textBox4";
      this.textBox4.Size = new System.Drawing.Size(232, 20);
      this.textBox4.TabIndex = 20;
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.Location = new System.Drawing.Point(6, 19);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(121, 17);
      this.checkBox2.TabIndex = 28;
      this.checkBox2.Text = "Last part as payload";
      this.checkBox2.UseVisualStyleBackColor = true;
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.label4);
      this.groupBox5.Controls.Add(this.password_textBox);
      this.groupBox5.Controls.Add(this.label13);
      this.groupBox5.Controls.Add(this.user_textBox);
      this.groupBox5.Controls.Add(this.label12);
      this.groupBox5.Controls.Add(this.port_textBox);
      this.groupBox5.Controls.Add(this.label11);
      this.groupBox5.Controls.Add(this.host_textBox);
      this.groupBox5.Location = new System.Drawing.Point(12, 12);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(283, 146);
      this.groupBox5.TabIndex = 32;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "MQTT Broker:";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 112);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(53, 13);
      this.label4.TabIndex = 9;
      this.label4.Text = "Password";
      // 
      // password_textBox
      // 
      this.password_textBox.Location = new System.Drawing.Point(65, 109);
      this.password_textBox.Name = "password_textBox";
      this.password_textBox.PasswordChar = '*';
      this.password_textBox.Size = new System.Drawing.Size(203, 20);
      this.password_textBox.TabIndex = 8;
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(6, 86);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(29, 13);
      this.label13.TabIndex = 7;
      this.label13.Text = "User";
      // 
      // user_textBox
      // 
      this.user_textBox.Location = new System.Drawing.Point(65, 83);
      this.user_textBox.Name = "user_textBox";
      this.user_textBox.Size = new System.Drawing.Size(203, 20);
      this.user_textBox.TabIndex = 6;
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(6, 51);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(26, 13);
      this.label12.TabIndex = 5;
      this.label12.Text = "Port";
      // 
      // port_textBox
      // 
      this.port_textBox.Location = new System.Drawing.Point(41, 47);
      this.port_textBox.Name = "port_textBox";
      this.port_textBox.Size = new System.Drawing.Size(69, 20);
      this.port_textBox.TabIndex = 4;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(6, 25);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(29, 13);
      this.label11.TabIndex = 3;
      this.label11.Text = "Host";
      // 
      // host_textBox
      // 
      this.host_textBox.Location = new System.Drawing.Point(41, 21);
      this.host_textBox.Name = "host_textBox";
      this.host_textBox.Size = new System.Drawing.Size(227, 20);
      this.host_textBox.TabIndex = 2;
      // 
      // groupBox6
      // 
      this.groupBox6.Controls.Add(this.label8);
      this.groupBox6.Controls.Add(this.label14);
      this.groupBox6.Controls.Add(this.mediaDuration_textBox);
      this.groupBox6.Location = new System.Drawing.Point(12, 217);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(283, 49);
      this.groupBox6.TabIndex = 31;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Short video length";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(6, 19);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(141, 13);
      this.label8.TabIndex = 33;
      this.label8.Text = "A short video is shorter than:";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(225, 20);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(43, 13);
      this.label14.TabIndex = 8;
      this.label14.Text = "minutes";
      // 
      // mediaDuration_textBox
      // 
      this.mediaDuration_textBox.Location = new System.Drawing.Point(190, 16);
      this.mediaDuration_textBox.Name = "mediaDuration_textBox";
      this.mediaDuration_textBox.Size = new System.Drawing.Size(29, 20);
      this.mediaDuration_textBox.TabIndex = 8;
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(12, 331);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(121, 23);
      this.button3.TabIndex = 33;
      this.button3.Text = "Show Inputmapping";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // WindowChange_checkBox
      // 
      this.WindowChange_checkBox.AutoSize = true;
      this.WindowChange_checkBox.Location = new System.Drawing.Point(9, 19);
      this.WindowChange_checkBox.Name = "WindowChange_checkBox";
      this.WindowChange_checkBox.Size = new System.Drawing.Size(174, 17);
      this.WindowChange_checkBox.TabIndex = 29;
      this.WindowChange_checkBox.Text = "Send event on window change";
      this.WindowChange_checkBox.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.WindowChange_checkBox);
      this.groupBox3.Location = new System.Drawing.Point(12, 167);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(283, 44);
      this.groupBox3.TabIndex = 34;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Event sending options";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.debug_checkBox);
      this.groupBox1.Location = new System.Drawing.Point(12, 272);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(283, 44);
      this.groupBox1.TabIndex = 36;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Log verbosity";
      // 
      // debug_checkBox
      // 
      this.debug_checkBox.AutoSize = true;
      this.debug_checkBox.Location = new System.Drawing.Point(9, 19);
      this.debug_checkBox.Name = "debug_checkBox";
      this.debug_checkBox.Size = new System.Drawing.Size(224, 17);
      this.debug_checkBox.TabIndex = 0;
      this.debug_checkBox.Text = "Debug mode (uncheck for normal logging)";
      this.debug_checkBox.UseVisualStyleBackColor = true;
      // 
      // FormSettings
      // 
      this.AcceptButton = this.ok_Button;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancel_Button;
      this.ClientSize = new System.Drawing.Size(307, 361);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.groupBox6);
      this.Controls.Add(this.groupBox5);
      this.Controls.Add(this.ok_Button);
      this.Controls.Add(this.cancel_Button);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormSettings";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MQTTPlugin Settings";
      this.Load += new System.EventHandler(this.FormSettings_Load);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.groupBox6.ResumeLayout(false);
      this.groupBox6.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ok_Button;
        private System.Windows.Forms.Button cancel_Button;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.TextBox textBox2;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox user_textBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox port_textBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox host_textBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox mediaDuration_textBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox WindowChange_checkBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox debug_checkBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox password_textBox;
  }
}