using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour, ISubGenerator
{
    [SerializeField]
    private TextAsset _prompt;

    public async Task<Chat> Generate(Chat chat)
    {
        var topic = chat.Topic.FindAll("Background Information");
        if (topic.Length > 0)
            chat.Topic = topic[0];
        var prompt = _prompt.Format(topic);
        var messages = await ChatClient.ChatAsync(prompt, false);
        prompt = messages[1].Content.ToString();
        var request = await ChatClient.API.ImagesEndPoint.GenerateImageAsync(
            new OpenAI.Images.ImageGenerationRequest(prompt, model: "dall-e-3", size: "1792x1024"));
        var image = request.First();
        
        chat.Background = image.Texture;

        return chat;
    }
}
