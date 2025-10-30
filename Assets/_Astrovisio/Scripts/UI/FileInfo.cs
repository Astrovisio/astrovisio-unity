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
