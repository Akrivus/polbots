using System;
using UnityEngine;

public class Sentiment : ScriptableObject
{
    [Header("Sentiment")]
    public string Name;

    [Range(-1, 1)]
    public float Score;

    [Header("Appearance")]
    public Color Color;
    public Texture2D Eyes;
    public Texture2D Lips;
}
