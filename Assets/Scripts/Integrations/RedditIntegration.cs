using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using UnityEngine;

public class RedditIntegration : MonoBehaviour
{
    [SerializeField]
    private ChatGenerator ChatGenerator;

    [SerializeField]
    private string[] subreddits;

    private void Awake()
    {
        ChatManager.Instance.OnChatQueueEmpty += AddToChatQueue;
    }

    private void AddToChatQueue()
    {
        var subreddit = subreddits[Random.Range(0, subreddits.Length)];
        var ideas = SubReddit(subreddit);
        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);
    }

    public Idea[] SubReddit(string subreddit, string list = "")
    {
        var url = $"https://www.reddit.com/r/{subreddit}/{list}.json";
        var client = new WebClient();

        client.Headers.Add("User-Agent", "polbot:1.0 (by /u/Akrivus)");

        var json = client.DownloadString(url);
        var data = JObject.Parse(json);
        var tokens = data.SelectTokens("$.data.children[*].data");

        return tokens
            .Where(post => !post.Value<bool>("stickied") && post.Value<int>("score") > 1)
            .Select(post => new Idea
            {
                Prompt = $"{post.Value<string>("title")}\n{post.Value<string>("selftext")}",
                Author = post.Value<string>("author"),
                Source = post.Value<string>("subreddit_name_prefixed")
            }).ToArray();
    }
}