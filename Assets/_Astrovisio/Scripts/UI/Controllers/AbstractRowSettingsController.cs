using System.Linq;

namespace Astrovisio
{
    public abstract class AbstractRowSettingsController
    {
        public Variable Variable { get; protected set; }

        protected AbstractRowSettingsController(Variable variable)
        {
            Variable = variable;
        }
        
    }

}
