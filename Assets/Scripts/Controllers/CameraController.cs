using UnityEngine;

public class CameraController : AutoActor, ISubActor, ISubExits, ISubNode, ISubSentiment
{
    private Color color => _camera.backgroundColor;

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
        rotation.x += UnityEngine.Random.Range(minimumRotation.x, maximumRotation.x);
        rotation.y += UnityEngine.Random.Range(minimumRotation.y, maximumRotation.y);
        rotation.z += UnityEngine.Random.Range(minimumRotation.z, maximumRotation.z);
        _camera.transform.parent.rotation = Quaternion.Euler(rotation);

        var position = _camera.transform.localPosition;
        position.x = UnityEngine.Random.Range(distances.x, distances.y);
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
        _camera.backgroundColor = color.Lerp(_color, colorBlendSpeed * d);
    }

    public void AddColor(Color target, float blend = 0.25f)
    {
        target = target.Lerp(Color.black, 0.5f);
        _color = color.Lerp(target, blend);
    }

    public void UpdateSentiment(Sentiment sentiment)
    {
        AddColor(sentiment.Color);
    }

    public void UpdateActor(Actor actor, ActorContext context)
    {
        var color = actor.Color.Lerp(Color.black, 0.5f);
        _camera.backgroundColor = color;
    }

    public void Activate(ChatNode node)
    {
        if (!_ui.IsVisible)
            _ui.Show();
        if (_ui.IsMuted)
            _ui.Unmute();
    }

    public void Deactivate()
    {
        VideoCallUIManager.Instance.RemoveUI(_ui);
    }
}