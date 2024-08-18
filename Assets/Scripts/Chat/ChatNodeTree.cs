using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatNodeTree : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI subtitles;

    [SerializeField]
    private List<ChatNode> nodes;

    public void Add(ChatNode node)
    {
        if (nodes == null)
            nodes = new List<ChatNode>();
        nodes.Add(node);
    }

    public IEnumerator Play()
    {
        foreach (var node in nodes)
            yield return Activate(node);
        nodes.Clear();
    }

    private IEnumerator Activate(ChatNode node)
    {
        subtitles.text = node.Text;
        yield return node.Activate();
        subtitles.text = "";
    }
}