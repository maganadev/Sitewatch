using System.Text.Json;

public class JSON_Settings
{
    public string DiscordWebhookURL { get; set; }
    public string ChromiumBinPath { get; set; }

    private void initDefault()
    {
        DiscordWebhookURL = "";
        ChromiumBinPath = "";
    }

    private void sanitize()
    {
        //
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