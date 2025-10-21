namespace Astrovisio
{

    public interface IFileEntry
    {
        string Path { get; }
        string Name { get; }
        long Size { get; }
    }

    public readonly struct FileInfo : IFileEntry
    {
        private readonly string path;
        private readonly string name;
        private readonly long size;

        public FileInfo(string path, string name, long size)
        {
            this.path = path;
            this.name = name;
            this.size = size;
        }

        public string Path => path;
        public string Name => name;
        public long Size => size;
        
    }

    public struct FileState : IFileEntry
    {
        public FileInfo fileInfo;
        public File file;
        public bool processed;

        public FileState(FileInfo fileInfo, File file, bool processed)
        {
            this.fileInfo = fileInfo;
            this.file = file;
            this.processed = processed;
        }

        public string Path => fileInfo.Path;
        public string Name => fileInfo.Name;
        public long Size => fileInfo.Size;

    }

}
