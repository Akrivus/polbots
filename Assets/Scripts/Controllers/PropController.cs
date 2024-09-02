using System.Linq;
using UnityEngine;

public class PropController : MonoBehaviour
{
    [SerializeField]
    private CountryController controller;

    [SerializeField]
    private MeshRenderer mesh;

    private Prop[] props;
    private Prop prop;

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
        var time = Time.time * 0.5f + controller.transform.GetSiblingIndex() * 1000f;
        var y = Mathf.Sin(time) * 0.025f;
        var position = this.position - Vector3.up * y;

        mesh.transform.Rotate(Vector3.up, Mathf.Sin(time) * 0.025f);
        mesh.transform.localPosition = Vector3.Lerp(
            mesh.transform.localPosition,
            position,
            Time.deltaTime * 8.0f);
    }

    private void SetProp(Prop prop)
    {
        if (prop.Texture == null)
            return;
        this.prop = prop;
        mesh.material.mainTexture = prop.Texture;
    }

    private void Activate(ChatNode node)
    {
        var line = node.Line.ToLower();
        if (this.prop != null && this.prop.Is(line))
            return;
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
        Keywords = columns[1].Split('/');
    }

    private Texture2D LoadTexture()
    {
        if (_texture) return _texture;
        _texture = Resources.Load<Texture2D>($"props/{Name}");
        return _texture;
    }

    public bool Is(string keyphrase)
    {
        if (Name == "none")
            return false;
        foreach (var key in Keywords)
            if (keyphrase.Contains(key))
                return true;
        return false;
    }
}