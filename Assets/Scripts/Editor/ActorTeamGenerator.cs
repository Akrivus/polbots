using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class ActorTeamGenerator
{
    [MenuItem("Tools/Generate Character Spreadsheet")]
    public static async void GenerateActorSpreadsheet()
    {
        var csv = new List<string> { "Name,Note" };
        var actors = Resources.LoadAll<Actor>("Actors");

        foreach (var actor in actors)
            csv.Add($"{actor.Name},");
        await File.WriteAllLinesAsync("./Assets/Resources/Actors.csv", csv);
    }

    [MenuItem("Tools/Generate Character Prompts")]
    public static async void GenerateActorPrompts()
    {
        var asset = Resources.Load<TextAsset>($"Prompts/Tools/Actor Prompts");
        var actors = Resources.LoadAll<Actor>("Actors");

        var csv = File.ReadAllLines("./Assets/Resources/Actors.csv");
        for (var i = 1; i < csv.Length; ++i)
        {
            var columns = csv[i].Split(',');

            var actor = actors.FirstOrDefault(x => x.Name == columns[0]);
            var note = string.Join(",", columns.Skip(1));

            if (!string.IsNullOrWhiteSpace(note))
                note = "#### Writer's Note:\n\n" + note;

            if (actor)
                await GenerateActorPrompt(asset, actor, note);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Generate Character Color Schemes")]
    public static async void GenerateActorColorSchemes()
    {
        var asset = Resources.Load<TextAsset>($"Prompts/Tools/Skin Tones");
        var actors = Resources.LoadAll<Actor>("Actors");

        foreach (var actor in actors)
            await GenerateActorColorScheme(asset, actor);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Fix Character Pronouns")]
    public static void FixActorPronouns()
    {
        var actors = Resources.LoadAll<Actor>("Actors");

        foreach (var actor in actors)
        {
            if (actor.Pronouns.Contains("they/them"))
                actor.Pronouns = "they/them";
            else if (actor.Pronouns.Contains("she/her"))
                actor.Pronouns = "she/her";
            else if (actor.Pronouns.Contains("he/him"))
                actor.Pronouns = "he/him";
            else
                actor.Pronouns = "they/them";
            // mark the object as dirty so it gets saved
            EditorUtility.SetDirty(actor);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Generate Team Colors")]
    public static async void GenerateActorTeams()
    {
        var asset = Resources.Load<TextAsset>("Prompts/Tools/Team Names");
        var actors = Resources.LoadAll<Actor>("Actors");

        foreach (var actor in actors)
            await GenerateActorTeam(asset, actor);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static async Task GenerateActorTeam(TextAsset asset, Actor actor)
    {
        if (actor.Players.Length < 11)
        {
            var prompt = asset.Format(actor.Title);
            var output = await OpenAiIntegration.CompleteAsync(prompt, true);

            var start = output.IndexOf("```") + 3;
            if (start > 2)
                output = output.Substring(start, output.IndexOf("```", start) - start);
            actor.Players = output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();
        }

        var texture = Resources.Load<Texture2D>($"Flags/{actor.Name}");
        if (!texture)
            return;
        var pixels = texture.GetPixels();

        actor.Color1 = GenerateColor1(pixels);
        actor.Color2 = GenerateColor2(pixels);
        actor.Color3 = GenerateColor3(pixels);

        actor.Color  = GenerateColor(pixels);

        EditorUtility.SetDirty(actor);
    }

    private static Color GenerateColor(Color[] colors)
    {
        var color = Color.black;
        for (var i = 0; i < colors.Length; ++i)
            if (color.a > 0.9f)
                color += colors[i];
        color /= colors.Length;
        color.a = 1f;
        return color;
    }

    private static Color[] SortColors(Color[] colors)
    {
        var count = new Dictionary<Color, int>();
        foreach (var color in colors)
        {
            if (color.a < 0.9f)
                continue;
            if (count.ContainsKey(color))
                count[color]++;
            else
                count[color] = 1;
        }
        return count.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
    }

    private static Color GenerateColor1(Color[] colors)
    {
        var sortedColors = SortColors(colors);
        return sortedColors[0];
    }

    private static Color GenerateColor2(Color[] colors)
    {
        var i = Mathf.Min(1, colors.Length - 1);
        var sortedColors = SortColors(colors);
        return sortedColors[i];
    }

    private static Color GenerateColor3(Color[] colors)
    {
        var i = Mathf.Min(2, colors.Length - 1);
        var sortedColors = SortColors(colors);
        return sortedColors[i];
    }

    private static async Task GenerateActorPrompt(TextAsset asset, Actor actor, string note)
    {
        var prompt = asset.Format(actor.Title, actor.Pronouns, note);
        var output = await OpenAiIntegration.CompleteAsync(prompt, false);

        output = output.Replace("```", string.Empty).Trim();

        File.WriteAllText($"./Assets/Resources/Prompts/Actors/{actor.Name}.md", output);
    }

    private static async Task GenerateActorColorScheme(TextAsset asset, Actor actor)
    {
        var prompt = asset.Format(actor.Name);
        var output = await OpenAiIntegration.CompleteAsync(prompt, true);

        actor.ColorScheme = output.Find("Color Scheme");
        EditorUtility.SetDirty(actor);
    }
}
