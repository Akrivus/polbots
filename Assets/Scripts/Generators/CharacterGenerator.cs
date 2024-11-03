using System.Linq;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour, ISubGenerator.Sync
{
    public Chat Generate(Chat chat)
    {
        chat.Actors = chat.Nodes
            .Select(node => node.Actor)
            .Distinct()
            .OfType<Actor>()
            .ToArray();
        chat.Contexts = chat.Actors
            .Select(actor => new ActorContext(actor))
            .ToArray();
        return chat;
    }
}