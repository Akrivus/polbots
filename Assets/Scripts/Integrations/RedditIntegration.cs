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
    private TextAsset _prompt;

    public List<string> SubReddits = new List<string>();
    public int PostsPerIdea = 1;
    public float MaxPostAgeInHours = 24;
    public int BatchMax = 20;
    public int BatchLifetimeMax = 2000;
    public float BatchPeriodInMinutes = 60;

    public bool Forced = true;

    private Dictionary<string, DateTime> fetchTimes = new Dictionary<string, DateTime>();
    private DateTime lastBatchTime = DateTime.Now;

    private int i = 0;
    private int batchLifetimeTotal = 0;

    public void Configure(RedditConfigs c)
    {
        SubReddits = c.SubReddits;
        PostsPerIdea = c.PostsPerIdea;
        MaxPostAgeInHours = c.MaxPostAgeInHours;
        BatchMax = c.BatchMax * c.PostsPerIdea;
        BatchLifetimeMax = c.BatchLifetimeMax;
        BatchPeriodInMinutes = c.BatchPeriodInMinutes;

        ChatManager.Instance.OnChatQueueEmpty += AddToChatQueue;
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(RedditConfigs), "reddit", (config) => Configure((RedditConfigs) config));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Forced = true;
    }

    private void AddToChatQueue()
    {
        if (BatchMax == 0 || BatchLifetimeMax == 0)
            return;
        if (batchLifetimeTotal >= BatchLifetimeMax && !Forced)
            return;
        if (DateTime.Now.Subtract(lastBatchTime).TotalMinutes < BatchPeriodInMinutes && !Forced)
            return;
        
        var ideas = new List<Idea>();
        for (var _ = 0; _ < SubReddits.Count; _++)
        {
            ideas.AddRange(Fetch(SubReddits[i++]));
            if (ideas.Count >= BatchMax)
                break;

            if (i >= SubReddits.Count)
            {
                i = 0;
                break;
            }
        }

        lastBatchTime = DateTime.Now;
        batchLifetimeTotal += ideas.Count;

        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);
        Forced = false;
    }

    public List<Idea> Fetch(string subreddit)
    {
        var fetchTime = fetchTimes.GetValueOrDefault(subreddit, DateTime.Now.AddHours(-MaxPostAgeInHours));
        var cutoff = fetchTime.Subtract(EPOCH).TotalSeconds;

        var url = $"https://www.reddit.com/r/{subreddit}.json";
        var client = new WebClient();

        client.Headers.Add("User-Agent", "polbot:1.0 (by /u/Akrivus)");

        var json = client.DownloadString(url);
        var data = JObject.Parse(json);
        var tokens = data.SelectTokens("$.data.children[*].data");

        fetchTimes[subreddit] = DateTime.Now;

        if (PostsPerIdea > 1)
            return tokens
                .Where(post => post.Value<long>("created_utc") > cutoff)
                .Where(post => !Chat.FileExists(post.Value<string>("id")))
                .OrderByDescending(post => post.Value<long>("created_utc"))
                .Take(BatchMax)
                .Select((post, i) => new { post, i })
                .GroupBy(pair => pair.i / PostsPerIdea)
                .Select(group => group.Select(pair => pair.post).ToList())
                .Select(group => new Idea(
                    string.Join("\n", group.Select(post => post.Value<string>("subreddit_name_prefixed") + ": " + post.Value<string>("title"))),
                    string.Join(", ", group.Select(post => post.Value<string>("author"))),
                    string.Join( "+", group.Select(post => post.Value<string>("subreddit_name"))),
                    string.Join( "-", group.Select(post => post.Value<string>("id")))).RePrompt(_prompt)
                ).ToList();

        return tokens
            .Where(post => post.Value<long>("created_utc") > cutoff)
            .Where(post => !Chat.FileExists(post.Value<string>("id")))
            .OrderByDescending(post => post.Value<long>("created_utc"))
            .Take(BatchMax)
            .Select(post => new Idea(
                post.Value<string>("subreddit_name_prefixed") + ": " + post.Value<string>("title"),
                post.Value<string>("selftext"),
                post.Value<string>("author"),
                post.Value<string>("subreddit_name"),
                post.Value<string>("id")).RePrompt(_prompt)
            ).ToList();
    }
}