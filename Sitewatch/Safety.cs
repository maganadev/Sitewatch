using Sitewatch.JSON;
using System.Text.Json;

namespace Sitewatch
{
    public class Safety
    {
        public static async Task<Dictionary<string, bool>> getArchivedSiteContent(string pName)
        {
            Dictionary<string, bool> toReturn = new Dictionary<string, bool>();
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName + ".content");
                var temp = JsonSerializer.Deserialize<Dictionary<string, bool>>(await File.ReadAllTextAsync(filepath));
                toReturn = temp != null ? temp : toReturn;
            }
            catch (Exception) { }
            return toReturn;
        }

        public static async Task setArchivedSiteContent(SitewatchTaskConfig task,
            Dictionary<string, bool> additions,
            Dictionary<string, bool> noChanges,
            Dictionary<string, bool> deletions)
        {
            //Build our contents
            Dictionary<string, bool> contents = new Dictionary<string, bool>();
            foreach(var line in additions)
            {
                contents.TryAdd(line.Key, line.Value);
            }
            foreach (var line in noChanges)
            {
                contents.TryAdd(line.Key, line.Value);
            }
            if (!task.ShouldForgetDeletions)
            {
                foreach (var line in deletions)
                {
                    contents.TryAdd(line.Key, line.Value);
                }
            }

            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, task.name + ".content");
                string value = JsonSerializer.Serialize(contents, new JsonSerializerOptions { WriteIndented = true });
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
    }
}
