using System.Linq;
using NUnit.Framework;

namespace Switcheroo.Core.UnitTests
{
    [TestFixture]
    public class WindowFiltererTests
    {
        [TestCase("ol")]
        [TestCase("olz")]
        [TestCase("OLZD")]
        public void Filter_PinyinInitials_FindsWindowTitle(string query)
        {
            var window = new WindowText("欧陆字典", "eudic");
            var results = Filter(query, window, new WindowText("浏览器", "chrome"));

            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results[0].AppWindow, Is.SameAs(window));
            var bestMatch = results[0].WindowTitleMatchResults.First(match => match.Matched);
            Assert.That(bestMatch.StringParts.Any(part => part.IsMatch && part.Value == "欧"), Is.True);
        }

        [Test]
        public void Filter_PinyinInitials_FindsProcessTitle()
        {
            var window = new WindowText("词汇表", "欧陆字典");
            Assert.That(Filter("olz", window).Single().AppWindow, Is.SameAs(window));
        }

        [Test]
        public void Filter_ProcessAndTitleQuery_UsesPinyinForBothParts()
        {
            var window = new WindowText("学习笔记", "欧陆字典");
            var results = Filter("olz.xxbj", window, new WindowText("学习笔记", "chrome"),
                new WindowText("其他", "欧陆字典"));
            Assert.That(results.Single().AppWindow, Is.SameAs(window));
        }

        [Test]
        public void Filter_LiteralMatch_RanksAheadOfPinyinMatch()
        {
            var literal = new WindowText("ol notes", "editor");
            var pinyin = new WindowText("欧陆字典", "eudic");
            Assert.That(Filter("ol", pinyin, literal)[0].AppWindow, Is.SameAs(literal));
        }

        [Test]
        public void Filter_EmptyQuery_PreservesWindowOrder()
        {
            var windows = new[] { new WindowText("欧陆字典", "eudic"), new WindowText("Chrome", "chrome") };
            Assert.That(Filter("", windows).Select(result => result.AppWindow), Is.EqualTo(windows));
        }

        private static FilterResult<WindowText>[] Filter(string query, params WindowText[] windows)
        {
            return new WindowFilterer().Filter(new WindowFilterContext<WindowText>
            {
                ForegroundWindowProcessTitle = "eudic",
                Windows = windows
            }, query).ToArray();
        }

        private class WindowText : IWindowText
        {
            public string WindowTitle { get; private set; }
            public string ProcessTitle { get; private set; }

            public WindowText(string windowTitle, string processTitle)
            {
                WindowTitle = windowTitle;
                ProcessTitle = processTitle;
            }
        }
    }
}
