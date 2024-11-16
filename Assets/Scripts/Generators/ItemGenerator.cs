using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ItemGenerator : MonoBehaviour, ISubGenerator
{
    public static string[] ItemNames;

    [SerializeField]
    private TextAsset _prompt;

    private string context;

    private void Awake()
    {
        if (ItemNames == null)
            ItemNames = Resources.LoadAll("Items", typeof(Texture2D))
                .Select(t => t.name)
                .ToArray();
    }

    public async Task<Chat> Generate(Chat chat)
    {
        var names = chat.Headline.Names;
        var topic = chat.Headline.Topic;

        context = "";
        foreach (var node in chat.Nodes)
            context += string.Format("{0}: {1}\n", node.Actor.Name, node.Line);

        var itemSet = await GenerateItemSet(chat, names, topic);
        foreach (var item in itemSet)
            chat.Actors.Get(item.Key.Actor).Item = item.Value;

        return chat;
    }

    private async Task<Dictionary<ActorContext, string>> GenerateItemSet(Chat chat, string[] names, string topic)
    {
        var options = string.Join("\n- ", names);
        var prompt = _prompt.Format(options, topic, context);
        var messages = await ChatClient.ChatAsync(prompt, true);
        var message = messages[1];

        var lines = message.Content.ToString().Parse(names);

        return lines
            .Where(line => names.Contains(line.Key))
            .ToDictionary(
                line => chat.Actors.Get(line.Key),
                line => line.Value);
    }
}
