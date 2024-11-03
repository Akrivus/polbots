using Newtonsoft.Json;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.IO;
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

    public void Save()
    {
        if (!_locked) return;

        var uuid = Guid.NewGuid().ToString();
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);

        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var folder = Path.Combine(docs, "PolBol");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        folder = Path.Combine(folder, $"{uuid}.json");

        File.WriteAllText(folder, json);
    }

    public static Chat Load(string uuid)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, "PolBol", $"{uuid}.json");
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Chat>(json);
    }
}
