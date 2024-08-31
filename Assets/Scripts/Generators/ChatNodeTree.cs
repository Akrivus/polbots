using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ChatNodeTree : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI subtitles;

    [SerializeField]
    private List<ChatNode> nodes;

    public void Add(ChatNode node)
    {
        if (nodes == null)
            nodes = new List<ChatNode>();
        nodes.Add(node);
    }

    public IEnumerator Play()
    {
        foreach (var node in nodes)
            yield return Activate(node);
        nodes.Clear();
    }

    private IEnumerator Activate(ChatNode node)
    {
        if (node.Controller == null)
            yield break;

        if (!node.Controller.IsActive)
            yield return node.Controller.ShowEntrance();

        subtitles.color = node.Controller.Color;
        subtitles.text = node.Text;

        yield return node.Activate();

        subtitles.text = "";
    }
}

[Serializable]
public class ChatNode
{
    public static readonly Regex regex = new Regex(@"^\s*([*(\[]([^[\])*]+)[\])*])");

    public CountryController Controller;
    public string Name;
    public string Action;
    public string Text;
    public AudioClip VoiceLine;

    [HideInInspector]
    public Dictionary<CountryController, Face> Reactions;

    public ChatNode(CountryManager CountryManager, StoreNode node)
    {
        node.Sync(CountryManager);
        Controller = node.Controller;
        Name = node.Name;

        Action = regex.Match(node.Text).Groups[1].Value;
        Text = regex.Replace(node.Text, " ");
        VoiceLine = node.VoiceLine;

        Reactions = node.Reactions
            .Where(node => CountryManager.Has(node.Key))
            .Select(node => new KeyValuePair<CountryController, Face>(
                CountryManager.Get(node.Key),
                node.Value))
            .ToDictionary(n => n.Key, n => n.Value);
    }

    public IEnumerator Activate()
    {
        yield return Controller.Activate(this);
    }
}

public class StoreNode
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

    public StoreNode()
    {
        Reactions = new Dictionary<string, Face>();
    }

    public StoreNode(string text, string name, Country country) : this()
    {
        Text = text;
        Name = name;
        Country = country;
    }

    public StoreNode Sync(CountryManager manager)
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

public class SearchNode
{
    public bool NewEpisode { get; set; }
    public string Title { get; set; }
    public string Vibe { get; set; }
    public string[] Countries { get; set; }

    public DateTime Time { get; set; }
    public float Duration { get; set; }
}