using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloppyMaintenance.Forms
{
    public partial class frmFind : Form
    {
        public string fileName = "";
        public string extension = "";
        public frmFind()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            fileName = textBoxFileName.Text;
            extension = textBoxExtension.Text;
        }
    }
}
