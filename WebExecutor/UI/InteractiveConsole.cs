using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using NLua;

namespace WebExecutor
{
    public partial class InteractiveConsole : UserControl, IDebugConsole
    {
        const string MessageDivider = "---\n";

        MessageKind? previousMessageKind;

        public InteractiveConsole()
        {
            InitializeComponent();
        }

        private void InteractiveConsole_Load(object sender, EventArgs e)
        {
            logEditor.IsReadOnly = true;
        }

        public void AppendMessage(MessageKind kind, string message)
        {
            if (!message.EndsWith("\n")) { message += "\n"; }
            var offset = logEditor.Document.TextLength;

            string prefix = "";
            bool errorHasSameKind = kind != previousMessageKind;
            if (offset > 0 && (previousMessageKind == null || errorHasSameKind || kind == MessageKind.Error))
            {
                prefix = MessageDivider;
            }

            logEditor.IsReadOnly = false;
            logEditor.Document.Insert(offset, prefix + message);

            Color markerColor = Color.White;
            if (kind == MessageKind.Debug) { markerColor = Color.LightSkyBlue; }
            else if (kind == MessageKind.Error) { markerColor = Color.LightSalmon; }

            logEditor.Document.MarkerStrategy.AddMarker(
                new TextMarker(offset + prefix.Length, message.Length, TextMarkerType.SolidBlock, markerColor));

            logEditor.Document.RequestUpdate(new TextAreaUpdate(
                TextAreaUpdateType.PositionToEnd, logEditor.Document.OffsetToPosition(offset)));
            logEditor.Document.CommitUpdate();

            logEditor.IsReadOnly = true;
            logEditor.ActiveTextAreaControl.Caret.Position = logEditor.Document.OffsetToPosition(
                logEditor.Document.TextLength);
            logEditor.ActiveTextAreaControl.ScrollToCaret();

            previousMessageKind = kind;
        }

        public void AppendFolded(string message)
        {
            if (!message.EndsWith("\n")) { message += "\n"; }

            logEditor.IsReadOnly = false;
            var offset = logEditor.Document.TextLength;
            logEditor.Document.Insert(offset, message);

            logEditor.Document.RequestUpdate(new TextAreaUpdate(
                TextAreaUpdateType.PositionToEnd, logEditor.Document.OffsetToPosition(offset)));
            logEditor.Document.CommitUpdate();

            var foldings = new List<FoldMarker>(logEditor.Document.FoldingManager.FoldMarker);
            var title = message.Substring(0, Math.Min(
                Math.Min(message.Length, message.IndexOfAny(new[] {'\r', '\n'})), 70));
            if (title.Length < message.Length) { title += "..."; }
            foldings.Add(new FoldMarker(logEditor.Document, offset, message.Length - 1, title, true));
            logEditor.Document.FoldingManager.UpdateFoldings(foldings);

            logEditor.IsReadOnly = true;
            logEditor.ActiveTextAreaControl.Caret.Position = logEditor.Document.OffsetToPosition(
                logEditor.Document.TextLength);
            logEditor.ActiveTextAreaControl.ScrollToCaret();

            previousMessageKind = null;
        }

        public void Clear()
        {
            logEditor.Document.TextContent = "";
            logEditor.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
            logEditor.Document.CommitUpdate();
        }
    }
}
