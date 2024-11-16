using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChatClient
{
    public static string OPENAI_API_KEY => Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    public static string DOMAIN => "https://api.openai.com";
    public static string SLOW_MODEL => "gpt-4o";
    public static string FAST_MODEL => "gpt-4o-mini";

    public static OpenAIClient API => _api ??= new OpenAIClient(new OpenAIAuthentication(OPENAI_API_KEY), new OpenAISettings(DOMAIN));
    private static OpenAIClient _api;

    public static async Task<List<Message>> ChatAsync(List<Message> messages, bool fast = false)
    {
        Debug.Log(messages[messages.Count - 1].Content);
        var model = fast ? FAST_MODEL : SLOW_MODEL;
        var request = await API.ChatEndpoint.GetCompletionAsync(new ChatRequest(messages, model));
        var response = request.FirstChoice;
        if (response.FinishReason != "stop")
            throw new Exception(response.FinishDetails);
        messages.Add(response.Message);

        Debug.Log(response.Message.Content);
        return messages;
    }

    public static async Task<List<Message>> ChatAsync(string prompt, bool fast = false)
    {
        return await ChatAsync(new List<Message> { new Message(Role.System, prompt) }, fast);
    }

    public static async Task<string> CompleteAsync(string prompt, bool fast = false)
    {
        var messages = await ChatAsync(prompt, fast);
        return messages[messages.Count - 1].Content.ToString();
    }
}