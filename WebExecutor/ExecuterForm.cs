using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using System.IO;
using ICSharpCode.TextEditor.Document;

namespace WebExecutor
{
    public partial class ExecuterForm : Form, IDownloadManager
    {
        bool isSaved = false;
        bool hasUnsavedChanges = false;
        string fileName;

        ScriptExecutor executor;

        List<IDownload> downloads = new List<IDownload>();

        public IEnumerable<IDownload> Downloads
        {
            get { return downloads.ToArray(); }
        }

        public ExecuterForm()
        {
            InitializeComponent();

            HighlightingManager.Manager.AddSyntaxModeFileProvider(new LuaSyntaxModeProvider(typeof(ExecuterForm).Assembly));
            textEditor.SetHighlighting("SharpLua");
            textEditor.Document.DocumentChanged += Document_TextContentChanged;
        }

        public ExecuterForm(string scriptFile)
            : this()
        {
            if (scriptFile != null)
                LoadFile(scriptFile);
        }

        private void ExecuterForm_Load(object sender, EventArgs e)
        {
            UpdateWindowTitle();
        }

        private void ExecuterForm_Shown(object sender, EventArgs e)
        {
            textEditor.Focus();
        }

        private void Document_TextContentChanged(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateWindowTitle();
        }

        private void LoadFile(string scriptFile)
        {
            string script = System.IO.File.ReadAllText(scriptFile);

            textEditor.Document.TextContent = script;
            textEditor.Document.RequestUpdate(
                new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
            textEditor.Document.CommitUpdate();

            isSaved = true;
            hasUnsavedChanges = false;
            fileName = scriptFile;
        }

        private bool TrySaveChanges()
        {
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                       "Script file has changed. Would you like to save changes?",
                       this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    SaveFile();
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public bool SaveFile()
        {
            if (isSaved)
            {
                ProcessProjectSaving();
                hasUnsavedChanges = false;

                UpdateWindowTitle();

                return true;
            }
            else
            {
                bool saved = ForceSaveFile();
                return saved;
            }
        }        

        public bool ForceSaveFile()
        {
            MainForm parentForm = (MainForm)this.ParentForm;
            SaveFileDialog saveDialog = parentForm.SaveDialog;

            bool saved = saveDialog.ShowDialog() == DialogResult.OK;
            if (saved)
            {
                fileName = saveDialog.FileName;
                ProcessProjectSaving();
                isSaved = true;
                hasUnsavedChanges = false;
            }

            UpdateWindowTitle();

            return saved;
        }

        private void ProcessProjectSaving()
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(textEditor.Document.TextContent);
            }
        }

        private void UpdateWindowTitle()
        {
            this.Text = (hasUnsavedChanges ? "*" : string.Empty) +
                (isSaved ? Path.GetFileName(fileName) : "Untitled") +
                ((executor != null && executor.IsRunning) ? " - Running" : string.Empty);
        }

        public void RunScript()
        {
            if (executor != null && executor.IsRunning)
                return;

            executor = new ScriptExecutor(
                this, new InteractiveConsoleWriter(textBoxDebug));

            textBoxDebug.Clear();

            executor.Completed += executor_Completed;
            executor.Run(textEditor.Document.TextContent);

            UpdateWindowTitle();
        }

        private void executor_Completed(object sender, CompletedEventArgs e)
        {
            this.Invoke(new Action(() =>
                {
                    UpdateWindowTitle();
                }));

            ((ScriptExecutor)sender).Completed -= executor_Completed;
            if (e.Error != null)
                MessageBox.Show(e.Error.ToString());

            executor.Dispose();
        }

        public void StopScript()
        {
            if (executor != null)
            {
                executor.Stop();
            }
        }

        public void AddDownload(Uri resource, string fileName)
        {
            DownloadItem download = new DownloadItem(resource, fileName);
            var addDownload = new Action<DownloadItem>((item) =>
                {
                    downloads.Add(item);
                    flowLayoutPanelFiles.Controls.Add(item);
                    flowLayoutPanelFiles.SetFlowBreak(item, true);
                    flowLayoutPanelFiles.ScrollControlIntoView(item);
                    
                    item.Start();
                });

            if (flowLayoutPanelFiles.InvokeRequired)
            {
                flowLayoutPanelFiles.Invoke(addDownload, download);
            }
            else
            {
                addDownload(download);
            }
        }

        public void ClearDownloads()
        {
            downloads = downloads.Where(d =>
                d.DownloadState != DownloadState.Cancelled &&
                d.DownloadState != DownloadState.Completed &&
                d.DownloadState != DownloadState.Error).ToList();
        }

        private bool TryCancelDownloads()
        {
            int activeDownloads = downloads.Count(
                d => d.DownloadState == DownloadState.Downloading);

            if (activeDownloads > 0)
            {
                var result = MessageBox.Show(
                   string.Format("There are {0} active downloads. Cancel them?", activeDownloads),
                   this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    foreach (IDownload item in downloads)
                    {
                        item.Cancel();
                    }
                }
                else if (result == DialogResult.No)
                {
                    return false;
                }
            }

            return true;
        }

        private void ExecuterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool exit = TrySaveChanges() && TryCancelDownloads();

            if (exit)
            {
                StopScript();
            }

            e.Cancel = !exit;
        }
    }
}
