using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class StoryGenerator : MonoBehaviour
{
    private static List<string> ReservedHeaders = new List<string>
    {
        "Title", "Event", "Countries", "Scenario", "Resolution", "Dynamics", "Comedy"
    };

    public CountryManager CountryManager { get; private set; }
    public ChatNodeTree Chat { get; private set; }
    public TextToSpeechGenerator TextToSpeech { get; private set; }

    [SerializeField, TextArea(3, 30)]
    private string PromptForGeneratingStories;

    [SerializeField, TextArea(3, 30)]
    private string PromptForControllingEmotes;

    private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

    private void Awake()
    {
        CountryManager = GetComponent<CountryManager>();
        Chat = GetComponent<ChatNodeTree>();
        TextToSpeech = GetComponent<TextToSpeechGenerator>();
    }

    private void Start()
    {
        StartCoroutine(Generate());
    }

    public void AddIdeaToQueue(string story)
    {
        queue.Enqueue(story);
    }

    private IEnumerator Generate()
    {
        yield return new WaitUntil(() => queue.Count > 0);

        if (queue.TryDequeue(out var idea) && !string.IsNullOrEmpty(idea))
            yield return GenerateDialogue(idea);

        yield return Generate();
    }

    private IEnumerator GenerateDialogue(string seed)
    {
        var prompt = string.Format(PromptForGeneratingStories, seed);
        var request = ApiKeys.API.ChatEndpoint.GetCompletionAsync(
            new ChatRequest(
                new List<Message>
                    {
                        new Message(Role.System, prompt)
                    }, "gpt-4o"));
        yield return new WaitUntilTaskCompleted(request);

        if (request.IsFaulted)
            yield break;

        var choice = request.Result.FirstChoice;
        var message = choice.Message;

        if (choice.FinishReason != "stop" || message.Content == null)
            yield break;

        var lines = message.Content.ToString().Split('\n');

        var nodes = new List<StoryNode>();
        var logs = "";

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(':');
            if (parts.Length < 2)
                continue;

            var part0 = parts[0]
                .Replace("*", string.Empty)
                .Trim();
            var names = Regex
                .Split(part0, @",| and ")
                .Select(n => n.Trim());
            var text = string.Join(":", parts.Skip(1));

            foreach (var _ in names)
            {
                if (ReservedHeaders.Contains(_))
                    continue;
                var name = _;

                if (CountryManager[name] == null)
                    name = "United Nations";
                var node = new StoryNode(text, name,
                    CountryManager[name]);

                yield return GenerateFaces(logs, node, names.ToArray());
                yield return TextToSpeech.Generate(node);

                nodes.Add(node);
            }
        }

        var template = seed.Parse();
        var story = new Story
        {
            Title = template["Title"],
            Nodes = nodes,
            Countries = nodes
                .Select(n => n.Name)
                .Distinct()
                .ToArray(),
        };
        story.Save();
        StoryQueue.Instance.AddStoryToQueue(story);
    }

    private IEnumerator GenerateFaces(string context, StoryNode node, params string[] names)
    {
        var faces = string.Join(", ", Enum.GetNames(typeof(Face)));
        var list = string.Join("\n- ", names);
        var prompt = string.Format(PromptForControllingEmotes, faces, list);

        context += string.Format("{0}: {1}\n", node.Name, node.Text);
        var messages = new List<Message>()
        {
            new Message(Role.System, prompt),
            new Message(Role.User, context)
        };

        var request = ApiKeys.API.ChatEndpoint.GetCompletionAsync(
                new ChatRequest(messages, "gpt-4o-mini"));
        yield return new WaitUntilTaskCompleted(request);

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