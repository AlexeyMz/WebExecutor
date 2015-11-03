using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WebExecutor
{
    public partial class DownloadListPanel : DockContent, IDownloadManager
    {
        List<IDownload> downloads = new List<IDownload>();

        public IEnumerable<IDownload> Downloads
        {
            get
            {
                this.ThrowIfNotUIThread();
                return downloads.ToArray();
            }
        }

        public DownloadListPanel()
        {
            InitializeComponent();

            tableLayoutPanel.ColumnStyles.Clear();
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
        }

        public IDownload AddDownload(Uri resource, string fileName)
        {
            this.ThrowIfNotUIThread();

            DownloadItem item = new DownloadItem(resource, fileName);
            downloads.Add(item);

            int row = tableLayoutPanel.RowCount++;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0f));
            tableLayoutPanel.Controls.Add(item, 0, row);
            tableLayoutPanel.ScrollControlIntoView(item);

            item.Start();

            return item;
        }

        public void ClearDownloads()
        {
            this.ThrowIfNotUIThread();

            var toRemove = new List<IDownload>();
            foreach (IDownload download in downloads)
            {
                var state = download.DownloadState;
                if (state == DownloadState.Cancelled ||
                    state == DownloadState.Completed ||
                    state == DownloadState.Error)
                {
                    toRemove.Add(download);
                }
            }

            downloads = downloads.Except(toRemove).ToList();
            foreach (var download in toRemove)
            {
                Control child = (Control)download;
                int childIndex = tableLayoutPanel.Controls.GetChildIndex(child);
                tableLayoutPanel.RowStyles.RemoveAt(childIndex);
                tableLayoutPanel.Controls.RemoveAt(childIndex);
            }
        }

        public bool TryCancelDownloads()
        {
            this.ThrowIfNotUIThread();

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
    }
}
