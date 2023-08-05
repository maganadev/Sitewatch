using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitewatch
{
    public class SitewatchTask
    {
        public string name = "";
        public bool isStopped = false;
        public int failCounter = 0;
        public SitewatchTaskSettings settings = new SitewatchTaskSettings();

        public SitewatchTask(SitewatchTaskSettings pSettings, string pName)
        {
            settings = pSettings;
            name = pName;
        }
    }
}
