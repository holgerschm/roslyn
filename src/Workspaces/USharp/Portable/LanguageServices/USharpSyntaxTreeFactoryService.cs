using System.Composition;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.USharp
{
    [ExportLanguageServiceFactory(typeof(ISyntaxTreeFactoryService), LanguageNames.USharp), Shared]
    internal partial class USharpSyntaxTreeFactoryServiceFactory : ILanguageServiceFactory
    {
        private static readonly CSharpParseOptions _parseOptionWithLatestLanguageVersion = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

        public ILanguageService CreateLanguageService(HostLanguageServices provider)
        {
            return new USharpSyntaxTreeFactoryService(provider);
        }

        internal partial class USharpSyntaxTreeFactoryService : AbstractSyntaxTreeFactoryService
        {
            public USharpSyntaxTreeFactoryService(HostLanguageServices languageServices) : base(languageServices)
            {
            }

            public override ParseOptions GetDefaultParseOptions()
            {
                return CSharpParseOptions.Default;
            }

            public override ParseOptions GetDefaultParseOptionsWithLatestLanguageVersion()
            {
                return _parseOptionWithLatestLanguageVersion;
            }

            public override SyntaxTree CreateSyntaxTree(string fileName, ParseOptions options, Encoding encoding, SyntaxNode root)
            {
                options = options ?? GetDefaultParseOptions();
                return USharpSyntaxTree.Create((CSharpSyntaxNode)root, (CSharpParseOptions)options, fileName, encoding);
            }

            public override SyntaxTree ParseSyntaxTree(string fileName, ParseOptions options, SourceText text, CancellationToken cancellationToken)
            {
                options = options ?? GetDefaultParseOptions();
                return USharpSyntaxTree.ParseText(text, (CSharpParseOptions)options, fileName, cancellationToken);
            }

            public override SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken)
                => CSharpSyntaxNode.DeserializeFrom(stream, cancellationToken);

            public override bool CanCreateRecoverableTree(SyntaxNode root)
            {
                var cu = root as CompilationUnitSyntax;
                return base.CanCreateRecoverableTree(root) && cu != null && cu.AttributeLists.Count == 0;
            }

            public override SyntaxTree CreateRecoverableTree(ProjectId cacheKey, string filePath, ParseOptions options, ValueSource<TextAndVersion> text, Encoding encoding, SyntaxNode root)
            {
                System.Diagnostics.Debug.Assert(CanCreateRecoverableTree(root));
                return RecoverableSyntaxTree.CreateRecoverableTree(this, cacheKey, filePath, options ?? GetDefaultParseOptions(), text, encoding, (CompilationUnitSyntax)root);
            }
        }
    }
}
