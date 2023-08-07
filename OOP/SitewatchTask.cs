using Sitewatch.JSON;

namespace Sitewatch.OOP
{
    public class SitewatchTask
    {
        public SitewachTaskConfig settings = new SitewachTaskConfig();
        public string name = "";
        public int failCounter = 0;
        public System.Timers.Timer? timer = new System.Timers.Timer();

        public SitewatchTask(SitewachTaskConfig pSettings, string pName)
        {
            settings = pSettings;
            name = pName;
        }
    }
}
