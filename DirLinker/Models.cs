using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DirLinker
{
    internal static class Models
    {
        public static string GetFullPath(this DirectoryInfo it)
        {
            return Path.GetFullPath(it.ToString());
        }
    }

    public sealed class LinkMaker
    {
        public DirectoryInfo LinkName;
        public DirectoryInfo LinkTarget;

        public void MakeLink() {
            if (LinkName == null || LinkTarget == null)
                throw new InvalidOperationException("Insufficient parameters");

            string sourceDrive = Path.GetPathRoot(LinkName.GetFullPath());
            string targetDrive = Path.GetPathRoot(LinkTarget.GetFullPath());

            /*
            if (sourceDrive != targetDrive)
                throw new InvalidOperationException("same drive");
            */

            LinkTarget.CreateSymbolicLink(LinkTarget.GetFullPath()));
        }
    }
}
