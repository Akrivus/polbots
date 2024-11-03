using System.IO;
using System;
using UnityEngine;
using System.Linq;

public class FileIntegration : MonoBehaviour
{
    private void Awake()
    {
        ChatManager.Instance.OnChatQueueEmpty += AddToChatQueue;
    }

    private void AddToChatQueue()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, "PolBol");

        var chats = Directory.GetFiles(path, "*.json")
            .Where(file => File.GetLastWriteTime(file) > DateTime.Now.AddDays(-1))
            .OrderBy(_ => Guid.NewGuid())
            .Take(UnityEngine.Random.Range(1, 5))
            .Select(Path.GetFileNameWithoutExtension).Select(Chat.Load)
            .ToList();
        foreach (var chat in chats)
            ChatManager.Instance.AddToPlayList(chat);
    }
}