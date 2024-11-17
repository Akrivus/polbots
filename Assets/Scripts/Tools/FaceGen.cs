using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FaceGen : MonoBehaviour
{
    [SerializeField]
    public Camera _camera;

    [SerializeField]
    public ActorController _controller;

    [SerializeField]
    public TextAsset _prompt;

    [SerializeField]
    public Actor[] selectActors;

    private Actor[] actors => Resources.LoadAll<Actor>("Actors");
    private Sentiment[] sentiments => Resources.LoadAll<Sentiment>("Faces");

    void Start()
    {
        //StartCoroutine(TakeScreenshots());
        //StartCoroutine(GenerateCharacterPrompts());
        //StartCoroutine(ShortenCharacterPrompts());
    }

    private IEnumerator ShortenCharacterPrompts()
    {
        var listOfActors = selectActors.Length > 0 ? selectActors : actors;
        foreach (var actor in listOfActors)
            yield return ShortenCharacterPrompt(actor);
    }

    private IEnumerator ShortenCharacterPrompt(Actor actor)
    {
        var textFile = $"Assets/Resources/Prompts/Countries/{actor.Name}.md";
        var text = File.ReadAllText(textFile);

        _controller.Context = new ActorContext(actor);

        yield return new WaitForFixedUpdate();

        var prompt = string.Format("Shorten the following personality profile about {0} ({1})," +
            "keeping it concise, information-dense, and no more than 1 paragraph.\n\n{2}", actor.Name, actor.Pronouns.Trim(), text);
        var task = ChatClient.CompleteAsync(prompt, true);
        yield return new WaitUntil(() => task.IsCompleted);

        var shortText = task.Result;

        File.WriteAllText(textFile, shortText);
        yield return null;
    }

    private IEnumerator GenerateCharacterPrompts()
    {
        var listOfActors = selectActors.Length > 0 ? selectActors : actors;
        foreach (var actor in listOfActors)
            yield return GenerateCharacterPrompt(actor);
    }

    private IEnumerator GenerateCharacterPrompt(Actor actor)
    {
        _controller.Context = new ActorContext(actor);

        yield return new WaitForFixedUpdate();

        var prompt = _prompt.Format(actor.Name, actor.Pronouns.Trim());
        var task = ChatClient.CompleteAsync(prompt, true);
        yield return new WaitUntil(() => task.IsCompleted);

        var response = task.Result;

        var textFile = $"Assets/Resources/Prompts/Countries/{actor.Name}.md";
        File.WriteAllText(textFile, response);
    }

    private IEnumerator TakeScreenshots()
    {
        var listOfActors = selectActors.Length > 0 ? selectActors : actors;
        foreach (var sen in sentiments)
            foreach (var actor in listOfActors)
                yield return TakeScreenshot(actor, sen);
    }

    private IEnumerator TakeScreenshot(Actor actor, Sentiment sentiment)
    {
        var context = new ActorContext(actor);
        context.Sentiment = sentiment;
        _controller.Context = new ActorContext(actor);
        _controller.Sentiment = sentiment;

        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        var texture = new RenderTexture(256, 256, 16);

        _camera.targetTexture = texture;
        _camera.Render();

        RenderTexture.active = texture;

        var screenshot = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        screenshot.Apply();

        RenderTexture.active = null;

        _camera.targetTexture = null;

        Destroy(texture);

        var bytes = screenshot.EncodeToPNG();
        var path = $"WWW/{sentiment.Name}-{actor.Name.ToFileSafeString()}.png";
        File.WriteAllBytes(path, bytes);

        Destroy(screenshot);
    }
}
