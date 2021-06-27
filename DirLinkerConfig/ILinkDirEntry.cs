using System.IO;

namespace DirLinkerConfig
{
    public interface ILinkDirEntry
    {
        bool IsDemo { get; }
        string LinkDirName { get; set; }
        ILinkBlobEntry Add(Configuration.LinkBlob blob);
        void Remove(ILinkBlobEntry entry);
        ILinkBlobEntry GetOrCreateLink(string linkName, DirectoryInfo targetDir);
    }
}