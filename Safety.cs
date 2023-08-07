﻿using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System.Buffers.Text;
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
                foreach (var node in nodes)
                {
                    toReturn.Append(node.OuterHtml);
                }
                return toReturn.ToString();
            }
            catch (Exception)
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
            catch (Exception) { }
            return toReturn;
        }

        public static async Task<string> getArchivedSiteContent(string pName)
        {
            string toReturn = string.Empty;
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName + ".content");
                toReturn = await File.ReadAllTextAsync(filepath);
            }
            catch (Exception) { }
            return toReturn;
        }

        public static async Task setArchivedSiteContent(string pName, string pContents)
        {
            try
            {
                DirectoryInfo tasksDir = Directory.CreateDirectory("LastContents");
                string filepath = Path.Combine(tasksDir.FullName, pName + ".content");
                await File.WriteAllTextAsync(filepath, pContents);
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
