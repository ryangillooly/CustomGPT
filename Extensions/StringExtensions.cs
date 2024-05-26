using System.Text.RegularExpressions;

namespace ChatGptTest.Extensions;

public static partial class StringExtensions
{
    public static bool ContainsImageGenerationKeywords(this string input)
    {
        var keywords = new[] { "Generate", "Create", "Make", "Build", "Design" };

        // Check if the input contains the word "image" or "images" with case-insensitivity
        var containsImage = Regex.IsMatch(input, @"\bimage(s)?\b", RegexOptions.IgnoreCase);

        return keywords.Any(
            keyword => Regex.IsMatch(input, @"\b" + keyword + @"\b", RegexOptions.IgnoreCase) && containsImage
        );
    }

    public static int? GetImageCount(this string input)
    {
        var numberWords = new Dictionary<string, int>
        {
            { "One", 1 },
            { "Two", 2 },
            { "Three", 3 },
            { "Four", 4 },
            { "Five", 5 },
            { "Six", 6 },
            { "Seven", 7 },
            { "Eight", 8 },
            { "Nine", 9 },
            { "Ten", 10 },
        };

        foreach (var kvp in numberWords)
        {
            if (Regex.IsMatch(input, @"\b" + kvp.Key + @"\b", RegexOptions.IgnoreCase))
            {
                return kvp.Value;
            }
        }

        return 1;
    }

    [GeneratedRegex(@"\bimage\b")]
    private static partial Regex MyRegex();
}