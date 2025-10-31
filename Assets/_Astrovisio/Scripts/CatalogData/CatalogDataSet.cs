/*
 * iDaVIE (immersive Data Visualisation Interactive Explorer)
 * Copyright (C) 2024 IDIA, INAF-OACT
 *
 * This file is part of the iDaVIE project.
 *
 * iDaVIE is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * iDaVIE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * iDaVIE in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 * Additional information and disclaimers regarding liability and third-party 
 * components can be found in the DISCLAIMER and IDAVIE_NOTICE files included with this project.
 *
 * Modifications:
 * - Removed function to read the input file, the reading is now managed with a backend service
 * - Modified Constructor. The Data is now provided through the constructor.
 */
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CatalogData
{
    [Serializable]
    public class CatalogDataSet
    {

        public ColumnInfo[] ColumnDefinitions { get; private set; }
        public string FileName { get; private set; }
        public string[][] MetaColumns { get; private set; }
        public float[][] DataColumns { get; private set; }
        public int N { get; private set; }

        public CatalogDataSet(ColumnInfo[] info, float[][] data)
        {
            ColumnDefinitions = info;
            FileName = "";
            MetaColumns = new string[0][];
            DataColumns = data;
            N = data[0].Length;
        }

        public int GetDataColumnIndex(string name)
        {
            foreach (var column in ColumnDefinitions)
            {
                if (column.Type == ColumnType.Numeric && column.Name == name)
                {
                    return column.NumericIndex;
                }
            }

            return -1;
        }

    }
}
