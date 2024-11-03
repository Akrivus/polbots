using System;
using UnityEngine;

public class VideoCallUIManager : MonoBehaviour
{
    public static VideoCallUIManager Instance => _instance ?? (_instance = FindObjectOfType<VideoCallUIManager>());
    private static VideoCallUIManager _instance;

    public GameObject Container;
    public GameObject VideoScreenPrefab;
    public AudioSource AudioSource;

    [SerializeField]
    private SoundProfile _profile;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        ChatManager.Instance.OnChatQueueAdded += chat => Play(VideoCallSound.Ping);
    }

    public VideoCallUIController RegisterUI(ActorController actor, Camera camera)
    {
        var prefab = Instantiate(VideoScreenPrefab, Container.transform);
        var controller = prefab.GetComponent<VideoCallUIController>();
        controller.SetCamera(actor, camera);
        return controller;
    }

    public void RemoveUI(VideoCallUIController controller)
    {
        controller.Hide();
        Destroy(controller.gameObject);
    }

    public void Play(VideoCallSound sound)
    {
        switch (sound)
        {
            case VideoCallSound.Mute:
                AudioSource.PlayOneShot(_profile.Mute);
                break;
            case VideoCallSound.Unmute:
                AudioSource.PlayOneShot(_profile.Unmute);
                break;
            case VideoCallSound.Ping:
                AudioSource.PlayOneShot(_profile.Ping);
                break;
            case VideoCallSound.Join:
                AudioSource.PlayOneShot(_profile.Join);
                break;
            case VideoCallSound.Leave:
                AudioSource.PlayOneShot(_profile.Leave);
                break;
        }
    }
}

public enum VideoCallSound
{
    Mute,
    Unmute,
    Ping,
    Join,
    Leave
}

[Serializable]
public class SoundProfile
{
    public AudioClip Mute;
    public AudioClip Unmute;
    public AudioClip Ping;
    public AudioClip Join;
    public AudioClip Leave;
}
