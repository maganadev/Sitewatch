#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Text.Json;

namespace Sitewatch.JSON
{
    public class SitewachTaskConfig
    {
        public string URL { get; set; }
        public string querySelectorQuery { get; set; }
        public string Base64_ScriptToExecute { get; set; }
        public bool watchForPureAdditions { get; set; }
        public bool watchForPureDeletions { get; set; }
        public bool watchForChanges { get; set; }
        public bool watchForNoChanges { get; set; }
        public int SecondsToWaitBeforeEachCheck { get; set; }
        public int SecondsToWaitAfterScriptExecution { get; set; }

        public void initDefault()
        {
            URL = "";
            querySelectorQuery = "body";
            Base64_ScriptToExecute = "";
            watchForPureAdditions = false;
            watchForPureDeletions = false;
            watchForChanges = false;
            watchForNoChanges = false;
            SecondsToWaitBeforeEachCheck = 3600;
            SecondsToWaitAfterScriptExecution = 1;
        }

        private void sanitizeInputs()
        {
            URL = URL == null ? "" : URL;
            querySelectorQuery = querySelectorQuery == null ? "" : querySelectorQuery;
            Base64_ScriptToExecute = Base64_ScriptToExecute == null ? "" : Base64_ScriptToExecute;

            if (SecondsToWaitBeforeEachCheck <= 0)
            {
                SecondsToWaitBeforeEachCheck = 3600;
            }
            if (SecondsToWaitAfterScriptExecution <= 0)
            {
                SecondsToWaitAfterScriptExecution = 1;
            }
        }

        public static SitewachTaskConfig getSettings(FileInfo pFileInfo)
        {
            var toReturn = new SitewachTaskConfig();
            toReturn.initDefault();
            try
            {
                SitewachTaskConfig? temp = JsonSerializer.Deserialize<SitewachTaskConfig>(File.ReadAllText(pFileInfo.FullName));
                if (temp != null)
                {
                    temp.sanitizeInputs();
                    File.WriteAllText(pFileInfo.FullName, JsonSerializer.Serialize(temp, new JsonSerializerOptions { WriteIndented = true }));
                    toReturn = temp;
                }
            }
            catch (Exception) { }
            toReturn.sanitizeInputs();
            return toReturn;
        }
    }
}