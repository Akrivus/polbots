using OpenAI.Chat;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
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
    private string Prompt;

    [SerializeField, TextArea]
    private string[] Ideas;

    [SerializeField]
    private bool useSocket = true;

    [SerializeField]
    private int port = 12345;

    private TcpListener socket;

    private void Awake()
    {
        if (!Youtube) return;
        Youtube.OnMessage += ProduceStory;
        Youtube.Register();
    }

    private void Start()
    {
        if (!useSocket) return;
        socket = new TcpListener(IPAddress.Any, port);
        socket.Start();
        StartCoroutine(ListenForIdeas());
    }

    public void ProduceRandomStory()
    {
        ApiKeys.API.ChatEndpoint.GetCompletionAsync(new ChatRequest(new List<Message>
        {
            new Message(Role.System, "Write a short, one-sentence episode synopsis " +
                "for a sitcom about geopolitics where the characters are countries. " +
                "Focus on the humor and the relationships between the countries. " +
                "\n\n" +
                "Random Idea:\n- " + Ideas[Random.Range(0, Ideas.Length)])
        }, "gpt-4o")).ContinueWith(GenerateStory);
    }

    private void GenerateStory(Task<ChatResponse> task)
    {
        ProduceStory(task.Result.FirstChoice.Message.Content.ToString());
    }

    public void ProduceStory(string message)
    {
        var names = CountryManager.countries.Select(c => c.Name).ToArray();
        var prompt = string.Format(Prompt, string.Join(", ", names));

        Debug.Log(message);

        var task = ApiKeys.API.ChatEndpoint.GetCompletionAsync(new ChatRequest(new List<Message>
        {
            new Message(Role.System, prompt),
            new Message(Role.User, message)
        }, "gpt-4o")).ContinueWith(GenerateDialogue);
    }

    private void GenerateDialogue(Task<ChatResponse> task)
    {
        StoryQueue.Instance.Generate(task.Result.FirstChoice.Message.Content.ToString());
    }

    private IEnumerator ListenForIdeas()
    {
        yield return new WaitUntil(() => socket.Pending());

        using var client = socket.AcceptTcpClient();
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);

        var task = reader.ReadToEndAsync();
        yield return new WaitUntilTaskCompleted(task);

        if (task.IsFaulted)
            yield return ListenForIdeas();

        var message = task.Result;
        ProduceStory(message);

        yield return ListenForIdeas();
    }

    private void OnApplicationQuit()
    {
        if (socket != null)
            socket.Stop();
    }
}