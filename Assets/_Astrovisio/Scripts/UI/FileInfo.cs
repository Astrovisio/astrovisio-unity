
namespace Astrovisio
{
    public struct FileInfo
    {
        public string path;
        public string name;
        public long size;

        public FileInfo(string path, string name, long size)
        {
            this.path = path;
            this.name = name;
            this.size = size;
        }

    }

    public struct FileState
    {
        public FileInfo fileInfo;
        public bool state;

        public FileState(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            state = false;
        }

    }

}
