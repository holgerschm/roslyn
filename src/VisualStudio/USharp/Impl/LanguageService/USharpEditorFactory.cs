using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices.Implementation;

namespace Microsoft.VisualStudio.LanguageServices.USharp.LanguageService
{
    [Guid(Guids.USharpEditorFactoryIdString)]
    internal class USharpEditorFactory : AbstractEditorFactory
    {
        public USharpEditorFactory(IComponentModel componentModel)
            : base(componentModel)
        {
        }

        protected override string ContentTypeName => ContentTypeNames.USharpContentType;
        protected override string LanguageName => LanguageNames.USharp;
    }
}
