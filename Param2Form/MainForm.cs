using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using paracobNET;

namespace Param2Form
{
    public partial class MainForm : Form
    {
        ParamFile file;

        public MainForm()
        {
            InitializeComponent();
        }

        private void SetupTreeView()
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "param files";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                file = new ParamFile(dialog.FileName);
                SetupTreeView();
            }
        }
    }
}
