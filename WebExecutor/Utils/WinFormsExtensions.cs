using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebExecutor
{
    public static class WinFormsExtensions
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        public static void ThrowIfNotUIThread(this Control control)
        {
            if (control.InvokeRequired)
            {
                throw new InvalidOperationException("Operation is callable only on UI thread");
            }
        }
    }
}
