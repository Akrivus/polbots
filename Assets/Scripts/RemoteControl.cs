using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class RemoteControl : MonoBehaviour
{
    private Dictionary<string, Action> buttonActions;

    private void OnEnable()
    {
        buttonActions = new Dictionary<string, Action>
        {
            { "leftButton", Pause },
            { "leftArrow", Skip },
            { "rightArrow", Replay },
            { "upArrow", SpeedUp },
            { "downArrow", SlowDown },
            { "pageUp", SportsVolumeUp },
            { "pageDown", SportsVolumeDown },
            { "rightButton", Drop },
            { "contextMenu", ToggleGame }
        };
    }

    private void Awake()
    {
        InputSystem.onAnyButtonPress.Call((button) => buttonActions.GetValueOrDefault(button.name)?.Invoke());
    }

    private void Pause()
    {
        ChatManager.IsPaused = !ChatManager.IsPaused;
    }

    private void Skip()
    {
        ChatManager.SkipToEnd = true;
    }

    private void Replay()
    {
        var chat = ChatManager.Instance.NowPlaying;
        ChatManager.Instance.AddToPlayList(chat);
    }

    private void SpeedUp()
    {
        ActorController.GlobalSpeakingRate -= 0.1f;
    }

    private void SlowDown()
    {
        ActorController.GlobalSpeakingRate += 0.1f;
    }

    private void SportsVolumeUp()
    {
        SoccerGameSource.Instance.IncrementVolume();
    }

    private void SportsVolumeDown()
    {
        SoccerGameSource.Instance.DecrementVolume();
    }

    private void Drop()
    {
        RedditSource.Instance.TriggerDrop();
    }

    private void ToggleGame()
    {
        SoccerGameSource.Instance.ToggleGame();
    }
}
