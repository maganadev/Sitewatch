using System.Text.Json;

public class SitewatchTaskSettings
{
    public string URL { get; set; }
    public string querySelectorQuery { get; set; }
    public string additionalHeaders { get; set; }
    public string watchFor { get; set; }
    public int SecondsBetweenUpdate { get; set; }

    public void init()
    {
        URL = "";
        querySelectorQuery = "body";
        additionalHeaders = "";
        watchFor = "changes";
        SecondsBetweenUpdate = 1000;
    }

    public static SitewatchTaskSettings getSettings(FileInfo pFileInfo)
    {
        var toReturn = new SitewatchTaskSettings();
        toReturn.init();
        try
        {
            SitewatchTaskSettings? temp = JsonSerializer.Deserialize<SitewatchTaskSettings>(File.ReadAllText(pFileInfo.FullName));
            toReturn = temp != null ? temp : toReturn;
        }
        catch (Exception) { }
        return toReturn;
    }
}