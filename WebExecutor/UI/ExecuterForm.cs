using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;
using System.Threading.Tasks;
using System.Threading;

namespace WebExecutor
{
    public partial class ExecuterForm : DockContent
    {
        bool isSaved = false;
        bool hasUnsavedChanges = false;
        string fileName;

        IDownloadManager downloadManager;
        ScriptExecutor executor;
        CancellationTokenSource cancelSource;

        public TextAreaClipboardHandler TextEditorClipboardHandler
        {
            get { return textEditor.ActiveTextAreaControl.TextArea.ClipboardHandler; }
        }

        public ExecuterForm(IDownloadManager downloadManager)
        {
            this.downloadManager = downloadManager;
            InitializeComponent();
            
            textEditor.SetHighlighting("SharpLua");
            textEditor.Document.DocumentChanged += Document_TextContentChanged;
        }

        public ExecuterForm(IDownloadManager downloadManager, string scriptFile)
            : this(downloadManager)
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

        public async Task RunScript()
        {
            if (executor != null && executor.IsRunning)
                return;

            cancelSource = new CancellationTokenSource();
            using (executor = new ScriptExecutor(
                  TaskScheduler.FromCurrentSynchronizationContext(),
                  downloadManager, new InteractiveConsoleWriter(textBoxDebug)))
            {
                textBoxDebug.Clear();
                var task = executor.Run(textEditor.Document.TextContent, cancelSource.Token);
                UpdateWindowTitle();
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException || ex is ThreadAbortException) { /* ignore */ }
                    else { MessageBox.Show(ex.ToString()); }
                }
                UpdateWindowTitle();
            }
        }

        public void StopScript()
        {
            if (cancelSource != null && !cancelSource.IsCancellationRequested)
            {
                cancelSource.Cancel();
            }
        }

        private void ExecuterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool exit = TrySaveChanges();

            if (exit)
            {
                StopScript();
            }

            e.Cancel = !exit;
        }

        public void UndoInTextEditor()
        {
            textEditor.Undo();
        }

        public void RedoInTextEditor()
        {
            textEditor.Redo();
        }
    }
}
