using UnityEngine;

[CreateAssetMenu(fileName = "Country", menuName = "Country")]
public class Country : ScriptableObject
{
    public string ISO3166;
    public string Name;

    [Range(0, 2)]
    public float Size = 1f;

    [Header("Voice Settings")]
    public string Language = "en-US";
    public string Voice;
    public float SpeakingRate;
    public float Pitch;

    public Vector3 Scale => new Vector3(Size, Size, Size);
    public Texture Flag => Resources.Load<Texture>($"Flags/{ISO3166}.png");
}