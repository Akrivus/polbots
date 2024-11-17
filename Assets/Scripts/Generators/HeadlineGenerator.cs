using System.Linq;
using UnityEngine;

public class HeadlineGenerator : MonoBehaviour, ISubGenerator.Sync
{
    [SerializeField]
    private string title;

    public Chat Generate(Chat chat)
    {
        var actors = chat.Actors.Select(actor => actor.Name).ToArray();
        var duration = 0f;
        foreach (var node in chat.Nodes)
            duration += node.AudioClip.length;
        chat.Headline = new Headline
        {
            Title = chat.Topic.Find(title),
            Topic = chat.Context,
            Names = actors,
            Duration = duration
        };
        return chat;
    }
}
