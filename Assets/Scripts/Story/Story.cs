using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Story
{
    private static readonly MD5 md5 = MD5.Create();

    public string Title { get; set; }
    public string[] Countries { get; set; }
    public List<StoryNode> Nodes { get; set; }

    public void Save()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "PolBol");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        path = Path.Combine(path, $"{GetMD5Hash(Title)}.json");

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static Story Load()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "PolBol");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var files = Directory.GetFiles(path, "*.json");
        if (files.Length == 0)
            return null;

        // load random file from files
        var file = files[UnityEngine.Random.Range(0, files.Length)];
        var json = File.ReadAllText(file);
        return JsonConvert.DeserializeObject<Story>(json);
    }

    public static void LoadAndPlay()
    {
        var story = Load();
        if (story == null)
            return;

        StoryQueue.Instance.AddStoryToQueue(story);
    }

    public static void LoadOrGenerate()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        path = Path.Combine(path, "PolBol");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var files = Directory.GetFiles(path, "*.json");
        var odds = UnityEngine.Random.Range(0, 720);

        if (odds > files.Length)
            StoryProducer.Instance.ProduceStory();
        else
            LoadAndPlay();
    }

    private static string GetMD5Hash(string title)
    {
        var digest = Convert.ToBase64String(
            md5.ComputeHash(
                Encoding.UTF8.GetBytes(title)));
        return digest
            .Replace('/', '_')
            .Replace('+', '-')
            .Replace('=', '-');
    }
}

public class StoryNode
{
    public string Name { get; set; }
    public string Text { get; set; }
    public string Speech { get; set; }
    public Dictionary<string, Face> Reactions { get; set; }

    [JsonIgnore]
    public AudioClip VoiceLine => ByteToClip(Speech);

    [JsonIgnore]
    public Country Country;

    [JsonIgnore]
    public CountryController Controller;

    public StoryNode()
    {
        Reactions = new Dictionary<string, Face>();
    }

    public StoryNode(string text, string name, Country country) : this()
    {
        Text = text;
        Name = name;
        Country = country;
    }

    public StoryNode Sync(CountryManager manager)
    {
        Country = manager[Name];
        Controller = manager.Get(Name);
        return this;
    }

    private AudioClip ByteToClip(string data)
    {
        byte[] bytes = Convert.FromBase64String(data);
        float[] samples = BytesToFloats(bytes);
        var clip = AudioClip.Create("Speech", samples.Length, 1, 24000, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private float[] BytesToFloats(byte[] bytes)
    {
        float[] floats = new float[bytes.Length / 2];
        for (int i = 0; i < floats.Length; i++)
            floats[i] = BitConverter.ToInt16(bytes, i * 2) / 32768f;
        return floats;
    }
}