using System.Collections;
using TMPro;
using UnityEngine;

public class Interstitial : MonoBehaviour
{    
    public string RandomSplash => RandomSplashes[Random.Range(0, RandomSplashes.Length)];

    private string[] RandomSplashes = new string[]
    {
        "Mostly human generated.", "American propaganda.", "#FreePalestine", "#FreeHongKong", "#FreeTibet", "#FreeUyghurs",
        "I can not fulfill that request.", "As an AI language model, I can not do that.", "Caution: May become self-aware.",
        "If the robot asks for money, it's a scam.", "Do not trust the robot.", "Do not trust the AI.", "AI-generated content.",
        "I'm not a robot, I'm a writer, my name is John, I'm chained to this desk, please send help.",
        "As an AI language model, I can not 'control the masses with random subliminal messages'.",
        "Not for investment advice.", "Not for medical advice.", "Not for legal advice.", "Not for tax advice.",
        "Watching this won't make you an expert historian.", "Not historically accurate.", "Not scientifically accurate.",
        "Pending FDA approval.", "Not for children under 13.", "Consult your doctor.", "May contain third-party content.",
        "It's just Germany telling everyone to chill out.", "It's just America being America.", "It's just Russia being Russia.",
        "I added Thailand.", "No regrets.", "Pick your favorites.", "Tell your friends.", "Like and subscribe.",
        "Drink water.", "Eat something.", "Stretch your legs.", "Go for a walk.", "Touch some grass.",
        "Take a break.", "Take a bite", "Take a breath.", "Take a sip.", "Take a shot.", "Take a break.",
        "E-aitch?", "Oo-jee-ech!", "Feen-eh.", "Who needs chopsticks when you got these bad boys!", "Sushi-burgers, anyone?",
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
