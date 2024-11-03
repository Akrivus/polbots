using System;
using System.Collections;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public event Action<ChatNode> OnActivation;
    public event Action<ActorController> OnActorUpdate;
    public event Action<Sentiment> OnSentimentUpdate;

    public float TotalVolume => voice.GetAmplitude() + sound.GetAmplitude();
    public float VoiceVolume => voice.GetAmplitude();
    public bool IsTalking => voice.isPlaying;

    public AudioSource Voice => voice;
    public AudioSource Sound => sound;

    public bool IsInitial { get; private set; }

    public Color TextColor;

    [SerializeField]
    private AudioSource voice;

    [SerializeField]
    private AudioSource sound;

    public ActorContext Context { get; set; }
    public Actor Actor => Context.Actor;

    public Sentiment Sentiment
    {
        get => _sentiment;
        set
        {
            _sentiment = value;
            OnUpdateSentimentCallbacks();
        }
    }

    private Sentiment _sentiment;

    private ISubActor[] sub_Actor;
    private ISubSentiment[] sub_Sentiment;
    private ISubNode[] sub_Nodes;
    private ISubChats[] sub_Chats;
    private ISubExits[] sub_Exits;

    private void Awake()
    {
        sub_Actor = GetComponents<ISubActor>();
        sub_Sentiment = GetComponents<ISubSentiment>();
        sub_Nodes = GetComponents<ISubNode>();
        sub_Chats = GetComponents<ISubChats>();
        sub_Exits = GetComponents<ISubExits>();
    }

    private void Start()
    {
        _sentiment = Actor ? Actor.DefaultSentiment : SentimentConverter.Convert("Neutral");
        foreach (var sub in sub_Chats)
            sub.Initialize(ChatManager.Instance.NowPlaying);
        OnUpdateActorCallbacks();
    }

    public void OnUpdateActorCallbacks()
    {
        if (Actor == null) return;
        foreach (var subActor in sub_Actor)
            subActor.UpdateActor(Actor, Context);
        OnActorUpdate?.Invoke(this);
    }

    public void OnUpdateSentimentCallbacks()
    {
        if (Sentiment == null) return;
        foreach (var sub in sub_Sentiment)
            sub.UpdateSentiment(Sentiment);
        OnSentimentUpdate?.Invoke(Sentiment);
    }

    public IEnumerator Activate(ChatNode node)
    {
        yield return new WaitForSeconds(node.Delay);

        OnActivation?.Invoke(node);
        foreach (var subNode in sub_Nodes)
            subNode.Activate(node);

        var clip = node.AudioClip;
        voice.clip = clip;
        voice.Play();

        if (!node.Async)
        {
            var delay = clip.length * voice.pitch;
            yield return new WaitForSeconds(delay);
        }
    }

    public IEnumerator Deactivate()
    {
        foreach (var sub in sub_Exits)
            sub.Deactivate();
        Destroy(gameObject);
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
    }

    public IEnumerator Initialize()
    {
        if (IsInitial)
            yield break;
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
    }

    public ActorController SetInitial()
    {
        IsInitial = true;
        return this;
    }
}