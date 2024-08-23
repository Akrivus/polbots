using Newtonsoft.Json;
using PolBol.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class YoutubeScanner : MonoBehaviour
{
    public Action<string> OnMessage;

    [SerializeField]
    private CountryManager CountryManager;

    [SerializeField]
    private int pollingInterval = 5;

    private string liveChatId;
    private string nextPageToken;

    private DateTime lastScanTime;

    public void Register()
    {
        StoryQueue.Instance.OnQueueClosed += ScanForPrompts;
        StartCoroutine(RegisterLiveStream());
    }

    public void ScanForPrompts()
    {
        StartCoroutine(Scan());
    }

    private IEnumerator Scan()
    {
        StoryQueue.Instance.CanChatSuggestTopics = false;

        if (DateTime.Now < lastScanTime.AddSeconds(pollingInterval))
            yield break;
        if (string.IsNullOrEmpty(liveChatId))
            yield return RegisterLiveStream();
        if (string.IsNullOrEmpty(liveChatId))
            yield break;

        var url = $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&part=snippet&sort=date&key={ApiKeys.GOOGLE}";
        if (!string.IsNullOrEmpty(nextPageToken))
            url += $"&pageToken={nextPageToken}";

        var www = new WWW(url);
        yield return www;

        lastScanTime = DateTime.Now;

        StoryQueue.Instance.CanChatSuggestTopics = www.error == null;

        if (!StoryQueue.Instance.CanChatSuggestTopics)
            yield break;

        var list = JsonConvert.DeserializeObject<ChatMessageList>(www.text);

        var keywords = CountryManager.countries.Select(c => c.Name.ToLower()).ToList();
        var messages = list.Items
            .Where(m => m.Snippet.Type == "textMessageEvent");
        var results = messages
            .Where(m => keywords.Any(k => m.Contains(k)));
        var items = results.GroupBy(m => keywords.First(k => m.Contains(k))).ToList();

        var message = "";

        foreach (var i in messages)
            message += i.Snippet.DisplayMessage + "\n";
        OnMessage(message);

        foreach (var item in items)
        {
            message = $"Suggested topic: {item.Key}\n";
            foreach (var i in item)
                message += i.Snippet.DisplayMessage + "\n";
            OnMessage(message);
        }

        nextPageToken = list.NextPageToken;
    }

    private IEnumerator RegisterLiveStream()
    {
        var url = $"https://www.googleapis.com/youtube/v3/videos?part=liveStreamingDetails&id={ApiKeys.STREAM}&key={ApiKeys.GOOGLE}";
        var www = new WWW(url);
        yield return www;

        StoryQueue.Instance.CanChatSuggestTopics = www.error == null;

        if (!StoryQueue.Instance.CanChatSuggestTopics)
        {
            Debug.LogError(www.error);
            yield break;
        }
        else
        {
            var list = JsonConvert.DeserializeObject<VideoList>(www.text);
            if (list.Items.Count == 0) yield break;
            var item = list.Items.First();
            liveChatId = item.LiveStreamingDetails.ActiveLiveChatId;
            yield return Scan();
        }
    }
}

namespace PolBol.Models
{
    public class ChatMessageList
    {
        [JsonProperty("items")]
        public List<ChatMessage> Items { get; set; }

        [JsonProperty("nextPageToken")]
        public string NextPageToken { get; set; }

        [JsonProperty("pollingIntervalMillis")]
        public int PollingIntervalMillis { get; set; }
    }

    public class ChatMessage
    {
        [JsonProperty("snippet")]
        public ChatMessageSnippet Snippet { get; set; }

        public bool Contains(string keyword)
        {
            return Snippet.DisplayMessage.ToLower().Contains(keyword);
        }
    }

    public class ChatMessageSnippet
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("liveChatId")]
        public string LiveChatId { get; set; }

        [JsonProperty("authorChannelId")]
        public string AuthorChannelId { get; set; }

        [JsonProperty("publishedAt")]
        public DateTime PublishedAt { get; set; }

        [JsonProperty("displayMessage")]
        public string DisplayMessage { get; set; }
    }

    public class VideoList
    {
        [JsonProperty("items")]
        public List<Video> Items { get; set; }
    }

    public class Video
    {
        [JsonProperty("liveStreamingDetails")]
        public LiveStreamingDetails LiveStreamingDetails { get; set; }
    }

    public class LiveStreamingDetails
    {
        [JsonProperty("activeLiveChatId")]
        public string ActiveLiveChatId { get; set; }
    }
}