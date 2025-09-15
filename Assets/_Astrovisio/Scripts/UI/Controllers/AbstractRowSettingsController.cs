using System.Linq;

namespace Astrovisio
{
    public abstract class AbstractRowSettingsController
    {
        public string ParamName { get; protected set; }
        public Project Project { get; protected set; }
        public Variables Param { get; protected set; }

        protected AbstractRowSettingsController(string paramName, Project project)
        {
            ParamName = paramName;
            Project = project;
            Param = Project.Files.Params.FirstOrDefault(p => p.Key == ParamName).Value;
        }
    }

}
