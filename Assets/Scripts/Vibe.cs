using UnityEngine;

[CreateAssetMenu(fileName = "Vibe", menuName = "Vibe")]
public class Vibe : ScriptableObject
{
    public string Key => key;
    public AudioClip Song => song;

    [SerializeField]
    private string key;

    [SerializeField]
    private AudioClip song;
}
