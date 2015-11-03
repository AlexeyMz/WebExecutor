using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebExecutor
{
    public class InteractiveConsoleWriter : TextWriter
    {
        public TextBox OutputTextBox { get; private set; }

        public bool WriteToTextBox { get; set; }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public InteractiveConsoleWriter(TextBox output)
        {
            if (output == null) { throw new ArgumentNullException(nameof(output)); }

            OutputTextBox = output;
            WriteToTextBox = true;
        }

        public override void Write(string value)
        {
            AppendText(value);
        }

        public override void Write(char value)
        {
            AppendText(value.ToString());
        }

        public override void Write(char[] buffer)
        {
            AppendText(new string(buffer));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            AppendText(new string(buffer, index, count));
        }

        private void AppendText(string value)
        {
            if (!WriteToTextBox) { return; }
            OutputTextBox.InvokeIfRequired(() => OutputTextBox.AppendText(value));
        }
    }
}
