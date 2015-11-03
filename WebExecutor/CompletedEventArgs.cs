using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebExecutor
{
    public class CompletedEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public CompletedEventArgs()
            : this(null)
        {
        }

        public CompletedEventArgs(Exception error)
        {
            Error = error;
        }
    }
}
