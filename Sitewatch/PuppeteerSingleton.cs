#pragma warning disable CS8602 // Dereference of a possibly null reference.
using NLog.Common;
using PuppeteerSharp;
using Sitewatch;
using Sitewatch.OOP;
using System.Text;

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

    public static async Task<string> getPageSource(string url, List<PreprocessStep> preprocessSteps)
    {
        string toReturn = string.Empty;
        pool.WaitOne();

        //Create our page
        IPage page = null;

        //Get content
        try
        {
            page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            for (int i = 0; i < preprocessSteps.Count; i++)
            {
                await ExecuteStep(url, page, preprocessSteps[i]);
            }
            toReturn = await page.GetContentAsync();
        }
        catch (Exception) { }

        //Close Page
        try
        {
            await page.CloseAsync();
        }
        catch (Exception) { }

        pool.Release();
        return toReturn;
    }

    public static async Task ExecuteStep(string url, IPage? page, PreprocessStep step)
    {
        switch (step.action.ToLower())
        {
            case "wait":
                await TryWait(url, page, step);
                break;
            case "exec_b64_js":
                await TryExec(url, page, step);
                break;
            case "click":
                await TryClick(url, page, step);
                break;
            case "type":
                await TryType(url, page, step);
                break;
            default:
                Program.logger.Warn("A preprocessing step action was not recognized");
                break;
        }
    }

    public static async Task TryWait(string url, IPage? page, PreprocessStep step)
    {
        int secondsToWait = 0;
        if (int.TryParse(step.value, out secondsToWait))
        {
            await page.WaitForTimeoutAsync(secondsToWait * 1000);
        }
        else
        {
            Program.logger.Warn("Unable to parse value for wait on URL "+url);
        }
    }

    public static async Task TryExec(string url, IPage? page, PreprocessStep step)
    {
        string decoded = string.Empty;
        try
        {
            byte[] data = Convert.FromBase64String(step.value);
            decoded = System.Text.Encoding.UTF8.GetString(data);
        }
        catch (Exception)
        {
            Program.logger.Warn("Unable to parse script to exec on URL " + url);
            return;
        }

        try
        {
            if (decoded != string.Empty)
            {
                await page.EvaluateExpressionAsync(decoded);
            }
        }
        catch(Exception)
        {
            Program.logger.Warn("Error while executing script on URL " + url);
            return;
        }
    }

    public static async Task TryClick(string url, IPage? page, PreprocessStep step)
    {
        try
        {
            await page.ClickAsync(step.value);
        }
        catch (Exception)
        {
            Program.logger.Warn("Unable to click element on URL "+url);
        }
    }

    public static async Task TryType(string url, IPage? page, PreprocessStep step)
    {
        try
        {
            var meme = step.value.Split('|');
            await page.TypeAsync(meme[0], meme[1], new PuppeteerSharp.Input.TypeOptions { Delay = 100 });
        }
        catch (Exception)
        {
            Program.logger.Warn("Unable to type element on URL "+url);
        }
    }
}