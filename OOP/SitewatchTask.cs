using Sitewatch.JSON;

namespace Sitewatch.OOP
{
    public class SitewatchTask
    {
        public JSON_SitewatchTaskSettings settings = new JSON_SitewatchTaskSettings();
        public string name = "";
        public int failCounter = 0;
        public System.Timers.Timer? timer = new System.Timers.Timer();

        public SitewatchTask(JSON_SitewatchTaskSettings pSettings, string pName)
        {
            settings = pSettings;
            name = pName;
        }
    }
}
