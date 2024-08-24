using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class StoryQueue : MonoBehaviour
{
    public static StoryQueue Instance => _instance ??= FindObjectOfType<StoryQueue>();
    private static StoryQueue _instance;

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
    private MusicStateMachine music;

    private ConcurrentQueue<Story> queue = new ConcurrentQueue<Story>();
    private Stopwatch stopwatch = new Stopwatch();

    private void Awake()
    {
        _instance = this;
        Generator = GetComponent<StoryGenerator>();
        CountryManager = GetComponent<CountryManager>();
        Chat = GetComponent<ChatNodeTree>();
    }

    private void Start()
    {
        Story.LoadAndPlay();
        StartCoroutine(PlayQueue());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Generate(string story)
    {
        Generator.AddIdeaToQueue(story);
    }

    public void AddStoryToQueue(Story story)
    {
        queue.Enqueue(story);
    }

    private IEnumerator PlayQueue()
    {
        yield return new WaitForSeconds(1);

        OnQueueOpen();

        if (queue.Count == 0 && CanChatSuggestTopics)
        {
            titleCard.text = "suggest topics in the chat";
            yield return new WaitForSeconds(30);
        }

        yield return WaitForQueue();

        if (queue.TryDequeue(out var story))
        {
            yield return Interstitial.Activate();
            titleCard.text = story.Title;
            OnQueueClosed();
            yield return PlayStory(story);
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
        var dots = new string('.', rollover % 5);

        stopwatch.Start();

        titleCard.text = $"generating{dots}";
        Story.LoadOrGenerate();

        yield return new WaitUntil(() => queue.Count > 0
            || stopwatch.Elapsed.TotalSeconds > 5);

        stopwatch.Reset();

        if (queue.Count == 0)
            yield return WaitForQueue(++rollover);
    }
}