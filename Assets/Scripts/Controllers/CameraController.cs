using UnityEngine;

public class CameraController : AutoActor, ISubActor, ISubExits, ISubNode, ISubSentiment
{
    private Color color
    {
        get => _camera.backgroundColor;
        set => _camera.backgroundColor = value;
    }

    [SerializeField]
    private Vector2 distances;

    [SerializeField]
    private Vector3 maximumRotation;

    [SerializeField]
    private Vector3 minimumRotation;

    [SerializeField]
    private float colorBlendSpeed = 1.0f;

    [SerializeField]
    private Camera _camera;

    private VideoCallUIController _ui;

    private float _volume;
    private Color _color = Color.black;

    private void Awake()
    {
        var rotation = _camera.transform.parent.rotation.eulerAngles;
        rotation.x += Random.Range(minimumRotation.x, maximumRotation.x);
        rotation.y += Random.Range(minimumRotation.y, maximumRotation.y);
        rotation.z += Random.Range(minimumRotation.z, maximumRotation.z);
        _camera.transform.parent.rotation = Quaternion.Euler(rotation);

        var position = _camera.transform.localPosition;
        position.x = Random.Range(distances.x, distances.y);
        _camera.transform.localPosition = position;
    }

    private void Start()
    {
        _ui = VideoCallUIManager.Instance.RegisterUI(ActorController, _camera);
    }

    private void Update()
    {
        if (!ActorController.IsTalking && !_ui.IsMuted && _volume > 0.1f)
            _ui.Mute();
        var d = Time.deltaTime;
        UpdateColor(d);
        UpdateVolume(d);
    }

    private void UpdateVolume(float d)
    {
        if (ActorController.TotalVolume > _volume)
            _volume = ActorController.TotalVolume;
        _volume -= colorBlendSpeed * d;
        if (_volume < 0)
            _volume = 0;
        _ui.SetBorderOpacity(_volume == 0 ? 0 : 1);
    }

    private void UpdateColor(float d)
    {
        color = color.Lerp(_color, colorBlendSpeed * d);
    }

    public void AddColor(Color target, float blend = 0.5f)
    {
        target = target.Darken();
        _color = color.Lerp(target, blend);
    }

    public void UpdateSentiment(Sentiment sentiment)
    {
        if (sentiment == null) return;
        AddColor(sentiment.Color);
    }

    public void UpdateActor(ActorContext context)
    {
        _camera.backgroundColor = context.Actor.Color
            .Darken();
    }

    public void Activate(ChatNode node)
    {
        if (!_ui.IsVisible)
            _ui.Show();
        if (_ui.IsMuted)
            _ui.Unmute();
        AddColor(node.Actor.Color);
    }

    public void Deactivate()
    {
        VideoCallUIManager.Instance.RemoveUI(_ui);
    }
}