
namespace FloppyMaintenance
{
    partial class frmDialogConvertDSKToIMA
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
            this.textBoxTargetFileName = new System.Windows.Forms.TextBox();
            this.labelTagetFileName = new System.Windows.Forms.Label();
            this.buttonBrowseTargetFileName = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxFileFormat = new System.Windows.Forms.TextBox();
            this.labelFileFormat = new System.Windows.Forms.Label();
            this.labelSides = new System.Windows.Forms.Label();
            this.textBoxSides = new System.Windows.Forms.TextBox();
            this.labelDensity = new System.Windows.Forms.Label();
            this.textBoxDensity = new System.Windows.Forms.TextBox();
            this.labelPhysicalSize = new System.Windows.Forms.Label();
            this.textBoxPhysicalSize = new System.Windows.Forms.TextBox();
            this.labelFileSize = new System.Windows.Forms.Label();
            this.textBoxFileSize = new System.Windows.Forms.TextBox();
            this.labelMaxTrack = new System.Windows.Forms.Label();
            this.textBoxMaxTrack = new System.Windows.Forms.TextBox();
            this.textBoxMaxSector = new System.Windows.Forms.TextBox();
            this.labelSector = new System.Windows.Forms.Label();
            this.textBoxSectorsOnTrack0 = new System.Windows.Forms.TextBox();
            this.labelSectorsOnTrack0 = new System.Windows.Forms.Label();
            this.labelTagetImage = new System.Windows.Forms.Label();
            this.labelSectorsToEndOfDirectory = new System.Windows.Forms.Label();
            this.textBoxSectorsToEndOfDirectory = new System.Windows.Forms.TextBox();
            this.labelSectorsToAddToFreeChain = new System.Windows.Forms.Label();
            this.textBoxSectorsToAddToFreeChain = new System.Windows.Forms.TextBox();
            this.textBoxTargetSectorOnTrackZero = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxTargetFileSize = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxMessageArea = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxTargetFileName
            // 
            this.textBoxTargetFileName.Location = new System.Drawing.Point(112, 26);
            this.textBoxTargetFileName.Name = "textBoxTargetFileName";
            this.textBoxTargetFileName.Size = new System.Drawing.Size(225, 20);
            this.textBoxTargetFileName.TabIndex = 0;
            // 
            // labelTagetFileName
            // 
            this.labelTagetFileName.AutoSize = true;
            this.labelTagetFileName.Location = new System.Drawing.Point(23, 30);
            this.labelTagetFileName.Name = "labelTagetFileName";
            this.labelTagetFileName.Size = new System.Drawing.Size(83, 13);
            this.labelTagetFileName.TabIndex = 1;
            this.labelTagetFileName.Text = "Target Filename";
            // 
            // buttonBrowseTargetFileName
            // 
            this.buttonBrowseTargetFileName.Location = new System.Drawing.Point(343, 25);
            this.buttonBrowseTargetFileName.Name = "buttonBrowseTargetFileName";
            this.buttonBrowseTargetFileName.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseTargetFileName.TabIndex = 2;
            this.buttonBrowseTargetFileName.Text = "Browse";
            this.buttonBrowseTargetFileName.UseVisualStyleBackColor = true;
            this.buttonBrowseTargetFileName.Click += new System.EventHandler(this.buttonBrowseTargetFileName_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.Location = new System.Drawing.Point(126, 357);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "C&onvert";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(239, 357);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxFileFormat
            // 
            this.textBoxFileFormat.Location = new System.Drawing.Point(112, 54);
            this.textBoxFileFormat.Name = "textBoxFileFormat";
            this.textBoxFileFormat.Size = new System.Drawing.Size(138, 20);
            this.textBoxFileFormat.TabIndex = 5;
            // 
            // labelFileFormat
            // 
            this.labelFileFormat.AutoSize = true;
            this.labelFileFormat.Location = new System.Drawing.Point(23, 54);
            this.labelFileFormat.Name = "labelFileFormat";
            this.labelFileFormat.Size = new System.Drawing.Size(58, 13);
            this.labelFileFormat.TabIndex = 6;
            this.labelFileFormat.Text = "File Format";
            // 
            // labelSides
            // 
            this.labelSides.AutoSize = true;
            this.labelSides.Location = new System.Drawing.Point(256, 54);
            this.labelSides.Name = "labelSides";
            this.labelSides.Size = new System.Drawing.Size(33, 13);
            this.labelSides.TabIndex = 7;
            this.labelSides.Text = "Sides";
            // 
            // textBoxSides
            // 
            this.textBoxSides.Location = new System.Drawing.Point(293, 54);
            this.textBoxSides.Name = "textBoxSides";
            this.textBoxSides.Size = new System.Drawing.Size(15, 20);
            this.textBoxSides.TabIndex = 8;
            // 
            // labelDensity
            // 
            this.labelDensity.AutoSize = true;
            this.labelDensity.Location = new System.Drawing.Point(314, 54);
            this.labelDensity.Name = "labelDensity";
            this.labelDensity.Size = new System.Drawing.Size(42, 13);
            this.labelDensity.TabIndex = 9;
            this.labelDensity.Text = "Density";
            // 
            // textBoxDensity
            // 
            this.textBoxDensity.Location = new System.Drawing.Point(364, 54);
            this.textBoxDensity.Name = "textBoxDensity";
            this.textBoxDensity.Size = new System.Drawing.Size(54, 20);
            this.textBoxDensity.TabIndex = 10;
            // 
            // labelPhysicalSize
            // 
            this.labelPhysicalSize.AutoSize = true;
            this.labelPhysicalSize.Location = new System.Drawing.Point(23, 78);
            this.labelPhysicalSize.Name = "labelPhysicalSize";
            this.labelPhysicalSize.Size = new System.Drawing.Size(69, 13);
            this.labelPhysicalSize.TabIndex = 11;
            this.labelPhysicalSize.Text = "Physical Size";
            // 
            // textBoxPhysicalSize
            // 
            this.textBoxPhysicalSize.Location = new System.Drawing.Point(112, 78);
            this.textBoxPhysicalSize.Name = "textBoxPhysicalSize";
            this.textBoxPhysicalSize.Size = new System.Drawing.Size(34, 20);
            this.textBoxPhysicalSize.TabIndex = 12;
            // 
            // labelFileSize
            // 
            this.labelFileSize.AutoSize = true;
            this.labelFileSize.Location = new System.Drawing.Point(158, 78);
            this.labelFileSize.Name = "labelFileSize";
            this.labelFileSize.Size = new System.Drawing.Size(46, 13);
            this.labelFileSize.TabIndex = 14;
            this.labelFileSize.Text = "File Size";
            // 
            // textBoxFileSize
            // 
            this.textBoxFileSize.Location = new System.Drawing.Point(211, 78);
            this.textBoxFileSize.Name = "textBoxFileSize";
            this.textBoxFileSize.Size = new System.Drawing.Size(100, 20);
            this.textBoxFileSize.TabIndex = 15;
            // 
            // labelMaxTrack
            // 
            this.labelMaxTrack.AutoSize = true;
            this.labelMaxTrack.Location = new System.Drawing.Point(23, 102);
            this.labelMaxTrack.Name = "labelMaxTrack";
            this.labelMaxTrack.Size = new System.Drawing.Size(58, 13);
            this.labelMaxTrack.TabIndex = 17;
            this.labelMaxTrack.Text = "Max Track";
            // 
            // textBoxMaxTrack
            // 
            this.textBoxMaxTrack.Location = new System.Drawing.Point(112, 102);
            this.textBoxMaxTrack.Name = "textBoxMaxTrack";
            this.textBoxMaxTrack.Size = new System.Drawing.Size(34, 20);
            this.textBoxMaxTrack.TabIndex = 18;
            // 
            // textBoxMaxSector
            // 
            this.textBoxMaxSector.Location = new System.Drawing.Point(226, 102);
            this.textBoxMaxSector.Name = "textBoxMaxSector";
            this.textBoxMaxSector.Size = new System.Drawing.Size(34, 20);
            this.textBoxMaxSector.TabIndex = 20;
            // 
            // labelSector
            // 
            this.labelSector.AutoSize = true;
            this.labelSector.Location = new System.Drawing.Point(158, 102);
            this.labelSector.Name = "labelSector";
            this.labelSector.Size = new System.Drawing.Size(61, 13);
            this.labelSector.TabIndex = 19;
            this.labelSector.Text = "Max Sector";
            // 
            // textBoxSectorsOnTrack0
            // 
            this.textBoxSectorsOnTrack0.Location = new System.Drawing.Point(384, 102);
            this.textBoxSectorsOnTrack0.Name = "textBoxSectorsOnTrack0";
            this.textBoxSectorsOnTrack0.Size = new System.Drawing.Size(34, 20);
            this.textBoxSectorsOnTrack0.TabIndex = 22;
            // 
            // labelSectorsOnTrack0
            // 
            this.labelSectorsOnTrack0.AutoSize = true;
            this.labelSectorsOnTrack0.Location = new System.Drawing.Point(276, 102);
            this.labelSectorsOnTrack0.Name = "labelSectorsOnTrack0";
            this.labelSectorsOnTrack0.Size = new System.Drawing.Size(93, 13);
            this.labelSectorsOnTrack0.TabIndex = 21;
            this.labelSectorsOnTrack0.Text = "Sector on Track 0";
            // 
            // labelTagetImage
            // 
            this.labelTagetImage.AutoSize = true;
            this.labelTagetImage.Location = new System.Drawing.Point(147, 168);
            this.labelTagetImage.Name = "labelTagetImage";
            this.labelTagetImage.Size = new System.Drawing.Size(126, 13);
            this.labelTagetImage.TabIndex = 23;
            this.labelTagetImage.Text = "Target Image Parameters";
            // 
            // labelSectorsToEndOfDirectory
            // 
            this.labelSectorsToEndOfDirectory.AutoSize = true;
            this.labelSectorsToEndOfDirectory.Location = new System.Drawing.Point(23, 126);
            this.labelSectorsToEndOfDirectory.Name = "labelSectorsToEndOfDirectory";
            this.labelSectorsToEndOfDirectory.Size = new System.Drawing.Size(138, 13);
            this.labelSectorsToEndOfDirectory.TabIndex = 24;
            this.labelSectorsToEndOfDirectory.Text = "Sectors To End of Directory";
            // 
            // textBoxSectorsToEndOfDirectory
            // 
            this.textBoxSectorsToEndOfDirectory.Location = new System.Drawing.Point(170, 126);
            this.textBoxSectorsToEndOfDirectory.Name = "textBoxSectorsToEndOfDirectory";
            this.textBoxSectorsToEndOfDirectory.Size = new System.Drawing.Size(27, 20);
            this.textBoxSectorsToEndOfDirectory.TabIndex = 25;
            // 
            // labelSectorsToAddToFreeChain
            // 
            this.labelSectorsToAddToFreeChain.AutoSize = true;
            this.labelSectorsToAddToFreeChain.Location = new System.Drawing.Point(215, 126);
            this.labelSectorsToAddToFreeChain.Name = "labelSectorsToAddToFreeChain";
            this.labelSectorsToAddToFreeChain.Size = new System.Drawing.Size(151, 13);
            this.labelSectorsToAddToFreeChain.TabIndex = 26;
            this.labelSectorsToAddToFreeChain.Text = "Sectors To Add To Free Chain";
            // 
            // textBoxSectorsToAddToFreeChain
            // 
            this.textBoxSectorsToAddToFreeChain.Location = new System.Drawing.Point(384, 126);
            this.textBoxSectorsToAddToFreeChain.Name = "textBoxSectorsToAddToFreeChain";
            this.textBoxSectorsToAddToFreeChain.Size = new System.Drawing.Size(34, 20);
            this.textBoxSectorsToAddToFreeChain.TabIndex = 27;
            // 
            // textBoxTargetSectorOnTrackZero
            // 
            this.textBoxTargetSectorOnTrackZero.Location = new System.Drawing.Point(350, 197);
            this.textBoxTargetSectorOnTrackZero.Name = "textBoxTargetSectorOnTrackZero";
            this.textBoxTargetSectorOnTrackZero.Size = new System.Drawing.Size(34, 20);
            this.textBoxTargetSectorOnTrackZero.TabIndex = 43;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(242, 197);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 13);
            this.label4.TabIndex = 42;
            this.label4.Text = "Sector on Track 0";
            // 
            // textBoxTargetFileSize
            // 
            this.textBoxTargetFileSize.Location = new System.Drawing.Point(82, 197);
            this.textBoxTargetFileSize.Name = "textBoxTargetFileSize";
            this.textBoxTargetFileSize.Size = new System.Drawing.Size(100, 20);
            this.textBoxTargetFileSize.TabIndex = 37;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 197);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "File Size";
            // 
            // textBoxMessageArea
            // 
            this.textBoxMessageArea.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.textBoxMessageArea.Location = new System.Drawing.Point(46, 237);
            this.textBoxMessageArea.Multiline = true;
            this.textBoxMessageArea.Name = "textBoxMessageArea";
            this.textBoxMessageArea.Size = new System.Drawing.Size(348, 106);
            this.textBoxMessageArea.TabIndex = 44;
            // 
            // frmDialogConvertDSKToIMA
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(441, 398);
            this.Controls.Add(this.textBoxMessageArea);
            this.Controls.Add(this.textBoxTargetSectorOnTrackZero);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxTargetFileSize);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxSectorsToAddToFreeChain);
            this.Controls.Add(this.labelSectorsToAddToFreeChain);
            this.Controls.Add(this.textBoxSectorsToEndOfDirectory);
            this.Controls.Add(this.labelSectorsToEndOfDirectory);
            this.Controls.Add(this.labelTagetImage);
            this.Controls.Add(this.textBoxSectorsOnTrack0);
            this.Controls.Add(this.labelSectorsOnTrack0);
            this.Controls.Add(this.textBoxMaxSector);
            this.Controls.Add(this.labelSector);
            this.Controls.Add(this.textBoxMaxTrack);
            this.Controls.Add(this.labelMaxTrack);
            this.Controls.Add(this.textBoxFileSize);
            this.Controls.Add(this.labelFileSize);
            this.Controls.Add(this.textBoxPhysicalSize);
            this.Controls.Add(this.labelPhysicalSize);
            this.Controls.Add(this.textBoxDensity);
            this.Controls.Add(this.labelDensity);
            this.Controls.Add(this.textBoxSides);
            this.Controls.Add(this.labelSides);
            this.Controls.Add(this.labelFileFormat);
            this.Controls.Add(this.textBoxFileFormat);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonBrowseTargetFileName);
            this.Controls.Add(this.labelTagetFileName);
            this.Controls.Add(this.textBoxTargetFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDialogConvertDSKToIMA";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Convert .DSk to .IMA image";
            this.Load += new System.EventHandler(this.frmDialogConvertDSKToIMA_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTargetFileName;
        private System.Windows.Forms.Label labelTagetFileName;
        private System.Windows.Forms.Button buttonBrowseTargetFileName;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxFileFormat;
        private System.Windows.Forms.Label labelFileFormat;
        private System.Windows.Forms.Label labelSides;
        private System.Windows.Forms.TextBox textBoxSides;
        private System.Windows.Forms.Label labelDensity;
        private System.Windows.Forms.TextBox textBoxDensity;
        private System.Windows.Forms.Label labelPhysicalSize;
        private System.Windows.Forms.TextBox textBoxPhysicalSize;
        private System.Windows.Forms.Label labelFileSize;
        private System.Windows.Forms.TextBox textBoxFileSize;
        private System.Windows.Forms.Label labelMaxTrack;
        private System.Windows.Forms.TextBox textBoxMaxTrack;
        private System.Windows.Forms.TextBox textBoxMaxSector;
        private System.Windows.Forms.Label labelSector;
        private System.Windows.Forms.TextBox textBoxSectorsOnTrack0;
        private System.Windows.Forms.Label labelSectorsOnTrack0;
        private System.Windows.Forms.Label labelTagetImage;
        private System.Windows.Forms.Label labelSectorsToEndOfDirectory;
        private System.Windows.Forms.TextBox textBoxSectorsToEndOfDirectory;
        private System.Windows.Forms.Label labelSectorsToAddToFreeChain;
        private System.Windows.Forms.TextBox textBoxSectorsToAddToFreeChain;
        private System.Windows.Forms.TextBox textBoxTargetSectorOnTrackZero;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxTargetFileSize;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxMessageArea;
    }
}