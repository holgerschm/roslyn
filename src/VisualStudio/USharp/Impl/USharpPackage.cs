// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.VisualStudio.LanguageServices.CSharp.ObjectBrowser;
using Microsoft.VisualStudio.LanguageServices.CSharp.ProjectSystemShim;
using Microsoft.VisualStudio.LanguageServices.CSharp.ProjectSystemShim.Interop;
using Microsoft.VisualStudio.LanguageServices.Implementation;
using Microsoft.VisualStudio.LanguageServices.Implementation.LanguageService;
using Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem;
using Microsoft.VisualStudio.LanguageServices.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Options = Microsoft.VisualStudio.LanguageServices.CSharp.Options;
using Microsoft.VisualStudio.LanguageServices.CSharp.LanguageService;

// NOTE(DustinCa): The EditorFactory registration is in VisualStudioComponents\CSharpPackageRegistration.pkgdef.
// The reason for this is because the ProvideEditorLogicalView does not allow a name value to specified in addition to
// its GUID. This name value is used to identify untrusted logical views and link them to their physical view attributes.
// The net result is that using the attributes only causes designers to be loaded in the preview tab, even when they
// shouldn't be.

namespace Microsoft.VisualStudio.LanguageServices.USharp.LanguageService
{
    // TODO(DustinCa): Put all of this in CSharpPackageRegistration.pkgdef rather than using attributes
    // (See vsproject\cool\coolpkg\pkg\VCSharp_Proj_System_Reg.pkgdef for an example).
    [Guid(Guids.USharpPackageIdString)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideRoslynVersionRegistration(Guids.USharpPackageIdString, "Microsoft Visual U#", productNameResourceID: 116, detailsResourceID: 117)]
    [ProvideLanguageExtension(typeof(USharpLanguageService), ".us")]
    [ProvideLanguageService(Guids.USharpLanguageServiceIdString, "USharp", languageResourceID: 101, RequestStockColors = true, ShowDropDownOptions = true)]

    // C# option pages tree:
    //   CSharp
    //     General (from editor)
    //     Scroll Bars (from editor)
    //     Tabs (from editor)
    //     Advanced
    //     Code Style (category)
    //       General
    //       Formatting (category)
    //         General
    //         Indentation
    //         New Lines
    //         Spacing
    //         Wrapping
    //       Naming
    //     IntelliSense

    [ProvideLanguageEditorOptionPage(typeof(Options.AdvancedOptionPage), "USharp", null, "Advanced", pageNameResourceId: "#102", keywordListResourceId: 306)]
    [ProvideLanguageEditorToolsOptionCategory("USharp", "Code Style", "#114")]
    [ProvideLanguageEditorOptionPage(typeof(Options.Formatting.CodeStylePage), "USharp", @"Code Style", "General", pageNameResourceId: "#108", keywordListResourceId: 313)]
    [ProvideLanguageEditorToolsOptionCategory("CSharp", @"Code Style\Formatting", "#107")]
    [ProvideLanguageEditorOptionPage(typeof(Options.Formatting.FormattingOptionPage), "USharp", @"Code Style\Formatting", "General", pageNameResourceId: "#108", keywordListResourceId: 307)]
    [ProvideLanguageEditorOptionPage(typeof(Options.Formatting.FormattingIndentationOptionPage), "USharp", @"Code Style\Formatting", "Indentation", pageNameResourceId: "#109", keywordListResourceId: 308)]
    [ProvideLanguageEditorOptionPage(typeof(Options.Formatting.FormattingWrappingPage), "USharp", @"Code Style\Formatting", "Wrapping", pageNameResourceId: "#110", keywordListResourceId: 311)]
    [ProvideLanguageEditorOptionPage(typeof(Options.Formatting.FormattingNewLinesPage), "USharp", @"Code Style\Formatting", "NewLines", pageNameResourceId: "#111", keywordListResourceId: 309)]
    [ProvideLanguageEditorOptionPage(typeof(Options.Formatting.FormattingSpacingPage), "USharp", @"Code Style\Formatting", "Spacing", pageNameResourceId: "#112", keywordListResourceId: 310)]
    [ProvideLanguageEditorOptionPage(typeof(Options.NamingStylesOptionPage), "USharp", @"Code Style", "Naming", pageNameResourceId: "#115", keywordListResourceId: 314)]
    [ProvideLanguageEditorOptionPage(typeof(Options.IntelliSenseOptionPage), "USharp", null, "IntelliSense", pageNameResourceId: "#103", keywordListResourceId: 312)]

    [ProvideAutomationProperties("TextEditor", "USharp", Guids.TextManagerPackageString, profileNodeLabelId: 101, profileNodeDescriptionId: 106, resourcePackageGuid: Guids.USharpPackageIdString)]
    [ProvideAutomationProperties("TextEditor", "USharp-Specific", packageGuid: Guids.USharpPackageIdString, profileNodeLabelId: 104, profileNodeDescriptionId: 105)]
    [ProvideService(typeof(USharpLanguageService), ServiceName = "C# Language Service", IsAsyncQueryable = true)]
    [ProvideService(typeof(ICSharpTempPECompilerService), ServiceName = "C# TempPE Compiler Service", IsAsyncQueryable = true)]
    internal class USharpPackage : AbstractPackage<USharpPackage, USharpLanguageService>, IVsUserSettingsQuery
    {
        private ObjectBrowserLibraryManager _libraryManager;
        private uint _libraryManagerCookie;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            try
            {
                await base.InitializeAsync(cancellationToken, progress).ConfigureAwait(true);

                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

                this.RegisterService<ICSharpTempPECompilerService>(async ct =>
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(ct);
                    return new TempPECompilerService(this.Workspace.Services.GetService<IMetadataService>());
                });

                await RegisterObjectBrowserLibraryManagerAsync(cancellationToken).ConfigureAwait(true);
            }
            catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
            {
            }
        }

        protected override VisualStudioWorkspaceImpl CreateWorkspace()
        {
            return this.ComponentModel.GetService<VisualStudioWorkspaceImpl>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                JoinableTaskFactory.Run(() => UnregisterObjectBrowserLibraryManagerAsync(CancellationToken.None));
            }

            base.Dispose(disposing);
        }

        private async Task RegisterObjectBrowserLibraryManagerAsync(CancellationToken cancellationToken)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (await GetServiceAsync(typeof(SVsObjectManager)).ConfigureAwait(true) is IVsObjectManager2 objectManager)
            {
                _libraryManager = new ObjectBrowserLibraryManager(this, ComponentModel, Workspace);

                if (ErrorHandler.Failed(objectManager.RegisterSimpleLibrary(_libraryManager, out _libraryManagerCookie)))
                {
                    _libraryManagerCookie = 0;
                }
            }
        }

        private async Task UnregisterObjectBrowserLibraryManagerAsync(CancellationToken cancellationToken)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (_libraryManagerCookie != 0)
            {
                if (await GetServiceAsync(typeof(SVsObjectManager)).ConfigureAwait(true) is IVsObjectManager2 objectManager)
                {
                    objectManager.UnregisterLibrary(_libraryManagerCookie);
                    _libraryManagerCookie = 0;
                }

                _libraryManager.Dispose();
                _libraryManager = null;
            }
        }

        int IVsUserSettingsQuery.NeedExport(string pageID, out int needExport)
        {
            // We need to override MPF's definition of NeedExport since it doesn't know about our automation object
            needExport = (pageID == "TextEditor.CSharp-Specific") ? 1 : 0;

            return VSConstants.S_OK;
        }

        protected override object GetAutomationObject(string name)
        {
            if (name == "CSharp-Specific")
            {
                var workspace = this.ComponentModel.GetService<VisualStudioWorkspace>();
                return new Options.AutomationObject(workspace);
            }

            return base.GetAutomationObject(name);
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories()
        {
            var editorFactory = new CSharpEditorFactory(this.ComponentModel);
            var codePageEditorFactory = new CSharpCodePageEditorFactory(editorFactory);

            return new IVsEditorFactory[] { editorFactory, codePageEditorFactory };
        }

        protected override USharpLanguageService CreateLanguageService()
        {
            return new USharpLanguageService(this);
        }

        protected override void RegisterMiscellaneousFilesWorkspaceInformation(MiscellaneousFilesWorkspace miscellaneousFilesWorkspace)
        {
            miscellaneousFilesWorkspace.RegisterLanguage(
                Guids.USharpLanguageServiceId,
                LanguageNames.CSharp,
                ".csx");
        }

        protected override string RoslynLanguageName
        {
            get
            {
                return LanguageNames.CSharp;
            }
        }
    }
}
