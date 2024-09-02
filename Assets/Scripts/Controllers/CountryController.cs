using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountryController : MonoBehaviour
{
    public event Action<ChatNode> OnActivate;

    public Color Color => country.Color;
    public string Name { get; set; }

    public bool IsActive { get; private set; }

    public FaceController Face { get; private set; }
    public FlagController Flag { get; private set; }

    public Vector3 position { get; set; }
    public CountryManager manager { get; set; }

    [SerializeField]
    private AudioSource voice;

    [SerializeField]
    private TextMeshPro actionLabel;

    [SerializeField]
    private TextMeshPro nameLabel;

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
        yield return new WaitForSeconds(1.5f);
    }

    public IEnumerator Activate(ChatNode node)
    {
        foreach (var Controller in node.Reactions.Keys)
            Controller.SetFace(node.Reactions[Controller], transform);
        Name = node.Name;
        actionLabel.text = node.Action;
        OnActivate(node);

        voice.clip = node.VoiceLine;
        voice.Play();
        yield return new WaitForSeconds(node.VoiceLine.length);
        manager.CenterCamera();
        actionLabel.text = "";
    }

    private void SetFace(Face face, Transform target)
    {
        if (Face.Face != face)
            IsActive = true;
        Face.SetFace(face, target);
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
        var time = Time.time * 0.4f + position.magnitude;
        var sin = Mathf.Abs(Mathf.Sin(time) * GetCurrentAmplitude()) * 0.5f;
        var d = Time.deltaTime * (IsActive ? 1 : 0);

        var hover = Mathf.Sin(time * 0.4f) * 0.1f;

        transform.localScale = country.Scale + Vector3.forward * sin;
        transform.localPosition = new Vector3(
            Mathf.Lerp(transform.localPosition.x, position.x, d),
            transform.localScale.z - 1f + hover, 0);

        Face.transform.localScale = new Vector3(
            1 / transform.localScale.x,
            1 / transform.localScale.y,
            1 / transform.localScale.z);

        nameLabel.text = Name;
    }

    public void SetCountry(Country country, string name)
    {
        this.name = name;
        this.country = country;
        Flag.Country = country;
        Name = name;
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

    public bool Equals(string name)
    {
        if (Name == name) return true;
        foreach (var alias in country.Aliases)
            if (alias == name) return true;
        return false;
    }
}