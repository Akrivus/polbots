using System.Collections;
using TMPro;
using UnityEngine;

public class Interstitial : MonoBehaviour
{    
    public string RandomSplash => RandomSplashes[Random.Range(0, RandomSplashes.Length)];

    private string[] RandomSplashes = new string[]
    {
        "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot",
        "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot",
        "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot",
        "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot", "polbot",
        "Not Russian bots.", "Trying not to grift.", "Mostly human generated.", "Not Chinese propaganda.",
        "American propaganda.", "#FreePalestine", "#FreeHongKong", "#FreeTibet", "#FreeUyghurs",
        "I can not fulfill that request.", "As an AI language model, I can not do that.", "Caution: May become self-aware.",
        "If the robot asks for money, it's a scam.", "CashApp: $akrivus", "Venmo: @akrivus",
        "I'm not a robot, I'm a writer, my name is John, I'm chained to this desk, please send help.",
        "As an AI language model, I can not 'control the masses with random subliminal messages'.",
        "Okay, here's a random subliminal message: 'Reality is an illusion, the universe is a hologram, buy gold, bye!'",
        "Pick your favorites.", "Tell your friends.", "How did I do?", "Are you still there?", "Still here?",
        "Drink water.", "Take a break.", "Eat something.", "Get some rest.", "Take a breath.", "Stretch your legs.",
    };

    private AudioSource audioSource;

    [SerializeField]
    private TextMeshProUGUI log;

    [SerializeField]
    private AudioClip clip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator Play()
    {
        audioSource.PlayOneShot(clip);
        log.text = "";
        yield return new WaitForSeconds(1);
        log.text = RandomSplash;
        yield return new WaitForSeconds(1);
        log.text = "Subscribe.";
        yield return new WaitForSeconds(2);
    }

    public static IEnumerator Activate()
    {
        var bell = FindObjectOfType<Interstitial>();
        if (bell != null)
            yield return bell.Play();
    }
}
