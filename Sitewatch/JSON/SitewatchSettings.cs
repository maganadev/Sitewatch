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

            bool fileExists = false;
            try
            {
                fileExists = File.Exists(settingsPath);
            }
            catch (Exception) { }

            if (fileExists)
            {
                try
                {
                    SitewatchSettings temp = JsonSerializer.Deserialize<SitewatchSettings>(File.ReadAllText(settingsPath));
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to load JSON for " + settingsPath);
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(settingsPath, JsonSerializer.Serialize<SitewatchSettings>(toReturn, JsonSerializerOptions.Default));
                }
                catch (Exception) { }
            }

            //Sanitize and return
            toReturn.sanitizeInputs();
            return toReturn;
        }
    }
}