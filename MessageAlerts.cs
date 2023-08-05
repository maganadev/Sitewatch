using Sitewatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MessageAlerts
{
    public static void sendDiscordWebhookMessage(string pURL, string message)
    {
        Program.logger.Info(message);
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                client.PostAsync(pURL, new StringContent("{\"content\":\"" + message + "\"}", Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            }
        }
        catch (Exception e)
        {
            //
        }
    }
}
