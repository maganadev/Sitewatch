﻿#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

using NLog;
using Sitewatch.OOP;
using Sitewatch.JSON;
using System.Timers;
using System.Text;
using System.Threading.Tasks;

namespace Sitewatch
{
    public class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static SitewatchSettings settings = SitewatchSettings.getSettings();
        public static List<SitewatchTaskConfig> tasks = new List<SitewatchTaskConfig>();

        public static void Main(string[] args)
        {
            applyLogConfig();
            PuppeteerSingleton.init().GetAwaiter().GetResult();
            AddTasks();
            launchTasks();

            //Sleep main thread in an endless loop
            while (true) { Thread.Sleep(1000); }
        }

        public static void AddTasks()
        {
            DirectoryInfo tasksDir = getTaskDirectory();
            var taskFiles = tasksDir.GetFiles("*.json");

            foreach (var taskFile in taskFiles)
            {
                SitewatchTaskConfig newTask = SitewatchTaskConfig.getSettings(taskFile);
                string newName = Safety.TruncateString(taskFile.Name, taskFile.Extension);
                newTask.name = newName;
                logger.Info("Adding task " + newName);
                tasks.Add(newTask);
            }

            if (tasks.Count == 0)
            {
                logger.Error("No tasks added, exiting");
                Environment.Exit(0);
            }
        }

        public static void launchTasks()
        {
            foreach (var task in tasks)
            {
                Task.Run(() => TimerUp_CheckOnTask(null, null, task));
            }
        }

        public static async void TimerUp_CheckOnTask(Object source, ElapsedEventArgs e, SitewatchTaskConfig task)
        {
            List<PreprocessStep> preprocessSteps = task.PreprocessSteps;
            string pageSource = await PuppeteerSingleton.getPageSource(task.URL, preprocessSteps);
            await HandleUpdateLogic(task, pageSource);
        }

        public static async Task HandleUpdateLogic(SitewatchTaskConfig task, string pageSource)
        {
            var doc = HTMLAgility.docFromString(pageSource);
            Dictionary<string, bool> oldHTMLChunks = await Safety.getArchivedSiteContent(task.name);
            Dictionary<string, bool> newHTMLChunks = HTMLAgility.QuerySelectorAll(doc, task.QuerySelectorAll_Query);

            HandleComparisons(oldHTMLChunks, newHTMLChunks, task);

            //Reset timer
            System.Timers.Timer? oldTimer = task.timer;
            task.timer = new System.Timers.Timer(task.UpdateCheckIntervalSeconds * 1000.0) { AutoReset = false };
            task.timer.Elapsed += new ElapsedEventHandler((sender, e) => TimerUp_CheckOnTask(null, null, task));
            task.timer.Start();
            if (oldTimer != null) { oldTimer.Dispose(); }
        }

        public static async void HandleComparisons(Dictionary<string, bool> oldHTMLChunks, Dictionary<string, bool> newHTMLChunks, SitewatchTaskConfig task)
        {
            if (await RespondOnSiteChange(oldHTMLChunks, newHTMLChunks, task))
            {
                return;
            }

            Dictionary<string, bool> additions = new Dictionary<string, bool>();
            Dictionary<string, bool> deletions = new Dictionary<string, bool>();
            Dictionary<string, bool> noChanges = new Dictionary<string, bool>();
            foreach (var chunk in oldHTMLChunks)
            {
                if (!newHTMLChunks.ContainsKey(chunk.Key))
                {
                    //Was deletion
                    deletions.TryAdd(chunk.Key, true);
                }
                else
                {
                    //Was noChange
                    noChanges.TryAdd(chunk.Key, true);
                }
            }
            foreach (var chunk in newHTMLChunks)
            {
                if (!oldHTMLChunks.ContainsKey(chunk.Key))
                {
                    //Was addition
                    additions.TryAdd(chunk.Key, true);
                }
            }

            StringBuilder messageToCraft = new StringBuilder();
            messageToCraft.Append(task.name);
            messageToCraft.Append("\n////////Updates:\n\n\n");
            bool didAddContent = false;

            if (task.ShouldWatchForAdditions)
            {
                foreach (string chunk in additions.Keys)
                {
                    messageToCraft.Append("////////Addition of:\n");
                    messageToCraft.Append(chunk);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }
            if (task.ShouldWatchForDeletions)
            {
                foreach (string chunk in deletions.Keys)
                {
                    messageToCraft.Append("////////Deletion of:\n");
                    messageToCraft.Append(chunk);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }
            if (task.ShouldWatchForNoChanges)
            {
                foreach (string chunk in noChanges.Keys)
                {
                    messageToCraft.Append("////////No change to:\n");
                    messageToCraft.Append(chunk);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }

            if (didAddContent)
            {
                await Safety.setArchivedSiteContent(task, additions, noChanges, deletions);
                await MessageAlerts.sendDiscordWebhookTextFile(settings.DiscordWebhookURL, task.name + ".txt", messageToCraft.ToString());
            }
        }

        public static async Task<bool> RespondOnSiteChange(Dictionary<string, bool> oldHTMLChunks, Dictionary<string, bool> newHTMLChunks, SitewatchTaskConfig task)
        {
            string message = string.Empty;

            //Check for failure
            if (newHTMLChunks.Count == 0)
            {
                task.failCounter++;
                logger.Info("Failed to access the site for task " + task.name);

                if (task.failCounter == 5)
                {
                    message = "Task " + task.name + " has failed 5 times. Check to see what's up.";
                    await MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                }

                return true;
            }
            else
            {
                if (task.failCounter >= 5)
                {
                    message = "Task " + task.name + " has succeeded and come back to life.";
                    await MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                }

                task.failCounter = 0;
            }

            if (oldHTMLChunks.Count == 0)
            {
                Dictionary<string, bool> emptyNoChanges = new Dictionary<string, bool>();
                Dictionary<string, bool> emptyDeletions = new Dictionary<string, bool>();
                await Safety.setArchivedSiteContent(task, newHTMLChunks, emptyNoChanges, emptyDeletions);
                message = "Setting initial content for task " + task.name;
                await MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                return true;
            }

            return false;
        }

        public static void applyLogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
        }

        public static DirectoryInfo getTaskDirectory()
        {
            try
            {
                if (settings.TaskFolderPath == string.Empty)
                {
                    return Directory.CreateDirectory("Tasks");
                }
                else
                {
                    return new DirectoryInfo(settings.TaskFolderPath);
                }
            }
            catch (Exception)
            {
                logger.Error("Unable to access Task folder, exiting");
                Environment.Exit(0);
                return null;
            }
        }
    }
}