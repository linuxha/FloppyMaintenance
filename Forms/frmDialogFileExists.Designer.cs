namespace FloppyMaintenance
{
    partial class frmDialogFileExists
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
            this.textBoxFileExists = new System.Windows.Forms.TextBox();
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonYesToAll = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxCurrentFile = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxFileExists
            // 
            this.textBoxFileExists.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxFileExists.Location = new System.Drawing.Point(24, 23);
            this.textBoxFileExists.Multiline = true;
            this.textBoxFileExists.Name = "textBoxFileExists";
            this.textBoxFileExists.ReadOnly = true;
            this.textBoxFileExists.Size = new System.Drawing.Size(432, 82);
            this.textBoxFileExists.TabIndex = 0;
            this.textBoxFileExists.TabStop = false;
            // 
            // buttonYes
            // 
            this.buttonYes.Location = new System.Drawing.Point(17, 143);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(75, 23);
            this.buttonYes.TabIndex = 0;
            this.buttonYes.Text = "&Yes";
            this.buttonYes.UseVisualStyleBackColor = true;
            this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
            // 
            // buttonYesToAll
            // 
            this.buttonYesToAll.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYesToAll.Location = new System.Drawing.Point(108, 143);
            this.buttonYesToAll.Name = "buttonYesToAll";
            this.buttonYesToAll.Size = new System.Drawing.Size(75, 23);
            this.buttonYesToAll.TabIndex = 2;
            this.buttonYesToAll.Text = "Yes to &All";
            this.buttonYesToAll.UseVisualStyleBackColor = true;
            this.buttonYesToAll.Click += new System.EventHandler(this.buttonYesToAll_Click);
            // 
            // buttonNo
            // 
            this.buttonNo.Location = new System.Drawing.Point(290, 143);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 23);
            this.buttonNo.TabIndex = 3;
            this.buttonNo.Text = "&No";
            this.buttonNo.UseVisualStyleBackColor = true;
            this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(199, 143);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxCurrentFile
            // 
            this.textBoxCurrentFile.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxCurrentFile.Location = new System.Drawing.Point(24, 119);
            this.textBoxCurrentFile.Name = "textBoxCurrentFile";
            this.textBoxCurrentFile.ReadOnly = true;
            this.textBoxCurrentFile.Size = new System.Drawing.Size(432, 13);
            this.textBoxCurrentFile.TabIndex = 5;
            this.textBoxCurrentFile.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(387, 143);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // frmDialogFileExists
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 191);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxCurrentFile);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYesToAll);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.textBoxFileExists);
            this.Name = "frmDialogFileExists";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmDialogFileExists";
            this.Load += new System.EventHandler(this.frmDialogFileExists_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxFileExists;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonYesToAll;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxCurrentFile;
        private System.Windows.Forms.Button buttonOK;
    }
}