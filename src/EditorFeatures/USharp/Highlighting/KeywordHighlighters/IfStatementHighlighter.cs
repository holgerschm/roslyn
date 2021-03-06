﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor.Implementation.Highlighting;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Editor.USharp.Highlighting.KeywordHighlighters
{
    [ExportHighlighter(LanguageNames.USharp)]
    internal class IfStatementHighlighter : AbstractKeywordHighlighter<IfStatementSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(
            IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            if (ifStatement.Parent.Kind() != SyntaxKind.ElseClause)
            {
                return ComputeSpans(ifStatement);
            }

            return Enumerable.Empty<TextSpan>();
        }

        private IEnumerable<TextSpan> ComputeSpans(
            IfStatementSyntax ifStatement)
        {
            yield return ifStatement.IfKeyword.Span;

            // Loop to get all the else if parts
            while (ifStatement != null && ifStatement.Else != null)
            {
                // Check for 'else if' scenario' (the statement in the else clause is an if statement)
                var elseKeyword = ifStatement.Else.ElseKeyword;

                if (ifStatement.Else.Statement is IfStatementSyntax elseIfStatement)
                {
                    if (OnlySpacesBetween(elseKeyword, elseIfStatement.IfKeyword))
                    {
                        // Highlight both else and if tokens if they are on the same line
                        yield return TextSpan.FromBounds(
                            elseKeyword.SpanStart,
                            elseIfStatement.IfKeyword.Span.End);
                    }
                    else
                    {
                        // Highlight the else and if tokens separately
                        yield return elseKeyword.Span;
                        yield return elseIfStatement.IfKeyword.Span;
                    }

                    // Continue the enumeration looking for more else blocks
                    ifStatement = elseIfStatement;
                }
                else
                {
                    // Highlight just the else and we're done
                    yield return elseKeyword.Span;
                    break;
                }
            }
        }

        public static bool OnlySpacesBetween(SyntaxToken first, SyntaxToken second)
        {
            return first.TrailingTrivia.AsString().All(c => c == ' ') &&
                   second.LeadingTrivia.AsString().All(c => c == ' ');
        }
    }
}
