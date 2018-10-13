using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.USharp
{
    public partial class USharpSyntaxTree
    {
        private class DebuggerSyntaxTree : ParsedSyntaxTree
        {
            public DebuggerSyntaxTree(CSharpSyntaxNode root, SourceText text)
                : base(
                    text,
                    text.Encoding,
                    text.ChecksumAlgorithm,
                    path: "",
                    options: CSharpParseOptions.Default,
                    root: root,
                    directives: Syntax.InternalSyntax.DirectiveStack.Empty)
            {
            }

            internal override bool SupportsLocations
            {
                get { return true; }
            }
        }
    }
}
