using Discord;
using FStudio.MatchEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class DiscordIntegration : MonoBehaviour, IConfigurable<DiscordConfigs>
{
    public static Dictionary<string, DiscordWebhook> Webhooks => webhooks;
    private static Dictionary<string, DiscordWebhook> webhooks;

    public Dictionary<string, string> WebhookURLs { get; private set; }

    private const string PremierRoleID = "<@&1277082258783604828>";

    public void Configure(DiscordConfigs c)
    {
        WebhookURLs = c.WebhookURLs;
        webhooks = WebhookURLs.ToDictionary(k => k.Key, v => new DiscordWebhook(v.Value));

        ChatManager.Instance.OnChatNodeActivated += SendChatNode;
        ChatManager.Instance.AfterIntermission += SendChatUpdates;
        SoccerIntegration.Instance.OnEmit += SendSportsUpdates;
        StartCoroutine(UpdateWebhooks());
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(DiscordConfigs), "discord", (config) => Configure((DiscordConfigs) config));
    }

    private IEnumerator UpdateWebhooks()
    {
        foreach (var webhook in webhooks.Values)
            if (webhook.MessageQueue.Count > 0)
                yield return webhook.SendAsync(webhook.MessageQueue.Dequeue());
        yield return new WaitForSeconds(0.375f);

        if (Application.isPlaying)
            yield return UpdateWebhooks();
    }

    private void SendChatUpdates(Chat chat)
    {
        webhooks["#stream"].Send(new DiscordMessage
        {
            Embeds = new[]
            {
                new EmbedBuilder
                {
                    Title = $"New Episode: {string.Join(", ", chat.Names)} {(chat.NewEpisode ? PremierRoleID : string.Empty)}",
                    Description = chat.Topic,
                    Color = new Discord.Color(0, 0, 255),
                    Fields = chat.Actors.Select(a => new EmbedFieldBuilder
                    {
                        Name = a.Name,
                        Value = a.Context,
                        IsInline = false
                    }).ToList()
                }.Build()
            }
        });
    }

    private void SendChatNode(ChatNode node)
    {
        webhooks["#stream"].Send(node.Line, node.Actor.Name, GetAvatarURL(node));
    }

    private void SendSportsUpdates(string message)
    {
        webhooks["#stream"].Send(message);
    }

    private void SendMatchStats()
    {
        var stats = MatchManager.Statistics;
    }

    private string GetAvatarURL(ChatNode node)
    {
        var reaction = node.Reactions.FirstOrDefault(r => r.Actor == node.Actor);
        var sentiment = node.Actor.DefaultSentiment.Name;
        if (reaction != null)
            sentiment = reaction.Sentiment.Name;

        var slug = node.Actor.Name.ToFileSafeString();

        return $"https://raw.githubusercontent.com/Akrivus/polbol/refs/heads/main/WWW/{sentiment}-{slug}.png";
    }
}

public class DiscordMessage
{
    public string Content { get; set; }
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public IEnumerable<Embed> Embeds { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(new
        {
            content = Content,
            embeds = Embeds,
            username = Username,
            avatar_url = AvatarUrl
        });
    }

    public static DiscordMessage operator +(DiscordMessage a, DiscordMessage b)
    {
        return new DiscordMessage
        {
            Content = a.Content + " " + b.Content,
            Username = a.Username,
            AvatarUrl = a.AvatarUrl,
            Embeds = b.Embeds
        };
    }
}

public class DiscordWebhook
{
    public string URL { get; set; }
    public WebClient Client { get; set; }
    public Queue<DiscordMessage> MessageQueue { get; private set; } = new Queue<DiscordMessage>();

    private ulong _id = 0L;
    private DiscordMessage _lastMessage;

    public DiscordWebhook(string url)
    {
        URL = url;
        Client = new WebClient();
        Client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
    }

    public void Send(string content, string username = null, string avatarUrl = null)
    {
        Send(new DiscordMessage
        {
            Content = content,
            Username = username,
            AvatarUrl = avatarUrl
        });
    }

    public void Send(DiscordMessage message)
    {
        MessageQueue.Enqueue(message);
    }

    public async Task SendAsync(DiscordMessage message)
    {
        _lastMessage = message;
        await SendWebhookAsync(message);
    }

    private async Task SendWebhookAsync(DiscordMessage message)
    {
        var data = await Client.UploadStringTaskAsync(URL, message.ToString());
        var response = JObject.Parse(data);
        _id = ulong.Parse(response["id"].ToString());
    }
}