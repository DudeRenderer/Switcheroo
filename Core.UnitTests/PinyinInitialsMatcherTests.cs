using System.Linq;
using NUnit.Framework;
using Switcheroo.Core.Matchers;

namespace Switcheroo.Core.UnitTests
{
    [TestFixture]
    public class PinyinInitialsMatcherTests
    {
        [TestCase("欧陆字典", "ol")]
        [TestCase("欧陆字典", "olz")]
        [TestCase("欧陆字典", "olzd")]
        [TestCase("欧陆字典", "OLZ")]
        [TestCase("文档 - 欧陆字典", "olz")]
        [TestCase("欧陆字典", "lz")]
        [TestCase("欧陆字典", "od")]
        [TestCase("欧陆 Dictionary", "old")]
        [TestCase("Chrome - 欧陆字典", "colz")]
        [TestCase("重庆", "cq")]
        [TestCase("重庆", "zq")]
        public void Evaluate_InitialsMatch_ReturnsMatchingResult(string input, string pattern)
        {
            var result = new PinyinInitialsMatcher().Evaluate(input, pattern);

            Assert.That(result.Matched, Is.True);
            Assert.That(result.Score, Is.EqualTo(2));
            Assert.That(string.Concat(result.StringParts.Select(part => part.Value)), Is.EqualTo(input));
            Assert.That(result.StringParts.Count(part => part.IsMatch), Is.EqualTo(pattern.Length));
        }

        [TestCase("欧陆字典", "ol", "欧陆")]
        [TestCase("查词 - 欧陆字典", "olz", "欧陆字")]
        [TestCase("欧陆 Dictionary", "old", "欧陆D")]
        [TestCase("欧陆字典", "od", "欧典")]
        public void Evaluate_Match_HighlightsOriginalCharacters(string input, string pattern, string highlighted)
        {
            var result = new PinyinInitialsMatcher().Evaluate(input, pattern);

            Assert.That(string.Concat(result.StringParts.Where(part => part.IsMatch).Select(part => part.Value)),
                Is.EqualTo(highlighted));
        }

        [TestCase("欧陆字典", "lo")]
        [TestCase("欧陆字典", "oll")]
        [TestCase("欧陆字典", "olzdx")]
        [TestCase("欧陆字典", ".*")]
        [TestCase("欧陆字典", "")]
        [TestCase("欧陆字典", null)]
        [TestCase("", "ol")]
        [TestCase("chrome", "crm")]
        [TestCase("欧陆 Chrome", "chrome")]
        [TestCase("😀 - 欧陆字典", "xyz")]
        public void Evaluate_NoPinyinMatch_PreservesUnhighlightedInput(string input, string pattern)
        {
            var result = new PinyinInitialsMatcher().Evaluate(input, pattern);

            Assert.That(result.Matched, Is.False);
            Assert.That(result.Score, Is.EqualTo(0));
            Assert.That(result.StringParts.Count, Is.EqualTo(1));
            Assert.That(result.StringParts[0].Value, Is.EqualTo(input));
            Assert.That(result.StringParts[0].IsMatch, Is.False);
        }

        [Test]
        public void Evaluate_NullInput_ReturnsNoStringParts()
        {
            var result = new PinyinInitialsMatcher().Evaluate(null, "ol");

            Assert.That(result.Matched, Is.False);
            Assert.That(result.StringParts, Is.Empty);
        }
    }
}
