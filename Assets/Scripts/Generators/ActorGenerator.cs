using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorGenerator : MonoBehaviour, ISubGenerator.Sync
{
    public Chat Generate(Chat chat)
    {
        var actors = new List<Actor>();
        foreach (var actor in Actor.All.List)
            foreach (var alias in actor.Aliases)
                if (chat.Topic.Contains($"{alias}:") || chat.Topic.Contains($"{alias}**:"))
                    actors.Add(actor);
        actors.AddRange(chat.Nodes
            .Select(node => node.Actor)
            .Where(actor => !actors.Contains(actor)));

        chat.Actors = actors
            .Distinct()
            .OfType<Actor>()
            .Select(actor => new ActorContext(actor))
            .OfType<ActorContext>()
            .ToArray();
        return chat;
    }
}