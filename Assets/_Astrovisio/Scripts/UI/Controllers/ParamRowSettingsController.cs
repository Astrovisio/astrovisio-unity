using System;

namespace Astrovisio
{
    public class ParamRowSettingsController : AbstractRowSettingsController, ICloneable
    {
        public ParamRenderSettings ParamRenderSettings { get; private set; }

        public ParamRowSettingsController(Variable variable)
            : base(variable)
        {
            ParamRenderSettings = new ParamRenderSettings(variable.Name, MappingType.None);
        }

        public object Clone()
        {
            return new ParamRowSettingsController(Variable)
            {
                ParamRenderSettings = ParamRenderSettings?.Clone() as ParamRenderSettings
            };
        }

        public void Reset()
        {
            ParamRenderSettings = new ParamRenderSettings(Variable.Name, MappingType.None);
        }
        
    }

}
