using Sitewatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MessageAlerts
{
    public static async Task sendDiscordWebhookMessage(string pURL, string message)
    {
        Program.logger.Info(message);
        using (HttpClient client = new HttpClient())
        {
            await client.PostAsync(pURL, new StringContent("{\"content\":\"content\"}", Encoding.UTF8, "application/json")); 
        }
    }
}
