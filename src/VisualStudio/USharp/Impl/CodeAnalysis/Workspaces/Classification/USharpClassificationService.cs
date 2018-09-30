using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.VisualStudio.LanguageServices.USharp.CodeAnalysis.Workspaces.Classification
{
    [ExportLanguageService(typeof(IClassificationService), LanguageNames.CSharp), Shared]
    internal class CSharpEditorClassificationService : AbstractClassificationService
    {
        public override void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
        {
            var temp = ArrayBuilder<ClassifiedSpan>.GetInstance();
            ClassificationHelpers.AddLexicalClassifications(text, textSpan, temp, cancellationToken);
            AddRange(temp, result);
            temp.Free();
        }

        public override ClassifiedSpan AdjustStaleClassification(SourceText text, ClassifiedSpan classifiedSpan)
            => ClassificationHelpers.AdjustStaleClassification(text, classifiedSpan);
    }
}
