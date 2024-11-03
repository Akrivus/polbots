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
        var content = $"<b><u>{name}</u></b>\n{text.Scrub()}";
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
        _title.text = chat.Idea.Source;
    }

    public void OnNodeActivated(ChatNode node)
    {
        var color = node.Actor.Color.Lerp(Color.white, 0.7f);
        SetSubtitle(node.Actor.Title, node.Text, color);
    }
}