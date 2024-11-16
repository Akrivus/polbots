using System.Linq;
using TMPro;
using UnityEngine;

public class SubtitlesUIManager : MonoBehaviour
{
    public static SubtitlesUIManager Instance => _instance ?? (_instance = FindObjectOfType<SubtitlesUIManager>());
    private static SubtitlesUIManager _instance;

    [SerializeField]
    private TextMeshProUGUI _title;

    [SerializeField]
    private TextMeshProUGUI _subtitle;

    [SerializeField]
    private TextMeshProUGUI _shadow;

    private void Awake()
    {
        _instance = this;
    }

    public void SetSubtitle(string name, string text, Color color)
    {
        var content = $"<b><u>{name}</u></b><size=75%>\n{text.Scrub()}";
        _subtitle.text = content;
        _subtitle.color = color;
        _shadow.text = "<mark=#000000aa>" + content;
    }

    public void SetSubtitle(string name, string text)
    {
        SetSubtitle(name, text, Color.white);
    }

    public void ClearSubtitle()
    {
        _subtitle.text = string.Empty;
        _shadow.text = string.Empty;
    }

    public void SetChatTitle(Chat chat)
    {
        var prompt = chat.Idea.Prompt;
        if (prompt.Length > 160)
            prompt = prompt.Substring(0, 160) + "...";
        _title.text = $"<u><b>{chat.Idea.Source} • </b></u> • {prompt}";
    }

    public void OnNodeActivated(ChatNode node)
    {
        SetSubtitle(node.Actor.Title, node.Text, node.Actor.Color
            .Lighten()
            .Lighten());
    }
}