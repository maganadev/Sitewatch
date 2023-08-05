#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Text.Json;

namespace Sitewatch.JSON
{
    public class JSON_Settings
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

        private void sanitize()
        {
            DiscordWebhookURL = DiscordWebhookURL == null ? "" : DiscordWebhookURL;
            ChromiumBinPath = ChromiumBinPath == null ? "" : ChromiumBinPath;
            TaskFolderPath = TaskFolderPath == null ? "" : TaskFolderPath;
        }

        public static JSON_Settings getSettings()
        {
            const string settingsPath = "settings.json";
            var toReturn = new JSON_Settings();
            toReturn.initDefault();
            try
            {
                if (File.Exists(settingsPath))
                {
                    JSON_Settings? temp = JsonSerializer.Deserialize<JSON_Settings>(File.ReadAllText(settingsPath));
                    toReturn = temp != null ? temp : toReturn;
                }
                else
                {
                    File.WriteAllText(settingsPath, JsonSerializer.Serialize<JSON_Settings>(toReturn, JsonSerializerOptions.Default));
                }
            }
            catch (Exception) { }
            toReturn.sanitize();
            return toReturn;
        }
    }
}