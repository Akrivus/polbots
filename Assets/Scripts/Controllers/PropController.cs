using System.Linq;
using UnityEngine;

public class PropController : MonoBehaviour
{
    [SerializeField]
    private CountryController controller;

    [SerializeField]
    private MeshRenderer mesh;

    private Prop[] props;

    private Vector3 position;

    void Awake()
    {
        var csv = Resources.Load<TextAsset>("props");
        var rows = csv.text.Split("\r\n");

        props = new Prop[rows.Length];
        for (var i = 0; i < rows.Length; ++i)
            if (!string.IsNullOrEmpty(rows[i]))
                props[i] = new Prop(rows[i]);
        SetProp(props[0]);
        position = mesh.transform.localPosition;
        controller.OnActivate += Activate;
    }

    void Update()
    {
        var time = Time.time * 0.2f;
        var y = Mathf.Sin(time) * 0.005f;
        var position = this.position - Vector3.up * y;

        mesh.transform.localPosition = Vector3.Lerp(
            mesh.transform.localPosition,
            position,
            Time.deltaTime * 8.0f);
    }

    private void SetProp(Prop prop)
    {
        mesh.material.mainTexture = prop.Texture;
    }

    private void Activate(ChatNode node)
    {
        var line = node.Line.ToLower();
        var prop = props
            .OrderBy(p => Random.value)
            .FirstOrDefault(p => p.Is(line));
        if (prop == null)
            return;
        SetProp(prop);
    }
}

public class Prop
{
    public string Name { get; private set; }
    public string[] Keywords { get; private set; }
    public Texture2D Texture => LoadTexture();
    private Texture2D _texture;

    public Prop(string row)
    {
        var columns = row.Split(',');
        Name = columns[0];

        var i = 0;
        foreach (var column in columns)
            if (string.IsNullOrEmpty(column))
                break;
            else
                ++i;
        Keywords = new string[--i];
        for (i = 0; i < Keywords.Length; ++i)
            Keywords[i] = columns[i + 1];
    }

    private Texture2D LoadTexture()
    {
        if (_texture) return _texture;
        _texture = Resources.Load<Texture2D>($"props/{Name}");
        return _texture;
    }

    public bool Is(string keyphrase)
    {
        foreach (var key in Keywords)
            if (keyphrase.Contains(key))
                return true;
        return false;
    }
}