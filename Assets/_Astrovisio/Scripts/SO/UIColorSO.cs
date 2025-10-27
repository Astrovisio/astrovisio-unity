/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
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

using UnityEngine;

[CreateAssetMenu(fileName = "UIColorSO", menuName = "Scriptable Objects/UIColorSO")]
public class UIColorSO : ScriptableObject
{

    [Header("Colors")]
    public Color accent100 = new Color(1f, 0.4f, 0.11f, 1f);
    public Color accent200 = new Color(0.84f, 0.25f, 0f, 1f);
    public Color white = new Color(1f, 1f, 1f, 1f);
    public Color black = new Color(0f, 0f, 0f, 1f);
    public Color transparent = new Color(1f, 1f, 1f, 0f);
    public Color bg10 = new Color(1f, 1f, 1f, 0.1f);
    public Color success = new Color(0.05f, 0.32f, 0.13f, 1f);
    public Color error = new Color(1f, 0.2f, 0.2f, 1f);
    public ColorContainer lightColorContainer;
    public ColorContainer darkColorContainer;

}

public enum ColorMode {
    Light,
    Dark
}

[System.Serializable]
public class ColorContainer
{
    public Color grey100;
    public Color grey150;
    public Color grey200;
    public Color grey300;
    public Color grey400;
    public Color grey450;
    public Color grey500;
    public Color grey600;
    public Color grey700;
    public Color grey800;
}