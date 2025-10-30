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
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DataInspector : MonoBehaviour
{

    [Header("Fresenel")]
    [SerializeField] private Color fresnelColor;
    [SerializeField][Range(0.1f, 2f)] private float fresnelPower = 1f;

    public bool IsActive { private set; get; }

    private MeshRenderer meshRenderer;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        SetFresnelColor(fresnelColor);
        SetFresnelPower(fresnelPower);
    }

    public void SetActiveState(bool state)
    {
        meshRenderer.enabled = state;
        IsActive = state;
    }

    public void SetFresnelColor(Color newColor)
    {
        meshRenderer.material.SetColor("_FresnelColor", newColor);
    }

    public void SetFresnelPower(float newPower)
    {
        meshRenderer.material.SetFloat("_FresnelPower", newPower);
    }

    public void SetScale(float newScale)
    {
        transform.localScale = Vector3.one * newScale;
    }

}
