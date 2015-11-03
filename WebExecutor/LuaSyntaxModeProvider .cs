using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebExecutor
{
    internal sealed class LuaSyntaxModeProvider : ISyntaxModeFileProvider
    {
        readonly Assembly assembly;
        readonly ImmutableList<SyntaxMode> syntaxModes;

        public ICollection<SyntaxMode> SyntaxModes
        {
            get { return syntaxModes; }
        }

        public LuaSyntaxModeProvider(Assembly assembly)
        {
            this.assembly = assembly;
            this.syntaxModes = ImmutableList<SyntaxMode>.Empty.Add(
                new SyntaxMode("SharpLua.xshd", "SharpLua", ".lua"));
        }

        public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
        {
            var stream = assembly.GetManifestResourceStream(
              assembly.GetManifestResourceNames().First(name => name.EndsWith(syntaxMode.FileName)));
            return new XmlTextReader(stream);
        }

        public void UpdateSyntaxModeList() {}
    }
}
