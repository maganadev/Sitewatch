using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Text;

namespace Sitewatch
{
    public class Safety
    {
        public static string QuerySelectorAll(HtmlDocument doc, string query)
        {
            StringBuilder toReturn = new StringBuilder();
            try
            {
                var nodes = doc.DocumentNode.QuerySelectorAll(query);
                foreach(var node in nodes )
                {
                    toReturn.Append(node.OuterHtml);
                }
                return toReturn.ToString();
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }

        public static HtmlDocument docFromString(string pContents)
        {
            HtmlDocument toReturn = new HtmlDocument();
            try
            {
                toReturn.LoadHtml(pContents);
            }
            catch(Exception)
            {
                //
            }
            return toReturn;
        }

        public static string getArchivedSiteContent(string pName)
        {
            string toReturn = string.Empty;
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName + ".content");
                toReturn = File.ReadAllText(filepath);
            }
            catch(Exception)
            {
                //
            }
            return toReturn;
        }

        public static void setArchivedSiteContent(string pName, string pContents)
        {
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName+".content");
                File.WriteAllText(filepath, pContents);
            }
            catch (Exception)
            {
                //
            }
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
