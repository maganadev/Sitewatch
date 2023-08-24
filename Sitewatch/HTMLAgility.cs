using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

public class HTMLAgility
{
    public static Dictionary<string, bool> QuerySelectorAll(HtmlDocument doc, string query)
    {
        Dictionary<string, bool> toReturn = new Dictionary<string, bool>();
        try
        {
            var nodes = doc.DocumentNode.QuerySelectorAll(query);
            foreach (var node in nodes)
            {
                toReturn.TryAdd(node.OuterHtml, true);
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