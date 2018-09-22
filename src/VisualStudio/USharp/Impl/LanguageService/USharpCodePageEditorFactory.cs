using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServices.Implementation;

namespace Microsoft.VisualStudio.LanguageServices.USharp.LanguageService
{
    [Guid(Guids.USharpCodePageEditorFactoryIdString)]
    internal class USharpCodePageEditorFactory : AbstractCodePageEditorFactory
    {
        public USharpCodePageEditorFactory(AbstractEditorFactory editorFactory)
            : base(editorFactory)
        {
        }
    }
}
