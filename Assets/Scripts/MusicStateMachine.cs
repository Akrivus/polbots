using System.Linq;
using UnityEngine;

public class MusicStateMachine : MonoBehaviour
{
    public Vibe[] Vibes => vibes;

    [SerializeField]
    private StoryQueue queue;

    [SerializeField]
    private AudioSource player;

    [SerializeField, Range(0, 1)]
    private float loudVolume = 0.5f;

    [SerializeField, Range(0, 1)]
    private float backVolume = 0.1f;

    [SerializeField]
    private AudioClip[] music;

    [SerializeField]
    private Vibe[] vibes;

    public void PlayVibe(string name)
    {
        var vibe = vibes.FirstOrDefault(v => v.Key == name);
        if (vibe == null)
            vibe = vibes[Random.Range(0, vibes.Length)];
        player.clip = vibe.Song;
        player.Play();
    }

    private void OnEnable()
    {
        queue.OnQueueClosed += StopBackgroundMusic;
        queue.OnQueueOpen += PlayBackgroundMusic;
    }

    private void OnDisable()
    {
        queue.OnQueueClosed -= StopBackgroundMusic;
        queue.OnQueueOpen -= PlayBackgroundMusic;
    }

    private void StopBackgroundMusic()
    {
        player.volume = backVolume;
        player.clip = null;
        player.loop = true;
        player.Stop();
    }

    private void PlayBackgroundMusic()
    {
        player.volume = loudVolume;
        player.clip = music[Random.Range(0, music.Length)];
        player.loop = false;
        player.Play();
    }
}
