namespace DirLinkerConfig
{
    public interface ILinkBlobEntry
    {
        string LinkName { get; set; }
        string TargetName { get; set; }
    }
}