﻿#pragma warning disable CS8602 // Dereference of a possibly null reference.
using NLog.Common;
using PuppeteerSharp;

public class PuppeteerSingleton
{
    public static IBrowser? browser = null;

    public static void init()
    {
        //Download and setup chrome
        using var browserFetcher = new BrowserFetcher();
        browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).GetAwaiter().GetResult();
        browser = Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }).GetAwaiter().GetResult();
        browser.DefaultWaitForTimeout = 30000;
    }

    public static async Task<string> getPageSource(string url)
    {
        try
        {
            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            await page.WaitForTimeoutAsync(3000);
            var content = await page.GetContentAsync();
            await page.CloseAsync();
            return content;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}