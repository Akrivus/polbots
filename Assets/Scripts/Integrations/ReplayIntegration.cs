using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class ReplayIntegration : MonoBehaviour
{
    private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

    [SerializeField]
    private ChatGenerator ChatGenerator;

    [SerializeField]
    private int replayRate = 80;

    [SerializeField]
    private int replayCount = 20;

    private List<string> replays = new List<string>();

    private void Awake()
    {
        ChatManager.Instance.OnChatQueueEmpty += ReplayEpisode;
    }

    private async void ReplayEpisode()
    {
        var ideas = new List<Idea>();
        await FetchFiles(replayCount);

        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);
    }

    private async Task FetchFiles(int count)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, Chat.FolderName);

        var tasks = Directory.GetFiles(path, "*.json")
            .Where(file => File.GetLastWriteTime(file) > DateTime.Now.AddDays(-1))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(title => !replays.Contains(title))
            .Shuffle().Take(count).Select(LogThenLoad)
            .ToList();

        foreach (var task in tasks)
            ChatManager.Instance.AddToPlayList(await task);
    }

    private async Task<Chat> LogThenLoad(string title)
    {
        replays = replays.TakeLast(replayRate - 1).ToList();
        replays.Add(title);
        return await Chat.Load(title);
    }
}