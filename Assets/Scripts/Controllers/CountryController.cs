using System.Collections;
using TMPro;
using UnityEngine;

public class CountryController : MonoBehaviour
{
    public string ISO3166 => country.ISO3166;
    public string Name => country.Name;
    public string Language => country.Language;
    public string Voice => country.Voice;
    public float SpeakingRate => country.SpeakingRate;
    public float Pitch => country.Pitch;

    public bool IsActive { get; private set; }

    public FaceController Face { get; private set; }
    public FlagController Flag { get; private set; }

    public Vector3 position { get; set; }
    public CountryManager manager { get; set; }

    [SerializeField]
    private AudioSource voice;

    [SerializeField]
    private TextMeshPro action;

    [SerializeField]
    private Country country;

    public void Hide()
    {
        transform.localPosition = position * 3f;
        IsActive = false;
    }

    public void Show()
    {
        transform.localPosition = position;
        IsActive = true;
    }

    public IEnumerator ShowEntrance()
    {
        IsActive = true;
        yield return new WaitForSeconds(1.2f);
    }

    public IEnumerator Activate(ChatNode node)
    {
        foreach (var Controller in node.Reactions.Keys)
            Controller.Face.SetFace(node.Reactions[Controller], transform);
        action.text = node.Action;

        if (!IsActive)
            yield return ShowEntrance();
        manager.CenterCamera();

        voice.clip = node.VoiceLine;
        voice.Play();
        yield return new WaitForSeconds(node.VoiceLine.length);
        action.text = "";
    }

    private void Awake()
    {
        Face = GetComponent<FaceController>();
        Flag = GetComponent<FlagController>();
    }

    private void Start()
    {
        if (IsActive)
            Show();
        else
            Hide();
    }

    private void Update()
    {
        var time = Time.time * 0.4f;
        var sin = Mathf.Abs(Mathf.Sin(time) * GetCurrentAmplitude()) * 0.5f;
        var d = Time.deltaTime * (IsActive ? 1 : 0);

        transform.localScale = Vector3.one + Vector3.forward * sin;
        transform.localPosition = new Vector3(
            Mathf.Lerp(transform.localPosition.x, position.x, d),
            transform.localScale.z - 1f, 0);

        Face.transform.localScale = new Vector3(
            1 / transform.localScale.x,
            1 / transform.localScale.y,
            1 / transform.localScale.z);
    }

    public void SetCountry(Country country)
    {
        this.country = country;
        Flag.Country = country;
    }

    private float GetCurrentAmplitude()
    {
        var size = 16;
        if (!voice.isPlaying || voice.clip == null
            || voice.timeSamples + size > voice.clip.samples)
            return 0;
        var samples = new float[size];
        voice.clip.GetData(samples, voice.timeSamples);

        var sum = 0f;
        for (var i = 0; i < samples.Length; i++)
            sum += Mathf.Abs(samples[i]);
        return sum / samples.Length;
    }
}