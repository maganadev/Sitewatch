using DiffLib;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NLog;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            logger.Info("Updating tasks");
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

                SitewatchTask newTask = new SitewatchTask(taskSettings, newName);
                tasks.Add(newName, newTask);
                TimerUp_CheckOnTask(null,null,newTask);
            }

            //Reset timer
            //tasksUpdateTimer.Dispose();
            tasksUpdateTimer = new System.Timers.Timer(60 * 1000){AutoReset = false};
            tasksUpdateTimer.Elapsed += new ElapsedEventHandler((sender, e) => TimerUp_UpdateTasks(sender, e));
            tasksUpdateTimer.Start();
        }

        public static async void TimerUp_CheckOnTask(Object source, ElapsedEventArgs e, SitewatchTask task)
        //public static void TimerUp_CheckOnTask(SitewatchTask task)
        {
            logger.Info("Checking on task " + task.name);
            var meme = PuppeteerSingleton.getPageSource(task.settings.URL);
            var doc = Safety.docFromString(meme.Result);
            string newHTMLChunk = Safety.QuerySelector(doc, task.settings.querySelectorQuery);
            string oldHTMLChunk = Safety.getArchivedSiteContent(task.name);

            handleComparisons(oldHTMLChunk, newHTMLChunk, task);

            //Reset timer
            //tasksUpdateTimer.Dispose();
            tasksUpdateTimer = new System.Timers.Timer(task.settings.SecondsBetweenUpdate * 1000){AutoReset = false};
            tasksUpdateTimer.Elapsed += new ElapsedEventHandler((sender, e) => TimerUp_CheckOnTask(sender, e, task));
            tasksUpdateTimer.Start();
        }

        public static void handleComparisons(string textBefore, string textAfter, SitewatchTask task)
        {
            //For later
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
                MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
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
                MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                return;
            }
            else if (task.settings.watchFor == "deletions" && wereDeletions)
            {
                Safety.setArchivedSiteContent(task.name, textAfter);
                message = "There were deletions for task " + task.name;
                MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                return;
            }
            else if (wereChanges)
            {
                Safety.setArchivedSiteContent(task.name, textAfter);
                message = "There were changes for task " + task.name;
                MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                return;
            }
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

        public static void applyLogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
        }
    }
}