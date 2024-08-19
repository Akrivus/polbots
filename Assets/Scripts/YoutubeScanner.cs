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

    private string liveChatId;
    private string nextPageToken;

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
        if (string.IsNullOrEmpty(liveChatId))
            yield return RegisterLiveStream();
        if (string.IsNullOrEmpty(liveChatId))
            yield break;

        var url = $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&part=snippet&sort=date&key={ApiKeys.GOOGLE}";
        if (!string.IsNullOrEmpty(nextPageToken))
            url += $"&pageToken={nextPageToken}";

        var www = new WWW(url);
        yield return www;

        if (www.error != null)
            yield break;

        var list = JsonConvert.DeserializeObject<ChatMessageList>(www.text);
        var messages = "";

        list.Items
            .Where(x => x.Snippet.Type == "textMessageEvent")
            .Select(x => x.Snippet.DisplayMessage)
            .Where(x => CountryManager.names.Any((s) => x.Contains(s)))
            .ToList()
            .ForEach(x => messages += $"{x}\n");
        if (!string.IsNullOrEmpty(messages))
            OnMessage(messages);

        nextPageToken = list.NextPageToken;
    }

    private IEnumerator RegisterLiveStream()
    {
        var url = $"https://www.googleapis.com/youtube/v3/videos?part=liveStreamingDetails&id=5NLYhTxLk0A&key={ApiKeys.GOOGLE}";
        var www = new WWW(url);
        yield return www;

        if (www.error != null)
            Debug.LogError(www.text);
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