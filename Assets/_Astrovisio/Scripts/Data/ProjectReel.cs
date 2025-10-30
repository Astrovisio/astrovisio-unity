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

namespace Astrovisio
{
    public readonly struct Reel
    {
        public readonly int FileId;
        public readonly DataContainer Data;

        public Reel(int fileId, DataContainer data)
        {
            FileId = fileId;
            Data = data;
        }
    }

    public class ProjectReel
    {
        private readonly List<Reel> reelList = new();

        public ProjectReel()
        {
        }

        public void AddOrUpdate(int fileId, DataContainer dataContainer)
        {
            int idx = IndexOf(fileId);
            if (idx >= 0)
            {
                reelList[idx] = new Reel(fileId, dataContainer);
            }
            else
            {
                reelList.Add(new Reel(fileId, dataContainer));
            }
        }

        public bool TryGet(int fileId, out DataContainer dataContainer)
        {
            int idx = IndexOf(fileId);
            if (idx >= 0)
            {
                dataContainer = reelList[idx].Data;
                return true;
            }

            dataContainer = null;
            return false;
        }

        public bool Remove(int fileId)
        {
            int idx = IndexOf(fileId);
            if (idx < 0)
            {
                return false;
            }

            reelList.RemoveAt(idx);
            return true;
        }

        public void Clear()
        {
            reelList.Clear();
        }

        public Reel GetAt(int index)
        {
            if (index < 0 || index >= reelList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return reelList[index];
        }

        public bool Move(int fileId, int newIndex)
        {
            int oldIndex = IndexOf(fileId);
            if (oldIndex < 0)
            {
                return false;
            }

            newIndex = Math.Clamp(newIndex, 0, reelList.Count - 1);
            if (oldIndex == newIndex)
            {
                return true;
            }

            Reel e = reelList[oldIndex];
            reelList.RemoveAt(oldIndex);
            reelList.Insert(newIndex, e);
            return true;
        }

        public void SetOrder(IReadOnlyList<int> orderedIds)
        {
            if (orderedIds == null || orderedIds.Count == 0)
            {
                return;
            }

            // ricostruisco in base all’ordine richiesto (ignorando id sconosciuti)
            var map = new Dictionary<int, DataContainer>(reelList.Count);
            foreach (var e in reelList)
            {
                map[e.FileId] = e.Data;
            }

            var newList = new List<Reel>(reelList.Count);
            var seen = new HashSet<int>();

            foreach (int id in orderedIds)
            {
                if (map.TryGetValue(id, out var dc) && seen.Add(id))
                {
                    newList.Add(new Reel(id, dc));
                }
            }

            // aggiungo in coda eventuali rimanenti
            foreach (var e in reelList)
            {
                if (seen.Add(e.FileId))
                {
                    newList.Add(e);
                }
            }

            reelList.Clear();
            reelList.AddRange(newList);
        }

        public void SortBy<TKey>(Func<DataContainer, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            if (keySelector == null)
            {
                return;
            }

            reelList.Sort((a, b) =>
            {
                var ca = keySelector(a.Data);
                var cb = keySelector(b.Data);
                return (comparer ?? Comparer<TKey>.Default).Compare(ca, cb);
            });
        }

        public IEnumerable<Reel> Enumerate()
        {
            return reelList;
        }

        public IReadOnlyList<int> OrderedIds
        {
            get
            {
                var ids = new int[reelList.Count];
                for (int i = 0; i < reelList.Count; i++)
                {
                    ids[i] = reelList[i].FileId;
                }

                return ids;
            }
        }

        private int IndexOf(int fileId)
        {
            for (int i = 0; i < reelList.Count; i++)
            {
                if (reelList[i].FileId == fileId)
                {
                    return i;
                }
            }

            return -1;
        }

    }
    
}
