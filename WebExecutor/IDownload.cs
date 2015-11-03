using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebExecutor
{
    public interface IDownload
    {
        DownloadState DownloadState { get; }

        event EventHandler StateChanged;

        void Start();
        void Cancel();
    }
}
