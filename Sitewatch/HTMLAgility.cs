using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

public class HTMLAgility
{
    public static Dictionary<string, int> QuerySelectorAll(HtmlDocument doc, string query)
    {
        Dictionary<string, int> toReturn = new Dictionary<string, int>();
        try
        {
            var nodes = doc.DocumentNode.QuerySelectorAll(query);
            foreach (var node in nodes)
            {
                if (toReturn.ContainsKey(node.OuterHtml))
                {
                    toReturn[node.OuterHtml] += 1;
                }
                else
                {
                    toReturn.Add(node.OuterHtml, 1);
                }
            }
        }
        catch (Exception) { }
        return toReturn;
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
}