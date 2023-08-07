#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

using DiffLib;
using NLog;
using Sitewatch.OOP;
using Sitewatch.JSON;
using System.Timers;
using System.Text;

namespace Sitewatch
{
    public class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static SitewatchSettings settings = SitewatchSettings.getSettings();

        public static void Main(string[] args)
        {
            applyLogConfig();
            PuppeteerSingleton.init().GetAwaiter().GetResult();
            AddTasks();

            //Sleep main thread in an endless loop
            while (true) { Thread.Sleep(1000); }
        }

        public static void AddTasks()
        {
            DirectoryInfo tasksDir = getTaskDirectory();
            var taskFiles = tasksDir.GetFiles("*.json");
            int tasksAdded = 0;

            foreach (var taskFile in taskFiles)
            {
                SitewachTaskConfig taskSettings = SitewachTaskConfig.getSettings(taskFile);
                if (taskSettings.URL == string.Empty)
                {
                    continue;
                }

                string newName = Safety.TruncateString(taskFile.Name, taskFile.Extension);
                SitewatchTask newTask = new SitewatchTask(taskSettings, newName);
                logger.Info("Adding task " + newName);
                Task.Run(() => TimerUp_CheckOnTask(null, null, newTask));
                tasksAdded++;
            }

            if (tasksAdded == 0)
            {
                logger.Error("No tasks added, exiting");
                Environment.Exit(0);
            }
        }

        public static async void TimerUp_CheckOnTask(Object source, ElapsedEventArgs e, SitewatchTask task)
        {
            string scriptToExecute = Safety.GetUTF8FromBase64(task.settings.Base64_ScriptToExecute);
            string pageSource = await PuppeteerSingleton.getPageSource(task.settings.URL, task.settings.SecondsToWaitAfterScriptExecution, scriptToExecute);
            var doc = Safety.docFromString(pageSource);
            string newHTMLChunk = Safety.QuerySelectorAll(doc, task.settings.querySelectorQuery);
            string oldHTMLChunk = await Safety.getArchivedSiteContent(task.name);

            HandleComparisons(oldHTMLChunk, newHTMLChunk, task);

            //Reset timer
            System.Timers.Timer? oldTimer = task.timer;
            task.timer = new System.Timers.Timer(task.settings.SecondsToWaitBeforeEachCheck * 1000.0) { AutoReset = false };
            task.timer.Elapsed += new ElapsedEventHandler((sender, e) => TimerUp_CheckOnTask(null, null, task));
            task.timer.Start();
            if (oldTimer != null) { oldTimer.Dispose(); }
        }

        public static async void HandleComparisons(string textBefore, string textAfter, SitewatchTask task)
        {
            if (await RespondOnSiteChange(textBefore, textAfter, task))
            {
                return;
            }

            List<TextDiff> pureAdditions = new List<TextDiff>();
            List<TextDiff> pureDeletions = new List<TextDiff>();
            List<TextDiff> changes = new List<TextDiff>();
            List<TextDiff> noChanges = new List<TextDiff>();
            getChanges(textBefore, textAfter, out pureAdditions, out pureDeletions, out changes, out noChanges);

            StringBuilder messageToCraft = new StringBuilder();
            messageToCraft.Append(task.name);
            messageToCraft.Append("\n////////Updates:\n\n\n");
            bool didAddContent = false;

            if (task.settings.watchForPureAdditions)
            {
                foreach (TextDiff diff in pureAdditions)
                {
                    messageToCraft.Append("////////Addition of:\n");
                    messageToCraft.Append(diff.textAfter);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }
            if (task.settings.watchForPureDeletions)
            {
                foreach (TextDiff diff in pureDeletions)
                {
                    messageToCraft.Append("////////Deletion of:\n");
                    messageToCraft.Append(diff.textBefore);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }
            if (task.settings.watchForChanges)
            {
                foreach (TextDiff diff in changes)
                {
                    messageToCraft.Append("////////Change of:\n");
                    messageToCraft.Append(diff.textBefore);
                    messageToCraft.Append("\nInto:\n");
                    messageToCraft.Append(diff.textAfter);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }
            if (task.settings.watchForNoChanges)
            {
                foreach (TextDiff diff in noChanges)
                {
                    messageToCraft.Append("////////No change to:\n");
                    messageToCraft.Append(diff.textBefore);
                    messageToCraft.Append("\n\n\n");

                    didAddContent = true;
                }
            }

            if (didAddContent)
            {
                await Safety.setArchivedSiteContent(task.name, textAfter);
                await MessageAlerts.sendDiscordWebhookTextFile(settings.DiscordWebhookURL, task.name + ".txt", messageToCraft.ToString());
            }
        }

        public static async Task<bool> RespondOnSiteChange(string textBefore, string textAfter, SitewatchTask task)
        {
            string message = string.Empty;

            //Check for failure
            if (textAfter == string.Empty)
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

            if (textBefore == string.Empty)
            {
                await Safety.setArchivedSiteContent(task.name, textAfter);
                message = "Setting initial content for task " + task.name;
                await MessageAlerts.sendDiscordWebhookMessage(settings.DiscordWebhookURL, message);
                return true;
            }

            return false;
        }

        public static void getChanges(
            string textBefore,
            string textAfter,
            out List<TextDiff> pPureAdditions,
            out List<TextDiff> pPureDeletions,
            out List<TextDiff> pChanges,
            out List<TextDiff> pNoChanges
            )
        {
            pPureAdditions = new List<TextDiff>();
            pPureDeletions = new List<TextDiff>();
            pChanges = new List<TextDiff>();
            pNoChanges = new List<TextDiff>();
            IEnumerable<DiffSection> sections = Diff.CalculateSections(textBefore.ToCharArray(), textAfter.ToCharArray());
            int p1 = 0;
            int p2 = 0;
            foreach (var section in sections)
            {
                string beforeText = textBefore.Substring(p1, section.LengthInCollection1);
                string afterText = textAfter.Substring(p2, section.LengthInCollection2);
                p1 += section.LengthInCollection1;
                p2 += section.LengthInCollection2;
                TextDiff toAdd = new TextDiff(beforeText, afterText);
                switch (toAdd.type)
                {
                    case DiffType.PureAddition:
                        pPureAdditions.Add(toAdd);
                        break;
                    case DiffType.PureDeletion:
                        pPureDeletions.Add(toAdd);
                        break;
                    case DiffType.Change:
                        pChanges.Add(toAdd);
                        break;
                    case DiffType.NoChange:
                        pNoChanges.Add(toAdd);
                        break;
                }
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