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

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System;

public class CSVLoader : MonoBehaviour
{

    private string LoadCSVFile(string path)
    {
        string csvContent = string.Empty;

        try
        {
            if (File.Exists(path))
            {
                csvContent = File.ReadAllText(path);
            }
            else
            {
                Debug.LogError("Il file CSV non esiste: " + path);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError("Errore durante la lettura del file: " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.LogError("Accesso negato al file: " + ex.Message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore imprevisto: " + ex.Message);
        }

        return csvContent;
    }

}
