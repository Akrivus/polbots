using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TextureGenerator : MonoBehaviour, ISubGenerator
{
    [SerializeField]
    private TextAsset _prompt;

    public async Task<Chat> Generate(Chat chat)
    {
        var prompt = _prompt.Format(chat.Headline.Topic);
        var messages = await ChatClient.ChatAsync(prompt, false);
        prompt = messages[1].Content.ToString();
        var request = await ChatClient.API.ImagesEndPoint.GenerateImageAsync(
            new OpenAI.Images.ImageGenerationRequest(prompt, model: "dall-e-3", size: "1792x1024"));
        var image = request.First();
        
        chat.Texture = image.Texture;

        return chat;
    }
}
