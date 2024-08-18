using UnityEngine;

public class MusicStateMachine : MonoBehaviour
{
    [SerializeField]
    private StoryQueue queue;

    [SerializeField]
    private AudioSource player;

    [SerializeField, Range(0, 1)]
    private float interstitialVolume = 0.5f;

    [SerializeField, Range(0, 1)]
    private float backgroundVolume = 0.1f;

    [SerializeField]
    private AudioClip[] interstitialMusic;

    [SerializeField]
    private AudioClip[] backgroundMusic;

    private void OnEnable()
    {
        queue.OnQueueClosed += PlayBackgroundMusic;
        queue.OnQueueOpen += PlayInterstitialMusic;
    }

    private void OnDisable()
    {
        queue.OnQueueClosed -= PlayBackgroundMusic;
        queue.OnQueueOpen -= PlayInterstitialMusic;
    }

    private void PlayBackgroundMusic()
    {
        player.volume = backgroundVolume;
        player.clip = backgroundMusic[Random.Range(0, backgroundMusic.Length)];
        player.loop = true;
        player.Play();
    }

    private void PlayInterstitialMusic()
    {
        player.volume = interstitialVolume;
        player.clip = interstitialMusic[Random.Range(0, interstitialMusic.Length)];
        player.loop = false;
        player.Play();
    }
}
