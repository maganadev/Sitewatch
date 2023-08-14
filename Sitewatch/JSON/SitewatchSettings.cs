#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Text.Json;

namespace Sitewatch.JSON
{
    public class SitewatchSettings
    {
        public string DiscordWebhookURL { get; set; }
        public string ChromiumBinPath { get; set; }
        public string TaskFolderPath { get; set; }

        private void initDefault()
        {
            DiscordWebhookURL = "";
            ChromiumBinPath = "";
            TaskFolderPath = "";
        }

        private void sanitizeInputs()
        {
            DiscordWebhookURL = DiscordWebhookURL == null ? "" : DiscordWebhookURL;
            ChromiumBinPath = ChromiumBinPath == null ? "" : ChromiumBinPath;
            TaskFolderPath = TaskFolderPath == null ? "" : TaskFolderPath;
        }

        public static SitewatchSettings getSettings()
        {
            const string settingsPath = "settings.json";
            var toReturn = new SitewatchSettings();
            toReturn.initDefault();
            try
            {
                if (File.Exists(settingsPath))
                {
                    SitewatchSettings? temp = JsonSerializer.Deserialize<SitewatchSettings>(File.ReadAllText(settingsPath));
                    toReturn = temp != null ? temp : toReturn;
                }
                else
                {
                    File.WriteAllText(settingsPath, JsonSerializer.Serialize<SitewatchSettings>(toReturn, JsonSerializerOptions.Default));
                }
            }
            catch (Exception) { }
            toReturn.sanitizeInputs();
            return toReturn;
        }
    }
}