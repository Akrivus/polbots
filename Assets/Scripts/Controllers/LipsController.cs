using uLipSync;
using UnityEngine;

public class LipsController : AutoActor, ISubChats
{
    [SerializeField]
    private MeshRenderer lipsRenderer;

    [SerializeField]
    private uLipSyncTexture lipSync;

    private Sentiment sentiment;

    private void Update()
    {
        if (sentiment != ActorController.Sentiment)
            UpdateLips();
    }

    private void UpdateLips()
    {
        sentiment = ActorController.Sentiment;
        lipsRenderer.material.mainTexture = sentiment.Lips;
    }

    public void Initialize(Chat chat)
    {
        var context = chat.Actors.Get(Actor);
        if (context != null)
            sentiment = context.Sentiment;
        UpdateLips();
    }
}
