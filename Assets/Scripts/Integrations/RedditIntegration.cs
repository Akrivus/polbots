using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public class RedditIntegration : MonoBehaviour
{
    private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

    [SerializeField]
    private ChatGenerator ChatGenerator;

    [SerializeField]
    private string[] subreddits;

    [SerializeField]
    private int batchSize = 20;

    [SerializeField]
    private int debugBatchSize = 0;

    [SerializeField]
    private int replayRate = 80;

    [SerializeField]
    private int replaySize = 20;

    private int localBatchSize = 0;

    private int i = 0;

    private List<string> replays = new List<string>();
    private Dictionary<string, DateTime> fetchTimes = new Dictionary<string, DateTime>();

    private void Awake()
    {
        subreddits = subreddits.Shuffle().ToArray();
        ChatManager.Instance.OnChatQueueEmpty += AddToChatQueue;

        if (Application.isEditor)
        {
            localBatchSize = batchSize;
            batchSize = debugBatchSize;
        }
    }

    private void AddToChatQueue()
    {
        var ideas = new List<Idea>();
        i = 0;

        if (batchSize > 0)
            do
                ideas.AddRange(Fetch(subreddits[i++]));
            while (ideas.Count < batchSize && i < subreddits.Length);
        if (batchSize > 0)
            localBatchSize = batchSize - ideas.Count;
        if (localBatchSize > 0)
            FetchFiles(localBatchSize + replaySize);

        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);
    }

    public Idea[] Fetch(string subreddit)
    {
        var url = $"https://www.reddit.com/r/{subreddit}.json";
        var client = new WebClient();

        client.Headers.Add("User-Agent", "polbot:1.0 (by /u/Akrivus)");

        var json = client.DownloadString(url);
        var data = JObject.Parse(json);
        var tokens = data.SelectTokens("$.data.children[*].data");

        var fetchTime = fetchTimes.GetValueOrDefault(subreddit, DateTime.Now.AddHours(-8));
        var cutoff = fetchTime.Subtract(EPOCH).TotalSeconds;

        fetchTimes[subreddit] = DateTime.Now;

        return tokens
            .Where(post => post.Value<long>("created_utc") > cutoff)
            .Where(post => !Chat.FileExists(post.Value<string>("title")))
            .OrderByDescending(post => post.Value<long>("created_utc"))
            .Take(batchSize)
            .Select(post => new Idea(
                post.Value<string>("title"),
                post.Value<string>("selftext"),
                post.Value<string>("author"),
                post.Value<string>("subreddit_name_prefixed"))
            ).ToArray();
    }

    private async void FetchFiles(int count)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, "PolBol");

        var tasks = Directory.GetFiles(path, "*.json")
            .Where(file => File.GetLastWriteTime(file) > DateTime.Now.AddDays(-1))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(title => !replays.Contains(title))
            .Shuffle().Take(count).Select(Chat.Load)
            .ToList();

        foreach (var task in tasks)
        {
            var chat = await task;

            replays = replays.TakeLast(replayRate - 1).ToList();
            replays.Add(chat.FileName);
            ChatManager.Instance.AddToPlayList(chat);
        }
    }
}