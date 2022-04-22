

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Xml;
using System.IO;
using System.Diagnostics;

using Microsoft.VisualBasic;
using System.Security.Authentication.ExtendedProtection;
using System.Runtime.Remoting.Lifetime;
using FloppyMaintenance.Forms;

namespace FloppyMaintenance
{
    /// <summary>
    /// Summary description for frmFloppymaintDialog.
    /// </summary>
    public partial class frmFloppymaintDialog : System.Windows.Forms.Form
    {
        #region variables

        private MRUManager mruManager;

        XmlDocument os9XmlDocument = new XmlDocument();
        XmlDocument uniFLEXXmlDocument = new XmlDocument();
        XmlDocument minixXmlDocument = new XmlDocument();

        private bool m_nExpandTabs = true;
        private bool m_nAddLinefeed = false;
        private bool m_nCompactBinary = true;
        private bool m_nStripLinefeed = true;
        private bool m_nCompressSpaces = true;
        private bool m_nConvertLfOnly = false;
        private bool m_nConvertLfOnlyToCrLf = false;
        private bool m_nConvertLfOnlyToCr = true;

        private string dialogConfigType = "FloppyMaintenance";
        private string editor = "";
        private bool useExternalEditor = false;
        private bool logOS9FloppyWrite = false;
        private string os9FloppyWritesFile = "";

        private bool iamgeFileDirectoryDisplayed = false;

        public bool ExpandTabs
        {
            get { return m_nExpandTabs; }
            set { m_nExpandTabs = value; }
        }
        public bool AddLinefeed
        {
            get { return m_nAddLinefeed; }
            set { m_nAddLinefeed = value; }
        }
        public bool CompactBinary
        {
            get { return m_nCompactBinary; }
            set { m_nCompactBinary = value; }
        }
        public bool StripLinefeed
        {
            get { return m_nStripLinefeed; }
            set { m_nStripLinefeed = value; }
        }
        public bool CompressSpaces
        {
            get { return m_nCompressSpaces; }
            set { m_nCompressSpaces = value; }
        }

        public string DialogConfigType
        {
            get { return dialogConfigType; }
            set { dialogConfigType = value; }
        }

        public bool ConvertLfOnly { get => m_nConvertLfOnly; set => m_nConvertLfOnly = value; }
        public bool ConvertLfOnlyToCrLf { get => m_nConvertLfOnlyToCrLf; set => m_nConvertLfOnlyToCrLf = value; }
        public bool ConvertLfOnlyToCr { get => m_nConvertLfOnlyToCr; set => m_nConvertLfOnlyToCr = value; }
        public bool LogOS9FloppyWrite { get => logOS9FloppyWrite; set => logOS9FloppyWrite = value; }
        public string Os9FloppyWritesFile { get => os9FloppyWritesFile; set => os9FloppyWritesFile = value; }

        private uint m_nTotalSectors = 0;
        private uint m_nVolumeNumber = 0;

        string cDriveFileTitle = "";
        string cDriveFileName = "";
        string cDrivePathName = "";

        private bool m_bOpenedReadOnly;

        public VirtualFloppyManipulationRoutines virtualFloppyManipulationRoutines = null;  // = new VirtualFloppyManipulationRoutines();
        #endregion

        string _version = Program.version.ToString();

        public frmFloppymaintDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            ReloadOptions();

            this.mruManager = new MRUManager(
                //the menu item that will contain the recent files
                this.recentFilesToolStripMenuItem,

                //the name of your program
                "FloppyMaintenance",

                //the funtion that will be called when a recent file gets clicked.
                this.myOwnRecentFileGotClicked_handler,

                //an optional function to call when the user clears the list of recent items
                this.myOwnRecentFilesGotCleared_handler);
        }

        private void ShowVersionInTitle(string _cDriveFileTitle)
        {
            if (_cDriveFileTitle != null && cDriveFileName.Length != 0)
                this.Text = string.Format("{0} {1}: {2}", dialogConfigType, _version, cDriveFileName);
            else
                this.Text = string.Format("{0} {1}", dialogConfigType, _version);
        }

        private void myOwnRecentFileGotClicked_handler(object obj, EventArgs evt)
        {
            string fName = (obj as ToolStripItem).Text;
            if (!File.Exists(fName))
            {
                if (MessageBox.Show(string.Format("{0} doesn't exist. Remove from recent workspaces?", fName), "File not found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    this.mruManager.RemoveRecentFile(fName);
                return;
            }

            //do something with the file here
            //MessageBox.Show(string.Format("Through the 'Recent Files' menu item, you opened: {0}", fName));

            // ToDo: do something with the file

            OpenTheFileAndShowIt(Path.GetFileName(fName), fName);
            ShowVersionInTitle(cDriveFileTitle);
        }

        private void myOwnRecentFilesGotCleared_handler(object obj, EventArgs evt)
        {
            //prior to this function getting called, all recent files in the registry and 
            //in the program's 'Recent Files' menu are cleared.

            //perhaps you want to do something here after all this happens
            MessageBox.Show("You just cleared all recent files.");
        }

        /// <summary>
        /// Sets the enable status of the buttons based on whether or not a file is active and whether or not it is readonly
        /// </summary>
        private void SetButtons()
        {
            // if we have an open file - enable the proper buttons

            if (iamgeFileDirectoryDisplayed)
            {
                btnButtonDeselectAll.Enabled = true;
                btnButtonExport.Enabled = true;
                verifyImageIntegrityToolStripMenuItem.Enabled = false;

                switch (labelFileFormat.Text)
                {
                    case "FLEX":
                    case "FLEX_IDE":
                    case "FLEX_IMA":
                        btnButtonRefresh.Enabled = true;
                        if (labelFileFormat.Text != "FLEX_IDE")
                            verifyImageIntegrityToolStripMenuItem.Enabled = true;
                        break;
                    default:
                        btnButtonRefresh.Enabled = false;
                        break;
                }
                btnButtonSelectAll.Enabled = true;
                btnButtonClose.Enabled = true;

                if ((virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_OS9) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX_IMA) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX_IDE))
                {
                    btnButtonNewDirectory.Enabled = true;
                    if ((virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_OS9))
                        btnButtonImportDir.Enabled = true;
                    else
                        btnButtonImportDir.Enabled = false;
                }
                else
                {
                    btnButtonNewDirectory.Enabled = false;
                    btnButtonImportDir.Enabled = false;
                }

                // if this file is NOT read only - enable delete and import

                if (!m_bOpenedReadOnly)
                {
                    btnButtonImport.Enabled = true;
                    btnButtonDelete.Enabled = true;

                    if ((virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_OS9) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX_IMA) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX_IDE))
                    {
                        btnButtonNewDirectory.Enabled = true;
                        if ((virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_OS9))
                            btnButtonImportDir.Enabled = true;
                        else
                            btnButtonImportDir.Enabled = false;
                    }
                    else
                    {
                        btnButtonNewDirectory.Enabled = false;
                        btnButtonImportDir.Enabled = false;
                    }
                }
                else
                {
                    btnButtonImport.Enabled = false;
                    btnButtonDelete.Enabled = false;
                    btnButtonNewDirectory.Enabled = false;
                    btnButtonImportDir.Enabled = false;
                }

                picStaticRwStatus.Visible = true;
            }
            else
            {
                btnButtonDeselectAll.Enabled = false;
                btnButtonExport.Enabled = false;
                btnButtonRefresh.Enabled = false;
                btnButtonSelectAll.Enabled = false;
                btnButtonClose.Enabled = false;
                btnButtonImport.Enabled = false;
                btnButtonDelete.Enabled = false;
                btnButtonNewDirectory.Enabled = false;
                btnButtonImportDir.Enabled = false;
            }

            // we will disable delete and import for OS9 until be get them working

            //if (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_OS9)
            //{
            //    btnButtonImport.Enabled = true;         // <- enable it while working on it
            //    btnButtonDelete.Enabled = true;
            //}
        }

        /// <summary>
        ///  set up buttons when program initally loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmFloppymaintDialog_Load(object sender, EventArgs e)
        {
            ShowVersionInTitle(null);

            treeViewFiles.Visible = false;

            // disable the buttons that require a file to be active

            btnButtonDelete.Enabled = false;
            btnButtonDeselectAll.Enabled = false;
            btnButtonExport.Enabled = false;
            btnButtonImport.Enabled = false;
            btnButtonRefresh.Enabled = false;
            btnButtonSelectAll.Enabled = false;
            btnButtonClose.Enabled = false;
            btnButtonNewDirectory.Enabled = false;
            btnButtonImportDir.Enabled = false;

            picStaticRwStatus.Visible = false;

            foreach (string arg in Program.args)
            {
                if (!arg.StartsWith("-"))
                {
                    cDrivePathName = arg;
                    OpenTheFile();
                    SetButtons();
                }
                // else - handle commandline switches.
            }

            ShowVersionInTitle(null);
        }

        #region helper functions

        private void InitPartitionListHeadings()
        {
            lstViewPartitions.Items.Clear();
            lstViewPartitions.Visible = true;
        }

        /// <summary>
        /// this is used for partitioned IDE drives - they have a slightly different format than normal Floppy diskette images.
        /// they have extra bytes at the end to define the following: the first two byts are the size of the information about the 
        /// drive including the info size bytes.
        /// </summary>
        private void FillPartitionList(FileStream fp)
        {
            lstViewPartitions.Top = 102;

            FileInfo fi = new FileInfo(virtualFloppyManipulationRoutines.currentlyOpenedImageFileName);
            long lFilePosition = fp.Position;               //long lFilePosition = ftell (m_fp);

            InitPartitionListHeadings();

            long fileLength = fi.Length;
            {
                long lOffset = 0x0310;
                while (fileLength > lOffset)
                {
                    // count the number of partitions and get the partition name for each

                    byte[] caVolumeNumber = new byte[2];
                    byte[] szPartitionName = new byte[12]; szPartitionName[11] = 0x00;

                    fp.Seek(lOffset, SeekOrigin.Begin);         // fseek(m_fp, lOffset, SEEK_SET);
                    fp.Read(szPartitionName, 0, 11);            // fread(szPartitionName, 1, 11, m_fp);
                    fp.Read(caVolumeNumber, 0, 2);              // fread(caVolumeNumber, 1, 2, m_fp);

                    int nVolumeNumber = caVolumeNumber[0] * 256 + caVolumeNumber[1];
                    string strVolumeNumber;
                    strVolumeNumber = string.Format("{0}", nVolumeNumber);

                    string strPartitionName = Encoding.ASCII.GetString(szPartitionName);

                    ListViewItem item = new ListViewItem(strPartitionName.Trim('\0'));
                    item.SubItems.Add(strVolumeNumber);
                    item.Tag = lOffset - 0x0310;

                    lstViewPartitions.Items.Add(item);

                    lOffset += 16777216;    //lOffset += 16711680;
                }
            }
        }

        private void ClearFormData()
        {
            InitFileListHeadings();
        }

        public int RefreshList()
        {
            DIR_ENTRY stDirEntry;
            int rowCount = 0;

            byte cNextDirTrack = 0;
            byte cNextDirSector = 5;
            bool nDirectoryEnd = false;
            int nMaxSector;
            int nRandomFileInd;

            int i;
            long lOffset;
            string szVolumeName;
            byte[] caDirHeader = new byte[16];
            string szFileName;
            string szFileExtension;
            string szCreationDate;
            string szStartTrackSector;
            string szEndTrackSector;

            string szTotalSectors;
            int nTotalSectors;

            if (virtualFloppyManipulationRoutines != null)
            {
                if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
                {
                    FileStream m_fp = virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream;

                    m_fp.Flush();

                    // First get the System Information Record

                    switch (virtualFloppyManipulationRoutines.CurrentFileFormat)
                    {
                        case fileformat.fileformat_FLEX:
                        case fileformat.fileformat_FLEX_IDE:
                        case fileformat.fileformat_FLEX_IMA:
                            {
                                // get the system information record.

                                m_fp.Seek(virtualFloppyManipulationRoutines.PartitionBias + 0x0310 - (virtualFloppyManipulationRoutines.sectorSize * virtualFloppyManipulationRoutines.SectorBias), SeekOrigin.Begin);
                                RAW_SIR stSystemInformationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR(m_fp);

                                szVolumeName = ASCIIEncoding.ASCII.GetString(stSystemInformationRecord.caVolumeLabel);

                                m_nVolumeNumber = (uint)(stSystemInformationRecord.cVolumeNumberHi * 256 + stSystemInformationRecord.cVolumeNumberLo);
                                nMaxSector = stSystemInformationRecord.cMaxSector;
                                m_nTotalSectors = (uint)(stSystemInformationRecord.cTotalSectorsHi * 256 + stSystemInformationRecord.cTotalSectorsLo);

                                int nYear = 1900 + stSystemInformationRecord.cYear;
                                if (nYear < 1970)
                                {
                                    nYear = 2000 + stSystemInformationRecord.cYear;
                                }
                                szCreationDate = string.Format("{0}-{1}-{2}", nYear.ToString("0000"), stSystemInformationRecord.cMonth.ToString("00"), stSystemInformationRecord.cDay.ToString("00"));
                                szTotalSectors = m_nTotalSectors.ToString();
                                string szVolumeNameNumber = string.Format("{0} [{1}]", szVolumeName, m_nVolumeNumber.ToString());

                                lblStaticVolumeName.Text = szVolumeNameNumber;
                                lblStaticCreationDate.Text = szCreationDate;
                                lblStaticRemainingSectors.Text = szTotalSectors;

                                // Now fill Directory File List

                                ClearFormData();

                                rowCount = 0;

                                // here is where we will implement HIER handling. Start by default to the root directory

                                byte startDirTrack = 0;
                                byte cFirstDirSector = 5;

                                // now see if the user has clicked an entry in the list that points to a HIER directory

                                if (hierDirectories.Count > 0)
                                {
                                    // if there is any entry in hierDirectories - use the track /sector specified in the DIR_ENTRY entry in the last position of the list

                                    startDirTrack = hierDirectories[hierDirectories.Count - 1].cStartTrack;
                                    cFirstDirSector = hierDirectories[hierDirectories.Count - 1].cStartSector;
                                }

                                for (cNextDirTrack = startDirTrack, cNextDirSector = cFirstDirSector; !nDirectoryEnd;)
                                {
                                    // if the sector in the linkage track and sector is 0 - this is the last sector in the directory - processint and then leave

                                    if (cNextDirSector == 0)
                                        nDirectoryEnd = true;

                                    if (!nDirectoryEnd)
                                    {
                                        lOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, cNextDirTrack, cNextDirSector);
                                        m_fp.Seek(lOffset, SeekOrigin.Begin);       // fseek (m_fp, lOffset, SEEK_SET);
                                        m_fp.Read(caDirHeader, 0, 16);              // fread (&caDirHeader, 1, sizeof (caDirHeader), m_fp);
                                        cNextDirTrack = caDirHeader[0];
                                        cNextDirSector = caDirHeader[1];
                                        for (i = 0; i < 10; i++)
                                        {
                                            stDirEntry = virtualFloppyManipulationRoutines.ReadFLEX_DIR_ENTRY(m_fp, false);                  // fread (&stDirEntry,  1, sizeof (stDirEntry), m_fp);
                                            if (stDirEntry.caFileName[0] != '\0')
                                            {
                                                if ((stDirEntry.caFileName[0] & 0x80) != 0x80)
                                                {

                                                    // We are now pointing at a Directory Entry

                                                    szFileName = ASCIIEncoding.ASCII.GetString(stDirEntry.caFileName).TrimEnd('\0');                // memcpy (szFileName,      stDirEntry.caFileName,      8);
                                                    szFileExtension = ASCIIEncoding.ASCII.GetString(stDirEntry.caFileExtension).TrimEnd('\0');      // memcpy(szFileExtension, stDirEntry.caFileExtension, 3);

                                                    nRandomFileInd = stDirEntry.cRandomFileInd;

                                                    nYear = 1900 + stDirEntry.cYear;
                                                    if (nYear < 1970)
                                                    {
                                                        nYear = 2000 + stDirEntry.cYear;
                                                    }
                                                    szCreationDate = string.Format("{0}-{1}-{2}", nYear.ToString("0000"), stDirEntry.cMonth.ToString("00"), stDirEntry.cDay.ToString("00"));
                                                    szStartTrackSector = string.Format("{0}-{1}", stDirEntry.cStartTrack.ToString("000"), stDirEntry.cStartSector.ToString("000"));
                                                    szEndTrackSector = string.Format("{0}-{1}", stDirEntry.cEndTrack.ToString("000"), stDirEntry.cEndSector.ToString("000"));
                                                    nTotalSectors = stDirEntry.cTotalSectorsHi * 256 + stDirEntry.cTotalSectorsLo;
                                                    szTotalSectors = string.Format("{0}", nTotalSectors.ToString("000000"));        // sprintf_s (szTotalSectors, sizeof (szTotalSectors), "%6d", nTotalSectors);

                                                    ListViewItem item = new ListViewItem(szFileName.Trim('\0'));
                                                    item.SubItems.Add(szFileExtension.Trim('\0'));
                                                    if (nRandomFileInd != 0)
                                                        item.SubItems.Add("Y");
                                                    else
                                                        item.SubItems.Add(" ");

                                                    item.SubItems.Add(szCreationDate.Trim('\0'));
                                                    item.SubItems.Add(szStartTrackSector.Trim('\0'));
                                                    item.SubItems.Add(szEndTrackSector.Trim('\0'));
                                                    item.SubItems.Add(szTotalSectors.Trim('\0'));

                                                    string attributes = "";
                                                    stDirEntry.isHierDirectoryEntry = false;

                                                    if ((stDirEntry.cAttributes & 0x80) == 0x80)
                                                        attributes += "W";
                                                    else
                                                        attributes += " ";
                                                    if ((stDirEntry.cAttributes & 0x40) == 0x40)
                                                    {
                                                        attributes += "D";
                                                        if (szFileExtension == "DIR")
                                                        {
                                                            long saveOffset = m_fp.Position;

                                                            // read the sector pointed to by the dirst tracka and sector

                                                            byte[] possibleDirHeader = new byte[16];

                                                            long possibleDirlOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, stDirEntry.cStartTrack, stDirEntry.cStartSector);
                                                            m_fp.Seek(possibleDirlOffset, SeekOrigin.Begin);       // fseek (m_fp, lOffset, SEEK_SET);
                                                            m_fp.Read(possibleDirHeader, 0, 16);

                                                            string possibleDirHeaderName = "";
                                                            for (int index = 6; index < 14; index++)
                                                            {
                                                                if (possibleDirHeader[index] != 0x00)
                                                                {
                                                                    possibleDirHeaderName += (char)possibleDirHeader[index];
                                                                }
                                                                else
                                                                    break;
                                                            }

                                                            if (possibleDirHeaderName == szFileName)
                                                            {
                                                                stDirEntry.isHierDirectoryEntry = true;
                                                            }

                                                            m_fp.Seek(saveOffset, SeekOrigin.Begin);       // reset position
                                                        }
                                                    }
                                                    else
                                                        attributes += " ";
                                                    if ((stDirEntry.cAttributes & 0x20) == 0x20)
                                                        attributes += "R";
                                                    else
                                                        attributes += " ";
                                                    if ((stDirEntry.cAttributes & 0x10) == 0x10)
                                                        attributes += "C";
                                                    else
                                                        attributes += " ";

                                                    if (stDirEntry.isHierDirectoryEntry)
                                                        attributes += "H";

                                                    item.SubItems.Add(attributes);

                                                    ListViewItem lvi = lstviewListFiles.Items.Add(item);
                                                    lvi.Tag = stDirEntry;

                                                    rowCount++;
                                                }
                                            }
                                            else
                                            {
                                                nDirectoryEnd = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (m_bOpenedReadOnly)
                                {
                                    picStaticRwStatus.Image = Properties.Resources.reddot;
                                    btnButtonImport.Enabled = false;
                                    btnButtonDelete.Enabled = false;
                                }
                                else
                                {
                                    picStaticRwStatus.Image = Properties.Resources.greendot;
                                    btnButtonImport.Enabled = true;
                                    btnButtonDelete.Enabled = true;
                                }

                                btnButtonExport.Enabled = true;
                                btnButtonRefresh.Enabled = true;

                                picStaticRwStatus.Visible = true;

                                lblStaticFileCount.Text = string.Format("{0}", rowCount.ToString());        // sprintf_s (szTotalFiles, sizeof (szTotalFiles), "%5d", nRow);
                            }
                            break;
                    }
                }
            }

            return rowCount;
        }

        class FILE_HEADING
        {
            public string Text;
            public int Width;
        }

        private void InitFileListHeadings()
        {
            List<FILE_HEADING> stHeadings = new List<FILE_HEADING>();

            FILE_HEADING fh;

            fh = new FILE_HEADING(); fh.Text = "FileName  "; fh.Width = 70; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "Ext "; fh.Width = 39; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "R"; fh.Width = 20; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "Date"; fh.Width = 70; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "Start    "; fh.Width = 53; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "End      "; fh.Width = 53; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "Count"; fh.Width = 50; stHeadings.Add(fh);
            fh = new FILE_HEADING(); fh.Text = "Attr"; fh.Width = 40; stHeadings.Add(fh);

            if (lstviewListFiles.Items.Count > 0)
                lstviewListFiles.Items.Clear();

            lstviewListFiles.Columns.Clear();

            foreach (FILE_HEADING h in stHeadings)
            {
                lstviewListFiles.Columns.Add(h.Text, h.Width);
            }
            lstviewListFiles.FullRowSelect = true;
        }

        /// <summary>
        /// AddNode will add a node to the node specified by inXmlNode in the treeview from the xml node specified in inTreeNode
        /// </summary>
        /// <param name="inXmlNode" description="the node in the xml document to add"></param>
        /// <param name="inTreeNode" description="the tree node to add this child to"></param>
        private void AddOS9Node(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            try
            {
                // default to 1 so we show the root node

                int nAttributes = 0x80;

                // if this is not the root node - get the attribute byte so we can see if this is a directory or a file node
                //
                //      all nodes have at least one attribute - that is the Real Name of the directory or file
                //      the root node only has one attribute

                try
                {
                    if (inXmlNode.Attributes.Count > 1)
                        nAttributes = Int32.Parse(inXmlNode.Attributes["attributes"].Value);

                    // Loop through the XML nodes until the leaf is reached. Add the nodes to the TreeView during the looping process.

                    //if (inXmlNode.HasChildNodes)
                    if ((nAttributes & 0x80) != 0)           // will be 1 if this is a directory
                    {
                        // handle node as a directory

                        nodeList = inXmlNode.ChildNodes;
                        for (i = 0; i <= nodeList.Count - 1; i++)
                        {
                            NodeAttributes attributes = new NodeAttributes();

                            xNode = inXmlNode.ChildNodes[i];                                // point xNode at the first child node to add
                            int index = inTreeNode.Nodes.Add(new TreeNode(xNode.Attributes["RealName"].Value));     // add this child node to the tree view
                            tNode = inTreeNode.Nodes[i];

                            try
                            {
                                if (xNode.Attributes.Count > 0)
                                {
                                    attributes.fileAttributes = Int32.Parse(xNode.Attributes["attributes"].Value);
                                    attributes.fileDesriptorSector = Int32.Parse(xNode.Attributes["FileDesriptorSector"].Value);
                                    inTreeNode.Nodes[index].ToolTipText = string.Format("File DescriptorSector = {0}", attributes.fileDesriptorSector);
                                    if (xNode.Attributes.Count > 1)
                                    {
                                        attributes.byteCount = Int32.Parse(xNode.Attributes["ByteCount"].Value);
                                        inTreeNode.Nodes[index].ToolTipText = string.Format("File DescriptorSector = {0} Byte Count = {1}", attributes.fileDesriptorSector, attributes.byteCount);
                                    }
                                }
                                else
                                {
                                    attributes.fileDesriptorSector = 0;
                                }
                            }
                            catch (Exception e3)
                            {
                                MessageBox.Show(e3.Message);
                            }

                            tNode.Tag = attributes;
                            AddOS9Node(xNode, tNode);
                        }
                    }
                    else
                    {
                        // handle node as a file
                        //
                        //      Here you need to pull the data from the XmlNode based on the type of node, whether attribute values are required, and so forth.

                        // inTreeNode.Text = (inXmlNode.OuterXml).Trim();
                    }
                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        /// <summary>
        /// AddUniFLEXNode will add a node to the node specified by inXmlNode in the treeview from the xml node specified in inTreeNode
        /// </summary>
        /// <param name="inXmlNode" description="the node in the xml document to add"></param>
        /// <param name="inTreeNode" description="the tree node to add this child to"></param>
        private void AddUniFLEXNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            try
            {
                // if this is not the root node - get the attribute byte so we can see if this is a directory or a file node
                //
                //      all nodes have at least ne attribute - that is the Real Name of the directory or file
                //      the root node only has one attribute

                try
                {
                    // in UniFLEX we will use the nodes count of the node to determine if this is Directory instead of the permissions bits like in OS9

                    if (inXmlNode.HasChildNodes)
                    {
                        // Loop through the XML nodes until the leaf is reached. Add the nodes to the TreeView during the looping process.

                        // handle node as a directory

                        nodeList = inXmlNode.ChildNodes;
                        for (i = 0; i <= nodeList.Count - 1; i++)
                        {
                            NodeAttributes attributes = new NodeAttributes();

                            xNode = inXmlNode.ChildNodes[i];                                                        // point xNode at the first child node to add
                            int index = inTreeNode.Nodes.Add(new TreeNode(xNode.Attributes["RealName"].Value));     // add this child node to the tree view
                            tNode = inTreeNode.Nodes[i];

                            try
                            {
                                if (xNode.Attributes["attributes"] != null) attributes.fileAttributes = Int32.Parse(xNode.Attributes["attributes"].Value); else attributes.fileAttributes = 0;
                                if (xNode.Attributes["blk"] != null) attributes.blk = Int32.Parse(xNode.Attributes["blk"].Value); else attributes.blk = 0;
                                if (xNode.Attributes["fdnIndex"] != null) attributes.fdnIndex = Int32.Parse(xNode.Attributes["fdnIndex"].Value); else attributes.fdnIndex = 0;

                                inTreeNode.Nodes[index].ToolTipText = string.Format("blk = {0}", attributes.blk);

                                if (xNode.Attributes["ByteCount"] != null) attributes.byteCount = Int32.Parse(xNode.Attributes["ByteCount"].Value); else attributes.byteCount = 0;
                                inTreeNode.Nodes[index].ToolTipText = string.Format("blk = {0} Byte Count = {1} {2}", attributes.blk, attributes.byteCount, (attributes.fileAttributes & 0x80) == 0 ? "file" : "directory");
                            }
                            catch (Exception e3)
                            {
                                MessageBox.Show(e3.Message);
                            }

                            tNode.Tag = attributes;
                            AddUniFLEXNode(xNode, tNode);
                        }
                    }
                    else
                    {
                        // handle node as a file
                        //
                        //      Here you need to pull the data from the XmlNode based on the type of node, whether attribute values are required, and so forth.

                        // inTreeNode.Text = (inXmlNode.OuterXml).Trim();
                    }

                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void AddMinixNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            try
            {
                // if this is not the root node - get the attribute byte so we can see if this is a directory or a file node
                //
                //      all nodes have at least ne attribute - that is the Real Name of the directory or file
                //      the root node only has one attribute

                try
                {
                    // in UniFLEX we will use the nodes count of the node to determine if this is Directory instead of the permissions bits like in OS9

                    if (inXmlNode.HasChildNodes)
                    {
                        // Loop through the XML nodes until the leaf is reached. Add the nodes to the TreeView during the looping process.

                        // handle node as a directory

                        nodeList = inXmlNode.ChildNodes;
                        for (i = 0; i <= nodeList.Count - 1; i++)
                        {
                            NodeAttributes attributes = new NodeAttributes();

                            xNode = inXmlNode.ChildNodes[i];                                                        // point xNode at the first child node to add
                            int index = inTreeNode.Nodes.Add(new TreeNode(xNode.Attributes["RealName"].Value));     // add this child node to the tree view
                            tNode = inTreeNode.Nodes[i];

                            try
                            {
                                if (xNode.Attributes["iNode"] != null) attributes.iNode = Int32.Parse(xNode.Attributes["iNode"].Value); else attributes.iNode = 0;
                                if (xNode.Attributes["ByteCount"] != null) attributes.byteCount = Int32.Parse(xNode.Attributes["ByteCount"].Value); else attributes.byteCount = 0;
                                if (xNode.Attributes["Mode"] != null) attributes.mode = Int32.Parse(xNode.Attributes["Mode"].Value); else attributes.byteCount = 0;

                                //inTreeNode.Nodes[index].ToolTipText = string.Format("blk = {0} Byte Count = {1} {2}", attributes.blk, attributes.byteCount, (attributes.fileAttributes & 0x80) == 0 ? "file" : "directory");
                            }
                            catch (Exception e3)
                            {
                                MessageBox.Show(e3.Message);
                            }

                            tNode.Tag = attributes;
                            AddMinixNode(xNode, tNode);
                        }
                    }
                    else
                    {
                        // handle node as a file
                        //
                        //      Here you need to pull the data from the XmlNode based on the type of node, whether attribute values are required, and so forth.

                        // inTreeNode.Text = (inXmlNode.OuterXml).Trim();
                    }

                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        string dsketteDescription = "";

        /// <summary>
        /// OpenTheFile
        /// 
        ///     This is where we open the .dsk floppy image file and assign the file format. It is also
        ///     the one and only place where we instantiate the virtualFloppyManipulationRoutines class.
        ///     
        ///     It is called from:
        ///         FrmFloppymaintDialog_Load   (1 place)
        ///         GetVirtualFloppy            (3 places)
        ///     
        /// </summary>
        private void OpenTheFile()
        {
            FileStream fp = null;
            bool canceled = false;

            // we are going to re-open the file = so close it if it is open

            if (virtualFloppyManipulationRoutines != null)
            {
                if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
                    virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream.Close();
            }

            if (File.Exists(cDrivePathName))
            {
                FileAttributes fa = File.GetAttributes(cDrivePathName);
                if (fa.HasFlag(FileAttributes.ReadOnly))
                {
                    // see if the user wants to open it in read only mode or if the user wishes to close it for editing, then re-open it when finished

                    DialogResult nResult = DialogResult.Yes;
                    //nResult = MessageBox.Show("Do you wish to view the file in Read/Only mode?\r\n\r\n" +
                    //                             "Click Yes = open in Read/Only mode.\r\n" +
                    //                             "Click No  = abort this operation.", "File is write protected", MessageBoxButtons.YesNo);

                    switch (nResult)
                    {
                        case DialogResult.Yes:
                            // see if we can open it in read binary mode
                            fp = File.Open(cDrivePathName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None); //err = fopen_s(&fp, (LPCTSTR)cDriveFileName, "rb");      
                            if (fp != null)
                            {
                                btnButtonImport.Enabled = false;
                                switch (labelFileFormat.Text)
                                {
                                    case "FLEX":
                                    case "FLEX_IDE":
                                    case "FLEX_IMA":
                                        btnButtonRefresh.Enabled = true;
                                        break;
                                    default:
                                        btnButtonRefresh.Enabled = false;
                                        break;
                                }

                                m_bOpenedReadOnly = true;

                                btnButtonChangeDisk.Text = "C&hange Disk";
                                InitFileListHeadings();

                                picStaticRwStatus.Visible = true;
                            }
                            else
                                MessageBox.Show("Unable to open file in read only mode, some other process must have it open unshared.");

                            break;

                        case DialogResult.No:
                            canceled = true;
                            break;
                    }
                }
                else
                {
                    try
                    {
                        // FileStream fs = File.Open(driveImagePaths[i], FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                        fp = File.Open(cDrivePathName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite); // see if we can open it in read/write binary mode
                        if (fp != null)
                        {
                            btnButtonImport.Enabled = false;
                            switch (labelFileFormat.Text)
                            {
                                case "FLEX":
                                case "FLEX_IDE":
                                case "FLEX_IMA":
                                    btnButtonRefresh.Enabled = true;
                                    break;
                                default:
                                    btnButtonRefresh.Enabled = false;
                                    break;
                            }

                            m_bOpenedReadOnly = false;

                            btnButtonChangeDisk.Text = "C&hange Disk";
                            InitFileListHeadings();
                        }
                        else
                        {
                            fp = File.Open(cDrivePathName, FileMode.Open, FileAccess.Read, FileShare.Read); // see if we can open it in read binary mode
                            if (fp != null)
                            {
                                btnButtonImport.Enabled = false;
                                switch (labelFileFormat.Text)
                                {
                                    case "FLEX":
                                    case "FLEX_IDE":
                                    case "FLEX_IMA":
                                        btnButtonRefresh.Enabled = true;
                                        break;
                                    default:
                                        btnButtonRefresh.Enabled = false;
                                        break;
                                }

                                m_bOpenedReadOnly = true;
                                picStaticRwStatus.Image = Properties.Resources.reddot;
                                picStaticRwStatus.Visible = true;

                                btnButtonChangeDisk.Text = "C&hange Disk";
                                InitFileListHeadings();
                            }
                            else
                                MessageBox.Show("Unable to open file.");
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            fp = File.Open(cDrivePathName, FileMode.Open, FileAccess.Read, FileShare.Read); // see if we can open it in read binary mode
                            if (fp != null)
                            {
                                btnButtonImport.Enabled = false;
                                switch (labelFileFormat.Text)
                                {
                                    case "FLEX":
                                    case "FLEX_IDE":
                                    case "FLEX_IMA":
                                        btnButtonRefresh.Enabled = true;
                                        break;
                                    default:
                                        btnButtonRefresh.Enabled = false;
                                        break;
                                }

                                m_bOpenedReadOnly = true;
                                picStaticRwStatus.Image = Properties.Resources.reddot;
                                picStaticRwStatus.Visible = true;

                                btnButtonChangeDisk.Text = "C&hange Disk";
                                InitFileListHeadings();
                            }
                            else
                            {
                                canceled = true;
                                MessageBox.Show("Unable to open file: " + ex.Message);
                            }
                        }
                        catch (Exception e)
                        {
                            canceled = true;
                            MessageBox.Show("Unable to open file: " + e.Message);
                        }
                    }

                }

                // Once we get here we have a file opened - hopefully it is a valid DSK file with a format that we can deal with.

                if (fp != null)
                {
                    if (!canceled)
                    {
                        // if there is HIER suff being handled - clear it out.

                        if (textBoxListViewCurrentPath.Visible)
                        {
                            textBoxListViewCurrentPath.Text = "";
                            textBoxListViewCurrentPath.Visible = false;
                            btnButtonBack.Visible = false;

                            lstviewListFiles.Height = lstviewListFiles.Height + 40;

                        }
                        hierDirectories.Clear();

                        // user selected a file and clicked open from the OPenFileDialog and if the files was read-only answer
                        // yes to the question - open in read only mode.

                        virtualFloppyManipulationRoutines = new VirtualFloppyManipulationRoutines(cDrivePathName, fp);
                        fileformat ff = virtualFloppyManipulationRoutines.CurrentFileFormat;

                        switch (ff)
                        {
                            #region FLEX format
                            case fileformat.fileformat_FLEX:
                            case fileformat.fileformat_FLEX_IMA:
                            case fileformat.fileformat_FLEX_IDE:
                                treeViewFiles.Visible = false;
                                lstviewListFiles.Visible = true;

                                labelFileFormat.Text = ff == fileformat.fileformat_FLEX ? "FLEX" : ff == fileformat.fileformat_FLEX_IMA ? "FLEX_IMA" : ff == fileformat.fileformat_FLEX_IDE ? "FLEX_IDE" : "UNKNOWN";
                                btnButtonRefresh.Enabled = true;

                                lblStatic3.Text = "Remaining Sectors";
                                if (fp != null)
                                {
                                    if (ff == fileformat.fileformat_FLEX_IDE)
                                    {
                                        // we have to get the user to tell us which partition to look at so we can calculate the virtualFloppyManipulationRoutines.PartitionBias.

                                        if (lstviewListFiles.Top == 102)
                                        {
                                            // only resize if it has not already been resized

                                            lstviewListFiles.Top = lstviewListFiles.Top + lstViewPartitions.Height + 4;
                                            lstviewListFiles.Height = lstviewListFiles.Height - lstViewPartitions.Height - 4;
                                        }

                                        lstviewListFiles.Visible = true;
                                        FillPartitionList(fp);

                                        lblStaticVolumeName.Text = "";
                                        lblStaticCreationDate.Text = "";
                                        lblStaticFileCount.Text = "";
                                        lblStaticRemainingSectors.Text = "";

                                        labelFileFormat.Text = "FLEX IDE";
                                        btnButtonRefresh.Enabled = true;
                                    }
                                    else
                                    {
                                        lstViewPartitions.Visible = false;

                                        if (lstviewListFiles.Top != 102)
                                        {
                                            // we need to reset the top of the list view for files. and adjust the height
                                            lstviewListFiles.Height = lstviewListFiles.Height + lstViewPartitions.Height + 4;
                                            lstviewListFiles.Top = 102;
                                        }

                                        RefreshList();
                                    }

                                    iamgeFileDirectoryDisplayed = true;
                                }
                                break;
                            #endregion

                            #region OS/9 format
                            case fileformat.fileformat_OS9:
                                lstviewListFiles.Visible = false;
                                treeViewFiles.ShowNodeToolTips = true;
                                treeViewFiles.Width = lstviewListFiles.Width;
                                treeViewFiles.Height = lstviewListFiles.Height;

                                // for now disable delete and import

                                btnButtonImport.Enabled = true;     // <- enable while working on implementing this feature
                                btnButtonDelete.Enabled = false;

                                treeViewFiles.Visible = true;

                                //if (fp != null)
                                //{
                                //    fp.Close();
                                //    fp = null;
                                //}

                                os9XmlDocument = virtualFloppyManipulationRoutines.LoadOS9DisketteImageFile(cDrivePathName);
                                dsketteDescription = virtualFloppyManipulationRoutines.strDescription;

                                if (os9XmlDocument.DocumentElement != null)
                                {
                                    string szVolumeName = os9XmlDocument.DocumentElement.Attributes["RealName"].Value;

                                    lblStaticVolumeName.Text = virtualFloppyManipulationRoutines.os9VolumeName;
                                    lblStaticCreationDate.Text = virtualFloppyManipulationRoutines.os9CreationDate;

                                    // os9SystemRequiredSectors is already factored in - their bits are marked used in the allocation map.
                                    //
                                    lblStatic3.Text = "Remaining Clusters";
                                    lblStaticRemainingSectors.Text = virtualFloppyManipulationRoutines.os9UnusedAllocationBits.ToString();
                                    lblStaticFileCount.Text = virtualFloppyManipulationRoutines.os9TotalFiles.ToString();

                                    string filename = Path.GetFileName(cDrivePathName);
                                    try
                                    {
                                        // SECTION 2. Initialize the TreeView control.

                                        treeViewFiles.Nodes.Clear();
                                        int index = treeViewFiles.Nodes.Add(new TreeNode(os9XmlDocument.DocumentElement.Attributes["RealName"].Value));

                                        // create a TreeNode to work with and assign it the node we just added

                                        TreeNode tNode = treeViewFiles.Nodes[index];

                                        NodeAttributes attributes = new NodeAttributes();
                                        if (os9XmlDocument.DocumentElement.Attributes.Count > 0)
                                        {
                                            attributes.fileAttributes = Int32.Parse(os9XmlDocument.DocumentElement.Attributes["attributes"].Value);
                                            attributes.fileDesriptorSector = Int32.Parse(os9XmlDocument.DocumentElement.Attributes["FileDesriptorSector"].Value);
                                            tNode.ToolTipText = string.Format("File DescriptorSector = {0}", attributes.fileDesriptorSector);
                                            if (os9XmlDocument.DocumentElement.Attributes.Count > 1)
                                            {
                                                attributes.byteCount = Int32.Parse(os9XmlDocument.DocumentElement.Attributes["ByteCount"].Value);
                                                tNode.ToolTipText = string.Format("File DescriptorSector = {0} Byte Count = {1}", attributes.fileDesriptorSector, attributes.byteCount);
                                            }
                                        }
                                        else
                                        {
                                            attributes.fileDesriptorSector = 0;
                                        }

                                        tNode.Tag = attributes;

                                        // SECTION 3. Populate the TreeView with the DOM nodes.

                                        AddOS9Node(os9XmlDocument.DocumentElement, tNode);
                                        treeViewFiles.Nodes[0].Expand();

                                        iamgeFileDirectoryDisplayed = true;
                                    }
                                    catch (XmlException xmlEx)
                                    {
                                        MessageBox.Show(xmlEx.Message);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                                labelFileFormat.Text = "OS-9";
                                btnButtonRefresh.Enabled = false;

                                break;
                            #endregion

                            #region UniFlex format
                            case fileformat.fileformat_UniFLEX:
                                lstviewListFiles.Visible = false;
                                iamgeFileDirectoryDisplayed = false;

                                treeViewFiles.ShowNodeToolTips = true;
                                treeViewFiles.Width = lstviewListFiles.Width;
                                treeViewFiles.Height = lstviewListFiles.Height;

                                // for now disable delete and import

                                btnButtonImport.Enabled = true;     // <- enable while working on implementing this feature
                                btnButtonDelete.Enabled = false;

                                treeViewFiles.Visible = true;

                                // This funtion will build the xml document used to load the tree view. It also builds the Description text block.

                                uniFLEXXmlDocument = virtualFloppyManipulationRoutines.LoadUniFLEXDisketteImageFile(cDrivePathName);
                                dsketteDescription = virtualFloppyManipulationRoutines.strDescription;

                                // If we were successfull, truck on and populate the tree view

                                if (uniFLEXXmlDocument.DocumentElement != null)
                                {
                                    string szVolumeName = uniFLEXXmlDocument.DocumentElement.Attributes["RealName"].Value;

                                    lblStaticVolumeName.Text = virtualFloppyManipulationRoutines.uniFLEXVolumeName;
                                    lblStaticCreationDate.Text = virtualFloppyManipulationRoutines.uniFLEXCreationDate;

                                    lblStatic3.Text = "Remaining Blocks";
                                    lblStaticRemainingSectors.Text = virtualFloppyManipulationRoutines.uniFLEXTotalFreeBlocks.ToString();
                                    lblStaticFileCount.Text = virtualFloppyManipulationRoutines.uniFLEXTotalFiles.ToString();

                                    string filename = Path.GetFileName(cDrivePathName);
                                    try
                                    {
                                        // SECTION 2. Initialize the TreeView control.

                                        treeViewFiles.Nodes.Clear();
                                        int index = treeViewFiles.Nodes.Add(new TreeNode(uniFLEXXmlDocument.DocumentElement.Attributes["RealName"].Value));

                                        // create a TreeNode to work with and assign it the node we just added

                                        TreeNode tNode = treeViewFiles.Nodes[index];

                                        NodeAttributes attributes = new NodeAttributes();
                                        if (uniFLEXXmlDocument.DocumentElement.Attributes.Count > 0)
                                        {
                                            try
                                            {
                                                if (uniFLEXXmlDocument.DocumentElement.Attributes["ByteCount"] != null)
                                                    attributes.byteCount = Int32.Parse(uniFLEXXmlDocument.DocumentElement.Attributes["ByteCount"].Value);
                                                else
                                                    attributes.byteCount = 0;
                                            }
                                            catch (Exception e)
                                            {
                                                MessageBox.Show(e.Message);
                                            }
                                        }
                                        else
                                        {
                                            attributes.blk = 0;
                                        }

                                        tNode.Tag = attributes;

                                        // SECTION 3. Populate the TreeView with the DOM nodes.

                                        try
                                        {
                                            AddUniFLEXNode(uniFLEXXmlDocument.DocumentElement, tNode);
                                            treeViewFiles.Nodes[0].Expand();

                                            iamgeFileDirectoryDisplayed = true;
                                        }
                                        catch (Exception e1)
                                        {
                                            MessageBox.Show(e1.Message);
                                        }
                                    }
                                    catch (XmlException xmlEx)
                                    {
                                        MessageBox.Show(xmlEx.Message);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }

                                labelFileFormat.Text = "UniFLEX";
                                btnButtonRefresh.Enabled = false;

                                //MessageBox.Show("UniFLEX File Format not yet supported");
                                break;
                            #endregion

                            #region MINIX format
                            case fileformat.fileformat_MINIX_68K:
                            case fileformat.fileformat_MINIX_IBM:
                                lstviewListFiles.Visible = false;
                                iamgeFileDirectoryDisplayed = false;

                                //treeViewFiles.ShowNodeToolTips = true;
                                treeViewFiles.Width = lstviewListFiles.Width;
                                treeViewFiles.Height = lstviewListFiles.Height;

                                // for now disable delete and import

                                btnButtonImport.Enabled = false;     // <- enable while working on implementing this feature
                                btnButtonDelete.Enabled = false;
                                btnButtonExport.Enabled = true;

                                treeViewFiles.Visible = true;

                                //lblStatic3.Text = "Remaining Sectors";

                                labelFileFormat.Text = "";
                                btnButtonRefresh.Enabled = false;

                                minixXmlDocument = virtualFloppyManipulationRoutines.minixImage.LoadMinixDisketteImageFile(cDrivePathName);

                                if (ff == fileformat.fileformat_MINIX_68K)
                                    labelFileFormat.Text = "minix 68K";
                                else if (ff == fileformat.fileformat_MINIX_IBM)
                                    labelFileFormat.Text = "minix IBM";

                                if (minixXmlDocument == null)
                                    MessageBox.Show(string.Format("FileFormat {0} is not yet supported.", ff == fileformat.fileformat_MINIX_68K ? "MINIX big endian" : "MINIX little endian"));
                                else
                                {
                                    try
                                    {
                                        // SECTION 2. Initialize the TreeView control.

                                        treeViewFiles.Nodes.Clear();
                                        int index = treeViewFiles.Nodes.Add(new TreeNode(minixXmlDocument.DocumentElement.Attributes["RealName"].Value));

                                        // create a TreeNode to work with and assign it the node we just added

                                        TreeNode tNode = treeViewFiles.Nodes[index];

                                        NodeAttributes attributes = new NodeAttributes();
                                        if (minixXmlDocument.DocumentElement.Attributes.Count > 0)
                                        {
                                            try
                                            {
                                                if (minixXmlDocument.DocumentElement.Attributes["ByteCount"] != null)
                                                    attributes.byteCount = Int32.Parse(minixXmlDocument.DocumentElement.Attributes["ByteCount"].Value);
                                                else
                                                    attributes.byteCount = 0;

                                                if (minixXmlDocument.DocumentElement.Attributes["iNode"] != null)
                                                    attributes.iNode = Int32.Parse(minixXmlDocument.DocumentElement.Attributes["iNode"].Value);
                                                else
                                                    attributes.iNode = 0;

                                                if (minixXmlDocument.DocumentElement.Attributes["Mode"] != null)
                                                    attributes.mode = Int32.Parse(minixXmlDocument.DocumentElement.Attributes["Mode"].Value);
                                                else
                                                    attributes.mode = 0;
                                            }
                                            catch (Exception e)
                                            {
                                                MessageBox.Show(e.Message);
                                            }
                                        }
                                        else
                                        {
                                            attributes.blk = 0;
                                        }

                                        tNode.Tag = attributes;

                                        // SECTION 3. Populate the TreeView with the DOM nodes.

                                        try
                                        {
                                            AddMinixNode(minixXmlDocument.DocumentElement, tNode);
                                            treeViewFiles.Nodes[0].Expand();
                                            iamgeFileDirectoryDisplayed = true;
                                        }
                                        catch (Exception e1)
                                        {
                                            MessageBox.Show(e1.Message);
                                        }
                                    }
                                    catch (XmlException xmlEx)
                                    {
                                        MessageBox.Show(xmlEx.Message);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                                break;
                            #endregion

                            case fileformat.fileformat_UNKNOWN:
                                lstviewListFiles.Visible = false;
                                iamgeFileDirectoryDisplayed = false;
                                lblStatic3.Text = "Remaining Sectors";

                                labelFileFormat.Text = "";
                                btnButtonRefresh.Enabled = false;

                                fp.Close();
                                fp = null;

                                MessageBox.Show("Unknown File Format");
                                break;
                        }

                        //virtualFloppyManipulationRoutines.currentFileFileFormat = ff;
                    }
                }
                else
                {
                    MessageBox.Show("Unable to open file: " + cDrivePathName);
                }
            }
            else
                BtnIdcancel_Click(null, null);

            SetButtons();
        }

        // Open the file and show it

        private void OpenTheFileAndShowIt(string SafeFileName, string FileName)
        {
            String strDefaultDir;
            String strKey;

            this.mruManager.AddRecentFile(FileName);

            strKey = string.Format("Default{0}Dir", dialogConfigType);
            strDefaultDir = Program.GetConfigurationAttribute("Directories", strKey, "");

            cDriveFileName = SafeFileName;                                 //        cDriveFileName  = pDlg.FileName ();   file name only - no path and includes extension
            cDriveFileTitle = Path.GetFileNameWithoutExtension(FileName);  //        cDriveFileTitle = pDlg.FileTitle ();  File name only - no path and no extension
            cDrivePathName = FileName;                                     //        cDrivePathName  = pDlg.FilePath ();   Path file name and extension

            strDefaultDir = cDrivePathName.Replace("\\" + cDriveFileName, "");

            XmlReader reader = null;
            FileStream xmlDocStream = null;

            try
            {
                xmlDocStream = File.OpenRead(Program.configFileName);
                reader = XmlReader.Create(xmlDocStream);
            }
            catch
            {

            }

            XmlDocument newDoc = null;

            if (reader != null)
            {
                XmlDocument doc = new XmlDocument();
                if (doc != null)
                {
                    doc.Load(reader);

                    strKey = string.Format("Default{0}Dir", dialogConfigType);
                    Program.SaveConfigurationAttribute(doc, "Directories", strKey, strDefaultDir);

                    newDoc = (XmlDocument)doc.Clone();
                }
                reader.Close();
                reader.Dispose();

                xmlDocStream.Close();

                newDoc.Save(Program.configFileName);

                bool bCancelled = false;

                if (!bCancelled)
                {
                    OpenTheFile();
                }
            }

            SetButtons();
        }

        private DialogResult GetVirtualFloppy(bool bJustRefreshFlag)
        {
            DialogResult nDlgReturn = DialogResult.Cancel;

            if (!bJustRefreshFlag)
            {
                // we get here ONLY when the Change Disk button is pressed

                String strDefaultDir;
                String strKey;

                strKey = string.Format("Default{0}Dir", dialogConfigType);
                strDefaultDir = Program.GetConfigurationAttribute("Directories", strKey, "");

                OpenFileDialog pDlg = new OpenFileDialog();     // true, ".dsk", null, 0, "Virtual Disk Files (*.dsk) | *.dsk||");

                pDlg.DefaultExt = ".dsk";
                pDlg.InitialDirectory = strDefaultDir;
                DialogResult dr = pDlg.ShowDialog();

                nDlgReturn = dr;

                if (dr == DialogResult.OK)
                {
                    OpenTheFileAndShowIt(pDlg.SafeFileName, pDlg.FileName);
                }
                else
                {
                    if (cDrivePathName.Length > 0)
                        OpenTheFile();
                }
                ShowVersionInTitle(cDriveFileTitle);
            }
            else
            {
                // Just do a refresh - this is where we get when the Refresh button is pushed as well.

                OpenTheFile();
                if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
                {
                    btnButtonImport.Enabled = true;
                    switch (labelFileFormat.Text)
                    {
                        case "FLEX":
                        case "FLEX_IDE":
                        case "FLEX_IMA":
                            btnButtonRefresh.Enabled = true;
                            break;
                        default:
                            btnButtonRefresh.Enabled = false;
                            break;
                    }

                    btnButtonChangeDisk.Text = "C&hange Disk";
                    InitFileListHeadings();
                    RefreshList();
                }

                ShowVersionInTitle(cDriveFileTitle);
            }

            SetButtons();

            return (nDlgReturn);
        }

        private int LocateSelectedFile()
        {
            int i, j = -1, nFileCount;

            for (i = 0, nFileCount = lstviewListFiles.Items.Count; i < nFileCount; i++)
            {
                if (lstviewListFiles.Items[i].Selected)
                {
                    j = i;
                    break;
                }
            }

            return j;
        }

        private bool CheckSelected(int nRow)
        {
            if (nRow < lstviewListFiles.Items.Count)
            {
                return lstviewListFiles.Items[nRow].Selected;
            }
            return false;
        }

        private void ReloadOptions()
        {
            // reload options from config file in case they were saved (with Apply) in the Option Dialog and them the user pressed cancel.

            m_nExpandTabs = Program.GetConfigurationAttribute("Global/FileMaintenance/FileExport/ExpandTabs", "enabled", "0") == "1" ? true : false;
            m_nAddLinefeed = Program.GetConfigurationAttribute("Global/FileMaintenance/FileExport/AddLinefeed", "enabled", "0") == "1" ? true : false;
            m_nCompactBinary = Program.GetConfigurationAttribute("Global/FileMaintenance/BinaryFile/CompactBinary", "enabled", "0") == "1" ? true : false;
            m_nStripLinefeed = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/StripLinefeed", "enabled", "0") == "1" ? true : false;
            m_nCompressSpaces = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/CompressSpaces", "enabled", "0") == "1" ? true : false;

            ConvertLfOnly = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/ConvertLfOnly", "enabled", "0") == "1" ? true : false;
            if (ConvertLfOnly)
            {
                ConvertLfOnlyToCrLf = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/ConvertLfOnlyToCrLf", "enabled", "0") == "1" ? true : false;
                ConvertLfOnlyToCr = Program.GetConfigurationAttribute("Global/FileMaintenance/FileImport/ConvertLfOnlyToCr", "enabled", "0") == "1" ? true : false;
            }
            else
            {
                ConvertLfOnlyToCrLf = false;
                ConvertLfOnlyToCr = false;
            }

            editor = Program.GetConfigurationAttribute("Global/FileMaintenance", "EditorPath", "");
            useExternalEditor = Program.GetConfigurationAttribute("Global/FileMaintenance", "UseExternalEditor", "N") == "Y" ? true : false;
            logOS9FloppyWrite = Program.GetConfigurationAttribute("Global/FileMaintenance", "LogOS9FloppyWrites", "N") == "Y" ? true : false;
            os9FloppyWritesFile = Program.GetConfigurationAttribute("Global/FileMaintenance", "os9FloppyWritesFile", "");
        }
        #endregion

        #region button handlers
        private void BtnIdcancel_Click(object sender, EventArgs e)
        {
            // make sure there are no active HIER directories. also make sure HIER controls are hidden
            // and restore size of List View

            hierDirectories.Clear();

            textBoxListViewCurrentPath.Visible = false;
            btnButtonBack.Visible = false;

            if (hierDirectories.Count > 0)
            { 
                // only do this if we have shortened the list view and made the back button visible for HEIR directories

                lstviewListFiles.Height = lstviewListFiles.Height + 40;
            }


            // close the file is it is open and set the stream to null so we know it is no longer valid to use it.

            if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
            {
                virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream.Close();
                virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream = null;
            }

            // clear the list view

            lstviewListFiles.Items.Clear();
            lstviewListFiles.Clear();

            // clear the tree view

            treeViewFiles.Nodes.Clear();

            // nothing is being displayed.

            iamgeFileDirectoryDisplayed = false;

            // make sure there is no valid virtual floppy

            SetButtons();

            labelFileFormat.Text = "";
            btnButtonRefresh.Enabled = false;

            cDriveFileName = "";
            ShowVersionInTitle(null);
        }

        private void BtnButtonChangeDisk_Click(object sender, EventArgs e)
        {
            // this will ask for a new file to open and refresh either the list view or the tree view depending on file format

            GetVirtualFloppy(false);
        }

        private void BtnButtonCreate_Click(object sender, EventArgs e)
        {
            frmDialogDiskParameters pdlg = new frmDialogDiskParameters();
            DialogResult r = pdlg.ShowDialog(this);
            if (pdlg.CreatedDisk)
            {
                if (cDrivePathName == "")
                {
                    cDrivePathName = pdlg.Filename;
                    cDriveFileTitle = Path.GetFileNameWithoutExtension(pdlg.Filename);
                    //RefreshList();
                }
            }
        }

        private void BtnButtonRefresh_Click(object sender, EventArgs e)
        {
            // this will reload the existing file and refresh either the list view or the tree view depending on file format

            GetVirtualFloppy(true);
        }

        private void BtnButtonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lstviewListFiles.Items)
            {
                lvi.Selected = true;
            }
        }

        private void BtnButtonDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lstviewListFiles.Items)
            {
                lvi.Selected = false;
            }
        }

        private void BtnButtonEditor_Click(object sender, EventArgs e)
        {
            editor = Program.GetConfigurationAttribute("Global/FileMaintenance", "EditorPath", "");
            useExternalEditor = Program.GetConfigurationAttribute("Global/FileMaintenance", "UseExternalEditor", "N") == "Y" ? true : false;

            frmDialogGetEditor pDlg = new frmDialogGetEditor();
            pDlg.editor = editor;
            pDlg.useExternalEditor = useExternalEditor;

            DialogResult dr = pDlg.ShowDialog();
            if (dr == DialogResult.OK)
            {
                editor = pDlg.editor;

                if (pDlg.useExternalEditor)
                    useExternalEditor = true;
                else
                    useExternalEditor = false;

                Program.SaveConfigurationAttribute("Global/FileMaintenance", "EditorPath", editor);
                Program.SaveConfigurationAttribute("Global/FileMaintenance", "UseExternalEditor", useExternalEditor ? "Y" : "N");
            }
        }

        private void BtnButtonOptions_Click(object sender, EventArgs e)
        {
            frmDialogOptions dlg = new frmDialogOptions();
            dlg.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = dlg.ShowDialog(this);

            ReloadOptions();
            if (virtualFloppyManipulationRoutines != null)
            {
                virtualFloppyManipulationRoutines.ReloadOptions();
            }
        }

        private void BtnButtonImport_Click(object sender, EventArgs e)
        {
            switch (virtualFloppyManipulationRoutines.CurrentFileFormat)
            {
                case fileformat.fileformat_FLEX:
                case fileformat.fileformat_FLEX_IDE:
                    ImportFLEXFileFormat(sender, e);
                    break;

                case fileformat.fileformat_OS9:
                    ImportOS9FileFormat(sender, e);
                    break;

                case fileformat.fileformat_UniFLEX:
                    ImportUniFLEXFileFormat(sender, e);
                    break;

                case fileformat.fileformat_FLEX_IMA:
                    ImportFLEXFileFormat(sender, e);
                    break;

                case fileformat.fileformat_MINIX_68K:
                    MessageBox.Show("File format MINIX Import not supported");
                    break;

                case fileformat.fileformat_MINIX_IBM:
                    MessageBox.Show("File format MINIX Import not supported");
                    break;

                default:
                    MessageBox.Show("File format Unknown Import not supported");
                    break;
            }
        }

        private void BtnButtonExport_Click(object sender, EventArgs e)
        {
            switch (virtualFloppyManipulationRoutines.CurrentFileFormat)
            {
                case fileformat.fileformat_FLEX:
                case fileformat.fileformat_FLEX_IDE:
                    ExportFLEXFileFormat(sender, e);
                    break;

                case fileformat.fileformat_OS9:
                    ExportOS9FileFormat(sender, e);
                    break;

                case fileformat.fileformat_UniFLEX:
                    ExportUniFLEXFileFormat(sender, e);
                    break;

                case fileformat.fileformat_FLEX_IMA:
                    ExportFLEXFileFormat(sender, e);
                    break;

                case fileformat.fileformat_MINIX_68K:
                    ExportMinixFileFormat(sender, e);
                    break;

                default:
                    MessageBox.Show("File format unknow - Export not supported");
                    break;
            }
        }

        private void PrepareImageForHier()
        {
            // re-write root directory sector with "home" as the directory name
        }

        private void BtnButtonNewDirectory_Click(object sender, EventArgs e)
        {
            // this is for making a new directory on an OS9 image

            if (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_OS9)
            {
                if (treeViewFiles.SelectedNode != null)
                {
                    TreeNode treeNode = treeViewFiles.SelectedNode;

                    if ((((NodeAttributes)treeNode.Tag).fileAttributes & 0x80) != 0)       // is this a directory?
                    {
                        frmNewDirectoryDialog pDlg = new frmNewDirectoryDialog();
                        DialogResult r = pDlg.ShowDialog();

                        if (r == DialogResult.OK)
                        {
                            if (pDlg.textBoxDirectoryName.Text.Length > 0 && pDlg.textBoxDirectoryName.Text.Length <= 29)
                            {
                                List<OS9_BYTES_TO_WRITE> bytesToWriteList = virtualFloppyManipulationRoutines.CreateOS9Directory(treeNode, os9XmlDocument, pDlg.textBoxDirectoryName.Text);
                                if (bytesToWriteList.Count > 0)
                                {
                                    virtualFloppyManipulationRoutines.WriteByteArrayToOS9Image(bytesToWriteList);

                                    // this will refresh the tree view and reload the file

                                    GetVirtualFloppy(true);
                                }
                            }
                        }
                    }
                    else
                        MessageBox.Show("You must have a directory selected not a file to create a new one.\r\n root is OK.");
                }
                else
                {
                    MessageBox.Show("You must have a directory selected to create a new one.\r\n root is OK.");
                }
            }
            else
            {
                if ((virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX_IMA) || (virtualFloppyManipulationRoutines.currentFileFileFormat == fileformat.fileformat_FLEX_IDE))
                {

                    // get the item selected in the list view

                    int nSelectedCount = 0;

                    if ((nSelectedCount = lstviewListFiles.SelectedItems.Count) > 1)
                    {
                        MessageBox.Show("Only a single item in the list can be selected to make a directory into");
                    }
                    else
                    {
                        // step 1 os to see if this image has already been preped to be an HIER diskette. This is done
                        // by readin the root dire sector and see if within the first 16 bytes, bytes 6-14 contain the
                        // word "home"

                        RAW_SIR stSystemInformationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR();
                        int nMaxSector = stSystemInformationRecord.cMaxSector;

                        FileStream m_fp = virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream;
                        RAW_DIR_SECTOR stDirSector = virtualFloppyManipulationRoutines.ReadFLEX_DIR_SECTOR(m_fp, 0, 5, false);

                        bool proceed = true;
                        bool cancel = false;

                        string dirName = ASCIIEncoding.ASCII.GetString(stDirSector.header.name).TrimEnd('\0');
                        if (dirName != "home")
                        {
                            DialogResult r = MessageBox.Show("This is not an HIER prepared diskette image\r\nWould you like to prepare this image for HIER?", "Prepare image for HIER", MessageBoxButtons.YesNoCancel);
                            switch (r)
                            {
                                case DialogResult.Yes:
                                    PrepareImageForHier();
                                    proceed = true;
                                    break;

                                case DialogResult.No:
                                    proceed = false;
                                    break;

                                case DialogResult.Cancel:
                                    proceed = false;
                                    cancel = true;
                                    break;
                                default:
                                    proceed = false;
                                    cancel = true;
                                    break;
                            }
                        }

                        if (proceed)
                        {
                            // now make sure it is an HIER directory that we are adding to - or the rrot of an HIER image

                            int nSelectedFile = LocateSelectedFile();
                            if (nSelectedFile == -1)
                            {
                                // there is no file/directory selected in the list view - must be the root of this level.
                                //
                                //      we might be at the root of the diskette or at a specifc sub directory.
                                //      we can tell this by looking at the text in the text in the list
                                //      textBoxListViewCurrentPath. If this is empty we are at the root
                                //      directory (track 0, sector 5), otherwise we are already in a directory
                                //      in the list view and now sub directory is selected

                                if (textBoxListViewCurrentPath.Text.Length == 0)
                                {
                                    // we are at the diskette root directory
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                // we need to see if the selected item in the list view is a directory or a file.
                                // if it is a file - alert the user and do not allow adding the directory.

                                DIR_ENTRY dirEntry = (DIR_ENTRY)lstviewListFiles.Items[nSelectedFile].Tag;
                                if (dirEntry.isHierDirectoryEntry)
                                {
                                    // this is a directory - so get the track and sector that it starts on.

                                    byte startTrack = dirEntry.cEndTrack;
                                    byte startSector = dirEntry.cStartSector;

                                    // make sure we have at least 4 sectors left on the disk to create the directory with

                                    if (virtualFloppyManipulationRoutines.freeChain.Count >= 4)
                                    {
                                        // find an empty direntry to create this dir in.

                                        // then get 4 sectors to create the directory

                                        // initialize the four sectors and write them to the image
                                        //  be sure to set the sequence numbers in the sectors
                                        //  they should already be linked.

                                        // update the free chain

                                        //
                                        //
                                    }
                                    else
                                        MessageBox.Show("Insufficient space to create new directory");
                                }
                                else
                                    MessageBox.Show("Cannot make a new directory in a file");
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("This is not an OS9 or FLEX image.");
            }
        }

        private void btnButtonImportDirectory_Click(object sender, EventArgs e)
        {
            if (treeViewFiles.SelectedNode != null)
            {
                TreeNode treeNode = treeViewFiles.SelectedNode;

                if ((((NodeAttributes)treeNode.Tag).fileAttributes & 0x80) != 0)       // is this a directory?
                {
                    // we can only import a directory into another directory (might be root)

                    // prompt user for directory structure to import

                    folderBrowserDialogImportDirectory.ShowDialog();

                }
                else
                    MessageBox.Show("You must have a directory selected not a file to create import a directory into.\r\n root is OK.");
            }
            else
            {
                MessageBox.Show("You must have a directory selected to import a directory into.\r\n root is OK.");
            }
        }

        private void BtnButtonDelete_Click(object sender, EventArgs e)
        {
            switch (virtualFloppyManipulationRoutines.currentFileFileFormat)
            {
                case fileformat.fileformat_FLEX:
                    DeleteFLEXFileFromImage(sender, e);
                    break;

                case fileformat.fileformat_OS9:
                    DeleteOS9ileFromImage(sender, e);
                    break;

                case fileformat.fileformat_UniFLEX:
                    MessageBox.Show("File format UniFLEX - Delete not supported");
                    break;

                case fileformat.fileformat_FLEX_IMA:
                    DeleteFLEXFileFromImage(sender, e);
                    break;

                default:
                    MessageBox.Show("File format Unknown cannot Delete");
                    break;
            }
        }
        #endregion

        #region Menu Handlers
        private void toolStripMenuItemFileOpen_Click(object sender, EventArgs e)
        {
            BtnButtonChangeDisk_Click(sender, e);
        }

        private void toolStripMenuItemFileNew_Click(object sender, EventArgs e)
        {
            BtnButtonCreate_Click(sender, e);
        }

        private void toolStripMenuItemFileClose_Click(object sender, EventArgs e)
        {
            BtnIdcancel_Click(sender, e);
        }

        private void toolStripMenuItemFileRefresh_Click(object sender, EventArgs e)
        {
            BtnButtonRefresh_Click(sender, e);
        }

        private void toolStripMenuItemFileExit_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItemEditSelectAll_Click(object sender, EventArgs e)
        {
            BtnButtonSelectAll_Click(sender, e);
        }

        private void toolStripMenuItemEditDeselectAll_Click(object sender, EventArgs e)
        {
            BtnButtonDeselectAll_Click(sender, e);
        }

        private void toolStripMenuItemToolsEditor_Click(object sender, EventArgs e)
        {
            BtnButtonEditor_Click(sender, e);
        }

        private void toolStripMenuItemToolsOptions_Click(object sender, EventArgs e)
        {
            BtnButtonOptions_Click(sender, e);
        }

        private void toolStripMenuItemImageFileImport_Click(object sender, EventArgs e)
        {
            BtnButtonImport_Click(sender, e);
        }

        private void toolStripMenuItemImageFileExport_Click(object sender, EventArgs e)
        {
            BtnButtonExport_Click(sender, e);
        }

        private void toolStripMenuItemImageFileCreateDirectory_Click(object sender, EventArgs e)
        {
            BtnButtonNewDirectory_Click(sender, e);
        }

        private void toolStripMenuItemImageFileImportDirectory_Click(object sender, EventArgs e)
        {
            btnButtonImportDirectory_Click(sender, e);
        }

        private void toolStripMenuItemImageFileDelete_Click(object sender, EventArgs e)
        {
            BtnButtonDelete_Click(sender, e);
        }
        #endregion

        private void ImportFLEXFileFormat(object sender, EventArgs e)
        {
            String strDefaultDir;
            String strKey;

            // importing to a HIER directory is not yet supported.

            if (hierDirectories.Count == 0)
            {
                strKey = string.Format("Default{0}ImportDir", dialogConfigType);
                strDefaultDir = Program.GetConfigurationAttribute("Directories", strKey, "");

                OpenFileDialog pDlg = new OpenFileDialog();     // true, ".dsk", null, 0, "Virtual Disk Files (*.dsk) | *.dsk||");
                pDlg.DefaultExt = ".*";
                pDlg.Multiselect = true;
                pDlg.InitialDirectory = strDefaultDir;
                DialogResult dr = pDlg.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    virtualFloppyManipulationRoutines.numberOfFileImported = 0;

                    foreach (string fileName in pDlg.FileNames)
                    {
                        //using (StreamReader reader = new StreamReader(File.Open(pDlg.FileName, FileMode.Open, FileAccess.Read, FileShare.None)))
                        //{
                        //    string _textToDisplay = reader.ReadToEnd();
                        //    string safeFileName = Path.GetFileName(fileName);

                        //  //virtualFloppyManipulationRoutines.WriteFileToImage(dialogConfigType, pDlg.SafeFileName, ASCIIEncoding.ASCII.GetBytes(_textToDisplay));
                        //    virtualFloppyManipulationRoutines.WriteFileToImage(dialogConfigType, safeFileName, ASCIIEncoding.ASCII.GetBytes(_textToDisplay));
                        //}
                        using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            byte[] fileContent = reader.ReadBytes((int)reader.BaseStream.Length);
                            string safeFileName = Path.GetFileName(fileName);

                            //virtualFloppyManipulationRoutines.WriteFileToImage(dialogConfigType, pDlg.SafeFileName, ASCIIEncoding.ASCII.GetBytes(_textToDisplay));
                            virtualFloppyManipulationRoutines.WriteFileToImage(dialogConfigType, safeFileName, fileContent);
                        }

                    }
                    RefreshList();
                }
            }
            else
                MessageBox.Show("Importing to HIER Sub Directory is not yet supported");
        }

        class FileToDelete
        {
            public string filename;
            public int numberOfClustersUsed;
        }

        List<OS9FileToCopy> filesToCopy = new List<OS9FileToCopy>();
        List<FileToDelete> filesToDelete = new List<FileToDelete>();

        private void ImportOS9FileFormat(object sender, EventArgs e)
        {


            // step one is determine what files are to be imported. We will do this with a OpenFile dialog
            //
            //      If the shift key is depressed when the Import button is clicked, the user will be presented 
            //      with an directory selection dialog instead of a file selection dialog
            //
            //  But before we begin, lets make sure that the user has selected a target directory for the file(s)
            //  we are about to import

            TreeNode node = treeViewFiles.SelectedNode;

            if (node != null)
            {
                if ((((NodeAttributes)node.Tag).fileAttributes & 0x80) != 0)       // is this a directory?
                {

                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        // do complete Folder import recursively into folder selected in the tree view (allow multi folder select)
                    }
                    else
                    {
                        // do import from single folder into folder selected in the tree view (allow multi file select)

                        String strDefaultDir;
                        String strKey;

                        strKey = string.Format("Default{0}ImportDir", dialogConfigType);
                        strDefaultDir = Program.GetConfigurationAttribute("Directories", strKey, "");

                        OpenFileDialog pDlg = new OpenFileDialog();     // true, ".dsk", null, 0, "Virtual Disk Files (*.dsk) | *.dsk||");
                        pDlg.DefaultExt = ".*";
                        pDlg.Multiselect = true;
                        pDlg.InitialDirectory = strDefaultDir;
                        DialogResult dr = pDlg.ShowDialog();

                        if (dr == DialogResult.OK)
                        {
                            string[] selectedFiles = pDlg.FileNames;

                            // first calculate the size of each file and make sure we have enough room left in the allocation map
                            // to be able to add all these files to the image

                            int bytesPerCluster = (int)(virtualFloppyManipulationRoutines.os9SectorsPerCluster * virtualFloppyManipulationRoutines.sectorSize);

                            filesToCopy.Clear();
                            filesToDelete.Clear();

                            // build a list of the files to copy along with their file size and the required number of clusters.

                            int totalClustersRequired = 0;
                            int totalClustersReclaimed = 0;

                            // calculate the total number of clusters required to do this copy as well as the total number
                            // of cluster that will be reclaimed. In addition - fill in the filesToCopy and FilesToDelete
                            // lists.

                            foreach (string file in selectedFiles)
                            {
                                OS9FileToCopy fileToCopy = new OS9FileToCopy();

                                fileToCopy.filename = file;
                                fileToCopy.safeFilename = Path.GetFileName(file);

                                FileInfo fi = new FileInfo(file);
                                fileToCopy.fileSize = fi.Length;
                                fileToCopy.requiredNumberOfClusters = (int)(fileToCopy.fileSize / bytesPerCluster);

                                // if there is bytes left over - add another cluster to put them in.

                                if (fileToCopy.fileSize % bytesPerCluster > 0)
                                    fileToCopy.requiredNumberOfClusters++;

                                totalClustersRequired += fileToCopy.requiredNumberOfClusters;

                                //  now lets see if this will be getting replaced in the diskette image file if it is
                                //  we can use the clusters that this file is using, because we will need to be deleting
                                //  it before we copy the replacement.

                                string justTheFilename = Path.GetFileName(file);
                                //string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace(@"\", "/").Replace(" ", "_");
                                string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace("\\", "/").Replace(" ", "_").Replace("(", "_").Replace(")", "_");
                                bool isLetter = !String.IsNullOrEmpty(nodeToSearchFor) && Char.IsLetter(nodeToSearchFor[0]);
                                if (!isLetter)
                                    nodeToSearchFor = "_" + nodeToSearchFor;

                                // see if the file already exists - if it does add it to files to delete

                                XmlNodeList nodes = os9XmlDocument.SelectNodes(nodeToSearchFor);
                                if (nodes.Count > 0)
                                {
                                    // if there is one - there will only be one - so we can safely assume that nodes[0] is the node we are looking at

                                    FileToDelete fileToDelete = new FileToDelete();
                                    fileToDelete.filename = nodeToSearchFor;

                                    int byteCount = Int32.Parse(nodes[0].Attributes["ByteCount"].Value);
                                    fileToDelete.numberOfClustersUsed = byteCount / bytesPerCluster;
                                    if (byteCount % bytesPerCluster > 0)
                                        fileToDelete.numberOfClustersUsed++;

                                    totalClustersReclaimed += fileToDelete.numberOfClustersUsed;

                                    filesToDelete.Add(fileToDelete);
                                }

                                // now add it to filesToCopy List

                                filesToCopy.Add(fileToCopy);
                            }

                            //  now we need to subtract the number of clusters we will be reclaiming from the number required for the files we will be writng
                            //  to see if we will have enough clusters to write these files

                            int actualClustersAvailable = virtualFloppyManipulationRoutines.os9UnusedAllocationBits + totalClustersReclaimed;

                            // now see if we have enough clusters available on the diskette image file.

                            if (totalClustersRequired <= actualClustersAvailable)
                            {
                                // we do - proceed

                                // see if any of the files we are going to copy from the source already exist on the target

                                bool filesExist = false;

                                foreach (OS9FileToCopy fileToCopy in filesToCopy)
                                {
                                    string justTheFilename = Path.GetFileName(fileToCopy.filename);
                                    //string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace(@"\", "/").Replace(" ", "_");
                                    string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace("\\", "/").Replace(" ", "_").Replace("(", "_").Replace(")", "_");
                                    bool isLetter = !String.IsNullOrEmpty(nodeToSearchFor) && Char.IsLetter(nodeToSearchFor[0]);
                                    if (!isLetter)
                                        nodeToSearchFor = "_" + nodeToSearchFor;

                                    XmlNodeList nodes = os9XmlDocument.SelectNodes(nodeToSearchFor);
                                    if (nodes.Count > 0)
                                    {
                                        fileToCopy.fileExists = true;
                                        filesExist = true;
                                    }
                                }

                                // Now only copy the files that do not already exist on the target.

                                foreach (OS9FileToCopy fileToCopy in filesToCopy)
                                {
                                    if (!fileToCopy.fileExists)
                                    {
                                        string justTheFilename = Path.GetFileName(fileToCopy.filename);
                                        //string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace(@"\", "/").Replace(" ", "_");
                                        string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace("\\", "/").Replace(" ", "_").Replace("(", "_").Replace(")", "_");
                                        bool isLetter = !String.IsNullOrEmpty(nodeToSearchFor) && Char.IsLetter(nodeToSearchFor[0]);
                                        if (!isLetter)
                                            nodeToSearchFor = "_" + nodeToSearchFor;

                                        // we have already determined that this file does not exist on the target - just copy it.

                                        FileInfo fi = new FileInfo(fileToCopy.filename);

                                        int filesize = (int)fi.Length;

                                        BinaryReader reader = new BinaryReader(File.Open(fileToCopy.filename, FileMode.Open, FileAccess.Read, FileShare.None));
                                        byte[] fileData = reader.ReadBytes(filesize);
                                        reader.Close();

                                        virtualFloppyManipulationRoutines.WriteFileToImage(dialogConfigType, fileToCopy.safeFilename, fileData, nodeToSearchFor);

                                        // reload the image to refresh the XML and the treeview as well as the SIR in the virtualFloppyManipulationRoutines

                                        os9XmlDocument = virtualFloppyManipulationRoutines.LoadOS9DisketteImageFile(cDrivePathName);
                                    }
                                }

                                // if any of the files exist, ask operator what to do for each or all files with frmFileExistsDialog
                                //
                                //      this will either set skipFile to true or leave it as false for the files that already exist on the target.
                                //      the files being copied from the source that do not already exist on the target will still have skipFile
                                //      set to false and will be copied.

                                bool ignoreSkipCopy = false;
                                bool cancelReplace = false;

                                if (filesExist)
                                {
                                    //  One or more of the files selected for copy already exist on the target file system. 
                                    //  If you  continue, the files on the target will first be deleted and them copied from the source. 
                                    //  Do you wish to continue by replacing the files?
                                    //
                                    //      Yes         will copy this one file 
                                    //      Yes to All  will copy All files
                                    //      No          will skip this one file
                                    //      Cancel      will copy no files.

                                    frmDialogFileExists fileExistsDialog = new frmDialogFileExists(filesToCopy);
                                    DialogResult r = fileExistsDialog.ShowDialog();
                                    {
                                        switch (r)
                                        {
                                            case DialogResult.Cancel:   // do not copy any existing files
                                                cancelReplace = true;
                                                break;
                                            case DialogResult.OK:       // let the dialog decide what to skip by replacing our copy of filesToCopy from the dialog's modified one
                                                filesToCopy = fileExistsDialog.filesToCopy;
                                                break;
                                            case DialogResult.Yes:      // Yes - ignore the skipCopy property on all files in the list
                                                ignoreSkipCopy = true;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }

                                //
                                //OS9_FILE_DESCRIPTOR
                                //
                                //        public byte cATT;       // File Attributes                              0

                                //        public byte cOWN;       // File Owner's User ID                         1
                                //        public byte cOWN1;      //                                              2

                                //        public byte cDAT;       // Date and Time Last Modified Y:M:D:H:M        3
                                //        public byte cDAT1;      //                                              4
                                //        public byte cDAT2;      //                                              5
                                //        public byte cDAT3;      //                                              6
                                //        public byte cDAT4;      //                                              7

                                //        public byte cLNK;       // Link Count                                   8

                                //        public byte cSIZ;       // File Size                                    9
                                //        public byte cSIZ1;      //                                              A
                                //        public byte cSIZ2;      //                                              B
                                //        public byte cSIZ3;      //                                              C

                                //        public byte cDCR;       // Date Created Y M D                           D
                                //        public byte cDCR1;      //                                              E
                                //        public byte cDCR2;      //                                              F

                                //        public byte[] cSEG = new byte[240];                // segment list     10
                                //        public ArrayList alSEGArray = new ArrayList();
                                //
                                //  Writes to /d1 when copying files from /d2 (this file copy requires a new segment be added to the directory
                                //  it is beinf copied into on the target (/d1)
                                //
                                //  1.  The allocation table to allocate more sectors to the /d1/src/ved disecttory
                                //
                                //      allocation map at disk offset 0x0100 before using OS9 to copy file to new dir secgment:
                                //
                                //          FF FF FF FF FF FF FF FF  FF FF FF FF FF E0
                                //
                                //      allocation map at disk offset 0x0100 After using OS9 to copy file to new dir secgment:
                                //
                                //          FF FF FF FF  FF FF FF FF   FF FF FF FF  FF F0 00 00
                                //
                                //  2.  The allocation tabel to get sectors to store the file and it's descriptor
                                //
                                //      allocation map at disk offset 0x0100 After using OS9 to copy file to new dir secgment:
                                //
                                //          FF FF FF FF  FF FF FF FF   FF FF FF FF  FF FF F0 00 
                                //
                                //  3.  The file descriptor for the ved directory ay 0x0F00
                                //
                                //      file descriptor of ved directory file before
                                //
                                //          BF 00 00 15 03 02 0F 1E  01 00 00 01 00 15 03 02  00 00 10 00 01 00 00 00  00 00 00 00 00 00 00 00
                                //
                                //      file descriptor of ved directory file after                  new block for more dir entries
                                //                                                                           ---------------       
                                //          BF 00 00 15 03 02 0F 1E  01 00 00 09 00 15 03 02  00 00 10 00 01 00 00 6C  00 08 00 00 00 00 00 00
                                //
                                //      it got written a second time to change the file size bytes from 0x00000900 to 0x00000120 (why smaller - because 900 was wrong.)
                                //
                                //          BF 00 00 15 03 02 0F 1E  01 00 00 01 20 15 03 02  00 00 10 00 01 00 00 6C  00 08 00 00 00 00 00 00
                                //
                                //  4.  Write new dir entry for file descriptor of the file being copied into newly created segment for ved directory (this is at offset 0x006C00)
                                //
                                //          53 55 42 53  2E C3 00 00   00 00 00 00  00 00 00 00   00 00 00 00  00 00 00 00   00 00 00 00  00 00 00 6B (zeros follow)
                                //
                                //  5.  Apparently the block allocated for the file's descriptor were allocated before the extra segment for the direntry since
                                //      the offset to the direntry is past the offset for the files' file descriptor - interesting. So now we write the new file's
                                //      file descriptor to LSN 0x6B specified in the above directory entry (step 4). This gets written to offset 0x00006B00 in the
                                //      imafe file.
                                //
                                //          3B 00 00 15  03 03 05 35   01 00 00 00  00 15 03 03   00 00 00 00  00 00 00 00   00 00 00 00  00 00 00 00 ...
                                //
                                //  6.  Now we allocate blocks for the actuall file which causes us to write the following to the allocation map at 0x0100
                                //
                                //      was:    FF FF FF FF  FF FF FF FF   FF FF FF FF  FF FF F0 00   00 00 00 00  00 00 00 00   00 00 00 00  00 00 00 00 ...
                                //      now:    FF FF FF FF  FF FF FF FF   FF FF FF FF  FF FF FF FF   FF E0 00 00  00 00 00 00   00 00 00 00  00 00 00 00 ....
                                //                                                             - --   -- -
                                //      allocated an additional 23 blocks for the file
                                //
                                //  7.  re-write the file descriptor to LSN 0x6B 
                                //
                                //      was:    3B 00 00 15  03 03 05 35   01 00 00 00  00 15 03 03   00 00 00 00  00 00 00 00   00 00 00 00  00 00 00 00 ...
                                //      now:    3B 00 00 15  03 03 05 35   01 00 00 17  00 15 03 03   00 00 74 00  17 00 00 00   00 00 00 00  00 00 00 00 ...
                                //                                            ^                       ^        ^
                                //              added the new size            --------- -- (bytes)    |        |
                                //              added the LSN pointer to the first data LSN           -------- |
                                //              added the number of contiguoue blocks to segment               --  --
                                //
                                //  8.  Then we write the 0x0017 blocks to the file starting at 0x00007400
                                //
                                //  9.  After we have written all of the sectors, we update the file descriptor at 0x00006B00
                                //
                                //      was:    3B 00 00 15  03 03 05 35   01 00 00 17  00 15 03 03   00 00 74 00  17 00 00 00   00 00 00 00  00 00 00 00 ...
                                //      now:    3B 00 00 15  03 03 05 35   01 00 00 16  4E 15 03 03   00 00 74 00  17 00 00 00   00 00 00 00  00 00 00 00 ...
                                //                                            -- -- --  --
                                //              change the file size to the actual number of bytes in the file (not the number of bytes allocated)
                                //
                                //  10. And for some reason write the file descriptor one more time with no changes.
                                //
                                //      was:    3B 00 00 15  03 03 05 35   01 00 00 16  4E 15 03 03   00 00 74 00  17 00 00 00   00 00 00 00  00 00 00 00 ...
                                //      now:    3B 00 00 15  03 03 05 35   01 00 00 16  4E 15 03 03   00 00 74 00  17 00 00 00   00 00 00 00  00 00 00 00 ...
                                //
                                //          ALL DONE

                                foreach (OS9FileToCopy fileToCopy in filesToCopy)
                                {
                                    // If the file did not already exist on the target when we started - it will have been already copied in a previous step

                                    if (!cancelReplace && fileToCopy.fileExists && (!fileToCopy.skipCopy || ignoreSkipCopy))
                                    {
                                        //  as we copy the files, before we do - see if this file already exists
                                        //  in the same directory path on the image - if it does - delete it and
                                        //  reclaim the clusters by modifying the allocation map.

                                        string justTheFilename = Path.GetFileName(fileToCopy.filename);
                                        //string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace(@"\", "/").Replace(" ", "_");
                                        string nodeToSearchFor = Path.Combine(node.FullPath, justTheFilename).Replace("\\", "/").Replace(" ", "_").Replace("(", "_").Replace(")", "_");
                                        bool isLetter = !String.IsNullOrEmpty(nodeToSearchFor) && Char.IsLetter(nodeToSearchFor[0]);
                                        if (!isLetter)
                                            nodeToSearchFor = "_" + nodeToSearchFor;

                                        XmlNodeList nodes = os9XmlDocument.SelectNodes(nodeToSearchFor);
                                        if (nodes.Count > 0)
                                        {
                                            // this files is already on the image disk in the same directory path of the file we are going to copy.
                                            // it must be deleted first.

                                            List<OS9_BYTES_TO_WRITE> bytesToWrite = virtualFloppyManipulationRoutines.DeleteOS9File(nodes[0]);
                                            virtualFloppyManipulationRoutines.WriteByteArrayToOS9Image(bytesToWrite);

                                            // reload the image to refresh the XML and the treeview as well as the SIR in the virtualFloppyManipulationRoutines

                                            os9XmlDocument = virtualFloppyManipulationRoutines.LoadOS9DisketteImageFile(cDrivePathName);
                                        }

                                        StreamReader reader = new StreamReader(File.Open(fileToCopy.filename, FileMode.Open, FileAccess.Read, FileShare.None));
                                        string _textToDisplay = reader.ReadToEnd();
                                        reader.Close();

                                        virtualFloppyManipulationRoutines.WriteFileToImage(dialogConfigType, fileToCopy.safeFilename, ASCIIEncoding.ASCII.GetBytes(_textToDisplay), nodeToSearchFor);

                                        // reload the image to refresh the XML and the treeview as well as the SIR in the virtualFloppyManipulationRoutines

                                        os9XmlDocument = virtualFloppyManipulationRoutines.LoadOS9DisketteImageFile(cDrivePathName);
                                    }
                                }

                                // We need to refresh the treview here - GetVirtualFloppy(true) will do it

                                GetVirtualFloppy(true);
                            }
                            else
                            {
                                string message = string.Format(
@"
Insufficient space on diskette image to import files selected

    totalClustersRequired:      {0}
    totalClustersReclaimed:     {1}
    actualClustersAvailable:    {2}
", totalClustersRequired, totalClustersReclaimed, actualClustersAvailable);

                                MessageBox.Show(message);
                            }
                        }
                    }
                }
                else
                    MessageBox.Show("You must have a directory in the tree view selected (this can also be the diskette name (root))");
            }
            else
                MessageBox.Show("You must have a directory in the tree view selected (this can also be the diskette name (root))");
        }

        private void ImportUniFLEXFileFormat(object sender, EventArgs e)
        {
            MessageBox.Show("File format UniFLEX Import not supported");
        }

        /// <summary>
        /// ExportSingleOS9File 
        /// </summary>
        /// <param name="fullTargetDirectoryPath"   description="full path to the directory where the file is coming from"                                                  ></param>
        /// <param name="innerSourcePath"           description="path from root where the file is to written to OS9 diskette image"                                         ></param>
        /// <param name="treeviewSelectedNodeTag"   description="the Tag assigned to the selected tree view node - contains fileDesriptorSector, byteCount, fileAttributes" ></param>
        /// <param name="selectedNodeFullPath       "></param>
        private void ExportSingleOS9File(string fullTargetDirectoryPath, string innerSourcePath, NodeAttributes treeviewSelectedNodeTag, string selectedNodeFullPath)
        {
            if (!Directory.Exists(fullTargetDirectoryPath))
                Directory.CreateDirectory(fullTargetDirectoryPath);

            //FileStream fs = File.Open(cDrivePathName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            //FileStream fs = virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream;
            try
            {
                if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
                {
                    OS9_FILE_DESCRIPTOR fd = new OS9_FILE_DESCRIPTOR();

                    int nAttributes = treeviewSelectedNodeTag.fileAttributes;
                    int firstFileDescriptorLSN = treeviewSelectedNodeTag.fileDesriptorSector;
                    long nByteCount = treeviewSelectedNodeTag.byteCount;
                    int nFileDescriptorOffset = treeviewSelectedNodeTag.fileDesriptorSector * virtualFloppyManipulationRoutines.nLSNBlockSize;

                    //virtualFloppyManipulationRoutines.GetOS9FileDescriptor(fs, ref fd, nFileDescriptorOffset);
                    virtualFloppyManipulationRoutines.GetOS9FileDescriptor(ref fd, nFileDescriptorOffset);
                    string currentPath = Path.GetDirectoryName(innerSourcePath);
                    string strFilename = Path.GetFileName(selectedNodeFullPath);

                    try
                    {
                        //virtualFloppyManipulationRoutines.SaveOS9ImageFiles(fs, fd, nAttributes, currentPath, strFilename, firstFileDescriptorLSN, nByteCount);
                        virtualFloppyManipulationRoutines.SaveOS9ImageFiles(fd, nAttributes, currentPath, strFilename, firstFileDescriptorLSN, nByteCount);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    //fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportOS9Directory(TreeNode treeNode, string defaultDir, string fullTargetDirectoryPath, string innerSourcePath, NodeAttributes treeviewSelectedNodeTag, string selectedNodeFullPath)
        {
            // first make sure that this directory exists - ExportSingleOS9File will just create the directory on the PC is we pass it a directory with no filename

            //ExportSingleOS9File(fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);

            // now iterate through all the direftories and files in this directory recursively

            try
            {
                for (int nodeIndex = 0; nodeIndex < treeNode.Nodes.Count; nodeIndex++)
                {
                    TreeNode childNode = treeNode.Nodes[nodeIndex];
                    int nFileDescriptorOffset = ((NodeAttributes)childNode.Tag).fileDesriptorSector * virtualFloppyManipulationRoutines.nLSNBlockSize;

                    string dskName = cDrivePathName.Replace(defaultDir + @"\", "");
                    string childInnerSourcePath = childNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    string saveAsFilename = Path.Combine(defaultDir, Path.GetFileNameWithoutExtension(dskName), childInnerSourcePath);        // create the target filename with complete path
                    string childFullTargetDirectoryPath = Path.GetDirectoryName(saveAsFilename); // get just the directory we are going to put the file in

                    int fileAttributes = ((NodeAttributes)childNode.Tag).fileAttributes;

                    if ((fileAttributes & 0x80) != 0)
                    {
                        // this is a directory - recurse

                        ExportOS9Directory(childNode, defaultDir, childFullTargetDirectoryPath, childInnerSourcePath, (NodeAttributes)childNode.Tag, childNode.FullPath);
                    }
                    else
                    {
                        // this is a file - save it
                        ExportSingleOS9File(childFullTargetDirectoryPath, childInnerSourcePath, (NodeAttributes)childNode.Tag, childNode.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportOS9FileFormat(object sender, EventArgs e)
        {
            string strDefaultDir;

            string drive = "";
            string dir = "";
            string fname = "";
            string ext = "";

            //char* pExt;

            fname = "";

            strDefaultDir = Program.GetConfigurationAttribute("Directories", "DefaultExportDir", "");
            if (strDefaultDir == "")
            {
                // copy the filename to a working buffer and split it up

                drive = Path.GetPathRoot(cDrivePathName);
                dir = Path.GetDirectoryName(cDrivePathName).Replace(drive, "");
                fname = Path.GetFileNameWithoutExtension(cDrivePathName);
                ext = Path.GetExtension(cDrivePathName);

                strDefaultDir = string.Format("{0}{1}", drive, dir);
            }

            // at this point strDefaultDir  will either be the directory we last saved to or the path to the .dsk file we are working with
            //

            // if we actually have a node selected in the tree view - proceed - otherwise ignore Export button - or maybe tell operator.

            if (treeViewFiles.SelectedNode != null)
            {
                if ((NodeAttributes)treeViewFiles.SelectedNode.Tag != null)
                {
                    string dskName = cDrivePathName.Replace(strDefaultDir + @"\", "");
                    string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    string saveAsFilename = Path.Combine(strDefaultDir, Path.GetFileNameWithoutExtension(dskName), innerSourcePath);        // create the target filename with complete path
                    string fullTargetDirectoryPath = Path.GetDirectoryName(saveAsFilename); // get just the directory we are going to put the file in

                    int fileAttributes = ((NodeAttributes)treeViewFiles.SelectedNode.Tag).fileAttributes;

                    // since this application does not support multi node select in the tree view, the only way to select multiple files
                    // for export is to select a directory in the tree view. This will copy all files in the directory recursively.

                    if ((fileAttributes & 0x80) != 0)
                    {
                        //  this is a directory we are exporting - that means multiple files, so build a directory 
                        //  path based on where in the tree structure this directory is and where the default
                        //  directory is relative to this path if we can.


                        DialogResult r = MessageBox.Show(string.Format("Save file {0} to {1}?", Path.GetFileName(saveAsFilename), fullTargetDirectoryPath), "Ok to save to location", MessageBoxButtons.YesNo);
                        if (r == DialogResult.Yes)
                        {
                            ExportOS9Directory(treeViewFiles.SelectedNode, strDefaultDir, fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);
                        }
                    }
                    else
                    {
                        // handle single file selected in tree view

                        DialogResult r = MessageBox.Show(string.Format("Save file {0} to {1}?", Path.GetFileName(saveAsFilename), fullTargetDirectoryPath), "Ok to save to location", MessageBoxButtons.YesNo);
                        if (r == DialogResult.Yes)
                        {
                            ExportSingleOS9File(fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);
                        }
                    }
                }
            }
        }

        private void ExportSingleUniFLEXFile(string fullTargetDirectoryPath, string innerSourcePath, NodeAttributes tag, string fullPath)
        {
            if (!Directory.Exists(fullTargetDirectoryPath))
                Directory.CreateDirectory(fullTargetDirectoryPath);

            try
            {
                if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
                {
                    uint nByteCount = (uint)tag.byteCount;

                    // TODO: Get strCurrentPath, strFname, fdnCurrent, blk, nFileSize, fs from tag for call to virtualFloppyManipulationRoutines.ExtractUniFLEXFile

                    string currentPath = Path.GetDirectoryName(innerSourcePath);
                    string strFilename = Path.GetFileName(fullPath);
                    uint blk = Convert.ToUInt32(tag.blk);
                    VirtualFloppyManipulationRoutines.fdn fdnCurrent = virtualFloppyManipulationRoutines.allFileDescriptorNodes[tag.fdnIndex - 1];

                    try
                    {
                        virtualFloppyManipulationRoutines.ExtractUniFLEXFile(fullTargetDirectoryPath, innerSourcePath, fdnCurrent, blk, nByteCount, virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void ExportUniFLEXDirectory(TreeNode selectedNode, string strDefaultDir, string fullTargetDirectoryPath, string innerSourcePath, NodeAttributes tag, string fullPath)
        {
            // first make sure that this directory exists - ExportSingleUniFLEXFile will just create the directory on the PC is we pass it a directory with no filename

            //ExportSingleUniFLEXFile(fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);

            // now iterate through all the direftories and files in this directory recursively

            try
            {
                for (int nodeIndex = 0; nodeIndex < selectedNode.Nodes.Count; nodeIndex++)
                {
                    TreeNode childNode = selectedNode.Nodes[nodeIndex];
                    int nFileDescriptorOffset = ((NodeAttributes)childNode.Tag).fileDesriptorSector * virtualFloppyManipulationRoutines.sectorSize;

                    string dskName = cDrivePathName.Replace(strDefaultDir + @"\", "");
                    string childInnerSourcePath = childNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    string saveAsFilename = Path.Combine(strDefaultDir, Path.GetFileNameWithoutExtension(dskName), childInnerSourcePath);        // create the target filename with complete path
                    string childFullTargetDirectoryPath = Path.GetDirectoryName(saveAsFilename); // get just the directory we are going to put the file in

                    int fileAttributes = ((NodeAttributes)childNode.Tag).fileAttributes;

                    if ((fileAttributes & 0x80) != 0)
                    {
                        // this is a directory - create it and recurse

                        string directoryToCreate = Path.Combine(childFullTargetDirectoryPath, Path.GetFileName(childInnerSourcePath));

                        if (!Directory.Exists(directoryToCreate))
                            Directory.CreateDirectory(directoryToCreate);

                        ExportUniFLEXDirectory(childNode, strDefaultDir, Path.GetFileName(childFullTargetDirectoryPath), childInnerSourcePath, (NodeAttributes)childNode.Tag, childNode.FullPath);
                    }
                    else
                    {
                        // this is a file - save it
                        ExportSingleUniFLEXFile(childFullTargetDirectoryPath, Path.GetFileName(childInnerSourcePath), (NodeAttributes)childNode.Tag, childNode.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportUniFLEXFileFormat(object sender, EventArgs e)
        {
            //MessageBox.Show("File format UniFLEX - Export not supported");

            string strDefaultDir;

            string drive = "";
            string dir = "";
            string fname = "";
            string ext = "";

            //char* pExt;

            fname = "";

            strDefaultDir = Program.GetConfigurationAttribute("Directories", "DefaultExportDir", "");
            if (strDefaultDir == "")
            {
                // copy the filename to a working buffer and split it up

                drive = Path.GetPathRoot(cDrivePathName);
                dir = Path.GetDirectoryName(cDrivePathName).Replace(drive, "");
                fname = Path.GetFileNameWithoutExtension(cDrivePathName);
                ext = Path.GetExtension(cDrivePathName);

                strDefaultDir = string.Format("{0}{1}", drive, dir);
            }

            // at this point strDefaultDir  will either be the directory we last saved to or the path to the .dsk file we are working with
            //

            // if we actually have a node selected in the tree view - proceed - otherwise ignore Export button - or maybe tell operator.

            if (treeViewFiles.SelectedNode != null)
            {
                if ((NodeAttributes)treeViewFiles.SelectedNode.Tag != null)
                {
                    string dskName = cDrivePathName.Replace(strDefaultDir + @"\", "");
                    string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    string saveAsFilename = Path.Combine(strDefaultDir, Path.GetFileNameWithoutExtension(dskName), innerSourcePath);    // create the target filename with complete path
                    string fullTargetDirectoryPath = Path.GetDirectoryName(saveAsFilename);                                             // get just the directory we are going to put the file in

                    // since this application does not support multi node select in the tree view, the only way to select multiple files
                    // for export is to select a directory in the tree view. This will copy all files in the directory recursively.

                    if (treeViewFiles.SelectedNode.Nodes.Count > 0)
                    {
                        //  this is a directory we are exporting - that means multiple files, so build a directory 
                        //  path based on where in the tree structure this directory is and where the default
                        //  directory is relative to this path if we can.


                        DialogResult r = MessageBox.Show(string.Format("Save file {0} to {1}?", Path.GetFileName(saveAsFilename), fullTargetDirectoryPath), "Ok to save to location", MessageBoxButtons.YesNo);
                        if (r == DialogResult.Yes)
                        {
                            ExportUniFLEXDirectory(treeViewFiles.SelectedNode, strDefaultDir, fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);
                        }
                    }
                    else
                    {
                        // handle single file selected in tree view

                        DialogResult r = MessageBox.Show(string.Format("Save file {0} to {1}?", Path.GetFileName(saveAsFilename), fullTargetDirectoryPath), "Ok to save to location", MessageBoxButtons.YesNo);
                        if (r == DialogResult.Yes)
                        {
                            ExportSingleUniFLEXFile(fullTargetDirectoryPath, Path.GetFileName(innerSourcePath), (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);
                        }
                    }
                }
            }
        }

        void ExportMinixDirectory(TreeNode selectedNode, string strDefaultDir, string fullTargetDirectoryPath, string innerSourcePath, NodeAttributes tag, string fullPath)
        {
            // first make sure that this directory exists - ExportSingleMinixFile will just create the directory on the PC is we pass it a directory with no filename

            //ExportSingleMinixFile(fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);

            // now iterate through all the direftories and files in this directory recursively

            try
            {
                for (int nodeIndex = 0; nodeIndex < selectedNode.Nodes.Count; nodeIndex++)
                {
                    TreeNode childNode = selectedNode.Nodes[nodeIndex];

                    string dskName = cDrivePathName.Replace(strDefaultDir + @"\", "");
                    string childInnerSourcePath = childNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    string saveAsFilename = Path.Combine(strDefaultDir, Path.GetFileNameWithoutExtension(dskName), childInnerSourcePath);        // create the target filename with complete path
                    string childFullTargetDirectoryPath = Path.GetDirectoryName(saveAsFilename); // get just the directory we are going to put the file in

                    NodeAttributes childNodeTag = (NodeAttributes)childNode.Tag;
                    int mode = childNodeTag.mode;

                    if ((mode & virtualFloppyManipulationRoutines.minixImage.I_DIRECTORY) == virtualFloppyManipulationRoutines.minixImage.I_DIRECTORY)
                    {
                        // this is a directory - create it and recurse

                        string directoryToCreate = Path.Combine(childFullTargetDirectoryPath, Path.GetFileName(childInnerSourcePath));

                        if (!Directory.Exists(directoryToCreate))
                            Directory.CreateDirectory(directoryToCreate);

                        ExportMinixDirectory(childNode, strDefaultDir, Path.GetFileName(childFullTargetDirectoryPath), childInnerSourcePath, (NodeAttributes)childNode.Tag, childNode.FullPath);
                    }
                    else
                    {
                        // this is a file - save it

                        virtualFloppyManipulationRoutines.minixImage.ExportSingleMinixFile(childFullTargetDirectoryPath, Path.GetFileName(childInnerSourcePath), (NodeAttributes)childNode.Tag, childNode.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportMinixFileFormat(object sender, EventArgs e)
        {
            //MessageBox.Show("File format Minix - Export not supported");

            string strDefaultDir;

            string drive = "";
            string dir = "";
            string fname = "";
            string ext = "";

            //char* pExt;

            fname = "";

            strDefaultDir = Program.GetConfigurationAttribute("Directories", "DefaultExportDir", "");
            if (strDefaultDir == "")
            {
                // copy the filename to a working buffer and split it up

                drive = Path.GetPathRoot(cDrivePathName);
                dir = Path.GetDirectoryName(cDrivePathName).Replace(drive, "");
                fname = Path.GetFileNameWithoutExtension(cDrivePathName);
                ext = Path.GetExtension(cDrivePathName);

                strDefaultDir = string.Format("{0}{1}", drive, dir);
            }

            // at this point strDefaultDir  will either be the directory we last saved to or the path to the .dsk file we are working with
            //

            // if we actually have a node selected in the tree view - proceed - otherwise ignore Export button - or maybe tell operator.

            if (treeViewFiles.SelectedNode != null)
            {
                if ((NodeAttributes)treeViewFiles.SelectedNode.Tag != null)
                {
                    string dskName = cDrivePathName.Replace(strDefaultDir + @"\", "");
                    string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    string saveAsFilename = Path.Combine(strDefaultDir, Path.GetFileNameWithoutExtension(dskName), innerSourcePath);    // create the target filename with complete path
                    string fullTargetDirectoryPath = Path.GetDirectoryName(saveAsFilename);                                             // get just the directory we are going to put the file in

                    // since this application does not support multi node select in the tree view, the only way to select multiple files
                    // for export is to select a directory in the tree view. This will copy all files in the directory recursively.

                    if (treeViewFiles.SelectedNode.Nodes.Count > 0)
                    {
                        //  this is a directory we are exporting - that means multiple files, so build a directory 
                        //  path based on where in the tree structure this directory is and where the default
                        //  directory is relative to this path if we can.


                        DialogResult r = MessageBox.Show(string.Format("Save file {0} to {1}?", Path.GetFileName(saveAsFilename), fullTargetDirectoryPath), "Ok to save to location", MessageBoxButtons.YesNo);
                        if (r == DialogResult.Yes)
                        {
                            ExportMinixDirectory(treeViewFiles.SelectedNode, strDefaultDir, fullTargetDirectoryPath, innerSourcePath, (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);
                        }
                    }
                    else
                    {
                        // handle single file selected in tree view

                        DialogResult r = MessageBox.Show(string.Format("Save file {0} to {1}?", Path.GetFileName(saveAsFilename), fullTargetDirectoryPath), "Ok to save to location", MessageBoxButtons.YesNo);
                        if (r == DialogResult.Yes)
                        {
                            virtualFloppyManipulationRoutines.minixImage.ExportSingleMinixFile(fullTargetDirectoryPath, Path.GetFileName(innerSourcePath), (NodeAttributes)treeViewFiles.SelectedNode.Tag, treeViewFiles.SelectedNode.FullPath);
                        }
                    }
                }
            }
        }

        private void ExportFLEXFile(DIR_ENTRY tag, string strDefaultDir, string szTargetFileName)
        {
            // this file is selected - export it

            BinaryWriter fp = null;

            bool nTextFile;
            bool nRandomFileInd;
            bool nDoingSpaceExpansion = false;

            int nSequenceNumber;
            long lOffset;

            string szFileName = ASCIIEncoding.ASCII.GetString(tag.caFileName).TrimEnd('\0');
            string szFileExtension = ASCIIEncoding.ASCII.GetString(tag.caFileExtension).TrimEnd('\0');

            byte[] caSectorBuffer = new byte[256];

            int nYear = 1900 + tag.cYear;
            if (nYear < 1970)
            {
                nYear = 2000 + tag.cYear;
            }

            string szDate = string.Format("{0}-{1}-{2}", nYear.ToString("0000"), tag.cMonth.ToString("00"), tag.cDay.ToString("00"));
            int nSectorCount = tag.cTotalSectorsHi * 256 + tag.cTotalSectorsLo;

            if (tag.cRandomFileInd != 0)
                nRandomFileInd = true;
            else
                nRandomFileInd = false;

            byte cNextTrack = tag.cStartTrack;                  // (byte)Convert.ToInt32(szStartTrack);
            byte cNextSector = tag.cStartSector;                 // (byte)Convert.ToInt32(szStartSector);

            if (!tag.isHierDirectoryEntry)
            {
                if (cNextTrack != 0)
                {
                    // First get the System Information Record

                    RAW_SIR stSystemInformationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR();
                    int nMaxSector = stSystemInformationRecord.cMaxSector;

                    Cursor = Cursors.WaitCursor;

                    fp = null;
                    Directory.CreateDirectory(strDefaultDir);

                    szTargetFileName = szTargetFileName.Replace("*", "@");

                    fp = new BinaryWriter(File.Open(szTargetFileName, FileMode.Create, FileAccess.Write));
                    if (fp != null)
                    {
                        if (!nRandomFileInd)
                        {
                            // get first char of file to see if TEXT or BINARY file

                            lOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, cNextTrack, cNextSector);
                            lOffset += 4;   // point to first byte of file
                            virtualFloppyManipulationRoutines.Seek(lOffset, SeekOrigin.Begin);

                            // get first character of the file. All binary files start with 0x02, but we will allow any file
                            // that does not start with a space, <cr>, tab or form feed to be treated as binary

                            virtualFloppyManipulationRoutines.Read(caSectorBuffer, 0, 1);                // fread(caSectorBuffer, 1, 1, m_fp);
                            if (
                                (caSectorBuffer[0] >= ' ') ||      // if > space character
                                (caSectorBuffer[0] == 0x09) ||      // or space compression character
                                (caSectorBuffer[0] == 0x0d) ||      // of carriage return
                                (caSectorBuffer[0] == 0x0c)         // or form feed
                                )
                                nTextFile = true;
                            else
                                nTextFile = false;
                        }
                        else
                            nTextFile = false;

                        for (int i = 0; i < nSectorCount; i++)
                        {
                            int nPos = 0;

                            if (cNextTrack == 0 && cNextSector == 0)
                            {
                                MessageBox.Show("Unexpected End Of Chain");
                                break;
                            }

                            lOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, cNextTrack, cNextSector);
                            virtualFloppyManipulationRoutines.Seek(lOffset, SeekOrigin.Begin);
                            virtualFloppyManipulationRoutines.Read(caSectorBuffer, 0, 256);                // fread(caSectorBuffer, 1, 256, m_fp);
                            cNextTrack = caSectorBuffer[0];
                            cNextSector = caSectorBuffer[1];
                            nSequenceNumber = caSectorBuffer[2] * 256 + caSectorBuffer[3];

                            // write the data to the target file (starts at caSectorBuffer[4])

                            // see if text file

                            if (nTextFile)
                            {
                                for (nPos = 4; nPos < 256; nPos++)
                                {
                                    if (m_nExpandTabs)
                                    {
                                        if ((caSectorBuffer[nPos] == 0x09) || nDoingSpaceExpansion)
                                        {
                                            int nSpaceCount;

                                            if (nPos != 255)    // make sure we are not at end of buffer
                                            {
                                                if (nDoingSpaceExpansion)
                                                {
                                                    if (nPos != 4)  // make sure we are not at start of buffer
                                                        nSpaceCount = caSectorBuffer[++nPos];
                                                    else
                                                        nSpaceCount = caSectorBuffer[nPos];
                                                }
                                                else
                                                    nSpaceCount = caSectorBuffer[++nPos];

                                                while (nSpaceCount-- > 0)
                                                    fp.Write(' ');                  //fwrite(" ", 1, 1, fp);

                                                nDoingSpaceExpansion = false;
                                            }
                                            else
                                                nDoingSpaceExpansion = true;
                                        }
                                        else
                                        {
                                            nDoingSpaceExpansion = false;
                                            if (caSectorBuffer[nPos] != 0x00)
                                                fp.Write(caSectorBuffer, nPos, 1);      // fwrite(&caSectorBuffer[nPos], 1, 1, fp);

                                            if ((caSectorBuffer[nPos] == 0x0D) && m_nAddLinefeed)
                                            {
                                                fp.Write('\n');      // fwrite("\n", 1, 1, fp);
                                            }
                                        }
                                    }
                                    else        // Not expanding tabs, but Text file
                                    {
                                        if (caSectorBuffer[nPos] == 0x09)
                                        {
                                            // so we don't mess up 0x09 0x0D sequence

                                            nDoingSpaceExpansion = true;
                                            if (caSectorBuffer[nPos] != 0x00)
                                                fp.Write(caSectorBuffer, nPos, 1);      // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                        }
                                        else
                                        {
                                            if (nDoingSpaceExpansion)
                                            {
                                                if (caSectorBuffer[nPos] != 0x00)
                                                    fp.Write(caSectorBuffer, nPos, 1);  // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                                nDoingSpaceExpansion = true;
                                            }
                                            else
                                            {
                                                if (caSectorBuffer[nPos] != 0x00)
                                                    fp.Write(caSectorBuffer, nPos, 1);  // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                                if ((caSectorBuffer[nPos] == 0x0D) && m_nAddLinefeed)
                                                {
                                                    fp.Write('\n');                     // fwrite("\n", 1, 1, fp);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else            // not a text file - save as binary
                            {
                                if (m_nCompactBinary)
                                {
                                    // not yet implemented - need to build a state machine

                                    fp.Write(caSectorBuffer, 4, 252);               // fwrite(&caSectorBuffer[4], 1, 252, fp);
                                }
                                else
                                    fp.Write(caSectorBuffer, 4, 252);               // fwrite(&caSectorBuffer[4], 1, 252, fp);
                            }
                        }
                        fp.Close();

                        string szYear = szDate.Substring(0, 4);
                        string szMonth = szDate.Substring(5, 2);
                        string szDay = szDate.Substring(8, 2);

                        try
                        {
                            // if there is a valid datetime for the file in the FLEX file systen - use it

                            DateTime st = new DateTime(Convert.ToInt16(szYear), Convert.ToInt16(szMonth), Convert.ToInt16(szDay), 12, 0, 0);
                            File.SetLastWriteTime(szTargetFileName, st);                // SetFileDateTime(szTargetFileName, ft, ft, ft);
                        }
                        catch
                        {
                            // otherwise use today's date

                            File.SetLastWriteTime(szTargetFileName, DateTime.Now);                // set to now
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to open target file");
                    }

                    Cursor = Cursors.Default;
                }
            }
            else
            {
                // we have encountered a directory in the items seleced from the list view.
                //
                //  we need to recurse after setting up the tag and default directory.
                //
                //  Step 1: get a list of files (as tags) into an array
                //  Step 2: foreach tag in tag array -> call ExportFLEXFile (tag, strDefaultDir, szTargetFileName)
                //          if this entry is a directory - add the name to the strDefaultDir string and recurse
                //  Step 3: rinse and repeat

                //if (Debugger.IsAttached)
                {
                    // since this is a directory - the tag start track and start sector will point to the directories file list

                    if (virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream != null)
                    {
                        FileStream m_fp = virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream;

                        m_fp.Flush();

                        long currentFilePosition = m_fp.Position;

                        RAW_SIR stSystemInformationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR();
                        int nMaxSector = stSystemInformationRecord.cMaxSector;

                        List<DIR_ENTRY> dirEntries = new List<DIR_ENTRY>();

                        // traverse this sub directory gathering directory entries

                        bool nDirectoryEnd = false;

                        for (byte cNextDirTrack = tag.cStartTrack, cNextDirSector = tag.cStartSector; !nDirectoryEnd;)
                        {
                            // if the sector in the linkage track and sector is 0 - this is the last sector in the directory - processint and then leave

                            if (cNextDirSector == 0)
                                nDirectoryEnd = true;

                            if (!nDirectoryEnd)
                            {
                                byte[] caDirHeader = new byte[16];

                                lOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, cNextDirTrack, cNextDirSector);
                                m_fp.Seek(lOffset, SeekOrigin.Begin);       // fseek (m_fp, lOffset, SEEK_SET);
                                m_fp.Read(caDirHeader, 0, 16);              // fread (&caDirHeader, 1, sizeof (caDirHeader), m_fp);
                                cNextDirTrack = caDirHeader[0];
                                cNextDirSector = caDirHeader[1];
                                for (int i = 0; i < 10; i++)
                                {
                                    DIR_ENTRY stDirEntry;

                                    stDirEntry = virtualFloppyManipulationRoutines.ReadFLEX_DIR_ENTRY(m_fp, false);                  // fread (&stDirEntry,  1, sizeof (stDirEntry), m_fp);
                                    if (stDirEntry.caFileName[0] != '\0')
                                    {
                                        DIR_ENTRY newTag = new DIR_ENTRY();

                                        if ((stDirEntry.caFileName[0] & 0x80) != 0x80)
                                        {

                                            // We are now pointing at a Directory Entry

                                            szFileName = ASCIIEncoding.ASCII.GetString(stDirEntry.caFileName).TrimEnd('\0');            // memcpy (szFileName,      stDirEntry.caFileName,      8);
                                            szFileExtension = ASCIIEncoding.ASCII.GetString(stDirEntry.caFileExtension).TrimEnd('\0');  // memcpy(szFileExtension, stDirEntry.caFileExtension, 3);

                                            nRandomFileInd = stDirEntry.cRandomFileInd == 0 ? false : true;

                                            nYear = 1900 + stDirEntry.cYear;
                                            if (nYear < 1970)
                                            {
                                                nYear = 2000 + stDirEntry.cYear;
                                            }
                                            string szCreationDate = string.Format("{0}-{1}-{2}", nYear.ToString("0000"), stDirEntry.cMonth.ToString("00"), stDirEntry.cDay.ToString("00"));
                                            string szStartTrackSector = string.Format("{0}-{1}", stDirEntry.cStartTrack.ToString("000"), stDirEntry.cStartSector.ToString("000"));
                                            string szEndTrackSector = string.Format("{0}-{1}", stDirEntry.cEndTrack.ToString("000"), stDirEntry.cEndSector.ToString("000"));
                                            int nTotalSectors = stDirEntry.cTotalSectorsHi * 256 + stDirEntry.cTotalSectorsLo;
                                            string szTotalSectors = string.Format("{0}", nTotalSectors.ToString("000000"));        // sprintf_s (szTotalSectors, sizeof (szTotalSectors), "%6d", nTotalSectors);

                                            string attributes = "";
                                            stDirEntry.isHierDirectoryEntry = false;

                                            if ((stDirEntry.cAttributes & 0x80) == 0x80)
                                                attributes += "W";
                                            else
                                                attributes += " ";
                                            if ((stDirEntry.cAttributes & 0x40) == 0x40)
                                            {
                                                attributes += "D";
                                                if (szFileExtension == "DIR")
                                                {
                                                    long saveOffset = m_fp.Position;

                                                    // read the sector pointed to by the dirst tracka and sector

                                                    byte[] possibleDirHeader = new byte[16];

                                                    long possibleDirlOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, stDirEntry.cStartTrack, stDirEntry.cStartSector);
                                                    m_fp.Seek(possibleDirlOffset, SeekOrigin.Begin);       // fseek (m_fp, lOffset, SEEK_SET);
                                                    m_fp.Read(possibleDirHeader, 0, 16);

                                                    string possibleDirHeaderName = "";
                                                    for (int index = 6; index < 14; index++)
                                                    {
                                                        if (possibleDirHeader[index] != 0x00)
                                                        {
                                                            possibleDirHeaderName += (char)possibleDirHeader[index];
                                                        }
                                                        else
                                                            break;
                                                    }

                                                    if (possibleDirHeaderName == szFileName)
                                                    {
                                                        stDirEntry.isHierDirectoryEntry = true;
                                                    }

                                                    m_fp.Seek(saveOffset, SeekOrigin.Begin);       // reset position
                                                }
                                            }
                                            else
                                                attributes += " ";
                                            if ((stDirEntry.cAttributes & 0x20) == 0x20)
                                                attributes += "R";
                                            else
                                                attributes += " ";
                                            if ((stDirEntry.cAttributes & 0x10) == 0x10)
                                                attributes += "C";
                                            else
                                                attributes += " ";

                                            if (stDirEntry.isHierDirectoryEntry)
                                                attributes += "H";

                                            newTag = stDirEntry;

                                            dirEntries.Add(newTag);
                                        }
                                    }
                                    else
                                    {
                                        nDirectoryEnd = true;
                                        break;
                                    }
                                }
                            }
                        }

                        strDefaultDir = Path.Combine(strDefaultDir, ASCIIEncoding.ASCII.GetString(tag.caFileName).TrimEnd('\0'));
                        foreach (DIR_ENTRY de in dirEntries)
                        {
                            if (de.isHierDirectoryEntry)
                            {
                                // if this is a sub directory within a directory - expand the strDefaultDir before we recurse.

                                //strDefaultDir = Path.Combine(strDefaultDir, ASCIIEncoding.ASCII.GetString(de.caFileName).TrimEnd('\0'));
                                szTargetFileName = "";
                            }
                            else
                            {
                                //strDefaultDir = Path.Combine(strDefaultDir, ASCIIEncoding.ASCII.GetString(de.caFileName).TrimEnd('\0'));
                                szTargetFileName = string.Format("{0}.{1}", Path.Combine(strDefaultDir, ASCIIEncoding.ASCII.GetString(de.caFileName).TrimEnd('\0')), ASCIIEncoding.ASCII.GetString(de.caFileExtension).TrimEnd('\0'));
                            }

                            ExportFLEXFile(de, strDefaultDir, Path.Combine(strDefaultDir, szTargetFileName));
                        }

                        m_fp.Seek(currentFilePosition, SeekOrigin.Begin);
                    }
                }
                //else
                //    MessageBox.Show("Exporting of HIER directories is not yet implemented");
            }
        }

        private void ExportFLEXFileFormat(object sender, EventArgs e)
        {
            int nSelectedFile;
            int nSelectedCount;
            bool nMultipleSelections = false;

            string strDefaultDir;

            string drive = "";
            string dir = "";
            string fname = "";

            fname = "";

            strDefaultDir = Program.GetConfigurationAttribute("Directories", "DefaultExportDir", "");
            if (strDefaultDir == "")
            {
                // copy the filename to a working buffer and split it up

                drive = Path.GetPathRoot(cDrivePathName);
                dir = Path.GetDirectoryName(cDrivePathName).Replace(drive, "");
                fname = Path.GetFileNameWithoutExtension(cDrivePathName);

                strDefaultDir = string.Format("{0}{1}", drive, dir);
            }

            if ((nSelectedCount = lstviewListFiles.SelectedItems.Count) > 1)
            {
                nSelectedFile = 0;
                nMultipleSelections = true;
            }
            else
                nSelectedFile = LocateSelectedFile();

            if (nSelectedFile >= 0)
            {
                DialogResult nDlgReturn = DialogResult.OK;
                string szTargetFileName = "";

                // copy a single file or directory

                if (!nMultipleSelections)     // this means a single file was selected
                {
                    DIR_ENTRY tag = (DIR_ENTRY)lstviewListFiles.Items[nSelectedFile].Tag;

                    string szFileName = ASCIIEncoding.ASCII.GetString(tag.caFileName).TrimEnd('\0');
                    string szFileExtension = ASCIIEncoding.ASCII.GetString(tag.caFileExtension).TrimEnd('\0');
                    string szFile = "";

                    // if this is a HIER directory - hanlde as dir copy instead of file copy

                    if (tag.isHierDirectoryEntry)
                    {
                        szFile = string.Format("{0}", szFileName);  // the name of the file 
                        FolderBrowserDialog fbd = new FolderBrowserDialog();

                        fbd.RootFolder = Environment.SpecialFolder.Desktop;
                        fbd.ShowNewFolderButton = true;
                        fbd.SelectedPath = strDefaultDir;

                        nDlgReturn = fbd.ShowDialog();
                        if (nDlgReturn == DialogResult.OK)
                        {
                            // strDefaultDir = Path.Combine(fbd.SelectedPath, szFile);
                            strDefaultDir = fbd.SelectedPath;
                        }
                        else
                            nSelectedCount = 0;

                    }
                    else
                    {
                        szFile = string.Format("{0}.{1}", szFileName, szFileExtension);

                        // allow user to specify the filename for non-multiple selections

                        SaveFileDialog pSaveDlg = new SaveFileDialog();
                        pSaveDlg.OverwritePrompt = true;
                        pSaveDlg.Filter = "All files (*.*) | *.*||";
                        pSaveDlg.InitialDirectory = strDefaultDir;

                        pSaveDlg.FileName = szFile;

                        nDlgReturn = pSaveDlg.ShowDialog();

                        // TODO: make directory get instead of file.

                        if (nDlgReturn == DialogResult.OK)
                        {
                            strDefaultDir = Path.GetDirectoryName(pSaveDlg.FileName);
                        }
                        else
                            nSelectedCount = 0;

                        szTargetFileName = pSaveDlg.FileName;
                    }
                }
                else
                {
                    DialogResult nRetVal;
                    string strMessage;

                    string strFname = fname;

                    // create a directory that has the name of the .dsk file without the extension under the default directory

                    if (textBoxListViewCurrentPath.Text.Length > 0 && textBoxListViewCurrentPath.Visible)
                    {
                        // we are in an HIER subdirectory, so add the HIER path to the defaultDir.

                        strDefaultDir = Path.Combine(drive, dir, fname, textBoxListViewCurrentPath.Text.Substring(1));
                    }
                    else
                        strDefaultDir = Path.Combine(drive, dir, fname);

                    Directory.CreateDirectory(strDefaultDir);

                    strMessage = string.Format
                                (
                                   "Multiple selection will put the files in the \r\n" +
                                    "default export directory with the same names \r\n" +
                                    "as the files on the FLEX diskette.\r\n\r\n" +
                                    "The default export directory is:\r\n\r\n" +
                                    "      {0}\r\n\r\n" +
                                    "Do you wish to proceed?\r\n",
                                    strDefaultDir
                                );

                    nRetVal = MessageBox.Show(strMessage, "Multiple Selections", MessageBoxButtons.YesNo);

                    if (nRetVal != DialogResult.Yes)
                    {
                        // See if user wants to provide a different directory to save result into

                        FolderBrowserDialog pDlg = new FolderBrowserDialog();

                        string pszTitle = "Choose Export Directory";
                        string szPath;

                        pDlg.Description = pszTitle;
                        DialogResult rc = pDlg.ShowDialog();        //  (pszTitle, szDisplayName, szPath);
                        szPath = pDlg.SelectedPath;

                        if (rc == DialogResult.OK)
                        {
                            strDefaultDir = szPath;
                        }
                        else
                            nSelectedCount = 0;
                    }

                    if (strDefaultDir.Length == 0)
                        strDefaultDir = ".";

                    nDlgReturn = DialogResult.OK;
                }

                while (nSelectedCount > 0 && nDlgReturn == DialogResult.OK)
                {
                    if (CheckSelected(nSelectedFile))
                    {
                        DIR_ENTRY tag = (DIR_ENTRY)lstviewListFiles.Items[nSelectedFile].Tag;

                        if (!tag.isHierDirectoryEntry)
                        {
                            // this is a file - just do a normal file copy

                            if (szTargetFileName == "")
                            {
                                string szFileName = ASCIIEncoding.ASCII.GetString(tag.caFileName).TrimEnd('\0');
                                string szFileExtension = ASCIIEncoding.ASCII.GetString(tag.caFileExtension).TrimEnd('\0');

                                string szFile = string.Format("{0}.{1}", szFileName, szFileExtension);

                                szTargetFileName = string.Format(@"{0}\{1}", strDefaultDir, szFile);
                            }

                            ExportFLEXFile(tag, strDefaultDir, szTargetFileName);
                            nSelectedCount--;

                            szTargetFileName = "";
                        }
                        else
                        {
                            // this is a directory to copy - get the list of files that are in this directory and call this ExportFLEXFile. It will recurse
                            // if a directory is encountered. The target path is already set up.
                            //
                            //      ExportFLEXFile will handle the gathering of files within the directory to copy. It will also handle detecting
                            //      directories within directorys and recurse as needed.

                            ExportFLEXFile(tag, strDefaultDir, szTargetFileName);
                            nSelectedCount--;

                            szTargetFileName = "";
                        }
                    }
                    nSelectedFile++;
                }
            }
            else
                MessageBox.Show("You must select and item from the list to Export");
        }

        private void DeleteFLEXFileFromImage(object sender, EventArgs e)
        {
            if (hierDirectories.Count == 0)
            {
                bool fileFound = false;

                ListView.SelectedListViewItemCollection slvic = lstviewListFiles.SelectedItems;
                int nSelectedCount = slvic.Count;

                string cSourceFileTitle = "";
                string cSourceFileExt = "";

                string message = string.Format("Are you sure you want to delete {0} files", nSelectedCount.ToString());
                DialogResult dr = DialogResult.Yes;

                if (nSelectedCount > 1)
                    dr = MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo);

                if (dr == DialogResult.Yes)
                {
                    foreach (ListViewItem lvi in slvic)
                    {
                        DIR_ENTRY tag = (DIR_ENTRY)lvi.Tag;

                        if (!tag.isHierDirectoryEntry)
                        {
                            cSourceFileTitle = lvi.SubItems[0].Text;
                            cSourceFileExt = lvi.SubItems[1].Text;

                            if (nSelectedCount == 1)
                            {
                                message = string.Format("Are you sure you want to delete {0}.{1}", cSourceFileTitle, cSourceFileExt);
                                dr = MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo);
                            }

                            if (dr == DialogResult.Yes)
                            {
                                RAW_SIR informationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR();
                                DIR_ENTRY dirEntry = virtualFloppyManipulationRoutines.FindFLEXDirEntry(informationRecord, cSourceFileTitle, cSourceFileExt, out fileFound);

                                int nMaxSector = (int)informationRecord.cMaxSector;
                                if (fileFound)
                                {
                                    virtualFloppyManipulationRoutines.DeleteFLEXFile(informationRecord, dirEntry, nMaxSector, false);
                                    RefreshList();
                                }
                            }
                        }
                        else
                            MessageBox.Show("Deleting HIER directories is not yet implemented");
                    }
                }
            }
            else
                MessageBox.Show("Deleteing from HIER Sub Directory is not yet supported");
        }

        private void DeleteOS9ileFromImage(object sender, EventArgs e)
        {
            //MessageBox.Show("File format OS9 - Delete not supported");

            string strDefaultDir;

            string drive = "";
            string dir = "";
            string fname = "";
            string ext = "";

            //char* pExt;

            fname = "";

            strDefaultDir = Program.GetConfigurationAttribute("Directories", "DefaultExportDir", "");
            if (strDefaultDir == "")
            {
                // copy the filename to a working buffer and split it up

                drive = Path.GetPathRoot(cDrivePathName);
                dir = Path.GetDirectoryName(cDrivePathName).Replace(drive, "");
                fname = Path.GetFileNameWithoutExtension(cDrivePathName);
                ext = Path.GetExtension(cDrivePathName);

                strDefaultDir = string.Format("{0}{1}", drive, dir);
            }

            // first see if a file is selected or a directory - and it cannot be the root (diskette name)
            if (treeViewFiles.SelectedNode != null)
            {
                if ((NodeAttributes)treeViewFiles.SelectedNode.Tag != null)
                {
                    string dskName = cDrivePathName.Replace(strDefaultDir + @"\", "");

                    string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(dskName + @"\", "");
                    string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(dskName);

                    //string nodeToSearchFor = treeViewFiles.SelectedNode.FullPath.Replace("\\", "/").Replace(" ", "_");
                    string nodeToSearchFor = treeViewFiles.SelectedNode.FullPath.Replace("\\", "/").Replace(" ", "_").Replace("(", "_").Replace(")", "_");
                    bool isLetter = !String.IsNullOrEmpty(nodeToSearchFor) && Char.IsLetter(nodeToSearchFor[0]);
                    if (!isLetter)
                        nodeToSearchFor = "_" + nodeToSearchFor;

                    XmlNodeList nodes = os9XmlDocument.SelectNodes(nodeToSearchFor);
                    if (nodes.Count > 0)
                    {
                        // this files is already on the image disk in the same directory path of the file we are going to copy.
                        // it must be deleted first.

                        List<OS9_BYTES_TO_WRITE> bytesToWrite = virtualFloppyManipulationRoutines.DeleteOS9File(nodes[0]);
                        virtualFloppyManipulationRoutines.WriteByteArrayToOS9Image(bytesToWrite);

                        // reload the image to refresh the XML and the treeview as well as the SIR in the virtualFloppyManipulationRoutines

                        GetVirtualFloppy(true);
                    }
                }
            }
        }

        #region list view event handlers

        // a place to store the chain of open HIER directories as we traverse the directory structure of
        // a FLEX/REXX diskette image that has HIER structure..
        //
        //      hierDirectories[hierDirectories.Length - 1] will be the directory that is being
        //      displayed in the list view.
        //
        //      double clicking on a HIER directory entry will cause a new DIR_ENTRY to be added to the List.
        //      and the list view will now display the files in that HIER directory
        //
        //      clicking the back button will remove the last entry of the List and back up the display one level.

        List<DIR_ENTRY> hierDirectories = new List<DIR_ENTRY>();

        private void LstviewListFiles_DoubleClick(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection slvic = lstviewListFiles.SelectedItems;
            int nSelectedCount = slvic.Count;

            if (nSelectedCount == 1)
            {
                int nSelectedFile;
                string szFileName;
                string szFileExtension;
                string szRandomFileInd;
                string szStartTrackSector;
                string szEndTrackSector;
                string szSectorCount;

                FileStream fp;
                int nSequenceNumber;
                int nSectorCount;
                bool nDoingSpaceExpansion = false;
                byte[] caSectorBuffer = new byte[256];
                string szFile;
                string szStartTrack, szStartSector, szEndTrack, szEndSector;
                string szTargetFileName;
                RAW_SIR stSystemInformationRecord;
                long lOffset;
                byte cNextTrack, cNextSector;
                int nMaxSector;
                int nMaxTrack;
                int nTotalSectors;

                bool nRandomFileInd;

                bool nTextFile = false;

                nSelectedFile = LocateSelectedFile();

                if (CheckSelected(nSelectedFile))
                {
                    DIR_ENTRY listViewTag = (DIR_ENTRY)lstviewListFiles.Items[nSelectedFile].Tag;

                    if (listViewTag.isHierDirectoryEntry == false)
                    {

                        szFileName = lstviewListFiles.Items[nSelectedFile].SubItems[0].Text;
                        szFileExtension = lstviewListFiles.Items[nSelectedFile].SubItems[1].Text;
                        szRandomFileInd = lstviewListFiles.Items[nSelectedFile].SubItems[2].Text;
                        szStartTrackSector = lstviewListFiles.Items[nSelectedFile].SubItems[4].Text;
                        szEndTrackSector = lstviewListFiles.Items[nSelectedFile].SubItems[5].Text;
                        szSectorCount = lstviewListFiles.Items[nSelectedFile].SubItems[6].Text;

                        nSectorCount = Int32.Parse(szSectorCount);
                        string[] startLocation = szStartTrackSector.Split('-');
                        szStartTrack = startLocation[0];
                        szStartSector = startLocation[1];
                        string[] endLocation = szEndTrackSector.Split('-');
                        szEndTrack = endLocation[0];
                        szEndSector = endLocation[1];

                        if (szRandomFileInd == "Y")
                            nRandomFileInd = true;
                        else
                            nRandomFileInd = false;

                        cNextTrack = (byte)Int32.Parse(szStartTrack);
                        cNextSector = (byte)Int32.Parse(szStartSector);

                        if (cNextTrack != 0)
                        {
                            // First get the System Information Record

                            stSystemInformationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR();    // fread(&stSystemInformationRecord, 1, sizeof(stSystemInformationRecord), m_fp);
                            nMaxSector = stSystemInformationRecord.cMaxSector;
                            nMaxTrack = stSystemInformationRecord.cMaxTrack;
                            nTotalSectors = stSystemInformationRecord.cTotalSectorsHi * 256 + stSystemInformationRecord.cTotalSectorsLo;


                            string strTempDir = Environment.GetEnvironmentVariable("TEMP");
                            if (strTempDir == string.Empty || strTempDir == null)
                            {
                                strTempDir = Program.GetConfigurationAttribute("Directories", "TempDir", "");
                            }

                            szFile = string.Format("{0}.{1}", szFileName, szFileExtension);         // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);

                            // make sure the FLEX filename does not have any illegal characters for a Windows filename

                            string[] illegalCharactersInFLEXFilename = { "/", @"\", "*", ":", "<", ">", "\"", "|", "?", "\t", "\b" };
                            foreach (string illegalCharacter in illegalCharactersInFLEXFilename)
                                szFile = szFile.Replace(illegalCharacter, "_");

                            // now build full filename

                            if (strTempDir != null && strTempDir != string.Empty)
                                szTargetFileName = string.Format("{0}\\{1}", strTempDir, szFile);       // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);
                            else
                                szTargetFileName = szFile;

                            Cursor currentCursor = Cursor.Current;
                            Cursor = Cursors.WaitCursor;                    // CWaitCursor cWait;


                            fp = null;
                            fp = File.Open(szTargetFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);      // fopen_s(&fp, szTargetFileName, "wb");
                            if (fp != null)
                            {
                                if (!nRandomFileInd)
                                {
                                    // get first char of file to see if TEXT or BINARY file

                                    lOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, cNextTrack, cNextSector);    // lOffset = ((cNextTrack * nMaxSector) + (cNextSector - m_nSectorBias)) * 256;
                                    lOffset += 4;                                                                                       // point to first byte of file
                                    virtualFloppyManipulationRoutines.Seek(lOffset, SeekOrigin.Begin);                                  // fseek(m_fp, lOffset, SEEK_SET);
                                    virtualFloppyManipulationRoutines.Read(caSectorBuffer, 0, 1);                                       // fread(caSectorBuffer, 1, 1, m_fp);

                                    // if the first byte of the file is a space character or greater or it's a <CR> or a <TAB> or a <FF> then this must be a text file

                                    if ((caSectorBuffer[0] >= ' ') || (caSectorBuffer[0] == 0x09) || (caSectorBuffer[0] == 0x0d) || (caSectorBuffer[0] == 0x0c))
                                        nTextFile = true;
                                    else
                                        nTextFile = false;
                                }
                                else
                                    nTextFile = false;

                                for (int i = 0; i < nSectorCount; i++)
                                {
                                    int nPos;

                                    if (cNextTrack == 0 && cNextSector == 0)
                                    {
                                        MessageBox.Show("Unexpected End Of Chain");
                                        break;
                                    }

                                    lOffset = virtualFloppyManipulationRoutines.CalcFileOffset(nMaxSector, cNextTrack, cNextSector);     // lOffset = ((cNextTrack * nMaxSector) + (cNextSector - m_nSectorBias)) * 256;
                                    virtualFloppyManipulationRoutines.Seek(lOffset, SeekOrigin.Begin);                                   // fseek(m_fp, lOffset, SEEK_SET);
                                    virtualFloppyManipulationRoutines.Read(caSectorBuffer, 0, 256);                                      // fread(caSectorBuffer, 1, 256, m_fp);
                                    cNextTrack = caSectorBuffer[0];
                                    cNextSector = caSectorBuffer[1];
                                    nSequenceNumber = caSectorBuffer[2] * 256 + caSectorBuffer[3];

                                    // write the data to the target file (starts at caSectorBuffer[4])

                                    // see if text file

                                    if (nTextFile)
                                    {
                                        for (nPos = 4; nPos < 256; nPos++)
                                        {
                                            if (m_nExpandTabs)
                                            {
                                                if ((caSectorBuffer[nPos] == 0x09) || nDoingSpaceExpansion)
                                                {
                                                    int nSpaceCount;

                                                    if (nPos != 255)    // make sure we are not at end of buffer
                                                    {
                                                        if (nDoingSpaceExpansion)
                                                        {
                                                            if (nPos != 4)  // make sure we are not at start of buffer
                                                                nSpaceCount = caSectorBuffer[++nPos];
                                                            else
                                                                nSpaceCount = caSectorBuffer[nPos];
                                                        }
                                                        else
                                                            nSpaceCount = caSectorBuffer[++nPos];

                                                        while (nSpaceCount-- > 0)
                                                        {
                                                            fp.WriteByte(0x20);              // fwrite(" ", 1, 1, fp);
                                                        }

                                                        nDoingSpaceExpansion = false;
                                                    }
                                                    else
                                                    {
                                                        nDoingSpaceExpansion = true;
                                                    }
                                                }
                                                else
                                                {
                                                    nDoingSpaceExpansion = false;
                                                    if (caSectorBuffer[nPos] != '\0')
                                                    {
                                                        fp.WriteByte(caSectorBuffer[nPos]);              // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                                        if ((caSectorBuffer[nPos] == 0x0D) && m_nAddLinefeed)
                                                        {
                                                            fp.WriteByte(0x0a);                         // fwrite("\n", 1, 1, fp);
                                                        }
                                                    }
                                                }
                                            }
                                            else        // Not expanding tabs, but Text file
                                            {
                                                if (caSectorBuffer[nPos] == 0x09)
                                                {
                                                    // so we don't mess up 0x09 0x0D sequence

                                                    nDoingSpaceExpansion = true;
                                                    fp.WriteByte(caSectorBuffer[nPos]);             // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                                }
                                                else
                                                {
                                                    if (nDoingSpaceExpansion)
                                                    {
                                                        fp.WriteByte(caSectorBuffer[nPos]);         // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                                        nDoingSpaceExpansion = true;
                                                    }
                                                    else
                                                    {
                                                        if (caSectorBuffer[nPos] != '\0')
                                                        {
                                                            fp.WriteByte(caSectorBuffer[nPos]);     // fwrite(&caSectorBuffer[nPos], 1, 1, fp);
                                                            if ((caSectorBuffer[nPos] == 0x0D) && m_nAddLinefeed)
                                                            {
                                                                fp.WriteByte(0x0a);                 // fwrite("\n", 1, 1, fp);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else            // not a text file - save as binary
                                    {
                                        if (m_nCompactBinary)
                                        {
                                            // not yet implemented - need to build a state machine

                                            fp.Write(caSectorBuffer, 4, 252);               // fwrite(&caSectorBuffer[4], 1, 252, fp);
                                        }
                                        else
                                            fp.Write(caSectorBuffer, 4, 252);               // fwrite(&caSectorBuffer[4], 1, 252, fp);
                                    }
                                }
                                fp.Close();
                            }
                            else
                            {
                                MessageBox.Show("Unable to open target file. Try creating a TEMP directory on your C: drive");
                            }

                            if (nTextFile)
                            {
                                #region Use External Editor
                                if (useExternalEditor)
                                {
                                    Process rc;

                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.FileName = editor;
                                    startInfo.Arguments = szTargetFileName;

                                    rc = Process.Start(startInfo);
                                }
                                #endregion
                                else
                                {
                                    MessageBox.Show("We currently do not have a built in editor");
                                }
                            }
                            else
                            {
                                if (useExternalEditor)
                                {
                                    Process rc;

                                    ProcessStartInfo startInfo = new ProcessStartInfo();
                                    startInfo.FileName = editor;
                                    startInfo.Arguments = szTargetFileName;

                                    rc = Process.Start(startInfo);
                                }
                                else
                                {
                                    MessageBox.Show("The internal editor does not support Binary file editing.");
                                }
                            }

                            Cursor = currentCursor;
                        }
                        else
                        {
                            // FLEX files NEVER start on track 0 - so this must be a bogus directory entry

                            MessageBox.Show("The directory entry points to a non-existent file");
                        }
                    }
                    else
                    {
                        // handle displaying the contents of an HIER directory in a new list View

                        textBoxListViewCurrentPath.Visible = true;
                        btnButtonBack.Visible = true;

                        // only do this if this is the first time

                        if (textBoxListViewCurrentPath.Text == "")
                            lstviewListFiles.Height = lstviewListFiles.Height - 40;

                        textBoxListViewCurrentPath.Text = textBoxListViewCurrentPath.Text + "/" + lstviewListFiles.Items[nSelectedFile].SubItems[0].Text;

                        hierDirectories.Add(listViewTag);

                        RefreshList();
                        //MessageBox.Show("HIER sub directories are not yet implemented");
                    }
                }
            }
        }

        private void btnButtonBack_Click(object sender, EventArgs e)
        {
            string[] pathParts = textBoxListViewCurrentPath.Text.Split('/');
            textBoxListViewCurrentPath.Text = "";

            for (int i = 1; i < pathParts.Length - 1; i++)
            {
                if (textBoxListViewCurrentPath.Text.Length > 0)
                    textBoxListViewCurrentPath.Text += "/";

                textBoxListViewCurrentPath.Text += pathParts[i];
            }

            if (textBoxListViewCurrentPath.Text == "")
            {
                textBoxListViewCurrentPath.Visible = false;
                btnButtonBack.Visible = false;

                lstviewListFiles.Height = lstviewListFiles.Height + 40;
            }
            else
                textBoxListViewCurrentPath.Text = "/" + textBoxListViewCurrentPath.Text;

            // remove the last one from the list

            hierDirectories.RemoveAt(hierDirectories.Count - 1);

            // and refresh the list.

            RefreshList();
        }

        #endregion

        private void lstViewPartitions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            //ListView.SelectedListViewItemCollection selectedItems = lstviewListPartitions.SelectedItems;

            ListViewItem lvi = e.Item;
            virtualFloppyManipulationRoutines.PartitionBias = (long)(lvi.Tag);
            RefreshList();
        }

        private void treeViewFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        bool CanCollapse = true;

        // Add a BeforeCollapse event handler
        private void treeViewFiles_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (CanCollapse)
                Application.DoEvents();
            else
            {
                e.Cancel = true;
                CanCollapse = true;
            }
        }

        private void treeViewFiles_OS9_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            CanCollapse = false;

            //string tagInfo = ((NodeAttributes)e.Node.Tag).byteCount + " " + ((NodeAttributes)e.Node.Tag).fileDesriptorSector;

            if ((NodeAttributes)e.Node.Tag != null)
            {
                if (((NodeAttributes)e.Node.Tag).fileDesriptorSector != virtualFloppyManipulationRoutines.os9StartingRootSector)
                {

                    // position to the file descriptor sector for this file.

                    int fileDescriptorOffset = ((NodeAttributes)e.Node.Tag).fileDesriptorSector * virtualFloppyManipulationRoutines.nLSNBlockSize;

                    virtualFloppyManipulationRoutines.Seek(fileDescriptorOffset, SeekOrigin.Begin);
                    byte[] buffer = new byte[virtualFloppyManipulationRoutines.nLSNBlockSize];

                    OS9_FILE_DESCRIPTOR fd = new OS9_FILE_DESCRIPTOR();
                    virtualFloppyManipulationRoutines.GetOS9FileDescriptor(ref fd, fileDescriptorOffset);

                    try
                    {
                        // put the file in the TempPath folder under the same directory structure as the original file

                        string tempPath = Path.GetTempPath();                           // get the user's temp path
                        string tempFilename = Path.Combine(tempPath, e.Node.FullPath);  // create the target filename with complete path
                        if ((((NodeAttributes)e.Node.Tag).fileAttributes & 0x80) != 0)
                        {
                            tempFilename = Path.Combine(tempPath, string.Format("{0}.dirSectors", e.Node.FullPath));   // create the target filename with complete path
                        }
                        string fullDirectoryPath = Path.GetDirectoryName(tempFilename); // get just the directory we are going to put the file in

                        // Now make sure the directory exists

                        Directory.CreateDirectory(fullDirectoryPath);

                        // Open the output file

                        using (BinaryWriter bw = new BinaryWriter(File.Open(tempFilename, FileMode.OpenOrCreate, FileAccess.Write)))
                        {
                            // The nByteCount has the file size and each segment array has a start sector and number of contiguous sectors in it.
                            //
                            //      read bytes until either one of these two is exhausted. If we run out of sectors in this segment array entry
                            //      but we still have bytes to read, go to next segment array entry

                            long bytesRemaining = ((NodeAttributes)e.Node.Tag).byteCount;

                            for (int index = 0; index < fd.alSEGArray.Count && bytesRemaining > 0; index++)
                            {
                                OS9_SEG_ENTRY segEntry = (OS9_SEG_ENTRY)fd.alSEGArray[index];

                                int sectorsRemainingInSegment = segEntry.nSize;
                                int sectorToRead = segEntry.nSector;

                                while (sectorsRemainingInSegment > 0)
                                {
                                    // here is where we read from the open file stream (fs) and write to the new file (bw)

                                    long position = virtualFloppyManipulationRoutines.CurrentPosition;        // remember current position

                                    long bytesToRead = virtualFloppyManipulationRoutines.nLSNBlockSize;
                                    if (bytesRemaining < virtualFloppyManipulationRoutines.nLSNBlockSize)
                                    {
                                        bytesToRead = bytesRemaining;
                                    }

                                    virtualFloppyManipulationRoutines.Seek(sectorToRead * virtualFloppyManipulationRoutines.nLSNBlockSize, SeekOrigin.Begin);
                                    virtualFloppyManipulationRoutines.Read(buffer, 0, (int)bytesToRead);
                                    bw.Write(buffer, 0, (int)bytesToRead);
                                    bytesRemaining -= bytesToRead;

                                    virtualFloppyManipulationRoutines.Seek(position, SeekOrigin.Begin);

                                    ++sectorToRead;
                                    --sectorsRemainingInSegment;
                                }
                            }
                            //bw.Close();
                        }

                        // now send the file to the editor

                        #region Use External Editor
                        if (useExternalEditor)
                        {
                            Process rc;

                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.FileName = editor;
                            startInfo.Arguments = tempFilename;

                            rc = Process.Start(startInfo);
                        }
                        #endregion
                        else
                        {
                            string szFile = string.Format("{0}", e.Node.Text);                      // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);
                            string szTargetFileName = string.Format("{0}\\{1}", tempPath, szFile);  // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);

                            MessageBox.Show("We currently do not have a built in editor");
                        }
                    }
                    catch (Exception ex)
                    {
                        //if (swOut == null)
                        Console.WriteLine(ex.Message);
                        //else
                        //    swOut.WriteLine(e.Message);
                    }
                }
                else
                {
                    // user double clicked the file name - gather the description and send to editor

                    string tempPath = Path.GetTempPath();
                    string tempFilename = Path.Combine(tempPath, string.Format("{0}.description", e.Node.FullPath));   // create the target filename with complete path
                    string fullDirectoryPath = Path.GetDirectoryName(tempFilename);                                         // get just the directory we are going to put the file in

                    // Now make sure the directory exists

                    Directory.CreateDirectory(fullDirectoryPath);

                    // Open the output file

                    BinaryWriter bw = new BinaryWriter(File.Open(tempFilename, FileMode.OpenOrCreate, FileAccess.Write));
                    bw.Write(dsketteDescription.ToCharArray(0, dsketteDescription.Length), 0, dsketteDescription.Length);
                    bw.Close();

                    // now send thr file to the editor

                    #region Use External Editor
                    if (useExternalEditor)
                    {
                        Process rc;

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = editor;
                        startInfo.Arguments = tempFilename;

                        rc = Process.Start(startInfo);
                    }
                    #endregion
                    else
                    {
                        string szFile = string.Format("{0}", e.Node.Text);                      // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);
                        string szTargetFileName = string.Format("{0}\\{1}", tempPath, szFile);  // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);

                        MessageBox.Show("We currently do not have a built in editor");
                    }
                }
            }
        }

        private void treeViewFiles_UniFLEX_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            CanCollapse = false;

            if ((NodeAttributes)e.Node.Tag != null)
            {
                if (((NodeAttributes)e.Node.Tag).fdnIndex != 0)
                {
                    NodeAttributes tagData = (NodeAttributes)e.Node.Tag;

                    try
                    {
                        // the user clicked on an actual file or directory in the tree view - put the file in the TempPath folder under the same directory structure as the original file

                        string tempPath = Path.GetTempPath();                           // get the user's temp path
                        string tempFilename = Path.Combine(tempPath, e.Node.FullPath);  // create the target filename with complete path

                        // see if the node has children - if it does, it is a directory

                        // if (e.Node.FirstNode != null)               
                        if ((tagData.fileAttributes & 0x80) != 0)
                        {
                            //// this not a file, but ratherm it is a directory - Show somthing interesting about it someday

                            //// BUT NOT THIS
                            #region don't do this
                            //tempFilename = Path.Combine(tempPath, string.Format("{0}.dirSectors", e.Node.FullPath));   // create the target filename with complete path

                            //// put the file in the TempPath folder under the same directory structure as the original file

                            //string fullDirectoryPath = Path.GetDirectoryName(tempFilename); // get just the directory we are going to put the file in

                            //// Now make sure the directory exists

                            //Directory.CreateDirectory(fullDirectoryPath);
                            //TreeNode treeNode = treeViewFiles.SelectedNode;

                            //string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(fullDirectoryPath + @"\", "");
                            //ExportSingleUniFLEXFile(fullDirectoryPath, Path.GetFileName(innerSourcePath), (NodeAttributes)(treeNode.Tag), treeNode.FullPath);

                            //// now send the file to the editor

                            //#region Use External Editor
                            //if (useExternalEditor)
                            //{
                            //    Process rc;

                            //    ProcessStartInfo startInfo = new ProcessStartInfo();
                            //    startInfo.FileName = editor;
                            //    startInfo.Arguments = tempFilename;

                            //    rc = Process.Start(startInfo);
                            //}
                            //#endregion
                            //else
                            //{
                            //    string szFile = string.Format("{0}", e.Node.Text);                      // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);
                            //    string szTargetFileName = string.Format("{0}\\{1}", tempPath, szFile);  // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);

                            //    frmFileEditor pDlg = new frmFileEditor(dialogConfigType, tempFilename, szFile, virtualFloppyManipulationRoutines);
                            //    pDlg.pDlgInvoker = this;
                            //    pDlg.Show(this.Parent);
                            //}
                            #endregion
                        }
                        else
                        {
                            // user clicked on a leaf - see if it is actually a file or just an empty directory (for now assume file)

                            // put the file in the TempPath folder under the same directory structure as the original file

                            string fullDirectoryPath = Path.GetDirectoryName(tempFilename); // get just the directory we are going to put the file in

                            // Now make sure the directory exists

                            Directory.CreateDirectory(fullDirectoryPath);

                            TreeNode treeNode = treeViewFiles.SelectedNode;

                            string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(fullDirectoryPath + @"\", "");
                            ExportSingleUniFLEXFile(fullDirectoryPath, Path.GetFileName(innerSourcePath), (NodeAttributes)(treeNode.Tag), treeNode.FullPath);

                            // now send the file to the editor

                            #region Use External Editor
                            if (useExternalEditor)
                            {
                                Process rc;

                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.FileName = editor;
                                startInfo.Arguments = tempFilename;

                                rc = Process.Start(startInfo);
                            }
                            #endregion
                            else
                            {
                                string szFile = string.Format("{0}", e.Node.Text);                      // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);
                                string szTargetFileName = string.Format("{0}\\{1}", tempPath, szFile);  // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);

                                MessageBox.Show("We currently do not have a built in editor");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    // user double clicked the diskette image file name - gather the description and send to editor

                    string tempPath = Path.GetTempPath();
                    string tempFilename = Path.Combine(tempPath, string.Format("{0}.description", e.Node.FullPath));   // create the target filename with complete path
                    string fullDirectoryPath = Path.GetDirectoryName(tempFilename);                                    // get just the directory we are going to put the file in

                    // Now make sure the directory exists

                    Directory.CreateDirectory(fullDirectoryPath);

                    // Open the output file

                    BinaryWriter bw = new BinaryWriter(File.Open(tempFilename, FileMode.OpenOrCreate, FileAccess.Write));
                    bw.Write(dsketteDescription.ToCharArray(0, dsketteDescription.Length), 0, dsketteDescription.Length);
                    bw.Close();

                    // now send thr file to the editor

                    #region Use External Editor
                    if (useExternalEditor)
                    {
                        Process rc;

                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = editor;
                        startInfo.Arguments = tempFilename;

                        rc = Process.Start(startInfo);
                    }
                    #endregion
                    else
                    {
                        string szFile = string.Format("{0}", e.Node.Text);                      // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);
                        string szTargetFileName = string.Format("{0}\\{1}", tempPath, szFile);  // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);

                        MessageBox.Show("We currently do not have a built in editor");
                    }
                }
            }
        }

        private void treeViewFiles_Minix_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ((NodeAttributes)e.Node.Tag != null)
            {
                NodeAttributes attributes = (NodeAttributes)e.Node.Tag;

                if (((NodeAttributes)e.Node.Tag).iNode != 0)
                {
                    // 0 is an invalid iNode - they are 1 based.

                    int iNodeOfFile = attributes.iNode;
                    long byteCount = attributes.byteCount;

                    // go read the file from the minix class passing in the iNode of the node Map entry
                    // and the byte count.
                    //
                    //      we need to pass in the node index as a zero based value, so subtract one
                    //      since it is 1 based in the tag.

                    byte[] fileContent = virtualFloppyManipulationRoutines.minixImage.RetrieveFile(iNodeOfFile - 1, byteCount);
                    try
                    {
                        string tempPath = Path.GetTempPath();                           // get the user's temp path
                        string tempFilename = Path.Combine(tempPath, e.Node.FullPath);  // create the target filename with complete path

                        if ((attributes.mode & virtualFloppyManipulationRoutines.minixImage.I_DIRECTORY) == virtualFloppyManipulationRoutines.minixImage.I_DIRECTORY)
                        {
                            // this not a file, but ratherm it is a directory - Show somthing interesting about it someday
                        }
                        else
                        {
                            // user clicked on a leaf - see if it is actually a file or just an empty directory (for now assume file)

                            // put the file in the TempPath folder under the same directory structure as the original file

                            string fullDirectoryPath = Path.GetDirectoryName(tempFilename); // get just the directory we are going to put the file in

                            // Now make sure the directory exists

                            Directory.CreateDirectory(fullDirectoryPath);

                            TreeNode treeNode = treeViewFiles.SelectedNode;

                            string innerSourcePath = treeViewFiles.SelectedNode.FullPath.Replace(fullDirectoryPath + @"\", "");
                            virtualFloppyManipulationRoutines.minixImage.ExportSingleMinixFile(fullDirectoryPath, Path.GetFileName(innerSourcePath), (NodeAttributes)(treeNode.Tag), treeNode.FullPath, fileContent);

                            // now send the file to the editor

                            #region Use External Editor
                            if (useExternalEditor)
                            {
                                Process rc;

                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.FileName = editor;
                                startInfo.Arguments = tempFilename;

                                rc = Process.Start(startInfo);
                            }
                            #endregion
                            else
                            {
                                string szFile = string.Format("{0}", e.Node.Text);                      // sprintf_s (szFile, sizeof (szFile), "%s.%s", szFileName, szFileExtension);
                                string szTargetFileName = string.Format("{0}\\{1}", tempPath, szFile);  // sprintf_s (szTargetFileName, sizeof (szTargetFileName), "%s\\%s", strTempDir, szFile);

                                MessageBox.Show("We currently do not have a built in editor");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void treeViewFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            switch (virtualFloppyManipulationRoutines.CurrentFileFormat)
            {
                case fileformat.fileformat_OS9:
                    treeViewFiles_OS9_NodeMouseDoubleClick(sender, e);
                    break;

                case fileformat.fileformat_UniFLEX:
                    treeViewFiles_UniFLEX_NodeMouseDoubleClick(sender, e);
                    break;

                case fileformat.fileformat_MINIX_68K:
                    treeViewFiles_Minix_NodeMouseDoubleClick(sender, e);
                    break;

                case fileformat.fileformat_MINIX_IBM:
                    treeViewFiles_Minix_NodeMouseDoubleClick(sender, e);
                    break;

                default:
                    MessageBox.Show("OOPS");
                    break;
            }
        }

        private void menuStrip1_MenuActivate(object sender, EventArgs e)
        {
            toolStripMenuItemFileClose.Enabled = btnButtonClose.Enabled;
            toolStripMenuItemFileRefresh.Enabled = btnButtonRefresh.Enabled;
            toolStripMenuItemEditSelectAll.Enabled = btnButtonSelectAll.Enabled;
            toolStripMenuItemEditDeselectAll.Enabled = btnButtonDeselectAll.Enabled;
            toolStripMenuItemImageFileImport.Enabled = btnButtonImport.Enabled;
            toolStripMenuItemImageFileExport.Enabled = btnButtonExport.Enabled;
            toolStripMenuItemImageFileCreateDirectory.Enabled = btnButtonNewDirectory.Enabled;
            toolStripMenuItemImageFileImportDirectory.Enabled = btnButtonImportDir.Enabled;
            toolStripMenuItemImageFileDelete.Enabled = btnButtonDelete.Enabled;
        }

        private void treeViewFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        private void treeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnButtonExport.Enabled = true;
            if (treeViewFiles.SelectedNode != null)
            {
                if ((NodeAttributes)treeViewFiles.SelectedNode.Tag != null)
                {
                    int fileAttributes = ((NodeAttributes)treeViewFiles.SelectedNode.Tag).fileAttributes;

                    // since this application does not support multi node select in the tree view, the only way to select multiple files
                    // for export is to select a directory in the tree view. This will copy all files in the directory recursively.

                    if ((fileAttributes & 0x80) != 0)
                    {
                        // if this is a directory and it is empty - turn off the Export button, otherwise - leave it on

                        int numberOfChildNodes = treeViewFiles.SelectedNode.GetNodeCount(false);
                        if (numberOfChildNodes == 0)
                            btnButtonExport.Enabled = false;
                    }
                }
            }
        }

        private void recentFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeViewFiles_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first deselect the selected line

            int selectedFileIndex = LocateSelectedFile();

            lstviewListFiles.Items[selectedFileIndex].Selected = false;
            lstviewListFiles.Items[selectedFileIndex].Focused = false;

            // now present dialog of what to find.

            frmFind dlg = new frmFind();
            DialogResult r = dlg.ShowDialog();
            if (r == DialogResult.OK)
            {
                string filename = dlg.fileName.ToLower();
                string extension = dlg.extension.ToLower();

                if (lstviewListFiles.Visible)
                {
                    for (int i = 0, nFileCount = lstviewListFiles.Items.Count; i < nFileCount; i++)
                    {
                        ListViewItem lvi = lstviewListFiles.Items[i];
                        if (lvi.Text.ToLower().Contains(filename))
                        {
                            bool found = true;

                            if (extension.Length > 0)
                            {
                                ListViewItem.ListViewSubItem lviSubItem1 = lstviewListFiles.Items[i].SubItems[1];
                                if (!lviSubItem1.Text.ToLower().Contains(extension))
                                {
                                    found = false;
                                }
                            }

                            if (found)
                            {
                                lstviewListFiles.Items[i].Selected = true;
                                lstviewListFiles.Items[i].Focused = true;
                                lstviewListFiles.Items[i].EnsureVisible();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void convertDSKToIMAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (virtualFloppyManipulationRoutines != null)
            {

                virtualFloppyManipulationRoutines.GetFLEXGeometry();
                //if (virtualFloppyManipulationRoutines.getGeometryErrorMessage.Length > 0)
                //{
                //    MessageBox.Show(virtualFloppyManipulationRoutines.getGeometryErrorMessage);
                //}

                //if (virtualFloppyManipulationRoutines.currentDiskDiameter != 0 && virtualFloppyManipulationRoutines.currentDiskGeometry != VirtualFloppyManipulationRoutines.ValidFLEXGeometries.UNKNOWN)
                //{
                //    string message = string.Format("    Cylinders: {0}\r\n    Sectors on track 0: {1}\r\n    Max Sectors: {2}\r\n\r\n", virtualFloppyManipulationRoutines.maxTrack + 1, virtualFloppyManipulationRoutines.sectorOnTrackZero, virtualFloppyManipulationRoutines.maxSector);

                //    switch (virtualFloppyManipulationRoutines.maxSector)
                //    {
                //        case 18:
                //            message += "Will convert to SSDD 5.25\" image";
                //            break;
                //        case 26:
                //            message += "Will convert to SSDD 8\" image";
                //            break;
                //        case 36:
                //            message += "Will convert to DSDD 5.25\" image";
                //            break;
                //        case 52:
                //            message += "Will convert to DSDD 8\" image";
                //            break;
                //    }

                //    DialogResult r = MessageBox.Show(string.Format("This appears to be a {0}\" {1} sided double density image\r\n\r\n{2}\r\n\r\nDo you wish to proceed?", virtualFloppyManipulationRoutines.isFiveInch ? "5.25" : "8", virtualFloppyManipulationRoutines.singleSided ? "single" : "double", message), "", MessageBoxButtons.YesNo);
                //    if (r == DialogResult.Yes)
                //    {

                frmDialogConvertDSKToIMA dlg = new frmDialogConvertDSKToIMA(virtualFloppyManipulationRoutines);
                dlg.ShowDialog();

                //    }
                //}
            }
            else
            {
                MessageBox.Show("You must have a diskette image file open first");
            }
        }

        private void verifyImageIntegrityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool linkageError = false;

            int directorySectorsOnTrackZero = virtualFloppyManipulationRoutines.DirectorySectorsOnTrackZero();
            int directorySectorsTotal = virtualFloppyManipulationRoutines.DirectorySectorsTotal();
            int directorySectorsFromFreeChain = directorySectorsTotal - directorySectorsOnTrackZero;

            // this can only be used on FLEX and FLEX_IMA diskette images

            // we start by getting a List of all of the files in the directory

            RAW_SIR informationRecord = virtualFloppyManipulationRoutines.ReadRAW_FLEX_SIR();
            List<DIR_ENTRY> dirEntries = virtualFloppyManipulationRoutines.GetFLEXDirectoryList(informationRecord);

            int freeChainStartTrack     = informationRecord.cFirstUserTrack;
            int freeChainStartSector    = informationRecord.cFirstUserSector;
            int freeChainEndTrack       = informationRecord.cLastUserTrack;
            int freeChainEndSector      = informationRecord.cLastUserSector;

            // this will be used to tally the sectors used to compare to the number
            // of sectors available in the free chain and the number of sector on
            // the disk.

            // the total number of sectors available on the disk immediately after formatting is
            // informationRecord.cMaxTrack * informationRecord.cMaxSector since the max track is
            // xero based and the maxSector is 1 based and we do not count track 0.

            int totalSectorsOnDisk = informationRecord.cMaxTrack * informationRecord.cMaxSector;
            int totalAvailableSectors = informationRecord.cTotalSectorsHi * 256 + informationRecord.cTotalSectorsLo;

            int totalUsedSectors = 0;

            string resultsMessage = "This image is intact";

            foreach (DIR_ENTRY dirEntry in dirEntries)
            {
                string filename     = ASCIIEncoding.ASCII.GetString(dirEntry.caFileName).Trim('\0');
                string extension    = ASCIIEncoding.ASCII.GetString(dirEntry.caFileExtension).Trim('\0');

                int startTrack  = dirEntry.cStartTrack;
                int startSector = dirEntry.cStartSector;
                int endTrack    = dirEntry.cEndTrack;
                int endSector   = dirEntry.cEndSector;

                bool randomFile = dirEntry.cRandomFileInd == 0x00 ? false : true;

                int totalSectors = dirEntry.cTotalSectorsHi * 256 + dirEntry.cTotalSectorsLo;

                totalUsedSectors += totalSectors;

                // we now have all of the information we need to see if this file is intact
                //
                //  start by getting the linkage data from the first sector and follow the linkage chain counting sectors.
                //
                //      if we do not have linkage bytes of 00 00 in the last sector - there is a problem.
                //      if we have linkage to a non-existing track or sector - we have a problem.

                byte[] linkage = new byte[2];

                byte track  = (byte)startTrack;
                byte sector = (byte)startSector;

                for (int i = 0; i < totalSectors; i++)
                {
                    // calculate the offset and read the linkage bytes

                    long offset = virtualFloppyManipulationRoutines.CalcFileOffset(informationRecord.cMaxSector, track, sector);
                    virtualFloppyManipulationRoutines.Seek(offset, SeekOrigin.Begin);
                    virtualFloppyManipulationRoutines.Read(linkage, 0, 2);

                    track = linkage[0];
                    sector = linkage[1];
                }

                if (linkage[0] != 0 && linkage[1] != 0)
                {
                    // the linkage bytes of the last sector read should be 00 00.
                    resultsMessage = string.Format("File {0}.{1} linkage bytes of last sector were not 00 00", filename, extension);
                    linkageError = true;
                    break;
                }
            }

            if (totalSectorsOnDisk - (totalUsedSectors + directorySectorsFromFreeChain ) != totalAvailableSectors)
            {
                if (linkageError)
                    resultsMessage += "\ntotalSectorsOnDisk - totalUsedSectors doe not equal totalAvailableSectors";
                else
                    resultsMessage = "totalSectorsOnDisk - totalUsedSectors doe not equal totalAvailableSectors";
            }

            MessageBox.Show(resultsMessage);
        }
    }

    // This cannot be the first class in the dialog. Compile time error will result if you attempt to move this to the top.

    public class OS9FileToCopy
    {
        public bool fileExists = false;
        public string filename;
        public string safeFilename;
        public int requiredNumberOfClusters;
        public long fileSize;
        public bool skipCopy = false;
    }
}
