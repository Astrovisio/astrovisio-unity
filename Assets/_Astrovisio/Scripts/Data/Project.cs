/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{
    public class Project
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
                }
            }
        }

        // public Project(string name, string description, bool favourite = false)
        // {
        //     Name = name;
        //     Description = description;
        //     Favourite = favourite;
        // }

        public File GetFile(int fileId)
        {
            return Files.FirstOrDefault(f => f.Id == fileId);
        }

        public void UpdateFrom(Project other)
        {
            // If 'other' is null, there is nothing to merge/update.
            if (other == null)
            {
                return;
            }

            // Copy basic project metadata from 'other' (shallow copy).
            Name = other.Name;
            Favourite = other.Favourite;
            Description = other.Description;
            Id = other.Id;
            Created = other.Created;
            LastOpened = other.LastOpened;


            // If the source project has no files, it means the canonical state is "no files".
            // In that case, clear our local list (if any) and notify the UI/bindings, then return.
            if (other.Files == null)
            {
                if (Files != null && Files.Count > 0)
                {
                    Files.Clear();
                }
                return;
            }

            // Ensure our local list exists before proceeding with sync.
            if (Files == null)
            {
                Files = new List<File>();
            }

            // --- Removal pass ---
            // Iterate backwards so we can safely remove items by index.
            // For each local file, check if it still exists in 'other' by Id; if not, remove it.
            for (int i = Files.Count - 1; i >= 0; i--)
            {
                File current = Files[i];
                // Defensive check: remove null entries if any have crept in.
                if (current == null)
                {
                    Files.RemoveAt(i);
                    continue;
                }

                bool existsInOther = false;
                // Linear scan of 'other.Files' to find a matching Id.
                foreach (File of in other.Files)
                {
                    if (of != null && of.Id == current.Id)
                    {
                        existsInOther = true;
                        break;
                    }
                }
                // If not found in 'other', this file was removed on the source side -> remove locally.
                if (!existsInOther)
                {
                    Files.RemoveAt(i);
                }
            }

            // --- Upsert pass (update or insert) ---
            // For each file in 'other':
            //   - If we already have it (same Id), update the existing instance (preserve reference).
            //   - If we don't have it, create a new instance and copy values from 'other'.
            foreach (File otherFile in other.Files)
            {
                if (otherFile == null)
                {
                    continue;
                }

                bool found = false;
                // Look for a local file with the same Id.
                for (int i = 0; i < Files.Count; i++)
                {
                    File current = Files[i];
                    if (current != null && current.Id == otherFile.Id)
                    {
                        // Keep the same reference and just update its fields.
                        current.UpdateFrom(otherFile);
                        found = true;
                        break;
                    }
                }

                // If not found, this is a new file -> instantiate and copy data.
                if (!found)
                {
                    File newFile = new File();
                    newFile.UpdateFrom(otherFile);
                    Files.Add(newFile);
                }
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
