using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class Project : INotifyPropertyChanged
    {
        private string name;
        private bool favourite;
        private string description;
        private int id;
        private DateTime? created;
        private DateTime? lastOpened;
        private List<File> files;


        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        [JsonProperty("favourite")]
        public bool Favourite
        {
            get => favourite;
            set
            {
                if (favourite != value)
                {
                    favourite = value;
                    OnPropertyChanged(nameof(Favourite));
                }
            }
        }

        [JsonProperty("description")]
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
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

        [JsonProperty("created")]
        public DateTime? Created
        {
            get => created;
            set
            {
                if (created != value)
                {
                    created = value;
                    OnPropertyChanged(nameof(Created));
                }
            }
        }

        [JsonProperty("last_opened")]
        public DateTime? LastOpened
        {
            get => lastOpened;
            set
            {
                if (lastOpened != value)
                {
                    lastOpened = value;
                    OnPropertyChanged(nameof(LastOpened));
                }
            }
        }

        [JsonProperty("files")]
        public List<File> Files
        {
            get => files;
            set
            {
                if (files != value)
                {
                    files = value;
                    OnPropertyChanged(nameof(Files));
                }
            }
        }


        public Project(string name, string description, bool favourite = false)
        {
            Name = name;
            Description = description;
            Favourite = favourite;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrom(Project other)
        {
            if (other == null)
            {
                return;
            }

            Name = other.Name;
            Favourite = other.Favourite;
            Description = other.Description;
            Id = other.Id;
            Created = other.Created;
            LastOpened = other.LastOpened;

            if (other.Files == null)
            {
                Files = new List<File>();
            }
            else
            {
                Files = JsonConvert.DeserializeObject<List<File>>(
                            JsonConvert.SerializeObject(other.Files)
                        ) ?? new List<File>();
            }
        }

        public Project DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            Debug.Log($"DeepCopy of {json}");
            return JsonConvert.DeserializeObject<Project>(json);
        }

        public string Print()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

}
