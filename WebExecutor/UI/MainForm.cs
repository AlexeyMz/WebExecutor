using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WebExecutor
{
    public partial class MainForm : Form
    {
        DownloadListPanel downloadListPanel;

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

            downloadListPanel = new DownloadListPanel();
            downloadListPanel.VisibleChanged += (s, args) => { downloadsToolStripMenuItem.Checked = downloadListPanel.Visible; };
            downloadsToolStripMenuItem.Click += (s, args) => { ToggleDocked(downloadListPanel); };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ShowDocked(downloadListPanel);
        }

        private void ShowDocked(DockContent dockContent)
        {
            DockState location = DockState.DockRight;
            dockContent.Show(dockPanel, location);
        }

        private void ToggleDocked(DockContent dockContent)
        {
            if (dockContent.Visible)
                dockContent.Hide();
            else
                ShowDocked(dockContent);
        }

        private void CreateWindow(string fileName)
        {
            ExecuterForm executerForm = new ExecuterForm(downloadListPanel, fileName);
            executerForm.Show(dockPanel, DockState.Document);
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
            ActiveExecuterForm?.SaveFile();
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.ForceSaveFile();
        }

        private void close_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.Close();
        }

        private void clearDownloadsButton_Click(object sender, EventArgs e)
        {
            downloadListPanel.ClearDownloads();
        }

        private async void run_Click(object sender, EventArgs e)
        {
            await ActiveExecuterForm?.RunScript();
        }

        private void stop_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.StopScript();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.UndoInTextEditor();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.RedoInTextEditor();
        }

        private void cut_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.TextEditorClipboardHandler.Cut(sender, e);
        }

        private void copy_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.TextEditorClipboardHandler.Copy(sender, e);
        }

        private void paste_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.TextEditorClipboardHandler.Paste(sender, e);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveExecuterForm?.TextEditorClipboardHandler.SelectAll(sender, e);
        }

        private void toolBarVisible_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!downloadListPanel.TryCancelDownloads()) { e.Cancel = true; }
            }
        }
    }
}
