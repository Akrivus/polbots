using System;
using UnityEngine;

public static class ColorExtensions
{
    public static Color Lerp(this Color color, Color target, float blend = 1f)
    {
        return Color.Lerp(color, target, blend);
    }

    public static Texture2D ToTexture2D(this string data)
    {
        var texture = new Texture2D(1, 1);

        if (string.IsNullOrEmpty(data))
            return texture;

        byte[] bytes = Convert.FromBase64String("" + data);
        texture.LoadImage(bytes);
        return texture;
    }

    public static string ToBase64(this Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        return Convert.ToBase64String(bytes);
    }
}