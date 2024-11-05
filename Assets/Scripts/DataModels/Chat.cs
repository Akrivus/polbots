using Newtonsoft.Json;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Chat
{
    public string Topic { get; set; }
    public ActorContext[] Contexts { get; set; }
    public List<ChatNode> Nodes { get; set; }
    public Idea Idea { get; set; }
    public Headline Headline { get; set; }

    [JsonConverter(typeof(ActorConverter))]
    public Actor[] Actors { get; set; }

    public string BackgroundData { get; set; }

    [JsonIgnore]
    public Texture2D Background
    {
        get => BackgroundData.ToTexture2D();
        set => BackgroundData = value.ToBase64();
    }

    [JsonIgnore]
    public bool IsLocked => _locked;

    private bool _locked;

    [JsonIgnore]
    public List<Message> Messages { get; set; }

    [JsonIgnore]
    public string FileSafeName => Idea.Prompt.Split(':').First();

    [JsonIgnore]
    public string FileName => $"{DateTime.Now.ToString("dd-MM-yy")}-{FileSafeName.ToFileSafeString()}";

    public Chat(Idea idea)
    {
        Idea = idea;
        Actors = new Actor[0];
        Contexts = new ActorContext[0];
        Nodes = new List<ChatNode>();
        _locked = false;
    }

    public Chat()
    {
        _locked = true;
    }

    public void Lock()
    {
        _locked = true;
    }

    public Chat At(DateTime time)
    {
        Headline = Headline.At(time);
        return this;
    }

    public async void Save()
    {
        if (!_locked) return;

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);

        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var folder = Path.Combine(docs, "PolBol");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        folder = Path.Combine(folder, $"{FileName}.json");

        await File.WriteAllTextAsync(folder, json);
    }

    public static async Task<Chat> Load(string slug)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, "PolBol", $"{slug}.json");
        var json = await File.ReadAllTextAsync(path);
        return JsonConvert.DeserializeObject<Chat>(json);
    }

    public static bool FileExists(string name)
    {
        var slug = $"{DateTime.Now.ToString("dd-MM-yy")}-{name.ToFileSafeString()}";
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, "PolBol", $"{slug}.json");
        return File.Exists(path);
    }
}
