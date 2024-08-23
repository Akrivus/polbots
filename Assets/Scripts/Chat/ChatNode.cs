using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class ChatNode
{
    public static readonly Regex regex = new Regex(@"\s*([*(\[]([^[\])*]+)[\])*])\s*");

    public CountryController Controller;
    public string Name;
    public string Action;
    public string Text;
    public AudioClip VoiceLine;

    [HideInInspector]
    public Dictionary<CountryController, Face> Reactions;

    public ChatNode(CountryManager CountryManager, StoryNode node)
    {
        node.Sync(CountryManager);
        Controller = node.Controller;
        Name = node.Name;

        Action = regex.Match(node.Text).Groups[1].Value.ToLower();
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