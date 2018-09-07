// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.VisualStudio.LanguageServices.CSharp.ProjectSystemShim;
using Microsoft.VisualStudio.LanguageServices.Implementation.DebuggerIntelliSense;
using Microsoft.VisualStudio.LanguageServices.Implementation.LanguageService;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem;
using Microsoft.VisualStudio.LanguageServices.CSharp.LanguageService;

namespace Microsoft.VisualStudio.LanguageServices.USharp.LanguageService
{
    [ExcludeFromCodeCoverage]
    [Guid(Guids.USharpLanguageServiceIdString)]
    internal partial class USharpLanguageService : AbstractLanguageService<USharpPackage, USharpLanguageService>
    {
        internal USharpLanguageService(USharpPackage package)
            : base(package)
        {
        }

        protected override Guid DebuggerLanguageId
        {
            get
            {
                return Guids.CSharpDebuggerLanguageId;
            }
        }

        protected override string ContentTypeName
        {
            get
            {
                return ContentTypeNames.CSharpContentType;
            }
        }

        public override Guid LanguageServiceId
        {
            get
            {
                return Guids.USharpLanguageServiceId;
            }
        }

        protected override string LanguageName
        {
            get
            {
                return USharpVSResources.CSharp;
            }
        }

        protected override string RoslynLanguageName
        {
            get
            {
                return LanguageNames.CSharp;
            }
        }

        protected override AbstractDebuggerIntelliSenseContext CreateContext(
            IWpfTextView view,
            IVsTextView vsTextView,
            IVsTextLines debuggerBuffer,
            ITextBuffer subjectBuffer,
            Microsoft.VisualStudio.TextManager.Interop.TextSpan[] currentStatementSpan)
        {
            return new CSharpDebuggerIntelliSenseContext(view,
                vsTextView,
                debuggerBuffer,
                subjectBuffer,
                currentStatementSpan,
                this.Package.ComponentModel,
                this.SystemServiceProvider);
        }
    }
}
