using DiffLib;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using NLog;
using System.Collections.Generic;
using System.Text;

namespace Sitewatch
{
    public class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static Mutex TaskListMutex = new Mutex();
        public static Dictionary<string, SitewatchTask> tasks = new Dictionary<string, SitewatchTask>();

        public static void Main(string[] args)
        {
            applyLogConfig();
            updateTasks();
            PuppeteerSingleton.init();

            foreach (var taskPair in tasks)
            {
                SitewatchTask task = taskPair.Value;
                var meme = PuppeteerSingleton.getPageSource(task.settings.URL);
                var doc = Safety.docFromString(meme.Result);
                string newHTMLChunk = Safety.QuerySelector(doc, task.settings.querySelectorQuery);
                string oldHTMLChunk = Safety.getArchivedSiteContent(taskPair.Key);

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

        public static void updateTasks()
        {
            //Lock
            TaskListMutex.WaitOne();

            DirectoryInfo tasksDir = Directory.CreateDirectory("Tasks");
            foreach (var taskFile in tasksDir.GetFiles("*.json"))
            {
                SitewatchTaskSettings taskSettings = SitewatchTaskSettings.getSettings(taskFile);
                if (taskSettings.URL == string.Empty)
                {
                    continue;
                }

                string newName = Safety.TruncateString(taskFile.Name,taskFile.Extension);
                if (tasks.ContainsKey(newName))
                {
                    continue;
                }

                tasks.Add(newName, new SitewatchTask(taskSettings, newName));
            }

            //Unlock
            TaskListMutex.ReleaseMutex();
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

        public static void handleComparisons(string textBefore, string textAfter, SitewatchTask task)
        {
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
                logger.Log(LogLevel.Info,"There were additions!");
                Safety.setArchivedSiteContent(task.name, textAfter);
                return;
            }
            else if (task.settings.watchFor == "deletions" && wereDeletions)
            {
                logger.Log(LogLevel.Info, "There were deletions!");
                Safety.setArchivedSiteContent(task.name, textAfter);
                return;
            }
            else if (wereChanges)
            {
                logger.Log(LogLevel.Info, "There were changes!");
                Safety.setArchivedSiteContent(task.name, textAfter);
                return;
            }
        }
    }
}