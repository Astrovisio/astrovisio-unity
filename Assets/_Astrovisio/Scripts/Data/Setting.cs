using Newtonsoft.Json;

namespace Astrovisio
{
    public class Setting
    {

        private string name;
        private double thrMin;
        private double thrMax;
        private double? thrMinSel;
        private double? thrMaxSel;
        private string scaling;
        private string mapping;
        private string colormap;
        private string opacity;
        private bool invertMapping;


        [JsonProperty("var_name")]
        public string Name
        {
            get => name;
            set => name = value;
        }

        [JsonProperty("vis_thr_min")]
        public double ThrMin
        {
            get => thrMin;
            set => thrMin = value;
        }

        [JsonProperty("vis_thr_min_sel")]
        public double? ThrMinSel
        {
            get => thrMinSel;
            set => thrMinSel = value;
        }

        [JsonProperty("vis_thr_max")]
        public double ThrMax
        {
            get => thrMax;
            set => thrMax = value;
        }

        [JsonProperty("vis_thr_max_sel")]
        public double? ThrMaxSel
        {
            get => thrMaxSel;
            set => thrMaxSel = value;
        }

        [JsonProperty("scaling")]
        public string Scaling
        {
            get => scaling;
            set => scaling = value;
        }

        [JsonProperty("mapping")]
        public string Mapping
        {
            get => mapping;
            set => mapping = value;
        }

        [JsonProperty("colormap")]
        public string Colormap
        {
            get => colormap;
            set => colormap = value;
        }

        [JsonProperty("opacity")]
        public string Opacity
        {
            get => opacity;
            set => opacity = value;
        }

        [JsonProperty("invert_mapping")]
        public bool InvertMapping
        {
            get => invertMapping;
            set => invertMapping = value;
        }

    }

}
