using System;
using System.Collections.Generic;
using System.Linq;

public static class PromptExtensions
{
    public static Dictionary<string, string> Parse(this string prompt)
    {
        var dict = new Dictionary<string, string>
        {
            { "Title", null },
            { "Event", null },
            { "Countries", null },
            { "Logline", null },
            { "Vibe", null }
        };

        var lines = prompt
                    .Replace("#", string.Empty)
                    .Replace("**", string.Empty)
                    .Split('\n');
        string key = null;

        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length < 2 && key != null)
                dict[key] += "\n" + line;
            else if (parts.Length > 1)
                dict[parts[0]
                    .Trim()
                    ] = string.Join(":", parts.Skip(1))
                    .Trim();
        }

        return dict;
    }
}