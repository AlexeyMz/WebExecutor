using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebExecutor
{
    public enum DownloadState
    {
        NotStarted,
        Downloading,
        Completed,
        Cancelled,
        Error,
    }
}
