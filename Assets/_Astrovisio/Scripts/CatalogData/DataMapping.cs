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
 * - Data Structure adapted to Astrovisio visualization necessities.
 *
 */

using System;
using UnityEngine;

namespace CatalogData
{
    [Serializable]
    public class DataMapping
    {
        public CoordinateSystem CoordinateSystem = CoordinateSystem.Astrophysics; // Z-up Y-forward di default
        public ColorMapEnum ColorMap = ColorMapEnum.Accent;
        public bool UniformColor;
        public bool UniformOpacity;
        public bool UseNoise;
        public bool isolateSelection = false;
        public MappingUniforms Uniforms = new MappingUniforms();

        public Mapping Mapping;

        public static DataMapping DefaultXyzMapping
        {
            get
            {
                DataMapping mapping = new DataMapping
                {
                    CoordinateSystem = CoordinateSystem.Astrophysics,
                    UniformColor = true,
                    UniformOpacity = true,
                    ColorMap = ColorMapEnum.Inferno,
                    UseNoise = false,
                    isolateSelection = false,

                    Uniforms = new MappingUniforms
                    {
                        Color = Color.white,
                        Opacity = 1.0f
                    },

                    Mapping = new Mapping
                    {
                        X = new MapFloatEntry { Source = "x" },
                        Y = new MapFloatEntry { Source = "y" },
                        Z = new MapFloatEntry { Source = "z" },
                    }
                };
                return mapping;
            }
        }
    }

    [Serializable]
    public class MappingUniforms
    {
        public Color Color;
        [Range(0.0f, 1.0f)] public float Opacity = 1.0f;
        public float NoiseStrength = 0.01f;
    }

    [Serializable]
    public class Mapping
    {
        public MapFloatEntry Cmap;
        public MapFloatEntry Opacity;
        public MapFloatEntry X;
        public MapFloatEntry Y;
        public MapFloatEntry Z;
    }

    [Serializable]
    public class MapFloatEntry
    {
        public bool Clamped;
        public float DataMinVal;
        public float DataMaxVal;
        public float TargetMinVal;
        public float TargetMaxVal;
        public bool InverseMapping;
        public ScalingType ScalingType = ScalingType.Linear;
        public string Source;
        [HideInInspector]
        public int SourceIndex;

        public GPUMappingConfig GpuMappingConfig => new GPUMappingConfig
        {
            Clamped = Clamped ? 1 : 0,
            DataMinVal = DataMinVal,
            DataMaxVal = DataMaxVal,
            TargetMinVal = InverseMapping ? TargetMaxVal : TargetMinVal,
            TargetMaxVal = InverseMapping ? TargetMinVal : TargetMaxVal,
            InverseMapping = InverseMapping ? 1 : 0,
            ScalingType = ScalingType.GetHashCode()
        };
    }

    // Struct used to store mapping config values on the GPU.
    // Each config struct has a size of 4 * 7 = 28 bytes
    // https://learn.microsoft.com/it-it/dotnet/csharp/language-reference/operators/sizeof
    public struct GPUMappingConfig
    {
        public int Clamped;
        public float DataMinVal;
        public float DataMaxVal;
        public int InverseMapping;
        public int ScalingType;
        public float TargetMinVal;
        public float TargetMaxVal;
    }

    [Serializable]
    public enum ScalingType
    {
        Linear,
        Log,
        Sqrt
    };

    [Serializable]
    public enum CoordinateSystem
    {
        Astrophysics,  // Z-up Y-forward  
        Unity        // Y-up Z-forward
    }

}