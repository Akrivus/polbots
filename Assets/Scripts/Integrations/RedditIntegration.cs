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
    private int batchMax = 20;

    [SerializeField]
    private int batchLifetimeMax = 2000;

    [SerializeField]
    private float batchPeriodInMinutes = 60;

    [Header("Debug")]
    [SerializeField]
    private int debugBatchMax = 0;

    private Dictionary<string, DateTime> fetchTimes = new Dictionary<string, DateTime>();
    private int i = 0;
    private int batchLifetime = 0;

    private void Awake()
    {
        subreddits = subreddits.Shuffle().ToArray();
        ChatManager.Instance.OnChatQueueEmpty += AddToChatQueue;

        if (Application.isEditor)
            batchMax = debugBatchMax;
    }

    private void AddToChatQueue()
    {
        if (batchMax == 0 || batchLifetimeMax == 0)
            return;
        if (batchLifetime >= batchLifetimeMax)
            return;
        
        var ideas = new List<Idea>();
        for (var _ = 0; _ < subreddits.Length; _++)
        {
            ideas.AddRange(Fetch(subreddits[i++]));
            if (ideas.Count >= batchMax)
                break;
        }

        batchLifetime += ideas.Count;

        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);
    }

    public Idea[] Fetch(string subreddit)
    {
        var fetchTime = fetchTimes.GetValueOrDefault(subreddit, DateTime.Now.AddDays(-14));
        var cutoff = fetchTime.Subtract(EPOCH).TotalSeconds;

        if (DateTime.Now.Subtract(fetchTime).TotalSeconds < batchPeriodInMinutes)
            return new Idea[0];

        var url = $"https://www.reddit.com/r/{subreddit}.json";
        var client = new WebClient();

        client.Headers.Add("User-Agent", "polbot:1.0 (by /u/Akrivus)");

        var json = client.DownloadString(url);
        var data = JObject.Parse(json);
        var tokens = data.SelectTokens("$.data.children[*].data");

        fetchTimes[subreddit] = DateTime.Now;

        return tokens
            .Where(post => post.Value<long>("created_utc") > cutoff)
            .Where(post => !Chat.FileExists(post.Value<string>("id")))
            .OrderByDescending(post => post.Value<long>("created_utc"))
            .Take(batchMax)
            .Select(post => new Idea(
                post.Value<string>("title"),
                post.Value<string>("selftext"),
                post.Value<string>("author"),
                post.Value<string>("subreddit_name_prefixed"),
                post.Value<string>("id"))
            ).ToArray();
    }
}