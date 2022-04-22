using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyMaintenance
{
    public partial class frmDialogFileExists : Form
    {
        public List<OS9FileToCopy> filesToCopy;
        int currentIndex = 0;

        //  This dialog has 5 buttons but only OK, Yes to All and Cancel return a dialog result. Yes and No only set the skipCopy property in the fileToCopy class
        //  to the appropriate value of either true or false. Yes to All will do nothing to the list and just return the Yes dialog result.
        //
        //  When the dialog returns, the Dialog result needs to be checked for OK or Cancel. If Yes to All is clicked OK is returned as the dialog result.
        //
        //      OK - proceed with replacing files on the target specified in the fileToCopy.skipCopy property for each file in the list.
        //      Cancel - do not replace any files

        public frmDialogFileExists(List<OS9FileToCopy> _filesToCopy)
        {
            InitializeComponent();

            filesToCopy = _filesToCopy;
        }

        private void frmDialogFileExists_Load(object sender, EventArgs e)
        {
            textBoxFileExists.Text = "One or more of the files selected for copy already exist on the target file system. If you  continue, the files on the target will first be deleted and them copied from the source. Do you wish to continue by replacing the files?\r\n\r\nYes will copy this one file, Yes to All will copy All files, No will skip this one file and Cancel will copy no files.";

            for (currentIndex = 0; currentIndex < filesToCopy.Count; currentIndex++)
            {
                if (filesToCopy[currentIndex].fileExists)
                {
                    // we are at the next file that already exists on the target

                    textBoxCurrentFile.Text = filesToCopy[currentIndex].safeFilename;
                    break;
                }
            }
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            // do - nothing skipFile is already false - just go to the next file that exists in files to copy and stay in the dialog

            for (currentIndex = currentIndex + 1; currentIndex < filesToCopy.Count; currentIndex++)
            {
                if (filesToCopy[currentIndex].fileExists)
                {
                    // we are at the next file that already exists on the target

                    textBoxCurrentFile.Text = filesToCopy[currentIndex].safeFilename;
                    break;
                }
            }

            if (currentIndex >= filesToCopy.Count)
                buttonOK_Click(sender, e);
        }

        private void buttonYesToAll_Click(object sender, EventArgs e)
        {
            // do nothing - and exit to caller.
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            // set skipCopy to true - operator does not want to replace this file - just go to the next file that exists in files to copy and stay in the dialog

            filesToCopy[currentIndex].skipCopy = true;

            for (currentIndex = currentIndex + 1; currentIndex < filesToCopy.Count; currentIndex++)
            {
                if (filesToCopy[currentIndex].fileExists)
                {
                    // we are at the next file that already exists on the target

                    textBoxCurrentFile.Text = filesToCopy[currentIndex].safeFilename;
                    break;
                }
            }

            if (currentIndex >= filesToCopy.Count)
                buttonOK_Click(sender, e);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // just return the dialog result and let the caller process the cancel
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // this should set the dialog result and force the form to close.

            this.DialogResult = DialogResult.OK;
        }
    }
}
