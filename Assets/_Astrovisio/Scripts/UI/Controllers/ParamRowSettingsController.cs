using System;

namespace Astrovisio
{
    public class ParamRowSettingsController : AbstractRowSettingsController, ICloneable
    {
        public ParamRenderSettings ParamRenderSettings { get; private set; }

        public ParamRowSettingsController(string paramName, Project project)
            : base(paramName, project)
        {
            ParamRenderSettings = new ParamRenderSettings(paramName, MappingType.None);
        }

        public object Clone()
        {
            return new ParamRowSettingsController(ParamName, Project)
            {
                ParamRenderSettings = ParamRenderSettings?.Clone() as ParamRenderSettings
            };
        }

        public void Reset()
        {
            ParamRenderSettings = new ParamRenderSettings(ParamName, MappingType.None);
        }
        
    }

}
