

using CatalogData;
using UnityEngine;

namespace Astrovisio
{

    public class RenderSettingsController
    {

        public DataRenderer DataRenderer { get; set; }

        private ParamRenderSettings paramRenderSettings;


        public RenderSettingsController()
        {

        }

        public void SetAxisSettings(AxisRenderSettings axisRenderSettings)
        {

            // Debug.Log($"SetAxisSettings: {axis} {thresholdMin} {thresholdMax} {scalingType}");
            DataRenderer.SetAxisAstrovisio(
                axisRenderSettings.Axis,
                axisRenderSettings.Name,
                axisRenderSettings.ThresholdMinSelected,
                axisRenderSettings.ThresholdMaxSelected,
                axisRenderSettings.ScalingType
            );
        }

        public void SetRenderSettings(ParamRenderSettings renderSettings)
        {
            if (renderSettings.Mapping == MappingType.Opacity && renderSettings.MappingSettings is OpacitySettings)
            {
                // Debug.Log("SetRenderSettings -> Opacity " + renderSettings.MappingSettings.ScalingType);
                SetOpacity(renderSettings);
            }
            else if (renderSettings.Mapping == MappingType.Colormap && renderSettings.MappingSettings is ColorMapSettings)
            {
                // Debug.Log("SetRenderSettings -> Colormap");
                SetColorMap(renderSettings);
            }
        }

        public void SetAxisAstrovisio(Axis axis, string paramName, float thresholdMin, float thresholdMax, ScalingType scalingType)
        {
            if (DataRenderer is not null)
            {
                DataRenderer.SetAxisAstrovisio(axis, paramName, thresholdMin, thresholdMax, scalingType);
            }
        }

        private void SetColorMap(ParamRenderSettings renderSettings)
        {
            if (renderSettings.Mapping == MappingType.Colormap && renderSettings.MappingSettings is ColorMapSettings)
            {
                ColorMapSettings colorMapSettings = renderSettings.MappingSettings as ColorMapSettings;

                string name = renderSettings.Name;
                ColorMapEnum colorMap = colorMapSettings.ColorMap;
                float thresholdMinSelected = colorMapSettings.ThresholdMinSelected;
                float thresholdMaxSelected = colorMapSettings.ThresholdMaxSelected;
                ScalingType scalingType = colorMapSettings.ScalingType;
                bool invert = colorMapSettings.Invert;

                DataRenderer.SetColorMap(name, colorMap, thresholdMinSelected, thresholdMaxSelected, scalingType, invert);
            }
            else
            {
                Debug.Log("Error on renderSettings.Mapping");
                return;
            }
        }

        public void RemoveColorMap()
        {
            if (DataRenderer is not null)
            {
                DataRenderer.RemoveColorMap();
            }
        }

        private void SetOpacity(ParamRenderSettings renderSettings)
        {
            if (
                DataRenderer is not null &&
                renderSettings.Mapping == MappingType.Opacity &&
                renderSettings.MappingSettings is OpacitySettings
                )
            {
                OpacitySettings opacitySettings = renderSettings.MappingSettings as OpacitySettings;

                string name = renderSettings.Name;

                DataRenderer.SetOpacity(name, opacitySettings.ThresholdMinSelected, opacitySettings.ThresholdMaxSelected, opacitySettings.ScalingType, opacitySettings.Invert);
            }
            else
            {
                Debug.Log("Error on renderSettings.Mapping");
                return;
            }
        }

        public void RemoveOpacity()
        {
            DataRenderer.RemoveOpacity();
        }

        public void SetNoise(bool state, float value = 0f)
        {
            AstrovisioDataSetRenderer astrovisioDataSetRenderer = DataRenderer.GetAstrovidioDataSetRenderer();
            astrovisioDataSetRenderer.SetNoise(state, value);
        }

    }

}