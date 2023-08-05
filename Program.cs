using DiffLib;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NLog;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Sitewatch
{
    public class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static JSON_Settings settings = JSON_Settings.getSettings();
        public static Dictionary<string, SitewatchTask> tasks = new Dictionary<string, SitewatchTask>();
        public static System.Timers.Timer tasksUpdateTimer = new System.Timers.Timer();

        public static void Main(string[] args)
        {
            applyLogConfig();
            PuppeteerSingleton.init();
            TimerUp_UpdateTasks(null,null);

            //Sleep main thread in an endless loop
            while (true) { Thread.Sleep(1000); }
        }

        public static async void TimerUp_UpdateTasks(Object source, ElapsedEventArgs e)
        {
            DirectoryInfo tasksDir = Directory.CreateDirectory("Tasks");
            foreach (var taskFile in tasksDir.GetFiles("*.json"))
            {
                SitewatchTaskSettings taskSettings = SitewatchTaskSettings.getSettings(taskFile);
                if (taskSettings.URL == string.Empty)
                {
                    continue;
                }

                string newName = Safety.TruncateString(taskFile.Name, taskFile.Extension);
                if (tasks.ContainsKey(newName))
                {
                    continue;
                }

                addNewTask(newName, new SitewatchTask(taskSettings, newName));
            }

            //Reset timer
            tasksUpdateTimer.Dispose();
            tasksUpdateTimer = new System.Timers.Timer(60 * 1000);
            tasksUpdateTimer.Elapsed += new ElapsedEventHandler(TimerUp_UpdateTasks);
            tasksUpdateTimer.Start();
        }

        public static void TimerUp_CheckOnTask(SitewatchTask task)
        {
            var meme = PuppeteerSingleton.getPageSource(task.settings.URL);
            var doc = Safety.docFromString(meme.Result);
            string newHTMLChunk = Safety.QuerySelector(doc, task.settings.querySelectorQuery);
            string oldHTMLChunk = Safety.getArchivedSiteContent(task.name);

            if (newHTMLChunk == string.Empty)
            {
                logger.Log(LogLevel.Warn, "Unable to get website content for site " + task.settings.URL);
            }
            if (oldHTMLChunk == string.Empty)
            {
                logger.Log(LogLevel.Info, "Skipping because we don't know what the website looked like before");
            }

            handleComparisons(oldHTMLChunk, newHTMLChunk, task);
        }

        public static void handleComparisons(string textBefore, string textAfter, SitewatchTask task)
        {
            string message = string.Empty;

            //Check for failure
            if (textAfter == string.Empty)
            {
                task.failCounter++;
                return;
            }
            else
            {
                task.failCounter = 0;
            }

            if (textBefore == string.Empty)
            {
                Safety.setArchivedSiteContent(task.name, textAfter);
                message = "Setting initial content for task " + task.name;
                Task.Run(() => MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message));
                return;
            }

            int additions = 0;
            int deletions = 0;
            getChangeCount(textBefore, textAfter, out additions, out deletions);
            bool wereAdditions = additions > 0;
            bool wereDeletions = deletions > 0;
            bool wereChanges = wereAdditions || wereDeletions;

            if (task.settings.watchFor == "additions" && wereAdditions)
            {
                Safety.setArchivedSiteContent(task.name, textAfter);
                message = "There were additions for task " + task.name;
                Task.Run(() => MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message));
                return;
            }
            else if (task.settings.watchFor == "deletions" && wereDeletions)
            {
                Safety.setArchivedSiteContent(task.name, textAfter);
                message = "There were deletions for task " + task.name;
                Task.Run(() => MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message));
                return;
            }
            else if (wereChanges)
            {
                Safety.setArchivedSiteContent(task.name, textAfter);
                message = "There were changes for task " + task.name;
                Task.Run(() => MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message));
                return;
            }
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

        public static void addNewTask(string newTaskName, SitewatchTask pTask)
        {
            tasks.Add(newTaskName, pTask);
        }

        

        public static void getChangeCount(string textBefore, string textAfter, out int pAdditions, out int pDeletions)
        {
            IEnumerable<DiffSection> sections = Diff.CalculateSections(textBefore.ToCharArray(), textAfter.ToCharArray());
            int additions = 0;
            int deletions = 0;
            foreach (var section in sections)
            {
                if (!section.IsMatch)
                {
                    if (section.LengthInCollection2 > 0)
                    {
                        additions++;
                    }
                    if (section.LengthInCollection1 > 0)
                    {
                        deletions++;
                    }
                }
            }
            pAdditions = additions;
            pDeletions = deletions;
        }
    }
}