using Sitewatch;
using Sitewatch.OOP;
using Sitewatch.JSON;

namespace Sitewatch_UnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Program.applyLogConfig();
            PuppeteerSingleton.init().GetAwaiter().GetResult();

            SitewatchTaskConfig taskConfig = new SitewatchTaskConfig();
            SitewatchTask task = new SitewatchTask(taskConfig, "dummy");

            Program.tasks.Add(task);
            Program.launchTasks();
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
    }
}