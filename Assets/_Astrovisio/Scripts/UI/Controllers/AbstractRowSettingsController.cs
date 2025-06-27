using System.Linq;

namespace Astrovisio
{
    public abstract class AbstractRowSettingsController
    {
        public string ParamName { get; protected set; }
        public Project Project { get; protected set; }
        public ConfigParam Param { get; protected set; }

        protected AbstractRowSettingsController(string paramName, Project project)
        {
            ParamName = paramName;
            Project = project;
            Param = Project.ConfigProcess.Params.FirstOrDefault(p => p.Key == ParamName).Value;
        }
    }

}
