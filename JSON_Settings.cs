using System.Text.Json;

public class JSON_Settings
{
    public string DiscordWebhookURL { get; set; }

    private void initDefault()
    {
        DiscordWebhookURL = "";
    }

    public const string settingsPath = "settings.json";

    public static JSON_Settings getSettings()
    {
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
        return toReturn;
    }
}