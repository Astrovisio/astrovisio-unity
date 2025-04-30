using System.Collections.Generic;
using UnityEngine;

namespace Astrovisio
{
    public class ParamRawController
    {
        public string ParamName { get; set; }
        public ConfigParam Param { get; set; }

        public List<string> FileList { get; set; }
        public bool XAxis { get; set; }
        public bool YAxis { get; set; }
        public bool ZAxis { get; set; }
        public float MinThreshold { get; set; }
        public float MaxThreshold { get; set; }
        public bool Active { get; set; } = true;

        public ParamRawController(string paramName, ConfigParam param)
        {
            ParamName = paramName;
            Param = param;

            FileList = new List<string>(param.Files);
            XAxis = param.XAxis;
            YAxis = param.YAxis;
            ZAxis = param.ZAxis;
            MinThreshold = param.ThrMinSel;
            MaxThreshold = param.ThrMaxSel;

            Debug.Log($"Initialized ParamRawController with variable '{paramName}' and unit '{param.Unit}'");
        }

        
    }
}
