using Sitewatch.JSON;
using System.Text.Json;

namespace Sitewatch
{
    public class Safety
    {
        public static async Task<Dictionary<string, int>> getArchivedSiteContent(string pName)
        {
            Dictionary<string, int> toReturn = new Dictionary<string, int>();
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName + ".content");
                var temp = JsonSerializer.Deserialize<Dictionary<string, int>>(await File.ReadAllTextAsync(filepath));
                toReturn = temp != null ? temp : toReturn;
            }
            catch (Exception) { }
            return toReturn;
        }

        public static async Task setArchivedSiteContent(string pName, Dictionary<string, int> pContents)
        {
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName + ".content");
                string value = JsonSerializer.Serialize(pContents, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filepath, value);
            }
            catch (Exception) { }
        }

        public static string TruncateString(string string1, string string2)
        {
            //if will fit
            int newLength = string1.Length - string2.Length;
            if (newLength >= 0)
            {
                return string1.Substring(0, newLength);
            }
            return string1;
        }

        public static string GetUTF8FromBase64(string b64encoded)
        {
            string toReturn = string.Empty;
            try
            {
                byte[] data = Convert.FromBase64String(b64encoded);
                toReturn = System.Text.Encoding.UTF8.GetString(data);
            }
            catch (Exception) { }
            return toReturn;
        }
    }
}
