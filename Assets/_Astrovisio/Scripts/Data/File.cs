using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Astrovisio
{
    public class File : INotifyPropertyChanged
    {
        private string fileType;
        private string filePath;
        private bool processed;
        private float downsampling = 1f;
        private string processedPath;
        private int id;
        private List<Variables> variables = new();

        [JsonProperty("file_type")]
        public string FileType
        {
            get => fileType;
            set
            {
                if (fileType != value)
                {
                    fileType = value;
                    OnPropertyChanged(nameof(FileType));
                }
            }
        }

        [JsonProperty("file_path")]
        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        [JsonProperty("processed")]
        public bool Processed
        {
            get => processed;
            set
            {
                if (processed != value)
                {
                    processed = value;
                    OnPropertyChanged(nameof(Processed));
                }
            }
        }

        [JsonProperty("downsampling")]
        public float Downsampling
        {
            get => downsampling;
            set
            {
                if (downsampling != value)
                {
                    downsampling = value;
                    OnPropertyChanged(nameof(Downsampling));
                }
            }
        }

        [JsonProperty("processed_path")]
        public string ProcessedPath
        {
            get => processedPath;
            set
            {
                if (processedPath != value)
                {
                    processedPath = value;
                    OnPropertyChanged(nameof(ProcessedPath));
                }
            }
        }

        [JsonProperty("id")]
        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        [JsonProperty("variables")]
        public List<Variables> Variables
        {
            get => variables;
            set
            {
                if (variables != value)
                {
                    variables = value ?? new List<Variables>();
                    OnPropertyChanged(nameof(Variables));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void UpdateFrom(File other)
        {
            if (other == null)
            {
                return;
            }

            FileType = other.FileType;
            FilePath = other.FilePath;
            Processed = other.Processed;
            Downsampling = other.Downsampling;
            ProcessedPath = other.ProcessedPath;
            Id = other.Id;

            if (other.Variables == null)
            {
                Variables = new List<Variables>();
            }
            else
            {
                List<Variables> copy = JsonConvert.DeserializeObject<List<Variables>>(JsonConvert.SerializeObject(other.Variables));
                Variables = copy ?? new List<Variables>();
            }
        }

        public File DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<File>(json);
        }

    }

}
