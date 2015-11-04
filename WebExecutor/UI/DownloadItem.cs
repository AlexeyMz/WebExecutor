using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebExecutor
{
    public partial class DownloadItem : UserControl, IDownload
    {
        const int BufferSize = 0x2000;

        Uri resource;
        string fileName;

        long contentLength;
        HttpClient httpClient;
        Task<HttpResponseMessage> response;
        FileStream fileStream;

        public DownloadState DownloadState { get; private set; }

        public event EventHandler StateChanged;

        public DownloadItem(Uri resource, string fileName)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            InitializeComponent();

            this.resource = resource;
            this.fileName = fileName;

            textBoxUrl.Text = resource.ToString();
            textBoxFile.Text = fileName;

            DownloadState = DownloadState.NotStarted;
        }

        public DownloadItem() { InitializeComponent(); }

        private void OnStateChanged()
        {
            EventHandler temp = StateChanged;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        private void InitializeDownload()
        {
            httpClient = new HttpClient();
            response = httpClient.GetAsync(resource);
            
            fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        }

        public void Start()
        {
            if (DownloadState == DownloadState.Downloading)
                return;

            try
            {
                InitializeDownload();
            }
            catch (Exception ex)
            {
                DisposeResources();
                DownloadState = DownloadState.Error;
                textBoxStatus.Text = "Error: " + ex.Message;
                buttonRetry.Enabled = true;

                OnStateChanged();
                return;
            }

            contentLength = -1;
            progressBar.Style = ProgressBarStyle.Marquee;

            textBoxStatus.Text = "Downloading";
            DownloadState = DownloadState.Downloading;
            buttonCancel.Enabled = true;
            buttonRetry.Enabled = false;

            OnStateChanged();
            backgroundWorker.RunWorkerAsync();
        }

        public void Cancel()
        {
            if (DownloadState != DownloadState.Downloading)
                return;

            backgroundWorker.CancelAsync();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        private void buttonRetry_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            HttpResponseMessage responseMessage = response.Result;
            long? streamLength = responseMessage.Content.Headers.ContentLength;
            if (streamLength.HasValue)
            {
                this.Invoke(new Action(() => progressBar.Style = ProgressBarStyle.Continuous));
            }

            Stream stream = responseMessage.Content.ReadAsStreamAsync().Result;
            byte[] buffer = new byte[BufferSize];
            long position = 0;

            int readed;
            do
            {
                readed = stream.Read(buffer, 0, buffer.Length);
                fileStream.Write(buffer, 0, readed);

                position += readed;

                if (contentLength >= 0)
                {
                    backgroundWorker.ReportProgress(
                        (int)((float)position * 100 / contentLength));
                }
            }
            while (readed != 0 && !backgroundWorker.CancellationPending);

            if (backgroundWorker.CancellationPending)
                e.Cancel = true;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (contentLength >= 0)
            {
                int progress = e.ProgressPercentage;
                textBoxStatus.Text = "Downloading - " + progress.ToString() + "%";
                progressBar.Value = progress;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DisposeResources();

            progressBar.Style = ProgressBarStyle.Continuous;

            if (e.Error != null)
            {
                DownloadState = DownloadState.Error;
                textBoxStatus.Text = "Error: " + e.Error.Message;
                buttonRetry.Enabled = true;
            }
            else if (e.Cancelled)
            {
                DownloadState = DownloadState.Cancelled;
                textBoxStatus.Text = "Cancelled";
                progressBar.Value = progressBar.Minimum;
                buttonRetry.Enabled = true;
            }
            else
            {
                DownloadState = DownloadState.Completed;
                textBoxStatus.Text = "Completed";
                progressBar.Value = progressBar.Maximum;
            }
            
            buttonCancel.Enabled = false;
            OnStateChanged();
        }

        private void DisposeResources()
        {
            httpClient?.Dispose();
            fileStream?.Close();
        }
    }
}
