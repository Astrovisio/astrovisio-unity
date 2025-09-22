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
        public readonly string path;
        public readonly string name;
        public readonly long size;

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
        public bool state;

        public FileState(FileInfo fileInfo, File file)
        {
            this.fileInfo = fileInfo;
            this.file = file;
            state = false;
        }

        public string Path => fileInfo.path;
        public string Name => fileInfo.name;
        public long Size => fileInfo.size;

    }

}
