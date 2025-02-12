using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class RedditSource : MonoBehaviour, IConfigurable<RedditConfigs>
{
    public static RedditSource Instance { get; private set; }

    private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

    public event Action OnBatchStart;
    public event Action OnBatchEnd;

    [SerializeField]
    private ChatGenerator generator;

    [SerializeField]
    private TextAsset _prompt;

    public List<string> SubReddits = new List<string>();
    public float MaxPostAgeInHours = 24;
    public int BatchMax = 20;
    public float BatchPeriodInMinutes = 60;

    private List<string> history = new List<string>();
    private Dictionary<string, DateTime> fetchTimes = new Dictionary<string, DateTime>();
    private Queue<Idea> ideas = new Queue<Idea>();

    private int i = 0;

    public void Configure(RedditConfigs c)
    {
        SubReddits = c.SubReddits.Shuffle().ToList();
        MaxPostAgeInHours = c.MaxPostAgeInHours;
        BatchMax = c.BatchMax;
        BatchPeriodInMinutes = c.BatchPeriodInMinutes;

        history = LoadHistory();
        StartCoroutine(UpdateContent());
    }

    public IEnumerator UpdateContent()
    {
        while (Application.isPlaying)
        {
            OnBatchStart?.Invoke();
            yield return FetchIdeas().AsCoroutine();

            while (ideas.TryDequeue(out var idea))
                yield return generator.GenerateAndPlay(idea).AsCoroutine();
            OnBatchEnd?.Invoke();

            yield return new WaitForSeconds(BatchPeriodInMinutes * 60);
        }
    }

    public async Task FetchIdeas()
    {
        for (var _ = i; _ < SubReddits.Count; _++)
        {
            var range = await FetchAsync(SubReddits[_]);
            ideas = new Queue<Idea>(range
                .Take(BatchMax)
                .Select(post => {
                    history.Add(post.Value<string>("id"));
                    return post;
                })
                .Select(post => new Idea(
                        post.Value<string>("title"),
                        post.Value<string>("selftext"),
                        post.Value<string>("author"),
                        post.Value<string>("subreddit_name_prefixed"),
                        post.Value<string>("id")
                    ).RePrompt(_prompt)
                ).ToList());
            if (ideas.Count >= BatchMax)
                break;
            i = _;
        }
        if (i >= SubReddits.Count - 1)
            i = 0;
    }

    private void Awake()
    {
        Instance = this;
        ConfigManager.Instance.RegisterConfig(typeof(RedditConfigs), "reddit", (config) => Configure((RedditConfigs) config));
    }

    private void OnDestroy()
    {
        File.WriteAllLines("reddit.txt", history);
    }

    private List<string> LoadHistory()
    {
        if (!File.Exists("reddit.txt"))
            return new List<string>();
        return File.ReadAllLines("reddit.txt").ToList();
    }

    public Task<IEnumerable<JToken>> FetchAsync(string subreddit, int batchMax = 0)
    {
        return Task.Run(() => Fetch(subreddit, batchMax));
    }

    public IEnumerable<JToken> Fetch(string subreddit, int batchMax = 0)
    {
        var fetchTime = fetchTimes.GetValueOrDefault(subreddit, DateTime.Now.AddHours(-MaxPostAgeInHours));
        var cutoff = fetchTime.Subtract(EPOCH).TotalSeconds;

        var url = $"https://www.reddit.com/r/{subreddit}.json";
        var client = new WebClient();

        client.Headers.Add("User-Agent", "polbot:1.0 (by /u/Akrivus)");

        var json = client.DownloadString(url);
        var data = JObject.Parse(json);

        fetchTimes[subreddit] = DateTime.Now;

        if (batchMax <= 0)
            batchMax = BatchMax;

        return data.SelectTokens("$.data.children[*].data")
            .OrderByDescending(post => post.Value<int>("score"))
            .Where(post => post.Value<long>("created_utc") > cutoff)
            .Where(post => !history.Contains(post.Value<string>("id")))
            .Take(batchMax);
    }
}