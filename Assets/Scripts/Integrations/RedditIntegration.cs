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

    public List<string> Subreddits = new List<string>();
    public int BatchMax = 20;
    public int BatchLifetimeMax = 2000;
    public float BatchPeriodInMinutes = 60;

    private Dictionary<string, DateTime> fetchTimes = new Dictionary<string, DateTime>();
    private int i = 0;
    private int batchLifetimeTotal = 0;

    public void Configure(RedditConfigs c)
    {
        Subreddits = c.Subreddits;
        BatchMax = c.BatchMax;
        BatchLifetimeMax = c.BatchLifetimeMax;
        BatchPeriodInMinutes = c.BatchPeriodInMinutes;

        ChatManager.Instance.OnChatQueueEmpty += AddToChatQueue;
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(RedditConfigs), "reddit", (config) => Configure((RedditConfigs) config));
    }

    private void AddToChatQueue()
    {
        if (BatchMax == 0 || BatchLifetimeMax == 0)
            return;
        if (batchLifetimeTotal >= BatchLifetimeMax)
            return;
        
        var ideas = new List<Idea>();
        for (var _ = 0; _ < Subreddits.Count; _++)
        {
            ideas.AddRange(Fetch(Subreddits[i++]));
            if (ideas.Count >= BatchMax)
                break;
        }

        batchLifetimeTotal += ideas.Count;

        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);
    }

    public List<Idea> Fetch(string subreddit)
    {
        var fetchTime = fetchTimes.GetValueOrDefault(subreddit, DateTime.Now.AddDays(-14));
        var cutoff = fetchTime.Subtract(EPOCH).TotalSeconds;

        if (DateTime.Now.Subtract(fetchTime).TotalSeconds < BatchPeriodInMinutes)
            return new List<Idea>();

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
            .Take(BatchMax)
            .Select(post => new Idea(
                post.Value<string>("title"),
                post.Value<string>("selftext"),
                post.Value<string>("author"),
                post.Value<string>("subreddit_name_prefixed"),
                post.Value<string>("id"))
            ).ToList();
    }
}