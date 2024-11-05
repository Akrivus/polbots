using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public class RedditIntegration : MonoBehaviour
{
    [SerializeField]
    private ChatGenerator ChatGenerator;

    [SerializeField]
    private string[] subreddits;

    [SerializeField]
    private int batchSize = 20;

    [SerializeField]
    private int debugBatchSize = 0;

    private int localBatchSize = 0;

    private int i = 0;

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
            FetchFiles(localBatchSize);

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
        var cutoff = DateTimeOffset.Now
            .AddHours(-1 * subreddits.Length)
            .ToUnixTimeSeconds();

        return tokens
            .Where(post => post.Value<long>("created_utc") > cutoff)
            .Where(post => !Chat.FileExists(post.Value<string>("title")))
            .OrderByDescending(post => post.Value<long>("created_utc"))
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

        var chats = Directory.GetFiles(path, "*.json")
            .Where(file => File.GetLastWriteTime(file) > DateTime.Now.AddDays(-1))
            .Shuffle().Take(count)
            .Select(Path.GetFileNameWithoutExtension).Select(Chat.Load)
            .ToList();
        foreach (var chat in chats)
            ChatManager.Instance.AddToPlayList(await chat);
    }
}