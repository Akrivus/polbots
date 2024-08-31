using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class StoryQueue : MonoBehaviour
{
    public static StoryQueue Instance => _instance ??= FindObjectOfType<StoryQueue>();
    private static StoryQueue _instance;

    public event Action<SearchNode> OnQueueAdded;
    public event Action<SearchNode> OnQueueTaken;

    public event Action OnQueueOpen;
    public event Action OnQueueClosed;

    public ChatNodeTree Chat { get; private set; }
    public CountryManager CountryManager { get; private set; }
    public StoryGenerator Generator { get; private set; }
    public bool CanChatSuggestTopics { get; set; }

    [SerializeField]
    private TextMeshProUGUI titleCard;

    [SerializeField]
    private TextMeshProUGUI titleName;

    [SerializeField]
    private TextMeshProUGUI premierUI;

    [SerializeField]
    private MusicStateMachine music;

    [SerializeField]
    private int maxFileCount = 1000;

    private ConcurrentQueue<Story> queue = new ConcurrentQueue<Story>();
    private Stopwatch stopwatch = new Stopwatch();
    private List<string> replays = new List<string>();

    private void Awake()
    {
        _instance = this;
        Generator = GetComponent<StoryGenerator>();
        CountryManager = GetComponent<CountryManager>();
        Chat = GetComponent<ChatNodeTree>();
    }

    private void Start()
    {
        ReplayEpisode();
        StartCoroutine(PlayQueue());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Generate(string story)
    {
        Generator.GenerateIdea(story);
    }

    public void AddStoryToQueue(Story story)
    {
        queue.Enqueue(story);
        OnQueueAdded(story.ToSearchNode());
    }

    private IEnumerator PlayQueue()
    {
        yield return new WaitForSeconds(1);

        OnQueueOpen();

        if (queue.Count == 0 && CanChatSuggestTopics)
        {
            titleCard.text = "Suggest topics on Discord.\nhttps://discord.gg/x7UEjRrudA";
            yield return new WaitForSeconds(30);
        }

        yield return WaitForQueue();

        if (queue.TryDequeue(out var story))
        {
            yield return Interstitial.Activate();

            premierUI.enabled = story.NewEpisode;
            titleCard.text = story.Title;
            yield return new WaitForSeconds(1);

            OnQueueClosed();
            yield return PlayStory(story);
            OnQueueTaken(story.ToSearchNode());
        }
        
        yield return PlayQueue();
    }

    public IEnumerator PlayStory(Story story)
    {
        var countries = story.Countries.Select((n) => CountryManager[n]).ToArray();
        CountryManager.SpawnCountries(countries);

        var nodes = story.Nodes.Take(countries.Length);
        for (int i = 0; i < nodes.Count(); i++)
        {
            var node = nodes.ElementAt(i);
            var controller = CountryManager.controllers[i];

            if (controller.Name != node.Name)
                break;
            controller.Show();
        }

        foreach (var node in story.Nodes)
            Chat.Add(new ChatNode(CountryManager, node));

        CountryManager.CenterCamera(true);

        titleCard.text = "";
        titleName.text = story.Title;
        music.PlayVibe(story.Vibe);

        yield return Chat.Play();
        yield return new WaitForSeconds(1);

        titleName.text = "";

        CountryManager.DespawnCountries();
    }

    private IEnumerator WaitForQueue(int rollover = 0)
    {
        var dots = new string('.', rollover % 4 + 1);

        stopwatch.Start();

        titleCard.text = $"Generating{dots}";
        for (int i = 0; i < rollover; i++)
            ReplayOrPremierEpisode();

        yield return new WaitUntil(() => queue.Count > 0
            || stopwatch.Elapsed.TotalSeconds > 1);

        stopwatch.Reset();

        if (queue.Count == 0)
            yield return WaitForQueue(++rollover);
    }

    public void ReplayEpisode()
    {
        var story = Story.Load();
        if (story == null)
            return;

        var count = Story.GetStoryCount();

        if (replays.Count > count * 0.2f)
            replays.RemoveAt(0);
        if (replays.Contains(story.Title))
            story = Story.Load();
        replays.Add(story.Title);

        AddStoryToQueue(story);
    }

    public void PremierEpisode()
    {
        StoryProducer.Instance.ProduceIdea();
    }

    public void ReplayOrPremierEpisode()
    {
        var count = Story.GetStoryCount();
        var odds = UnityEngine.Random.Range(0, maxFileCount);

        if (odds > count)
            PremierEpisode();
        else
            ReplayEpisode();
    }
}