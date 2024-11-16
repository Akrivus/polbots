using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ContextGenerator : MonoBehaviour, ISubGenerator
{
    public string Context => string.Empty;

    [SerializeField]
    private TextAsset _prompt1;

    [SerializeField]
    private TextAsset _prompt2;

    [SerializeField]
    private int _historyCount = 3;

    private List<string> _histories = new List<string>();
    private string _context;

    private ChatGenerator ChatGenerator;

    private void Awake()
    {
        ChatGenerator = GetComponent<ChatGenerator>();
        ChatGenerator.ContextGenerator += AddContext;
    }

    private async Task AddContext(Chat chat)
    {
        chat.AppendContext(_context);
        chat.FinalizeContext();
        await Task.CompletedTask;
    }

    public async Task<Chat> Generate(Chat chat)
    {
        _histories.Add(await ChatClient.CompleteAsync(
            _prompt1.Format(chat.Log, _context), true));

        var context = string.Empty;
        for (var i = 0; i < Math.Min(_historyCount, _histories.Count); i++)
            context += $"{i + 1}. " + _histories[_histories.Count - 1 - i] + "\n";

        _context = await ChatClient.CompleteAsync(
            _prompt2.Format(context), true);
        
        return chat;
    }
}