#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Text.Json;

namespace Sitewatch.JSON
{
    public class JSON_SitewatchTaskSettings
    {
        public string URL { get; set; }
        public string querySelectorQuery { get; set; }
        public string Base64_ScriptToExecute { get; set; }
        public bool watchForPureAdditions { get; set; }
        public bool watchForPureDeletions { get; set; }
        public bool watchForChanges { get; set; }
        public bool watchForNoChanges { get; set; }
        public int SecondsBetweenUpdate { get; set; }
        public int SecondsAfterPageLoad { get; set; }

        public void initDefault()
        {
            URL = "";
            querySelectorQuery = "body";
            Base64_ScriptToExecute = "";
            watchForPureAdditions = false;
            watchForPureDeletions = false;
            watchForChanges = false;
            watchForNoChanges = false;
            SecondsBetweenUpdate = 3600;
            SecondsAfterPageLoad = 1;
        }

        private void sanitize()
        {
            URL = URL == null ? "" : URL;
            querySelectorQuery = querySelectorQuery == null ? "" : querySelectorQuery;
            Base64_ScriptToExecute = Base64_ScriptToExecute == null ? "" : Base64_ScriptToExecute;

            if (SecondsBetweenUpdate <= 0)
            {
                SecondsBetweenUpdate = 3600;
            }
            if (SecondsAfterPageLoad <= 0)
            {
                SecondsAfterPageLoad = 1;
            }
        }

        public static JSON_SitewatchTaskSettings getSettings(FileInfo pFileInfo)
        {
            var toReturn = new JSON_SitewatchTaskSettings();
            toReturn.initDefault();
            try
            {
                JSON_SitewatchTaskSettings? temp = JsonSerializer.Deserialize<JSON_SitewatchTaskSettings>(File.ReadAllText(pFileInfo.FullName));
                if (temp != null)
                {
                    temp.sanitize();
                    File.WriteAllText(pFileInfo.FullName, JsonSerializer.Serialize(temp, new JsonSerializerOptions { WriteIndented = true }));
                    toReturn = temp;
                }
            }
            catch (Exception) { }
            toReturn.sanitize();
            return toReturn;
        }
    }
}