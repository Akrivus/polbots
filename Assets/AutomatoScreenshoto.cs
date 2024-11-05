using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AutomatoScreenshoto : MonoBehaviour
{
    [SerializeField]
    public Camera _camera;

    [SerializeField]
    public ActorController _controller;

    private Actor[] actors => Resources.LoadAll<Actor>("Actors");
    private Sentiment[] sentiments => Resources.LoadAll<Sentiment>("Faces");

    void Start()
    {
        StartCoroutine(TakeScreenshots());
    }

    private IEnumerator TakeScreenshots()
    {
        foreach (var sen in sentiments)
            foreach (var actor in actors)
                yield return TakeScreenshot(actor, sen);
    }

    private IEnumerator TakeScreenshot(Actor actor, Sentiment sentiment)
    {
        var context = new ActorContext(actor);
        context.Sentiment = sentiment;
        _controller.Context = new ActorContext(actor);
        _controller.Sentiment = sentiment;

        yield return new WaitForFixedUpdate();

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
