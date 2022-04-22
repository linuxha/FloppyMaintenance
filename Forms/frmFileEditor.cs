using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ActiproSoftware.SyntaxEditor;
using ActiproSoftware.Products.SyntaxEditor;

namespace FloppyMaintenance
{
    public partial class frmFileEditor : Form
    {
        private FindReplaceForm findReplaceForm;
        private FindReplaceOptions findReplaceOptions = new FindReplaceOptions();

        // Create a StatusBar control.

        private StatusBar statusBar = new StatusBar();

        // Create two StatusBarPanel objects to display in the StatusBar.

        private StatusBarPanel messagePanel = new StatusBarPanel();
        private StatusBarPanel panel2 = new StatusBarPanel();

        private string _textToDisplay = "";

        public frmFloppymaintDialog pDlgInvoker = null;

        string _targetFileName;
        string _virtualFloppyFileName;
        string _dialogConfigType;
        FileStream _fs;

        bool m_nExpandTabs    ;
        bool m_nAddLinefeed   ;
        bool m_nCompactBinary ;
        bool m_nStripLinefeed ;
        bool m_nCompressSpaces;

        bool m_nConvertLfOnly;
        bool m_nConvertLfOnlyToCrLf;
        bool m_nConvertLfOnlyToCr;

        VirtualFloppyManipulationRoutines vfmr;

        public string TextToDisplay
        {
            get { return _textToDisplay; }
            set { _textToDisplay = value; }
        }

        public frmFileEditor(string dialogConfigType, string targetFileName, string virtualFloppyFileName, FileStream fs, VirtualFloppyManipulationRoutines _vfmr)
        {
            InitializeComponent();

            _targetFileName = targetFileName;                   // this is the name of the temp file name for the file we just got the text for
            _virtualFloppyFileName = virtualFloppyFileName;     // this is the name of the file we read from the fvirtual floppy (incase we want to save it back)
            _dialogConfigType = dialogConfigType;               // this is the type of disk (Foppy IDE, etc..)
            _fs = fs;                                           // this is the file stream of the actual virtual floppy drive;

            vfmr = _vfmr;
        }

        public frmFileEditor(string dialogConfigType, string targetFileName, string virtualFloppyFileName, VirtualFloppyManipulationRoutines _vfmr)
        {
            InitializeComponent();

            _targetFileName = targetFileName;                   // this is the name of the temp file name for the file we just got the text for
            _virtualFloppyFileName = virtualFloppyFileName;     // this is the name of the file we read from the fvirtual floppy (incase we want to save it back)
            _dialogConfigType = dialogConfigType;               // this is the type of disk (Foppy IDE, etc..)
            _fs = _vfmr.currentlyOpenedImageFileStream;         // this is the file stream of the actual virtual floppy drive;

            vfmr = _vfmr;
        }

        private void frmFileEditor_Load(object sender, EventArgs e)
        {
            CenterToParent();
            CreateStatusBar();

            StreamReader reader = new StreamReader(File.Open(_targetFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None));      // fopen_s(&fp, szTargetFileName, "wb");
            _textToDisplay = reader.ReadToEnd();
            reader.Close();

            syntaxEditor.Text = _textToDisplay;
        }

        private void CreateStatusBar()
        {
            // Display the first panel with a sunken border style.
            messagePanel.BorderStyle = StatusBarPanelBorderStyle.Sunken;

            // Initialize the text of the panel.
            messagePanel.Text = "Ready...";

            // Set the AutoSize property to use all remaining space on the StatusBar.
            messagePanel.AutoSize = StatusBarPanelAutoSize.Spring;

            // Display the second panel with a raised border style.
            panel2.BorderStyle = StatusBarPanelBorderStyle.Raised;

            // Create ToolTip text that displays time the application was  
            //started.
            panel2.ToolTipText = "Started: " + System.DateTime.Now.ToShortTimeString();

            // Set the text of the panel to the current date.
            panel2.Text = System.DateTime.Today.ToLongDateString();

            // Set the AutoSize property to size the panel to the size of the contents.
            panel2.AutoSize = StatusBarPanelAutoSize.Contents;

            // Display panels in the StatusBar control.
            statusBar.ShowPanels = true;

            // Add both panels to the StatusBarPanelCollection of the StatusBar.			
            statusBar.Panels.Add(messagePanel);
            statusBar.Panels.Add(panel2);

            // Add the StatusBar to the form. 

            this.Controls.Add(statusBar);
        }

        private void ReloadOptions()
        {
            // reload options from config file in case they were saved (with Apply) in the Option Dialog and them the user pressed cancel.

            m_nExpandTabs           = Program.GetConfigurationAttribute("Global/FileMaintenance/FileExport/ExpandTabs"          , "enabled", "0") == "1" ? true : false;
            m_nAddLinefeed          = Program.GetConfigurationAttribute("Global/FileMaintenance/FileExport/AddLinefeed"         , "enabled", "0") == "1" ? true : false;
            m_nCompactBinary        = Program.GetConfigurationAttribute("Global/FileMaintenance/BinaryFile/CompactBinary"       , "enabled", "0") == "1" ? true : false;
            m_nStripLinefeed        = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/StripLinefeed"       , "enabled", "0") == "1" ? true : false;
            m_nCompressSpaces       = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/CompressSpaces"      , "enabled", "0") == "1" ? true : false;

            m_nConvertLfOnly        = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/ConvertLfOnly"       , "enabled", "0") == "1" ? true : false;
            if (m_nConvertLfOnly)
            {
                m_nConvertLfOnlyToCrLf  = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/ConvertLfOnlyToCrLf" , "enabled", "0") == "1" ? true : false;
                m_nConvertLfOnlyToCr    = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/ConvertLfOnlyToCr"   , "enabled", "0") == "1" ? true : false;
            }
            else
            {
                m_nConvertLfOnlyToCrLf  = false;
                m_nConvertLfOnlyToCr    = false;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _textToDisplay = syntaxEditor.Text.Replace("\n", "");
            StreamWriter writer = new StreamWriter(File.Open(_targetFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));      // fopen_s(&fp, szTargetFileName, "wb");
            writer.Write(_textToDisplay);
            writer.Close();
        }
        private void saveToVirtualFloppyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadOptions();

            vfmr.PartitionBias = 0;

            vfmr.ExpandTabs = m_nExpandTabs;
            vfmr.AddLinefeed = m_nAddLinefeed;
            vfmr.CompactBinary = m_nCompactBinary;
            vfmr.StripLinefeed = m_nStripLinefeed;
            vfmr.CompressSpaces = m_nCompressSpaces;

            vfmr.ConvertLfOnlyToCrLf = m_nConvertLfOnlyToCrLf;
            vfmr.ConvertLfOnlyToCr = m_nConvertLfOnlyToCr;
            vfmr.ConvertLfOnly = m_nConvertLfOnly;

            _textToDisplay = syntaxEditor.Text.Replace("\n", "");
            vfmr.WriteFileToImage(_dialogConfigType, _virtualFloppyFileName, ASCIIEncoding.ASCII.GetBytes(_textToDisplay), _fs);

            if (pDlgInvoker != null)
                pDlgInvoker.RefreshList();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_textToDisplay != syntaxEditor.Text.Replace("\n", ""))
            {
                DialogResult dr = MessageBox.Show("Do you want to save your work?", "You have unsaved work", MessageBoxButtons.YesNoCancel);
                switch (dr)
                {
                    case DialogResult.Yes:
                        saveToolStripMenuItem_Click(null, null);
                        this.Dispose();
                        break;
                    case DialogResult.No:
                        this.Dispose();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
            else
            {
                this.Dispose();
            }
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the find/replace form 

            SearchFindReplace(null);
        }
        private void searchToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (syntaxEditor.SelectedView.SelectedText.Length > 0)
                SearchFindReplace(syntaxEditor.SelectedView.SelectedText);
            else
                SearchFindReplace(null);
        }
        private void searchAgainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((findReplaceForm != null) && (findReplaceForm.Options.FindText != null) && (findReplaceForm.Options.FindText.Length > 0))
            {
                // Perform the find operation
                FindReplaceResultSet resultSet = syntaxEditor.SelectedView.FindReplace.Find(findReplaceForm.Options);

                // Find if the search went past the starting point
                if (resultSet.PastSearchStartOffset)
                {
                    MessageBox.Show(this, SR.GetString("FindReplaceForm_PastSearchStartOffset_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // If no matches were found...			
                if (resultSet.Count == 0)
                    MessageBox.Show(this, SR.GetString("FindReplaceForm_NotFound_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                SearchFindReplace(null);
        }

        public void SetStatusMessage(string text)
        {
            messagePanel.Text = text;
        }

        private void findReplaceForm_StatusChanged(object sender, FindReplaceStatusChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case FindReplaceStatusChangeType.Find:
                    this.SetStatusMessage("Find: \"" + e.Options.FindText + "\"");
                    break;
                case FindReplaceStatusChangeType.PastDocumentEnd:
                    this.SetStatusMessage("Past the end of the document");
                    break;
                case FindReplaceStatusChangeType.Ready:
                    this.SetStatusMessage("Ready");
                    break;
                case FindReplaceStatusChangeType.Replace:
                    this.SetStatusMessage("Replace: \"" + e.Options.FindText + "\", with: \"" + e.Options.ReplaceText + "\"");
                    break;
            }
        }
        private void SearchFindReplace(string searchTerm)
        {
            if (findReplaceForm == null)
            {
                findReplaceForm = new FindReplaceForm(syntaxEditor, findReplaceOptions);
                findReplaceForm.StatusChanged += new FindReplaceStatusChangeEventHandler(findReplaceForm_StatusChanged);
            }
            findReplaceForm.Owner = this;
            if (searchTerm != null)
            {
                findReplaceForm.Options.FindText = searchTerm;
            }
            if (findReplaceForm.Visible)
                findReplaceForm.Activate();
            else
                findReplaceForm.Show();
        }
    }
}
