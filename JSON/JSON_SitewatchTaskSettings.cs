using System.Text.Json;

namespace Sitewatch.JSON
{
    public class JSON_SitewatchTaskSettings
    {
        public const string stringAdditions = "additions";
        public const string stringDeletions = "deletions";
        public const string stringChanges = "changes";

        public string URL { get; set; }
        public string querySelectorQuery { get; set; }
        public bool watchForPureAdditions { get; set; }
        public bool watchForPureDeletions { get; set; }
        public bool watchForChanges { get; set; }
        public bool watchForNoChanges { get; set; }
        public int SecondsBetweenUpdate { get; set; }

        public void initDefault()
        {
            URL = "";
            querySelectorQuery = "body";
            SecondsBetweenUpdate = 1000;
        }

        private void sanitize()
        {
            if (SecondsBetweenUpdate <= 0)
            {
                SecondsBetweenUpdate = 3600;
            }
        }

        public static JSON_SitewatchTaskSettings getSettings(FileInfo pFileInfo)
        {
            var toReturn = new JSON_SitewatchTaskSettings();
            toReturn.initDefault();
            try
            {
                JSON_SitewatchTaskSettings? temp = JsonSerializer.Deserialize<JSON_SitewatchTaskSettings>(File.ReadAllText(pFileInfo.FullName));
                toReturn = temp != null ? temp : toReturn;
            }
            catch (Exception) { }
            toReturn.sanitize();
            return toReturn;
        }
    }
}