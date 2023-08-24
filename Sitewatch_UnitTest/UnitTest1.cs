using Sitewatch;
using Sitewatch.OOP;
using Sitewatch.JSON;
using System.Diagnostics.Metrics;

namespace Sitewatch_UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Program.applyLogConfig();
            PuppeteerSingleton.init().GetAwaiter().GetResult();
        }

        [Test]
        public void DummyTest()
        {
            Assert.True(true);
        }

        [Test]
        public void TruncateString_00()
        {
            //Check TruncateString
            var result = Safety.TruncateString("text.txt", ".txt");
            Assert.True(result == "text");
        }

        [Test]
        public void TruncateString_01()
        {
            //Check TruncateString
            var result = Safety.TruncateString("text.txt", "dfkdslfjkdls;fds");
            Assert.True(result == "text.txt");
        }

        [Test]
        public async Task ForgettingHistory()
        {
            const string TaskName = "ForgettingHistory";

            int count = 0;
            string[] history = {
                @"<body>
                    <food type=""apple""></food>
                    <food type=""banana""></food>
                    <food type=""carrots""></food>
                </body>",
                @"<body>
                    <food type=""apple""></food>
                    <food type=""banana""></food>
                </body>",
                @"<body>
                    <food type=""apple""></food>
                    <food type=""banana""></food>
                    <food type=""carrots""></food>
                </body>",
            };
            Dictionary<string,bool> emptyMap = new Dictionary<string,bool>();

            SitewatchTaskConfig taskConfig = new SitewatchTaskConfig();
            taskConfig.URL = "";
            taskConfig.QuerySelectorAll_Query = "food";
            taskConfig.PreprocessSteps = new List<PreprocessStep>();
            taskConfig.ShouldWatchForAdditions = true;
            taskConfig.ShouldWatchForDeletions = true;
            taskConfig.ShouldWatchForNoChanges = true;
            taskConfig.UpdateCheckIntervalSeconds = 1;
            taskConfig.ShouldForgetDeletions = true;
            taskConfig.name = TaskName;

            //Clear any previous tests
            await Safety.setArchivedSiteContent(taskConfig, emptyMap, emptyMap, emptyMap);

            //Set initial
            await Program.HandleUpdateLogic(taskConfig, history[0]);

            //Should now only contain 2
            await Program.HandleUpdateLogic(taskConfig, history[1]);
            count = (await Safety.getArchivedSiteContent(TaskName)).Count;
            Assert.That(count, Is.EqualTo(2));

            //Should now contain 3
            await Program.HandleUpdateLogic(taskConfig, history[2]);
            count = (await Safety.getArchivedSiteContent(TaskName)).Count;
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task NotForgettingHistory()
        {
            const string TaskName = "NotForgettingHistory";

            int count = 0;
            string[] history = {
                @"<body>
                    <food type=""apple""></food>
                    <food type=""banana""></food>
                    <food type=""carrots""></food>
                </body>",
                @"<body>
                    <food type=""apple""></food>
                    <food type=""banana""></food>
                </body>",
                @"<body>
                    <food type=""apple""></food>
                    <food type=""banana""></food>
                    <food type=""carrots""></food>
                </body>",
            };
            Dictionary<string, bool> emptyMap = new Dictionary<string, bool>();

            SitewatchTaskConfig taskConfig = new SitewatchTaskConfig();
            taskConfig.URL = "";
            taskConfig.QuerySelectorAll_Query = "food";
            taskConfig.PreprocessSteps = new List<PreprocessStep>();
            taskConfig.ShouldWatchForAdditions = true;
            taskConfig.ShouldWatchForDeletions = true;
            taskConfig.ShouldWatchForNoChanges = true;
            taskConfig.UpdateCheckIntervalSeconds = 1;
            taskConfig.ShouldForgetDeletions = false;
            taskConfig.name = TaskName;

            //Clear any previous tests
            await Safety.setArchivedSiteContent(taskConfig, emptyMap, emptyMap, emptyMap);

            //Set initial
            await Program.HandleUpdateLogic(taskConfig, history[0]);

            //Should still contain 3
            await Program.HandleUpdateLogic(taskConfig, history[1]);
            count = (await Safety.getArchivedSiteContent(TaskName)).Count;
            Assert.That(count, Is.EqualTo(3));

            //Should still contain 3
            await Program.HandleUpdateLogic(taskConfig, history[2]);
            count = (await Safety.getArchivedSiteContent(TaskName)).Count;
            Assert.That(count, Is.EqualTo(3));
        }
    }
}