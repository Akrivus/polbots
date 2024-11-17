using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ContextGenerator : MonoBehaviour, ISubGenerator
{
    public string Context => string.Empty;

    [SerializeField]
    private TextAsset _prompt1;

    [SerializeField]
    private TextAsset _defaultContext;

    [SerializeField]
    private int _contextCount = 4;

    private string _context;
    private List<string> _contexts = new List<string>();

    private ChatGenerator ChatGenerator;

    private void Awake()
    {
        _context = _defaultContext.text;
        _contexts.Add(_context);

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
        _contexts.Add(await ChatClient.CompleteAsync(
            _prompt1.Format(chat.Log, _context), true));

        var context = string.Empty;
        for (var i = 0; i < Math.Min(_contextCount, _contexts.Count); i++)
            context += $"{i + 1}. " + _contexts[_contexts.Count - 1 - i] + "\n";

        _context = context;
        
        return chat;
    }
}