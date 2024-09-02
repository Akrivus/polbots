using UnityEngine;

public class Country
{
    public string Language => "en-US";
    public float Size => 1f;
    public Vector3 Scale => new Vector3(Size, Size, Size);

    public string Name;
    public string Voice;
    public string[] Aliases;

    public float SpeakingRate;
    public float Pitch;
    public Color Color = Color.white;

    public Texture2D Texture => LoadTexture();
    private Texture2D _texture;

    public Country(string row)
    {
        var columns = row.Split(',');
        Name = columns[0];
        Voice = columns[1];
        Aliases = columns[2].Split('/');
    }

    private Texture2D LoadTexture()
    {
        if (_texture) return _texture;
        _texture = Resources.Load<Texture2D>($"country-flag/{Name}");
        if (_texture)
            Color = GenerateColor(_texture.GetPixels());
        return _texture;
    }

    private Color GenerateColor(Color[] colors)
    {
        var color = Color.black;
        for (var i = 0; i < colors.Length; ++i)
            color += colors[i];
        color /= colors.Length;
        color += Color.white;
        color /= 2;
        return color;
    }

    public bool Equals(string name)
    {
        if (Name == name) return true;
        foreach (var alias in Aliases)
            if (alias == name) return true;
        return false;
    }
}