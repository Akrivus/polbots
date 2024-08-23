using OpenAI.Chat;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System.Net;

public class StoryProducer : MonoBehaviour
{
    public static StoryProducer Instance => _instance ??= FindObjectOfType<StoryProducer>();
    private static StoryProducer _instance;

    [SerializeField]
    private CountryManager CountryManager;

    [SerializeField]
    private YoutubeScanner Youtube;

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingStories;

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingPrompts;

    private TcpListener socket;

    private void Awake()
    {
        _instance = this;
        if (!Youtube) return;
        Youtube.OnMessage += ProduceStory;
        Youtube.Register();
    }

    private void Start()
    {
        socket = new TcpListener(IPAddress.Any, 22450);
        socket.Start();
        StartCoroutine(ListenForIdeas());
    }

    public void ProduceStory(string message)
    {
        StartCoroutine(GenerateStory(message));
    }

    public void ProduceStory()
    {
        StartCoroutine(GenerateRandomStory());
    }

    private IEnumerator GenerateRandomStory()
    {
        var country = CountryManager.countries[Random.Range(0, CountryManager.countries.Length)].Name;
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
        var names = CountryManager.countries.Select(c => c.Name).ToArray();
        var prompt = string.Format(PromptForGeneratingStories,
            string.Join(", ", names), message);

        var task = ApiKeys.API.ChatEndpoint.GetCompletionAsync(new ChatRequest(new List<Message>
        {
            new Message(Role.System, prompt)
        }, "gpt-4o"));
        
        yield return new WaitFor(task);

        if (task.IsFaulted)
            yield break;

        StoryQueue.Instance.Generate(task.Result.FirstChoice.Message.Content.ToString());
    }

    private IEnumerator ListenForIdeas()
    {
        yield return new WaitUntil(() => socket.Pending());

        using var client = socket.AcceptTcpClient();
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);

        var task = reader.ReadToEndAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
            yield return ListenForIdeas();

        var message = task.Result;
        GenerateStory(message);

        yield return ListenForIdeas();
    }

    private void OnApplicationQuit()
    {
        if (socket != null)
            socket.Stop();
    }
}