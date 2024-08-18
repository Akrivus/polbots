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
        faceRenderer.transform.localPosition = Vector3.Lerp(
            faceRenderer.transform.localPosition,
            facePosition,
            Time.deltaTime * 8.0f);
        if (target == null)
            return;
        LookAt(target.position);
    }

    public void SetFace(Face face)
    {
        var texture = Resources.Load<Texture>($"Faces/{face}");
        faceRenderer.material.mainTexture = texture;
    }

    public void LookAt(Vector3 target)
    {
        if (target == null) return;
        var dist = target.z - transform.position.z;
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
    Worried
}