using UnityEngine;

public class FlagController : AutoActor, ISubActor
{
    [HideInInspector]
    public Color Color = Color.white;

    [SerializeField]
    private MeshRenderer flagRenderer;
    private Texture2D flagTexture;

    private Texture2D LoadTexture(string name)
    {
        flagTexture = Resources.Load<Texture2D>($"Actors/{name}");
        if (flagTexture)
            Color = GenerateColor(flagTexture.GetPixels());
        return flagTexture;
    }

    private Color GenerateColor(Color[] colors)
    {
        var color = Color.black;
        for (var i = 0; i < colors.Length; ++i)
            color += colors[i];
        color /= colors.Length;
        color.a = 1f;
        return color;
    }

    public void UpdateActor(Actor actor, ActorContext context)
    {
        LoadTexture(actor.Name);
        actor.Color = Color;
        flagRenderer.material.mainTexture = flagTexture;
    }
}
