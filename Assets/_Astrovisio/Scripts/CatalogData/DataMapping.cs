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
 * components can be found in the DISCLAIMER and NOTICE files included with this project.
 *
 */
using System;
using System.IO;
using UnityEngine;

namespace CatalogData
{
    [Serializable]
    public class DataMapping
    {
        public ColorMapEnum ColorMap = ColorMapEnum.Accent;
        public bool Spherical;
        public RenderType RenderType = RenderType.Billboard;
        public bool UniformColor;
        public bool UniformPointSize;
        public bool UniformPointShape;
        public bool UniformOpacity;
        public bool UseNoise;
        public bool isolateSelection = false;
        public MappingUniforms Uniforms = new MappingUniforms();

        public Mapping Mapping;
        public MetaMapping MetaMapping;

        // public static DataMapping CreateFromJson(string jsonString)
        // {
        //     DataMapping dataMapping = JsonConvert.DeserializeObject<DataMapping>(jsonString);
        //     if (!string.IsNullOrEmpty(dataMapping.Uniforms.ColorString))
        //     {
        //         Color parsedColor;
        //         if (ColorUtility.TryParseHtmlString(dataMapping.Uniforms.ColorString, out parsedColor))
        //         {
        //             dataMapping.Uniforms.Color = parsedColor;
        //         }
        //     }

        //     return dataMapping;
        // }

        // public static DataMapping CreateFromFile(string fileName)
        // {
        //     string mappingJson = File.ReadAllText(fileName);
        //     return CreateFromJson(mappingJson);
        // }

        // public string ToJson()
        // {
        //     return JsonConvert.SerializeObject(this);
        // }

        public static DataMapping DefaultXyzMapping
        {
            get
            {
                DataMapping mapping = new DataMapping
                {
                    Spherical = false,
                    RenderType = RenderType.Billboard,
                    ColorMap = ColorMapEnum.Inferno,
                    UniformColor = false,
                    UniformPointSize = true,
                    UniformPointShape = true,
                    UniformOpacity = true,
                    UseNoise = false,
                    isolateSelection = false,
                    Uniforms = new MappingUniforms
                    {
                        Scale = 0.001f,
                        PointSize = 0.3f,
                        PointShape = ShapeType.Circle,
                        Color = Color.white
                    },
                    Mapping = new Mapping
                    {
                        X = new MapFloatEntry {Source = "x"},
                        Y = new MapFloatEntry {Source = "y"},
                        Z = new MapFloatEntry {Source = "z"},
                    }
                };
                return mapping;
            }
        }

        public static DataMapping DefaultSphericalMapping
        {
            get
            {
                DataMapping mapping = new DataMapping
                {
                    Spherical = true,
                    RenderType = RenderType.Billboard,
                    UniformColor = true,
                    UniformPointSize = true,
                    UniformPointShape = true,
                    UniformOpacity = true,
                    UseNoise = false,
                    Uniforms = new MappingUniforms
                    {
                        Scale = 0.001f,
                        PointSize = 0.3f,
                        PointShape = ShapeType.Circle,
                        Color = Color.white
                    },
                    Mapping = new Mapping
                    {
                        Lat = new MapFloatEntry {Source = "glon"},
                        Lng = new MapFloatEntry {Source = "glat"},
                        R = new MapFloatEntry {Source = "Dm"}
                    }
                };
                return mapping;
            }
        }
    }

    [Serializable]
    public class MappingUniforms
    {
        [HideInInspector] public string ColorString;
        public Color Color;
        [HideInInspector] public float Scale = 1;
        public float PointSize = 0.1f;
        public ShapeType PointShape = ShapeType.OutlinedCircle;
        [Range(0.0f, 1.0f)] public float Opacity = 1.0f;
        public float NoiseStrength = 0.01f;
    }

    [Serializable]
    public class Mapping
    {
        public MapFloatEntry Cmap;
        public MapFloatEntry Lat;
        public MapFloatEntry Lng;
        public MapFloatEntry Opacity;
        public MapFloatEntry R;
        public MapFloatEntry PointSize;
        public MapFloatEntry PointShape;
        public MapFloatEntry X;
        public MapFloatEntry Y;
        public MapFloatEntry Z;
        public MapFloatEntry X2;
        public MapFloatEntry Y2;
        public MapFloatEntry Z2;
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
        public float Offset;
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
            Offset = Offset,
            ScalingType = ScalingType.GetHashCode()
        };
    }

    // Struct used to store mapping config values on the GPU.
    // Using 32-bit ints for clamped and scaling type instead of 8-bit bools for packing purposes
    // (For performance reasons, struct should be an integer multiple of 128 bits)
    // Each config struct has a size of 4 * 8 = 32 bytes
    public struct GPUMappingConfig
    {
        public int Clamped;
        public float DataMinVal;
        public float DataMaxVal;
        public int InverseMapping;
        public float Offset;

        public int ScalingType;

        // Future use: Will filter points based on this range
        public float TargetMinVal;
        public float TargetMaxVal;
    }

    [Serializable]
    public class MetaMapping
    {
        public MapMetaEntry Name;
    }

    [Serializable]
    public class MapMetaEntry
    {
        public string Source;
    }

    [Serializable]
    public enum RenderType
    {
        Billboard,
        Line
    };

    [Serializable]
    public enum ScalingType
    {
        Linear,
        Log,
        Sqrt
    };

    [Serializable]
    public enum ShapeType
    {
        Halo,
        Circle,
        OutlinedCircle,
        Square,
        OutlinedSquare,
        Triangle,
        OutlinedTriangle,
        Star
    };
}