/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * This file contains code derived from the iDaVIE project
 * (immersive Data Visualisation Interactive Explorer)
 * Original Copyright (C) 2024 IDIA, INAF-OACT
 * Original file: "DataSetRenderer.cs"
 *
 * This file is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This file is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 * Substantial modifications from original iDaVIE code include:
 * - General refactor
 * - Removed all unnecessary data structures for Astrovisio Visualization
 * - Reworked data initialization
 */

using System;
using Astrovisio;
using UnityEngine;

namespace CatalogData
{
    public class AstrovisioDataSetRenderer : MonoBehaviour
    {
        public DataMapping DataMapping = DataMapping.DefaultXyzMapping;
        public Texture2D ColorMapTexture;

        private ComputeBuffer[] _buffers;

        // The mapping buffer is used to store mapping configuration. Since each mapping has a similar set of options,
        // it's less verbose than storing a huge number of options individually
        private ComputeBuffer _mappingConfigBuffer;
        private ComputeBuffer _dataVisibleBuffer;
        private readonly GPUMappingConfig[] _mappingConfigs = new GPUMappingConfig[5];

        private CatalogDataSet _dataSet;
        private Material _catalogMaterial;
        private ColorMapEnum _appliedColorMap = ColorMapEnum.None;

        private bool _visible = true;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private Vector3 _initialLocalScale;
        private float _initialOpacity;

        #region Material Property IDs

        private int _idColorMap, _idColorMapIndex, _idNumColorMaps;
        private int _idDataX, _idDataY, _idDataZ, _idDataCmap, _idDataOpacity, _idDataVisible;
        private int _idUseUniformColor, _idUseUniformOpacity, _idUseNoise, _idIsolateSelection;
        private int _idColor, _idOpacity, _idNoiseStrength;
        private int _idMappingConfigs;

        private KDTreeComponent kdTreeComponent;
        private DataContainer dataContainer;

        private void Awake()
        {
            kdTreeComponent = GetComponent<KDTreeComponent>();
        }

        private void Start()
        {
            if (XRManager.Instance.IsVRActive)
            {
                TransformManipulator transformManipulator = FindAnyObjectByType<TransformManipulator>();
                transformManipulator.targetObject = kdTreeComponent.gameObject.transform;
            }
        }

        private void GetPropertyIds()
        {
            _idColorMap = Shader.PropertyToID("colorMap");
            _idColorMapIndex = Shader.PropertyToID("colorMapIndex");
            _idNumColorMaps = Shader.PropertyToID("numColorMaps");

            _idDataX = Shader.PropertyToID("dataX");
            _idDataY = Shader.PropertyToID("dataY");
            _idDataZ = Shader.PropertyToID("dataZ");

            _idDataCmap = Shader.PropertyToID("dataCmap");
            _idDataOpacity = Shader.PropertyToID("dataOpacity");

            _idDataVisible = Shader.PropertyToID("dataVisible");

            _idUseUniformColor = Shader.PropertyToID("useUniformColor");
            _idUseUniformOpacity = Shader.PropertyToID("useUniformOpacity");
            _idUseNoise = Shader.PropertyToID("useNoise");
            _idIsolateSelection = Shader.PropertyToID("isolateSelection");

            _idColor = Shader.PropertyToID("color");
            _idOpacity = Shader.PropertyToID("opacity");
            _idNoiseStrength = Shader.PropertyToID("noiseStrength");

            _idMappingConfigs = Shader.PropertyToID("mappingConfigs");

        }

        #endregion

        public void SetCatalogData(DataContainer dataContainer, bool debug = false)
        {
            ReleaseAllGpuResources();

            this.dataContainer = dataContainer;
            string[] headers = dataContainer.DataPack.Columns;
            float[][] data = dataContainer.TransposedData;

            // Dataset
            ColumnInfo[] columnInfo = new ColumnInfo[headers.Length];
            for (int i = 0; i < columnInfo.Length; i++)
            {
                columnInfo[i] = new ColumnInfo()
                {
                    Name = headers[i],
                    Type = ColumnType.Numeric,
                    NumericIndex = i
                };
            }
            _dataSet = new CatalogDataSet(columnInfo, data);

            if (!debug)
            {
                DataMapping.Mapping = new Mapping
                {
                    X = new MapFloatEntry
                    {
                        Source = dataContainer.XAxisName,
                        SourceIndex = dataContainer.XAxisIndex,
                        DataMinVal = dataContainer.XMinThreshold,
                        DataMaxVal = dataContainer.XMaxThreshold,
                        TargetMinVal = -0.5f,
                        TargetMaxVal = 0.5f
                    },
                    Y = new MapFloatEntry
                    {
                        Source = dataContainer.YAxisName,
                        SourceIndex = dataContainer.YAxisIndex,
                        DataMinVal = dataContainer.YMinThreshold,
                        DataMaxVal = dataContainer.YMaxThreshold,
                        TargetMinVal = -0.5f,
                        TargetMaxVal = 0.5f
                    },
                    Z = new MapFloatEntry
                    {
                        Source = dataContainer.ZAxisName,
                        SourceIndex = dataContainer.ZAxisIndex,
                        DataMinVal = dataContainer.ZMinThreshold,
                        DataMaxVal = dataContainer.ZMaxThreshold,
                        TargetMinVal = -0.5f,
                        TargetMaxVal = 0.5f
                    }
                };
            }
            else
            {
                DataMapping.Mapping.X.SourceIndex = Array.IndexOf(headers, DataMapping.Mapping.X.Source);
                DataMapping.Mapping.Y.SourceIndex = Array.IndexOf(headers, DataMapping.Mapping.Y.Source);
                DataMapping.Mapping.Z.SourceIndex = Array.IndexOf(headers, DataMapping.Mapping.Z.Source);
            }

            // SWAP Y Z If CoordinateSystem.Astrophysics
            if (DataMapping.CoordinateSystem == CoordinateSystem.Astrophysics)
            {
                var temp = DataMapping.Mapping.Y;
                DataMapping.Mapping.Y = DataMapping.Mapping.Z;
                DataMapping.Mapping.Z = temp;
            }

            if (_dataSet.DataColumns.Length == 0 || _dataSet.DataColumns[0].Length == 0)
            {
                Debug.LogWarning($"Problem loading data");
            }
            else
            {
                int numDataColumns = _dataSet.DataColumns.Length;
                _buffers = new ComputeBuffer[numDataColumns];

                for (var i = 0; i < numDataColumns; i++)
                {
                    _buffers[i] = new ComputeBuffer(_dataSet.N, sizeof(float));
                    _buffers[i].SetData(_dataSet.DataColumns[i]);
                }

                // Load instance of the material, so that each data set can have different material parameters
                GetPropertyIds();
                _catalogMaterial = new Material(Shader.Find("Astrovisio/PointShader"));

                _catalogMaterial.SetTexture(_idColorMap, ColorMapTexture);
                // Buffer holds XYZ, cmap, and opacity mapping configs (float = 4, int = 4, MappingConfig have 7 int/float params. 4*7=28)              
                _mappingConfigBuffer = new ComputeBuffer(5, 28);
                _catalogMaterial.SetBuffer(_idMappingConfigs, _mappingConfigBuffer);

                // Apply scaling from data set space to world space
                // transform.localScale *= DataMapping.Uniforms.Scale;
                // Debug.Log($"Scaling from data set space to world space: {ScalingString}");

                UpdateMappingColumns();
                UpdateMappingValues();
            }

            if (!DataMapping.UniformColor)
            {
                SetColorMap(DataMapping.ColorMap);
            }
            _initialLocalPosition = transform.localPosition;
            _initialLocalRotation = transform.localRotation;
            _initialLocalScale = transform.localScale;
            _initialOpacity = DataMapping.Uniforms.Opacity;

            // Init KDTree
            Vector3 dataCenter = DataMapping.CoordinateSystem == CoordinateSystem.Astrophysics ? new Vector3(dataContainer.Center.x, dataContainer.Center.z, dataContainer.Center.y) : dataContainer.Center;
            _ = kdTreeComponent.Initialize(data, dataCenter);

        }


        public void SetAxisAstrovisio(Astrovisio.Axis axis, string paramName, float thresholdMin, float thresholdMax, ScalingType scalingType)
        {
            var targetAxis = TransformAxis(axis);

            var entry = new MapFloatEntry
            {
                Source = paramName,
                SourceIndex = Array.IndexOf(dataContainer.DataPack.Columns, paramName),
                Clamped = true,
                DataMinVal = thresholdMin,
                DataMaxVal = thresholdMax,
                TargetMinVal = -0.5f,
                TargetMaxVal = 0.5f,
                ScalingType = scalingType,
                InverseMapping = false
            };

            switch (targetAxis)
            {
                case Astrovisio.Axis.X:
                    DataMapping.Mapping.X = entry;
                    break;
                case Astrovisio.Axis.Y:
                    DataMapping.Mapping.Y = entry;
                    break;
                case Astrovisio.Axis.Z:
                    DataMapping.Mapping.Z = entry;
                    break;
            }

            UpdateMappingColumns();
            UpdateMappingValues();
        }

        private Axis TransformAxis(Axis inputAxis)
        {
            if (DataMapping.CoordinateSystem == CoordinateSystem.Astrophysics)
            {
                switch (inputAxis)
                {
                    case Axis.X: return Axis.X; // X rimane X
                    case Axis.Y: return Axis.Z; // Y astro → Z Unity
                    case Axis.Z: return Axis.Y; // Z astro → Y Unity
                    default: return inputAxis;
                }
            }
            return inputAxis;
        }

        public Material GetMaterial()
        {
            return _catalogMaterial;
        }

        public void UpdateDataVisibility(int[] visibilityArray)
        {
            if (_dataSet == null || _catalogMaterial == null || visibilityArray == null)
            {
                return;
            }

            // Release the old one and recreate
            if (_dataVisibleBuffer != null)
            {
                _dataVisibleBuffer.Release();
                _dataVisibleBuffer = null;
            }

            int dataCount = _dataSet.DataColumns[0].Length;
            _dataVisibleBuffer = new ComputeBuffer(dataCount, sizeof(int));
            _dataVisibleBuffer.SetData(visibilityArray);
            _catalogMaterial.SetBuffer(_idDataVisible, _dataVisibleBuffer);
        }


        public float[] GetDataInfo()
        {
            if (kdTreeComponent == null) return null;

            // Check if we're in area selection mode
            if (kdTreeComponent.selectionMode != SelectionMode.SinglePoint)
            {
                var areaResult = kdTreeComponent.GetLastAreaSelection();
                if (areaResult != null && areaResult.Count > 0)
                {
                    return areaResult.AggregatedValues;
                }
            }
            else
            {
                // Original single point selection
                if (kdTreeComponent.GetLastNearest() != null)
                {
                    int index = kdTreeComponent.GetLastNearest().Value.index;
                    if (index >= 0)
                    {
                        return kdTreeComponent.GetDataInfo(index);
                    }
                }
            }

            return null;
        }

        public void SetNoneAstrovisio()
        {
            DataMapping.UniformColor = true;
            DataMapping.UniformOpacity = true;
            UpdateMappingColumns();
            UpdateMappingValues();
        }

        public void SetColorMapAstrovisio(string paramName, ColorMapEnum colorMap, float min, float max, ScalingType scalingType, bool inverseMapping)
        {
            SetColorMap(colorMap);
            DataMapping.UniformColor = false;
            DataMapping.Mapping.Cmap = new MapFloatEntry
            {
                Source = paramName,
                Clamped = true,
                DataMinVal = min,
                DataMaxVal = max,
                TargetMinVal = 0f,
                TargetMaxVal = 1f,
                ScalingType = scalingType,
                InverseMapping = inverseMapping
            };
            UpdateMappingColumns();
            UpdateMappingValues();
        }

        public void RemoveColorMapAstrovisio()
        {
            DataMapping.UniformColor = true;
            SetColorMap(ColorMapEnum.None);
            UpdateMappingColumns();
            UpdateMappingValues();
        }

        public void SetOpacityAstrovisio(string paramName, float min, float max, ScalingType scalingType, bool inverseMapping)
        {
            DataMapping.UniformOpacity = false;
            DataMapping.Mapping.Opacity = new MapFloatEntry
            {
                Source = paramName,
                Clamped = true,
                DataMinVal = min,
                DataMaxVal = max,
                TargetMinVal = 0f,
                TargetMaxVal = 1f,
                ScalingType = scalingType,
                InverseMapping = inverseMapping
            };
            UpdateMappingColumns();
            UpdateMappingValues();
        }

        public void RemoveOpacityAstrovisio()
        {
            DataMapping.UniformOpacity = true;
            SetOpacity(1f);
            UpdateMappingColumns();
            UpdateMappingValues();
        }

        [ContextMenu("UpdateMappingColumns")]
        public bool UpdateMappingColumns()
        {
            bool logErrors = false;
            // Set the color map buffer if we're not using a uniform color
            if (!DataMapping.UniformColor)
            {
                if (DataMapping.Mapping.Cmap != null)
                {
                    int cmapColumnIndex = _dataSet.GetDataColumnIndex(DataMapping.Mapping.Cmap.Source);
                    if (cmapColumnIndex >= 0)
                    {
                        _catalogMaterial.SetBuffer(_idDataCmap, _buffers[cmapColumnIndex]);
                    }
                    else
                    {
                        if (logErrors)
                        {
                            Debug.Log($"Can't find column {DataMapping.Mapping.Cmap.Source} (mapped to Cmap)");
                        }

                        return false;
                    }
                }
                else
                {
                    if (logErrors)
                    {
                        Debug.Log("No mapping for Cmap");
                    }

                    return false;
                }
            }

            // Set the opacity buffer if we're not using a uniform opacity
            if (!DataMapping.UniformOpacity)
            {
                if (DataMapping.Mapping.Opacity != null)
                {
                    int opacityColumnIndex = _dataSet.GetDataColumnIndex(DataMapping.Mapping.Opacity.Source);
                    if (opacityColumnIndex >= 0)
                    {
                        _catalogMaterial.SetBuffer(_idDataOpacity, _buffers[opacityColumnIndex]);
                    }
                    else
                    {
                        if (logErrors)
                        {
                            Debug.Log($"Can't find column {DataMapping.Mapping.Opacity.Source} (mapped to Opacity)");
                        }

                        return false;
                    }
                }
                else
                {
                    if (logErrors)
                    {
                        Debug.Log("No mapping for Opacity");
                    }

                    return false;
                }
            }

            // Otherwise default to Cartesian coordinates and mapping buffers
            if (DataMapping.Mapping.X != null && DataMapping.Mapping.Y != null && DataMapping.Mapping.Z != null)
            {
                int xColumnIndex = _dataSet.GetDataColumnIndex(DataMapping.Mapping.X.Source);
                int yColumnIndex = _dataSet.GetDataColumnIndex(DataMapping.Mapping.Y.Source);
                int zColumnIndex = _dataSet.GetDataColumnIndex(DataMapping.Mapping.Z.Source);
                if (xColumnIndex >= 0 && yColumnIndex >= 0 && zColumnIndex >= 0)
                {
                    // Spatial mapping and scaling
                    _catalogMaterial.SetBuffer(_idDataX, _buffers[xColumnIndex]);
                    _catalogMaterial.SetBuffer(_idDataY, _buffers[yColumnIndex]);
                    _catalogMaterial.SetBuffer(_idDataZ, _buffers[zColumnIndex]);
                }
                else
                {
                    if (logErrors)
                    {
                        Debug.Log($"Can't find columns {DataMapping.Mapping.X.Source}, {DataMapping.Mapping.Y.Source} and {DataMapping.Mapping.Z.Source} (mapped to X, Y and Z)");
                    }

                    return false;
                }

                return true;
            }

            if (logErrors)
            {
                Debug.Log("Can't find mappings for X, Y and Z");
            }

            return false;
        }

        public float GetInitialOpacity()
        {
            return _initialOpacity;
        }

        public void SetOpacity(float opacity)
        {
            DataMapping.Uniforms.Opacity = opacity;
        }

        public void SetVisibility(bool visible)
        {
            _visible = visible;
        }

        public void ResetLocalPosition()
        {
            transform.localPosition = _initialLocalPosition;
            transform.localRotation = _initialLocalRotation;
            transform.localScale = _initialLocalScale;
        }

        [ContextMenu("UpdateMappingValues")]
        public bool UpdateMappingValues()
        {
            if (!_catalogMaterial)
            {
                return false;
            }

            if (!DataMapping.UniformColor && DataMapping.Mapping.Cmap != null && !string.IsNullOrEmpty(DataMapping.Mapping.Cmap.Source))
            {
                _catalogMaterial.SetInt(_idUseUniformColor, 0);
                _mappingConfigs[3] = DataMapping.Mapping.Cmap.GpuMappingConfig;
                if (_appliedColorMap != DataMapping.ColorMap)
                {
                    _appliedColorMap = DataMapping.ColorMap;
                    int colorMapIndex = _appliedColorMap.GetHashCode();
                    _catalogMaterial.SetFloat(_idColorMapIndex, colorMapIndex);
                    _catalogMaterial.SetInt(_idNumColorMaps, ColorMapUtils.NumColorMaps);
                }
            }
            else
            {
                _catalogMaterial.SetInt(_idUseUniformColor, 1);
                _catalogMaterial.SetColor(_idColor, DataMapping.Uniforms.Color);
            }

            if (!DataMapping.UniformOpacity && DataMapping.Mapping.Opacity != null && !string.IsNullOrEmpty(DataMapping.Mapping.Opacity.Source))
            {
                _catalogMaterial.SetInt(_idUseUniformOpacity, 0);
                if (_visible)
                    _mappingConfigs[4] = DataMapping.Mapping.Opacity.GpuMappingConfig;
                else
                    _catalogMaterial.SetFloat(_idOpacity, 0);
            }
            else
            {
                _catalogMaterial.SetInt(_idUseUniformOpacity, 1);
                if (_visible)
                    _catalogMaterial.SetFloat(_idOpacity, DataMapping.Uniforms.Opacity);
                else
                    _catalogMaterial.SetFloat(_idOpacity, 0);
            }

            _catalogMaterial.SetInt(_idUseNoise, DataMapping.UseNoise ? 1 : 0);
            _catalogMaterial.SetInt(_idIsolateSelection, DataMapping.isolateSelection ? 1 : 0);
            _catalogMaterial.SetFloat(_idNoiseStrength, DataMapping.Uniforms.NoiseStrength);

            // Otherwise default to Cartesian coordinates and update properties
            if (DataMapping.Mapping.X != null && DataMapping.Mapping.Y != null && DataMapping.Mapping.Z != null)
            {
                // Spatial mapping and scaling
                _mappingConfigs[0] = DataMapping.Mapping.X.GpuMappingConfig;
                _mappingConfigs[1] = DataMapping.Mapping.Y.GpuMappingConfig;
                _mappingConfigs[2] = DataMapping.Mapping.Z.GpuMappingConfig;
                _mappingConfigBuffer.SetData(_mappingConfigs);
                return true;
            }

            return false;
        }

        // The color map array is calculated from the color map texture and sent to the GPU whenever the color map is changed
        public void SetColorMap(ColorMapEnum newColorMap)
        {
            DataMapping.ColorMap = newColorMap;
        }

        public void ShiftColorMap(int delta)
        {
            int numColorMaps = ColorMapUtils.NumColorMaps;
            int currentIndex = DataMapping.ColorMap.GetHashCode();
            int newIndex = (currentIndex + delta + numColorMaps) % numColorMaps;
            SetColorMap(ColorMapUtils.FromHashCode(newIndex));
        }

        private void Update()
        {
            UpdateMappingValues();
            GetDataInfo();
        }

        public void SetNoise(bool state, float value)
        {
            DataMapping.UseNoise = state;
            DataMapping.Uniforms.NoiseStrength = value;
        }

        public float GetNoiseValue()
        {
            return DataMapping.Uniforms.NoiseStrength;
        }

        public bool GetNoiseState()
        {
            return DataMapping.UseNoise;
        }

        // Add this method to get detailed info about area selection
        public SelectionResult GetAreaSelectionInfo()
        {
            if (kdTreeComponent != null && kdTreeComponent.selectionMode != SelectionMode.SinglePoint)
            {
                return kdTreeComponent.GetLastAreaSelection();
            }
            return null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5F);
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }

        void OnRenderObject()
        {
            if (_catalogMaterial is not null)
            {
                GL.PushMatrix();
                // Update the object transform and point scale on the GPU
                GL.MultMatrix(transform.localToWorldMatrix);
                //_catalogMaterial.SetMatrix(_idDataSetMatrix, transform.localToWorldMatrix);
                // Shader defines two passes: Pass #0 uses cartesian coordinates and Pass #1 uses spherical coordinates
                _catalogMaterial.SetPass(0);
                // Render points on the GPU using vertex pulling
                Graphics.DrawProceduralNow(MeshTopology.Points, _dataSet.N);
                GL.PopMatrix();
            }
        }

        private void ReleaseAllGpuResources()
        {
            // Columns
            if (_buffers != null)
            {
                for (int i = 0; i < _buffers.Length; i++)
                {
                    if (_buffers[i] != null)
                    {
                        _buffers[i].Release();
                        _buffers[i] = null;
                    }
                }
                _buffers = null;
            }

            // Mapping config
            if (_mappingConfigBuffer != null)
            {
                _mappingConfigBuffer.Release();
                _mappingConfigBuffer = null;
            }

            // Visibility
            if (_dataVisibleBuffer != null)
            {
                _dataVisibleBuffer.Release();
                _dataVisibleBuffer = null;
            }

            // Material
            if (_catalogMaterial != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(_catalogMaterial);
#else
                Destroy(_catalogMaterial);
#endif
                _catalogMaterial = null;
            }
        }

        void OnDisable() => ReleaseAllGpuResources();
        void OnDestroy() => ReleaseAllGpuResources();


    }

}