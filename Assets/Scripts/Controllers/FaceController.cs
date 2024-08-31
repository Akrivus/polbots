using UnityEngine;

public class FaceController : MonoBehaviour
{
    public Face Face
    {
        get => face;
        set
        {
            face = value;
            SetFace(face);
        }
    }

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    [SerializeField]
    private Face face;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private MeshRenderer faceRenderer;

    private Vector3 facePosition = Vector3.zero;

    public void Start()
    {
        SetFace(face);
    }

    public void Update()
    {
        var time = Time.time * 0.1f + Face.GetHashCode();
        var y = Mathf.Sin(time) * 0.001f;
        var position = facePosition + Vector3.up * y;
        faceRenderer.transform.localPosition = Vector3.Lerp(
            faceRenderer.transform.localPosition,
            position,
            Time.deltaTime * 8.0f);
    }

    public void SetFace(Face face, Transform target = null)
    {
        var texture = Resources.Load<Texture>($"faces/{face}");
        faceRenderer.material.mainTexture = texture;
        if (target != null) LookAt(target.position);
    }

    public void LookAt(Vector3 target)
    {
        if (target == null || transform.position == target) return;
        var dist = target.x - transform.position.x;
        if (dist > 0)
            facePosition = Vector3.back / 5f;
        else if (dist < 0)
            facePosition = Vector3.forward / 5f;
        else
            facePosition = Vector3.zero;
    }
}

public enum Face
{
    Alarmed,
    Angry,
    Annoyed,
    Anxious,
    Ashamed,
    Blush,
    Bored,
    Calm,
    Concerned,
    Confused,
    Crazy,
    Cry,
    Depressed,
    Drunk,
    Embarrassed,
    Enraged,
    Excited,
    Focused,
    Frustrated,
    Happy,
    Hesitant,
    Impatient,
    Impressed,
    Lonely,
    Love,
    Neutral,
    Pissed,
    Pleased,
    Sad,
    Scared,
    Shocked,
    Sleepy,
    Smug,
    Sob,
    Stern,
    Stressed,
    Stupid,
    Surprised,
    Suspicious,
    Tired,
    Wink,
    Worried,
    Stoned,
}