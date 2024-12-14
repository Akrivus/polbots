using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class DiscordIntegration : MonoBehaviour, IConfigurable<DiscordConfigs>
{
    public string WebhookURL;

    public void Configure(DiscordConfigs c)
    {
        WebhookURL = c.WebhookURL;

        ChatManager.Instance.OnChatNodeActivated += ReportChatNodeActivation;
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(DiscordConfigs), "discord", (config) => Configure((DiscordConfigs) config));
    }

    private void ReportChatNodeActivation(ChatNode node)
    {
        var reaction = node.Reactions.FirstOrDefault(r => r.Actor == node.Actor);
        var sentiment = node.Actor.DefaultSentiment.Name;
        if (reaction != null)
            sentiment = reaction.Sentiment.Name;

        var slug = node.Actor.Name.ToFileSafeString();

        SendWebhookAsync(WebhookURL, new DiscordWebhookMessage
        {
            Username = node.Actor.Title,
            Content = node.Line,
            Avatar = $"https://raw.githubusercontent.com/Akrivus/polbol/refs/heads/main/WWW/{sentiment}-{slug}.png",
        });
    }

    private void SendWebhookAsync(string uri, DiscordWebhookMessage message)
    {
        var client = new WebClient();
        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        client.UploadStringAsync(new Uri(uri),
            JsonConvert.SerializeObject(message));
    }
}

public class DiscordWebhookMessage
{
    [JsonProperty("content")]
    public string Content { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("avatar_url")]
    public string Avatar { get; set; }
    [JsonProperty("tts")]
    public bool TTS { get; set; }
    [JsonProperty("embeds")]
    public List<DiscordEmbed> Embeds { get; set; }
}

public class DiscordEmbed
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("url")]
    public string URL { get; set; }

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("color")]
    public int Color { get; set; }

    [JsonProperty("footer")]
    public DiscordEmbedFooter Footer { get; set; }

    [JsonProperty("image")]
    public DiscordEmbedImage Image { get; set; }

    [JsonProperty("thumbnail")]
    public DiscordEmbedThumbnail Thumbnail { get; set; }

    [JsonProperty("video")]
    public DiscordEmbedVideo Video { get; set; }

    [JsonProperty("provider")]
    public DiscordEmbedProvider Provider { get; set; }

    [JsonProperty("author")]
    public DiscordEmbedAuthor Author { get; set; }
}

public class DiscordEmbedFooter
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("icon_url")]
    public string Icon { get; set; }
}

public class DiscordEmbedImage
{
    [JsonProperty("url")]
    public string URL { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }
}

public class DiscordEmbedThumbnail
{
    [JsonProperty("url")]
    public string URL { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }
}

public class DiscordEmbedVideo
{
    [JsonProperty("url")]
    public string URL { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }
}

public class DiscordEmbedProvider
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("url")]
    public string URL { get; set; }
}

public class DiscordEmbedAuthor
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("url")]
    public string URL { get; set; }

    [JsonProperty("icon_url")]
    public string Icon { get; set; }
}
