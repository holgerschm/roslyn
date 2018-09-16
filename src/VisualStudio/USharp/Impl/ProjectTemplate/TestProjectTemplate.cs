using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.LanguageServices.USharp.ProjectTemplate
{
    public class TestProjectTemplate : IVsProjectFactory
    {
        public int CanCreateProject(string pszFilename, uint grfCreateFlags, out int pfCanCreate)
        {
            throw new NotImplementedException();
        }

        public int CreateProject(string pszFilename, string pszLocation, string pszName, uint grfCreateFlags, ref Guid iidProject, out IntPtr ppvProject, out int pfCanceled)
        {
            throw new NotImplementedException();
        }

        public int SetSite(OLE.Interop.IServiceProvider psp)
        {
            throw new NotImplementedException();
        }

        public int Close()
        {
            throw new NotImplementedException();
        }
    }
}
