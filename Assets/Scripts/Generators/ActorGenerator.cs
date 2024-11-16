using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorGenerator : MonoBehaviour, ISubGenerator.Sync
{
    public Chat Generate(Chat chat)
    {
        var actors = new List<Actor>();
        foreach (var name in GetCharacterNames())
            if (chat.Topic.Contains(name.Key))
                actors.Add(name.Value);
        actors.AddRange(chat.Nodes.Select(node => node.Actor));

        chat.Actors = actors
            .Distinct()
            .OfType<Actor>()
            .Select(actor => new ActorContext(actor))
            .OfType<ActorContext>()
            .ToArray();
        return chat;
    }

    private static Dictionary<string, Actor> _;

    private Dictionary<string, Actor> GetCharacterNames()
    {
        return _ ??= Actor.All.List.ToDictionary(k => k.Name, v => v);
    }
}