using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Story
{
    public string Title { get; set; }
    public string[] Countries { get; set; }
    public List<StoreNode> Nodes { get; set; }
    public string Vibe { get; set; }

    [JsonIgnore]
    public bool NewEpisode { get; set; }

    [JsonIgnore]
    public float Duration => Nodes.Select(n => n.Speech?.Length ?? 0 / 96000).Sum();

    public Story Save()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "PolBol");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path = Path.Combine(path, $"{GetFileName(Title)}.json");

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);

        return this;
    }

    public SearchNode ToSearchNode()
    {
        return new SearchNode
        {
            NewEpisode = NewEpisode,
            Duration = Duration,
            Title = Title,
            Vibe = Vibe,
            Countries = Countries,
        };
    }

    private string GetFileName(string title)
    {
        if (string.IsNullOrEmpty(title))
            title = "Polbots #" + UnityEngine.Random.Range(1000, 9999);
        title = title.ToLower().Replace(' ', '-');
        title = new string(title.ToCharArray().Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
        title = $"polbot-{title}";
        return title;
    }

    private static string GetFilePath(string title)
    {
        if (!title.StartsWith("polbot-"))
            title = $"polbot-{title.ToLower().Replace(' ', '-')}";

        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "PolBol");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return Path.Combine(path, $"{title}.json");
    }

    private static string GetFile(string title)
    {
        var path = GetFilePath(title);
        if (!File.Exists(path))
            return null;

        return File.ReadAllText(path);
    }

    public static Story Load()
    {
        var titles = GetStories().ToArray();
        var title = titles[UnityEngine.Random.Range(0, titles.Length)];
        return Load(title);
    }

    public static Story Load(string title)
    {
        return JsonConvert.DeserializeObject<Story>(GetFile(title));
    }

    public static bool Exists(string title)
    {
        return File.Exists(GetFilePath(title));
    }

    public static IEnumerable<string> GetStories()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "PolBol");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return Directory.GetFiles(path, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f));
    }

    public static int GetStoryCount()
    {
        return GetStories().Count();
    }

    public static string[] GetStories(string search, int max = 10)
    {
        search = search.ToLower();
        return GetStories()
            .Select(t => t.Replace('_', ' '))
            .OrderBy(t => ByDistance(search, t))
            .Take(max)
            .ToArray();
    }

    public static int ByDistance(string source, string target, int threshold = 3)
    {
        int length1 = source.Length;
        int length2 = target.Length;

        if (Math.Abs(length1 - length2) > threshold)
            return threshold;

        if (length1 > length2)
        {
            Swap(ref target, ref source);
            Swap(ref length1, ref length2);
        }

        int maxi = length1;
        int maxj = length2;

        int[] dCurrent = new int[maxi + 1];
        int[] dMinus1 = new int[maxi + 1];
        int[] dMinus2 = new int[maxi + 1];
        int[] dSwap;

        for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

        int jm1 = 0;

        for (int j = 1; j <= maxj; j++)
        {
            dSwap = dMinus2;
            dMinus2 = dMinus1;
            dMinus1 = dCurrent;
            dCurrent = dSwap;
            dCurrent[0] = j;

            int minDistance = int.MaxValue;
            int im1 = 0;
            int im2 = -1;

            for (int i = 1; i <= maxi; i++)
            {

                int cost = source[im1] == target[jm1] ? 0 : 1;

                int del = dCurrent[im1] + 1;
                int ins = dMinus1[i] + 1;
                int sub = dMinus1[im1] + cost;

                int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                    min = Math.Min(min, dMinus2[im2] + cost);

                dCurrent[i] = min;
                if (min < minDistance)
                    minDistance = min;
                im1++;
                im2++;
            }
            jm1++;
            if (minDistance > threshold)
                return threshold;
        }

        return dCurrent[maxi];
    }

    private static void Swap<T>(ref T arg1, ref T arg2)
    {
        T temp = arg1;
        arg1 = arg2;
        arg2 = temp;
    }
}