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
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                await client.PostAsync(pURL, new StringContent("{\"content\":\"" + message + "\"}", Encoding.UTF8, "application/json"));
                client.Dispose();
            }
        }
        catch (Exception)
        {
            //
        }
    }

    public static async Task sendDiscordWebhookTextFile(string pURL, string filename, string content)
    {
        Program.logger.Info(content);
        try
        {
            using (HttpClient client = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                var file_bytes = Encoding.UTF8.GetBytes(content);
                form.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), "Document", filename);
                await client.PostAsync(pURL, form);
                client.Dispose();
            }
        }
        catch (Exception)
        {
            //
        }
    }
}
