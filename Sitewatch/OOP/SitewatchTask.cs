using Sitewatch.JSON;

namespace Sitewatch.OOP
{
    public class SitewatchTask
    {
        public SitewatchTaskConfig settings = new SitewatchTaskConfig();
        public string name = "";
        public int failCounter = 0;
        public System.Timers.Timer? timer = new System.Timers.Timer();

        public SitewatchTask(SitewatchTaskConfig pSettings, string pName)
        {
            settings = pSettings;
            name = pName;
        }
    }
}
