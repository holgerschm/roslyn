using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Editor.Implementation.Highlighting;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.USharp.EditorFeatures.Highlighting.KeywordHighlighters
{
    [ExportHighlighter(LanguageNames.USharp)]
    internal class UsingStatementHighlighter : AbstractKeywordHighlighter<UsingStatementSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(
            UsingStatementSyntax usingStatement, CancellationToken cancellationToken)
        {
            yield return usingStatement.UsingKeyword.Span;
        }
    }
}
