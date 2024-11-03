using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LinearDialogueGenerator : MonoBehaviour, ISubGenerator
{
    [SerializeField]
    private TextAsset _prompt;

    public async Task<Chat> Generate(Chat chat)
    {
        var prompt = _prompt.Format(chat.Topic);
        var messages = await ChatClient.ChatAsync(prompt);

        var content = messages[1].Content.ToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(':');
            var name = parts[0];
            var text = string.Join(":", parts.Skip(1));

            var actor = ActorConverter.Find(name);
            if (actor != null)
                chat.Nodes.Add(new ChatNode(actor, text));
        }
        return chat;
    }
}
