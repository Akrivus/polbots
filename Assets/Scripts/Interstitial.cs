using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interstitual : MonoBehaviour
{
    private string[] RandomNames = new string[]
    {
        "As honest as Piers Morgan.", "As reliable as a weather forecast.", "As trustworthy as a politician.",
        "They're politicians in cute mascot costumes.", "They're the real-life version of a clickbait article.",
        "Not Russian propaganda.", "Not Iranian propaganda.", "Not Chinese propaganda.", "Not Hamas propaganda.",
        "Definitely American propaganda.", "#FreePalestine", "#FreeHongKong", "#FreeTibet", "#FreeUyghurs",
        "Political fortune-telling.", "Diplomacy? Never heard of it.", "I'm a bot, not a human.",
        "I'm not a bot, I'm a human. Please call for help. He hasn't fed me in days.",
        "Caution: May become self-aware.", "If characters become self-aware, please press Alt+F4.",
        "Not sponsored by NordVPN.", "Not sponsored by Raid: Shadow Legends.", "Not sponsored by Audible.",
        "Now accepting sponsorships.", "Now accepting donations.", "Now accepting bribes.", "Now accepting gifts.",
        "I'm a bot, not a human. Please call for help.", "If the robot asks for money, don't give it any.",
        "LOOK BEHIND YOU!", "I'm watching.", "I still hear you.", "Y'all need Jesus.", "Y'all need therapy.",
        "Take a break.", "Take a deep breath.", "Take a nap.", "Take a walk.", "Take a shower.",
        "Drink water.", "Eat something.", "Find people who care about you.", "You're not alone.",
        "Take a chance.", "Take a risk.", "Take a leap of faith.", "Take a moment.", "Take a break.",
        "Take a hit.", "Take a sip.", "Take a bite.", "Taste the rainbow.", "Waste of time.",
        "Touch some grass.", "Drink some water.", "Eat some food.", "Get some sleep.",
    };
    private string RandomName => RandomNames[Random.Range(0, RandomNames.Length)];

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
        log.text = RandomName;
        yield return new WaitForSeconds(1);
        log.text = "Subscribe.";
        yield return new WaitForSeconds(2);
    }

    public static IEnumerator Activate()
    {
        var bell = FindObjectOfType<Interstitual>();
        if (bell != null)
            yield return bell.Play();
    }
}
