#pragma warning disable CS8602 // Dereference of a possibly null reference.
using NLog.Common;
using PuppeteerSharp;
using Sitewatch;

public class PuppeteerSingleton
{
    private static IBrowser? browser = null;
    private static Semaphore pool = new Semaphore(4, 4);

    public static async Task init()
    {
        //Download and setup chrome

        if (Program.settings.ChromiumBinPath == string.Empty)
        {
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        }
        else
        {
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = Program.settings.ChromiumBinPath
            });
        }

        browser.DefaultWaitForTimeout = 30000;
    }

    public static async Task<string> getPageSource(string url, int secondsToWait, string scriptToExecute)
    {
        string toReturn = string.Empty;
        pool.WaitOne();

        try
        {
            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            if (scriptToExecute != string.Empty)
            {
                await page.EvaluateExpressionAsync(scriptToExecute);
            }
            await page.WaitForTimeoutAsync(secondsToWait * 1000);
            toReturn = await page.GetContentAsync();
            await page.CloseAsync();
        }
        catch (Exception)
        {
            //
        }

        pool.Release();
        return toReturn;
    }
}