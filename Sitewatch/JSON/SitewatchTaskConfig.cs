#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Text.Json;
using Sitewatch.OOP;

namespace Sitewatch.JSON
{
    public class SitewatchTaskConfig
    {
        public string URL { get; set; }
        public string QuerySelectorAll_Query { get; set; }
        public List<PreprocessStep> PreprocessSteps { get; set; }
        public bool ShouldWatchForAdditions { get; set; }
        public bool ShouldWatchForDeletions { get; set; }
        public bool ShouldWatchForNoChanges { get; set; }
        public int UpdateCheckIntervalSeconds { get; set; }

        public void initDefault()
        {
            URL = "";
            QuerySelectorAll_Query = "body";
            PreprocessSteps = new List<PreprocessStep>();
            ShouldWatchForAdditions = true;
            ShouldWatchForDeletions = false;
            ShouldWatchForDeletions = false;
            UpdateCheckIntervalSeconds = 3600;
        }

        private void sanitizeInputs()
        {
            URL = URL == null ? "" : URL;
            QuerySelectorAll_Query = QuerySelectorAll_Query == null ? "" : QuerySelectorAll_Query;
            if (PreprocessSteps == null)
            {
                PreprocessSteps = new List<PreprocessStep>();
            }

            if (UpdateCheckIntervalSeconds <= 0)
            {
                UpdateCheckIntervalSeconds = 3600;
            }
        }

        public static SitewatchTaskConfig getSettings(FileInfo pFileInfo)
        {
            var toReturn = new SitewatchTaskConfig();
            toReturn.initDefault();
            try
            {
                SitewatchTaskConfig? temp = JsonSerializer.Deserialize<SitewatchTaskConfig>(File.ReadAllText(pFileInfo.FullName), new JsonSerializerOptions
                {
                    IncludeFields = true
                });
                if (temp != null)
                {
                    temp.sanitizeInputs();
                    File.WriteAllText(pFileInfo.FullName, JsonSerializer.Serialize(temp, new JsonSerializerOptions {
                        WriteIndented = true,
                        IncludeFields = true,
                    }));
                    toReturn = temp;
                }
            }
            catch (Exception) { }
            toReturn.sanitizeInputs();
            return toReturn;
        }
    }
}