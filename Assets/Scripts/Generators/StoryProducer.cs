using OpenAI.Chat;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections.Concurrent;

public class StoryProducer : MonoBehaviour
{
    public static StoryProducer Instance => _instance ??= FindObjectOfType<StoryProducer>();
    private static StoryProducer _instance;

    public CountryManager CountryManager { get; private set; }

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingStories;

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingPrompts;

    [SerializeField]
    private MusicStateMachine Music;

    private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

    public void ProduceIdea(string idea)
    {
        queue.Enqueue(idea);
    }

    public void ProduceIdea()
    {
        StartCoroutine(GenerateRandomIdea());
    }

    private void Awake()
    {
        CountryManager = GetComponent<CountryManager>();
        _instance = this;
        _countries = CountryManager.countries.OrderBy(c => Random.value).ToArray();
    }

    private void Start()
    {
        StartCoroutine(GenerateStories());
    }

    private IEnumerator GenerateStories()
    {
        yield return new WaitUntil(() => queue.Count > 0);

        if (queue.TryDequeue(out var idea) && !string.IsNullOrEmpty(idea))
            yield return GenerateStory(idea);

        yield return GenerateStories();
    }

    private Country[] _countries;
    private int _index = 0;

    private IEnumerator GenerateRandomIdea()
    {
        var country = _countries[_index++ % _countries.Length].Name;
        var task = ApiKeys.API.ChatEndpoint.GetCompletionAsync(new ChatRequest(new List<Message>
        {
            new Message(Role.System, string.Format(PromptForGeneratingPrompts, country))
        }, "gpt-4o"));

        yield return new WaitFor(task);

        if (task.IsFaulted)
            yield break;

        yield return GenerateStory(task.Result.FirstChoice.Message.Content.ToString());
    }

    private IEnumerator GenerateStory(string message)
    {
        var vibes = string.Join(", ", Music.Vibes.Select(v => v.Key).ToArray());
        var names = string.Join(", ", CountryManager.countries.Select(c => c.Name).ToArray());
        var prompt = string.Format(PromptForGeneratingStories, vibes, names, message);

        var task = ApiKeys.API.ChatEndpoint.GetCompletionAsync(new ChatRequest(new List<Message>
        {
            new Message(Role.System, prompt)
        }, "gpt-4o"));
        
        yield return new WaitFor(task);

        if (task.IsFaulted)
            yield break;

        StoryQueue.Instance.Generate(task.Result.FirstChoice.Message.Content.ToString());
    }
}