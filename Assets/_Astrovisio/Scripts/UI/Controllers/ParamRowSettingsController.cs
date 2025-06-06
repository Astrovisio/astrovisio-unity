using System;
using System.Linq;

namespace Astrovisio
{
    public class ParamRowSettingsController : ICloneable
    {

        public string ParamName { get; private set; }
        public Project Project { get; private set; }
        public ConfigParam Param { get; private set; }
        public RenderSettings RenderSettings { get; private set; }


        public ParamRowSettingsController(string paramName, Project project)
        {
            ParamName = paramName;
            Project = project;
            Param = Project.ConfigProcess.Params.FirstOrDefault(p => p.Key == ParamName).Value;
            RenderSettings = new RenderSettings(ParamName, MappingType.None);
        }

        public void Reset()
        {
            RenderSettings = new RenderSettings(ParamName, MappingType.None);
        }

        public object Clone()
        {
            return new ParamRowSettingsController(ParamName, Project)
            {
                RenderSettings = this.RenderSettings?.Clone() as RenderSettings
            };
        }

    }

}

