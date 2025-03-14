using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utilities.WebRequestRest;

public static class ActorTeamGenerator
{
    [MenuItem("Tools/Transfer Map Names")]
    public static void TransferMapNames()
    {
        var actors = Resources.LoadAll<Actor>("Actors");
        foreach (var actor in actors)
        {
            if (actor.Costume.StartsWith(":"))
                actor.MapName = actor.Name;
            else
                actor.IsLegacy = true;
            EditorUtility.SetDirty(actor);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

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
                await GenerateActorPrompt(asset, actor, note, "");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Refine Character Prompts")]
    public static async void RefineActorPrompts()
    {
        var asset = Resources.Load<TextAsset>($"Prompts/Tools/Actor Refinition");
        var actors = Resources.LoadAll<Actor>("Actors");

        var csv = File.ReadAllLines("./Assets/Resources/Actors.csv");
        for (var i = 1; i < csv.Length; ++i)
        {
            var columns = csv[i].Split(',');

            var actor = actors.FirstOrDefault(x => x.Name == columns[0]);
            var note = string.Join(",", columns.Skip(1));

            if (actor)
                await GenerateActorPrompt(asset, actor, actor.Prompt.text, note);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Generate Character Backgrounds")]
    public static async void GenerateActorBackgrounds()
    {
        var asset = Resources.Load<TextAsset>($"Prompts/Tools/Actor Backgrounds");
        var actors = Resources.LoadAll<Actor>("Actors");

        foreach (var actor in actors)
            if (actor.Prompt == null)
                Debug.LogWarning($"Actor {actor.Name} has no personality.");
            else if (!File.Exists($"./Assets/Resources/Backgrounds/{actor.Name}.png"))
                await GenerateActorBackground(asset, actor);
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

    [MenuItem("Tools/Randomize Actor Voice")]
    public static void RandomizeActorVoice()
    {
        var actors = Resources.LoadAll<Actor>("Actors");

        foreach (var actor in actors)
        {
            actor.SpeakingRate = 1;
            actor.Volume = 1;
            actor.Pitch = UnityEngine.Random.Range(100f, 111f) / 100f;
            EditorUtility.SetDirty(actor);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Shorten Actor Teams")]
    public static void ShortenActorTeams()
    {
        var actors = Resources.LoadAll<Actor>("Actors");
        foreach (var actor in actors)
        {
            actor.Players = actor.Players.Select(x => x.Split(' ').First()).ToArray();
            EditorUtility.SetDirty(actor);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Fix Actor Costumes")]
    public static void FixActorCostumes()
    {
        var actors = Resources.LoadAll<Actor>("Actors");
        foreach (var actor in actors)
        {
            actor.Costume = $":{actor.Costume}:";
            EditorUtility.SetDirty(actor);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static async Task GenerateActorTeam(TextAsset asset, Actor actor)
    {
        if (actor.Players.Length < 11)
        {
            var prompt = asset.Format(actor.Title);
            var output = await LLM.CompleteAsync(prompt, true);

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

    private static async Task GenerateActorBackground(TextAsset asset, Actor actor, bool retried = false)
    {
        try
        {
            var metaprompt = asset.Format(actor.Name, actor.Pronouns, actor.Prompt.text);
            var prompt = await LLM.CompleteAsync(metaprompt, false);

            prompt = prompt.Replace("```", string.Empty).Trim();

            // write prompt to file
            File.WriteAllText($"./Assets/Resources/Prompts/Backgrounds/{actor.Name}.md", prompt);

            try
            {
                var request = await LLM.API.ImagesEndPoint.GenerateImageAsync(
                    new OpenAI.Images.ImageGenerationRequest(prompt, model: "dall-e-3", size: "1792x1024"));
                var image = request.First();

                // save the texture to resources
                var texture = image.Texture;
                File.WriteAllBytes($"./Assets/Resources/Backgrounds/{actor.Name}.png", texture.EncodeToPNG());
            }
            catch (RestException e)
            {
                if (e.Response.Code == 400 || !retried) // usually means the prompt triggered a safety filter, so we try again
                    await GenerateActorBackground(asset, actor, true);
                else
                    Debug.LogError($"Failed for {actor.Name}.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
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
        colors = SortColors(colors);
        if (colors.Length < 1)
            return Color.black;
        return colors[0];
    }

    private static Color GenerateColor2(Color[] colors)
    {
        colors = SortColors(colors);
        if (colors.Length < 2)
            return GenerateColor1(colors);
        var i = Mathf.Min(1, colors.Length - 1);
        return colors[i];
    }

    private static Color GenerateColor3(Color[] colors)
    {
        colors = SortColors(colors);
        if (colors.Length < 3)
            return GenerateColor2(colors);
        var i = Mathf.Min(2, colors.Length - 1);
        return colors[i];
    }

    private static async Task GenerateActorPrompt(TextAsset asset, Actor actor, string text, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            note = "No additional notes, go wild!";
        var prompt = asset.Format(actor.Name, actor.Pronouns, text, note);
        var output = await LLM.CompleteAsync(prompt, false);

        output = output.Replace("```", string.Empty).Trim();

        File.WriteAllText($"./Assets/Resources/Prompts/Actors/{actor.Name}.md", output);
    }

    private static async Task GenerateActorColorScheme(TextAsset asset, Actor actor)
    {
        var prompt = asset.Format(actor.Name);
        var output = await LLM.CompleteAsync(prompt, true);

        actor.ColorScheme = output.Find("Color Scheme");
        EditorUtility.SetDirty(actor);
    }
}
