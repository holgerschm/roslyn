using System.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.Editor.USharp.CodeAnalysis.Editor.LanguageServices
{
    [ExportContentTypeLanguageService(ContentTypeNames.USharpContentType, LanguageNames.USharp), Shared]
    internal class USharpContentTypeLanguageService : IContentTypeLanguageService
    {
        private readonly IContentTypeRegistryService _contentTypeRegistry;

        [ImportingConstructor]
        public USharpContentTypeLanguageService(IContentTypeRegistryService contentTypeRegistry)
        {
            _contentTypeRegistry = contentTypeRegistry;
        }

        public IContentType GetDefaultContentType()
        {
            return _contentTypeRegistry.GetContentType(ContentTypeNames.CSharpContentType);
        }
    }
}
