using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebExecutor
{
    public partial class MainForm : Form
    {
        public OpenFileDialog OpenDialog
        {
            get { return openFileDialog; }
        }

        public SaveFileDialog SaveDialog
        {
            get { return saveFileDialog; }
        }

        public ExecuterForm ActiveExecuterForm
        {
            get { return (ExecuterForm)ActiveMdiChild; }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void CreateWindow(string fileName)
        {
            ExecuterForm executerForm = new ExecuterForm(fileName);
            executerForm.MdiParent = this;
            executerForm.Show();
        }

        private void new_Click(object sender, EventArgs e)
        {
            CreateWindow(null);
        }

        private void open_Click(object sender, EventArgs e)
        {
            if (OpenDialog.ShowDialog() == DialogResult.OK)
            {
                CreateWindow(OpenDialog.FileName);
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (ActiveExecuterForm != null)
            {
                ActiveExecuterForm.SaveFile();
            }
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            if (ActiveExecuterForm != null)
            {
                ActiveExecuterForm.ForceSaveFile();
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            if (ActiveExecuterForm != null)
            {
                ActiveExecuterForm.Close();
            }
        }

        private void run_Click(object sender, EventArgs e)
        {
            if (ActiveExecuterForm != null)
            {
                ActiveExecuterForm.RunScript();
            }
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (ActiveExecuterForm != null)
            {
                ActiveExecuterForm.StopScript();
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void cut_Click(object sender, EventArgs e)
        {
        }

        private void copy_Click(object sender, EventArgs e)
        {
        }

        private void paste_Click(object sender, EventArgs e)
        {
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void toolBar_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void statusBar_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void cascade_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void tileVertical_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void tileHorizontal_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void arrangeIcons_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void closeAll_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void clearDownloadsButton_Click(object sender, EventArgs e)
        {
            if (ActiveExecuterForm != null)
            {
                ActiveExecuterForm.ClearDownloads();
            }
        }
    }
}
