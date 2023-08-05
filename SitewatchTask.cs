using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Sitewatch
{
    public class SitewatchTask
    {
        public JSON_SitewatchTaskSettings settings = new JSON_SitewatchTaskSettings();
        public string name = "";
        public int failCounter = 0;
        public System.Timers.Timer timer = new System.Timers.Timer();

        public SitewatchTask(JSON_SitewatchTaskSettings pSettings, string pName)
        {
            settings = pSettings;
            name = pName;
            timer = new System.Timers.Timer(pSettings.SecondsBetweenUpdate * 1000.0)
            {
                AutoReset = false
            };
        }
    }
}
