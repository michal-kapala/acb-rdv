namespace AcbRdv
{
    partial class AddUser
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
            this.Emaillabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.GamePidField = new System.Windows.Forms.TextBox();
            this.EmailField = new System.Windows.Forms.TextBox();
            this.PasswordField = new System.Windows.Forms.TextBox();
            this.NameField = new System.Windows.Forms.TextBox();
            this.generateUID = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.UbisoftIDField = new System.Windows.Forms.TextBox();
            this.CountryCodeField = new System.Windows.Forms.TextBox();
            this.CountryCode = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.PreferedLanguageField = new System.Windows.Forms.TextBox();
            this.add_user = new System.Windows.Forms.Button();
            this.generate_data = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Emaillabel
            // 
            this.Emaillabel.AutoSize = true;
            this.Emaillabel.Location = new System.Drawing.Point(12, 9);
            this.Emaillabel.Name = "Emaillabel";
            this.Emaillabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Emaillabel.Size = new System.Drawing.Size(32, 13);
            this.Emaillabel.TabIndex = 0;
            this.Emaillabel.Text = "Email";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Game Pid";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Name";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 160);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Password";
            // 
            // GamePidField
            // 
            this.GamePidField.Location = new System.Drawing.Point(12, 76);
            this.GamePidField.Name = "GamePidField";
            this.GamePidField.Size = new System.Drawing.Size(100, 20);
            this.GamePidField.TabIndex = 4;
            // 
            // EmailField
            // 
            this.EmailField.Location = new System.Drawing.Point(12, 25);
            this.EmailField.Name = "EmailField";
            this.EmailField.Size = new System.Drawing.Size(153, 20);
            this.EmailField.TabIndex = 5;
            // 
            // PasswordField
            // 
            this.PasswordField.Location = new System.Drawing.Point(12, 176);
            this.PasswordField.Name = "PasswordField";
            this.PasswordField.Size = new System.Drawing.Size(153, 20);
            this.PasswordField.TabIndex = 6;
            this.PasswordField.UseSystemPasswordChar = true;
            // 
            // NameField
            // 
            this.NameField.Location = new System.Drawing.Point(12, 127);
            this.NameField.Name = "NameField";
            this.NameField.Size = new System.Drawing.Size(153, 20);
            this.NameField.TabIndex = 7;
            // 
            // generateUID
            // 
            this.generateUID.Location = new System.Drawing.Point(178, 246);
            this.generateUID.Name = "generateUID";
            this.generateUID.Size = new System.Drawing.Size(90, 21);
            this.generateUID.TabIndex = 8;
            this.generateUID.Text = "Generate UID";
            this.generateUID.UseVisualStyleBackColor = true;
            this.generateUID.Click += new System.EventHandler(this.GenerateUBIid_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 203);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Ubisoft ID";
            // 
            // UbisoftIDField
            // 
            this.UbisoftIDField.Location = new System.Drawing.Point(12, 220);
            this.UbisoftIDField.Name = "UbisoftIDField";
            this.UbisoftIDField.Size = new System.Drawing.Size(256, 20);
            this.UbisoftIDField.TabIndex = 10;
            // 
            // CountryCodeField
            // 
            this.CountryCodeField.Location = new System.Drawing.Point(12, 276);
            this.CountryCodeField.Name = "CountryCodeField";
            this.CountryCodeField.Size = new System.Drawing.Size(100, 20);
            this.CountryCodeField.TabIndex = 11;
            // 
            // CountryCode
            // 
            this.CountryCode.AutoSize = true;
            this.CountryCode.Location = new System.Drawing.Point(12, 257);
            this.CountryCode.Name = "CountryCode";
            this.CountryCode.Size = new System.Drawing.Size(71, 13);
            this.CountryCode.TabIndex = 12;
            this.CountryCode.Text = "Country Code";
            this.CountryCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 318);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Prefered Language";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PreferedLanguageField
            // 
            this.PreferedLanguageField.Location = new System.Drawing.Point(12, 344);
            this.PreferedLanguageField.Name = "PreferedLanguageField";
            this.PreferedLanguageField.Size = new System.Drawing.Size(100, 20);
            this.PreferedLanguageField.TabIndex = 14;
            // 
            // add_user
            // 
            this.add_user.Location = new System.Drawing.Point(178, 389);
            this.add_user.Name = "add_user";
            this.add_user.Size = new System.Drawing.Size(75, 23);
            this.add_user.TabIndex = 15;
            this.add_user.Text = "Add User";
            this.add_user.UseVisualStyleBackColor = true;
            this.add_user.Click += new System.EventHandler(this.add_user_Click);
            // 
            // generate_data
            // 
            this.generate_data.Location = new System.Drawing.Point(15, 389);
            this.generate_data.Name = "generate_data";
            this.generate_data.Size = new System.Drawing.Size(112, 23);
            this.generate_data.TabIndex = 16;
            this.generate_data.Text = "Generate Data";
            this.generate_data.UseVisualStyleBackColor = true;
            this.generate_data.Click += new System.EventHandler(this.generate_data_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(198, 176);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(55, 24);
            this.button1.TabIndex = 17;
            this.button1.Text = "Toggle";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AddUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.generate_data);
            this.Controls.Add(this.add_user);
            this.Controls.Add(this.PreferedLanguageField);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CountryCode);
            this.Controls.Add(this.CountryCodeField);
            this.Controls.Add(this.UbisoftIDField);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.generateUID);
            this.Controls.Add(this.NameField);
            this.Controls.Add(this.PasswordField);
            this.Controls.Add(this.EmailField);
            this.Controls.Add(this.GamePidField);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Emaillabel);
            this.Name = "AddUser";
            this.Text = "AddUser";
            this.Load += new System.EventHandler(this.AddUser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Emaillabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox GamePidField;
        private System.Windows.Forms.TextBox EmailField;
        private System.Windows.Forms.TextBox PasswordField;
        private System.Windows.Forms.TextBox NameField;
        private System.Windows.Forms.Button generateUID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox UbisoftIDField;
        private System.Windows.Forms.TextBox CountryCodeField;
        private System.Windows.Forms.Label CountryCode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox PreferedLanguageField;
        private System.Windows.Forms.Button add_user;
        private System.Windows.Forms.Button generate_data;
        private System.Windows.Forms.Button button1;
    }
}