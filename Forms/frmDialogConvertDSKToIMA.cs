using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace FloppyMaintenance
{
    public partial class frmDialogConvertDSKToIMA : Form
    {
        public fileformat   currentFileFileFormat;
        public bool         singleSided;
        public bool         isFiveInch;
        public int          currentDiskDiameter;
        public int          maxSector;
        public int          maxTrack;
        public int          sectorOnTrackZero;
        public int          sectorsToEndOfDirectory;
        public bool         trackZeroIsBigEnough;
        public int          sectorsToAddToFreeChain;
        public string       getGeometryErrorMessage;
        public VirtualFloppyManipulationRoutines.ValidFLEXGeometries currentDiskGeometry;

        VirtualFloppyManipulationRoutines virtualFloppyManipulationRoutines;

        int sectorsToCopyForTrackZero = 0;

        public frmDialogConvertDSKToIMA(VirtualFloppyManipulationRoutines _virtualFloppyManipulationRoutines)
        {
            InitializeComponent();

            virtualFloppyManipulationRoutines = _virtualFloppyManipulationRoutines;

            currentFileFileFormat   = virtualFloppyManipulationRoutines.currentFileFileFormat;
            singleSided             = virtualFloppyManipulationRoutines.singleSided;
            isFiveInch              = virtualFloppyManipulationRoutines.isFiveInch;
            currentDiskDiameter     = virtualFloppyManipulationRoutines.currentDiskDiameter;
            maxSector               = virtualFloppyManipulationRoutines.maxSector;
            maxTrack                = virtualFloppyManipulationRoutines.maxTrack;
            sectorOnTrackZero       = virtualFloppyManipulationRoutines.sectorOnTrackZero;
            sectorsToEndOfDirectory = virtualFloppyManipulationRoutines.sectorsToEndOfDirectory;
            trackZeroIsBigEnough    = virtualFloppyManipulationRoutines.trackZeroIsBigEnough;
            sectorsToAddToFreeChain = virtualFloppyManipulationRoutines.sectorsToAddToFreeChain;
            currentDiskGeometry     = virtualFloppyManipulationRoutines.currentDiskGeometry;
            getGeometryErrorMessage = virtualFloppyManipulationRoutines.getGeometryErrorMessage;
        }

        private void frmDialogConvertDSKToIMA_Load(object sender, EventArgs e)
        {
            if (currentDiskGeometry == VirtualFloppyManipulationRoutines.ValidFLEXGeometries.UNKNOWN)
                buttonOK.Enabled = false;

            string targetFilename = Path.ChangeExtension(virtualFloppyManipulationRoutines.currentlyOpenedImageFileName, ".IMA");
            textBoxTargetFileName.Text = targetFilename;

            textBoxFileFormat.ReadOnly = true;
            textBoxFileFormat.BorderStyle = BorderStyle.None;
            if (currentFileFileFormat == fileformat.fileformat_FLEX)
                textBoxFileFormat.Text = "FLEX DSK";
            else
            {
                textBoxFileFormat.BackColor = textBoxFileFormat.BackColor;  // must set background to trigger setting foreground
                textBoxFileFormat.ForeColor = Color.Red;
                textBoxFileFormat.Text = "NOT FLEX DSK FORMAT";
            }

            textBoxSides.ReadOnly = true;
            textBoxSides.BorderStyle = BorderStyle.None;
            if (singleSided)
                textBoxSides.Text = "1";
            else
                textBoxSides.Text = "2";

            textBoxDensity.ReadOnly = true;
            textBoxDensity.BorderStyle = BorderStyle.None;
            if (sectorOnTrackZero == maxSector)
                textBoxDensity.Text = "double";
            else
                textBoxDensity.Text = "single";

            textBoxPhysicalSize.ReadOnly = true;
            textBoxPhysicalSize.BorderStyle = BorderStyle.None;
            textBoxPhysicalSize.Text = isFiveInch ? "5 1/4\"" : "8\"";

            textBoxFileSize.ReadOnly = true;
            textBoxFileSize.BorderStyle = BorderStyle.None;
            textBoxFileSize.Text = virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream.Length.ToString("N0");

            textBoxMaxTrack.ReadOnly = true;
            textBoxMaxTrack.BorderStyle = BorderStyle.None;
            textBoxMaxTrack.Text = maxTrack.ToString();

            textBoxMaxSector.ReadOnly = true;
            textBoxMaxSector.BorderStyle = BorderStyle.None;
            textBoxMaxSector.Text = maxSector.ToString();

            textBoxSectorsOnTrack0.ReadOnly = true;
            textBoxSectorsOnTrack0.BorderStyle = BorderStyle.None;
            textBoxSectorsOnTrack0.Text = sectorOnTrackZero.ToString();

            textBoxSectorsToEndOfDirectory.ReadOnly = true;
            textBoxSectorsToEndOfDirectory.BorderStyle = BorderStyle.None;
            textBoxSectorsToEndOfDirectory.Text = sectorsToEndOfDirectory.ToString();

            textBoxSectorsToAddToFreeChain.ReadOnly = true;
            textBoxSectorsToAddToFreeChain.BorderStyle = BorderStyle.None;
            textBoxSectorsToAddToFreeChain.Text = sectorsToAddToFreeChain.ToString();

            switch (currentDiskGeometry)
            {
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSDD35T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSSD35T:
                    sectorsToCopyForTrackZero = 10;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSDD35T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSSD35T:
                    sectorsToCopyForTrackZero = 20;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSDD40T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSSD40T:
                    sectorsToCopyForTrackZero = 10;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSDD40T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSSD40T:
                    sectorsToCopyForTrackZero = 20;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSDD80T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSSD80T:
                    sectorsToCopyForTrackZero = 10;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSDD80T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSSD80T:
                    sectorsToCopyForTrackZero = 20;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSDD77T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.SSSD77T:
                    sectorsToCopyForTrackZero = 15;
                    break;
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSDD77T:
                case VirtualFloppyManipulationRoutines.ValidFLEXGeometries.DSSD77T:
                    sectorsToCopyForTrackZero = 30;
                    break;
            }


            textBoxMessageArea.ReadOnly = true;
            textBoxMessageArea.BorderStyle = BorderStyle.None;
            textBoxMessageArea.BackColor = textBoxMessageArea.BackColor;
            textBoxMessageArea.ForeColor = Color.Red;
            if (!trackZeroIsBigEnough)
            {
                // for now do not allow converting if the directory is too big to fit on track 0

                //textBoxMessageArea.Text = "Conversion currently not supported because the source image directory requires more sectors than are available on the target format";
                textBoxMessageArea.Text = getGeometryErrorMessage;
                buttonOK.Enabled = false;
            }
            else
            {
                textBoxMessageArea.Text = getGeometryErrorMessage;
            }

            // now show target file info

            int targetFileSize = (sectorsToCopyForTrackZero * 256) + (maxTrack * maxSector * 256);
            textBoxTargetFileSize.ReadOnly = true;
            textBoxTargetFileSize.BorderStyle = BorderStyle.None;
            textBoxTargetFileSize.Text = targetFileSize.ToString("N0");

            textBoxTargetSectorOnTrackZero.ReadOnly = true;
            textBoxTargetSectorOnTrackZero.BorderStyle = BorderStyle.None;
            textBoxTargetSectorOnTrackZero.Text = sectorsToCopyForTrackZero.ToString();
        }

        private void buttonBrowseTargetFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = textBoxTargetFileName.Text;
            DialogResult r = dlg.ShowDialog();
            if (r == DialogResult.OK)
            {
                textBoxTargetFileName.Text = dlg.FileName;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Once we have enough information to create a .IMA image from the .DSK image do this
            //
            //    Step      Action
            //      1       open the new .IMA file using the same name with new extension
            //              while letting the user browse for a new file name
            //      2       write track 0
            //              a.  write the number of sectors specified in currentDiskGeometry
            //              b.  fix the linkage in the last directory sector on target
            //      3       position the source to track 1
            //      4       write the rest of the tracks.
            //      5       if there are extra sectors to link to the directory get them
            //              from the free chain and link them to the end of the directory
            //              sectors on track 0 while fixing the free chain links in the
            //              process.

            bool success = false;
            bool overwrite = true;

            if (File.Exists(textBoxTargetFileName.Text))
            {
                DialogResult r = MessageBox.Show(string.Format("{0} already exists - do you wish to overwrite it?", textBoxTargetFileName.Text), "Overwrite", MessageBoxButtons.YesNo);
                if (r != DialogResult.Yes)
                {
                    overwrite = false;
                }
            }

            if (overwrite)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(textBoxTargetFileName.Text, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    if (sectorsToCopyForTrackZero > 0)
                    {
                        // do last one separately - we have to make sure the linkage bytes are set to zero.
                        // if we have to add more sectors to the diectory we will link them in then.

                        FileStream sourceFile = virtualFloppyManipulationRoutines.currentlyOpenedImageFileStream;

                        sourceFile.Seek(0, SeekOrigin.Begin);       // position to start of source file

                        byte[] trackZeroSectors = new byte[256 * sectorsToCopyForTrackZero];
                        int bytesReadFromTrackZero = sourceFile.Read(trackZeroSectors, 0, 256 * (sectorsToCopyForTrackZero - 1));
                        if (bytesReadFromTrackZero == 256 * (sectorsToCopyForTrackZero - 1))
                        {
                            trackZeroSectors[256 * (sectorsToCopyForTrackZero - 1)] = 0x00;         // next track linkage
                            trackZeroSectors[256 * (sectorsToCopyForTrackZero - 1) + 1] = 0x00;         // next sector linkage
                            trackZeroSectors[256 * (sectorsToCopyForTrackZero - 1) + 2] = 0x00;         // diretory sectors do not have a sequence word
                            trackZeroSectors[256 * (sectorsToCopyForTrackZero - 1) + 3] = 0x00;         // diretory sectors do not have a sequence word

                            // now read the last sector's data (252 bytes) into the track buffer.

                            bytesReadFromTrackZero = sourceFile.Read(trackZeroSectors, 256 * (sectorsToCopyForTrackZero - 1) + 4, 252);

                            // now we have the track zero built - build the rest of the image and do any fixup's required.

                            byte[] theRestOfTheDisk = new byte[maxTrack * maxSector * 256];     // maxTrack is already the number of tracks - 1

                            // seek to start of track 1
                            sourceFile.Seek(sectorOnTrackZero * 256, SeekOrigin.Begin);
                            int remainingDiskBytesRead = sourceFile.Read(theRestOfTheDisk, 0, maxTrack * maxSector * 256);

                            // before we can wite this out we need to see if we need to move any directory entries from the source
                            // to a different spot on the target because the conversion had to truncate the entries because they
                            // would not fit in the new structure.
                            //
                            //  we do this by taking sectors from the free chain (as many as we need) and link them into the 
                            //  last directory entry on track 0. In the process of getting sector from the frre chain, we also
                            //  need to take care of maintaining the entegrity of the free chain.


                            // write the contents of the new disk

                            writer.Write(trackZeroSectors, 0, trackZeroSectors.Length);
                            writer.Write(theRestOfTheDisk, 0, theRestOfTheDisk.Length);

                            success = true;
                        }
                    }
                }

                if (success)
                {
                    MessageBox.Show("Target File created");
                }

                Close();
            }
        }
    }
}
