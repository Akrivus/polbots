using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoryGenerator : MonoBehaviour
{
    public static StoryGenerator Instance => _instance ??= FindObjectOfType<StoryGenerator>();
    private static StoryGenerator _instance;

    private static List<string> ReservedHeaders = new List<string>
    {
        "Title", "Vibe", "Event", "Countries", "Scenario", "Resolution", "Dynamics", "Comedy"
    };

    public CountryManager CountryManager { get; private set; }
    public ChatNodeTree Chat { get; private set; }
    public TextToSpeechGenerator TextToSpeech { get; private set; }

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingDialogue;

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingFaces;

    private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

    public void GenerateIdea(string story)
    {
        queue.Enqueue(story);
    }

    private void Awake()
    {
        CountryManager = GetComponent<CountryManager>();
        Chat = GetComponent<ChatNodeTree>();
        TextToSpeech = GetComponent<TextToSpeechGenerator>();

        _instance = this;
    }

    private void Start()
    {
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        yield return new WaitUntil(() => queue.Count > 0);

        if (queue.TryDequeue(out var idea) && !string.IsNullOrEmpty(idea))
            yield return GenerateDialogue(idea);

        yield return Generate();
    }

    private IEnumerator GenerateDialogue(string prompt)
    {
        prompt = string.Format(PromptForGeneratingDialogue, prompt);
        var request = ApiKeys.API.ChatEndpoint.GetCompletionAsync(
            new ChatRequest(
                new List<Message>
                    {
                        new Message(Role.System, prompt),
                    }, "gpt-4o"));
        yield return new WaitFor(request);

        if (request.IsFaulted)
            yield break;

        var choice = request.Result.FirstChoice;
        var message = choice.Message;

        if (choice.FinishReason != "stop" || message.Content == null)
            yield break;

        prompt = message.Content.ToString();
        var lines = prompt.Split('\n');

        var names = new List<string>();
        var nodes = new List<StoreNode>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(':');
            if (parts.Length < 2)
                continue;

            var text = string.Join(":", parts.Skip(1));
            var name = parts[0]
                .Replace("*", string.Empty)
                .Trim();

            if (ReservedHeaders.Contains(name))
                continue;

            if (CountryManager[name] == null)
            {
                text = $"*{name}* {text}";
                name = "United Nations";
            }

            var node = new StoreNode(text, name,
                CountryManager[name]);

            yield return TextToSpeech.Generate(node);

            nodes.Add(node);
            if (!names.Contains(name))
                names.Add(name);
        }

        var logs = "";

        foreach (var node in nodes)
            yield return GenerateFaces(logs, node, names.ToArray());

        var template = prompt.Parse();

        StoryQueue.Instance.AddStoryToQueue(new Story
        {
            NewEpisode = true,
            Title = template["Title"],
            Vibe = template["Vibe"],
            Nodes = nodes,
            Countries = nodes
                .Select(n => n.Name)
                .Distinct()
                .ToArray(),
        }.Save());
    }

    private IEnumerator GenerateFaces(string context, StoreNode node, params string[] names)
    {
        var faces = string.Join(", ", Enum.GetNames(typeof(Face)));
        var list = string.Join("\n- ", names);
        var prompt = string.Format(PromptForGeneratingFaces, faces, list);

        context += string.Format("{0}: {1}\n", node.Name, node.Text);
        var messages = new List<Message>()
        {
            new Message(Role.System, prompt),
            new Message(Role.User, context)
        };

        var request = ApiKeys.API.ChatEndpoint.GetCompletionAsync(
                new ChatRequest(messages, "gpt-4o-mini"));
        yield return new WaitFor(request);

        if (request.IsFaulted)
            yield break;

        var choice = request.Result.FirstChoice;
        var message = choice.Message;

        if (choice.FinishReason != "stop" || message.Content == null)
            yield break;

        var reactions = message.Content.ToString().Parse();
        foreach (var name in names)
            if (CountryManager.Knows(name) && reactions.ContainsKey(name) && Enum.TryParse(reactions[name], out Face face))
                node.Reactions.Add(name, face);
    }
}