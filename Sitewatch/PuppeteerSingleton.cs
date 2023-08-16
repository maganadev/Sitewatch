#pragma warning disable CS8602 // Dereference of a possibly null reference.
using NLog.Common;
using PuppeteerSharp;
using Sitewatch;
using Sitewatch.OOP;

public class PuppeteerSingleton
{
    private static IBrowser? browser = null;
    private static Semaphore pool = new Semaphore(4, 4);

    public static async Task init()
    {
        if (Program.settings.ChromiumBinPath == string.Empty)
        {
            //Download and setup Chromium
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false });
        }
        else
        {
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                ExecutablePath = Program.settings.ChromiumBinPath
            });
        }

        browser.DefaultWaitForTimeout = 30000;
    }

    public static async Task<string> getPageSource(string url, List<PreprocessStep> preprocessSteps)
    {
        string toReturn = string.Empty;
        pool.WaitOne();

        try
        {
            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            for (int i = 0; i < preprocessSteps.Count; i++)
            {
                await ExecuteStep(page, preprocessSteps[i]);
            }
            toReturn = await page.GetContentAsync();
            await page.CloseAsync();
        }
        catch (Exception) { }

        pool.Release();
        return toReturn;
    }

    public static async Task ExecuteStep(IPage? page, PreprocessStep step)
    {
        switch (step.action.ToLower())
        {
            case "wait":
                await TryWait(page, step);
                break;
            case "exec_b64_js":
                await TryExec(page, step);
                break;
            case "click":
                await TryClick(page, step);
                break;
            case "type":
                await TryType(page, step);
                break;
            default:
                Program.logger.Warn("A preprocessing step action was not recognized");
                break;
        }
    }

    public static async Task TryWait(IPage? page, PreprocessStep step)
    {
        int secondsToWait = 0;
        if (int.TryParse(step.value, out secondsToWait))
        {
            await page.WaitForTimeoutAsync(secondsToWait * 1000);
        }
        else
        {
            Program.logger.Warn("Unable to parse value for wait");
        }
    }

    public static async Task TryExec(IPage? page, PreprocessStep step)
    {
        try
        {
            string decoded = string.Empty;
            byte[] data = Convert.FromBase64String(step.value);
            decoded = System.Text.Encoding.UTF8.GetString(data);
            if (decoded != string.Empty)
            {
                await page.EvaluateExpressionAsync(decoded);
            }
        }
        catch (Exception)
        {
            Program.logger.Warn("Unable to parse script to exec");
        }
    }

    public static async Task TryClick(IPage? page, PreprocessStep step)
    {
        try
        {
            await page.ClickAsync(step.value);
        }
        catch (Exception)
        {
            Program.logger.Warn("Unable to click element");
        }
    }

    public static async Task TryType(IPage? page, PreprocessStep step)
    {
        try
        {
            await page.TypeAsync("", step.value);
        }
        catch (Exception)
        {
            Program.logger.Warn("Unable to click element");
        }
    }
}