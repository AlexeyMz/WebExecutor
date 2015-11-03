using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebExecutor
{
    public interface IDownloadManager
    {
        IEnumerable<IDownload> Downloads { get; }

        void AddDownload(Uri resource, string fileName);
    }
}
