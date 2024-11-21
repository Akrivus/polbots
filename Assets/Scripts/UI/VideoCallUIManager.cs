using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private int _maxVideoScreens = 12;

    private List<VideoCallUIController> _controllers = new List<VideoCallUIController>();

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        ChatManager.Instance.OnChatQueueAdded += chat => Play(VideoCallSound.Ping);
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        var quota = _controllers.Count - _maxVideoScreens;
        for (var i = _controllers.Count - 1; i >= 0; i--)
            _controllers[i].gameObject.SetActive(_controllers[i].IsActive || quota-- <= 0);
    }

    public VideoCallUIController RegisterUI(ActorController actor, Camera camera)
    {
        var controller = Instantiate(VideoScreenPrefab, Container.transform)
            .GetComponent<VideoCallUIController>();
        controller.SetCamera(actor, camera);

        _controllers.Add(controller);

        return controller;
    }

    public void RemoveUI(VideoCallUIController controller)
    {
        _controllers.Remove(controller);
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
