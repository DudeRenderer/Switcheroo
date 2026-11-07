using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.International.Converters.PinYinConverter;

namespace Switcheroo.Core.Matchers
{
    public class PinyinInitialsMatcher : IMatcher
    {
        // Cache per character so changing window titles cannot grow a title cache indefinitely.
        private static readonly ConcurrentDictionary<char, string> Initials =
            new ConcurrentDictionary<char, string>();

        public MatchResult Evaluate(string input, string pattern)
        {
            var result = new MatchResult();
            if (input == null)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(pattern))
            {
                var positions = new List<int>();
                var usedPinyin = false;
                for (var i = 0; i < input.Length && positions.Count < pattern.Length; i++)
                {
                    var expected = char.ToLowerInvariant(pattern[positions.Count]);
                    if (char.ToLowerInvariant(input[i]) == expected)
                    {
                        positions.Add(i);
                    }
                    else if (expected >= 'a' && expected <= 'z' && input[i] >= '\u3400' &&
                             Initials.GetOrAdd(input[i], GetInitials).IndexOf(expected) >= 0)
                    {
                        positions.Add(i);
                        usedPinyin = true;
                    }
                }

                // Leave literal-only matches and empty queries to the existing matchers.
                if (positions.Count == pattern.Length && usedPinyin)
                {
                    result.Matched = true;
                    result.Score = 2;
                    var position = 0;
                    foreach (var matchedPosition in positions)
                    {
                        if (matchedPosition > position)
                        {
                            result.StringParts.Add(new StringPart(input.Substring(position, matchedPosition - position)));
                        }
                        result.StringParts.Add(new StringPart(input.Substring(matchedPosition, 1), true));
                        position = matchedPosition + 1;
                    }
                    if (position < input.Length)
                    {
                        result.StringParts.Add(new StringPart(input.Substring(position)));
                    }
                    return result;
                }
            }

            result.StringParts.Add(new StringPart(input));
            return result;
        }

        private static string GetInitials(char character)
        {
            if (!ChineseChar.IsValidChar(character))
            {
                return string.Empty;
            }

            // Multiple readings can have different initials (e.g. 重: zhong/chong).
            return new string(new ChineseChar(character).Pinyins
                .Where(pinyin => !string.IsNullOrEmpty(pinyin))
                .Select(pinyin => char.ToLowerInvariant(pinyin[0]))
                .Distinct().ToArray());
        }
    }
}
