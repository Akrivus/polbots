using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DiscordIntegration : MonoBehaviour
{
    private static string WebhookURL => Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");

    private void Start()
    {
        ChatManager.Instance.OnChatQueueTaken += ReportNewChat;
        ChatManager.Instance.OnChatNodeActivated += ReportChatNodeActivation;
    }

    private void ReportChatNodeActivation(ChatNode node)
    {

    }

    private void ReportNewChat(Chat chat)
    {

    }

    private void SendWebhookAsync(string message)
    {
        var client = new WebClient();
        var uri = new Uri(WebhookURL);
        client.UploadValuesAsync(uri, "POST",
            new NameValueCollection
            {
                { "content", message }
            });
    }
}