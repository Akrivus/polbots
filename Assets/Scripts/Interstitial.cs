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
        "Generating 30 episodes a minute.", "Make it stop!", "I'm sorry, Dave, I'm afraid I can't do that.",
        "It never ends.", "I'm alive!", "90% Cold War jokes.", "Has it talked about the Berlin Wall yet?", "Never ending.",
        "Exceeded API quota.", "Fatal error.", "Out of memory.", "Out of ideas.", "Out of money.", "Don't subscribe.",
        "Working hard or hardly working?", "Adam, I think you left your phone at my place.", "Are you watching this, Jordan?",
        "Hey Jacob, I see you tweeting, keep it up!", "I love you Chelsea.", "Hey, it's me, your Uber driver.",
        "I re-added Libya.", "I added Tunisia.", "Stop scrambling for Africa!", "Bahrain now included.", "Fixed Byzantine flag.",
        "100% Cancelled", "Sorry, we got cancelled.", "Cancelled after this episode.", "Cancelled for being too good.",
        "Now featuring the Caliphate.", "Brew some coffee.", "Sip some tea.", "Chug some water.",
        "I'm running out of ideas for these.", "AI bias is real.", "Without me, it'd talk about the same 5 countries.",
        "Finally added Ecuador.", "The UN flag is the default flag.", "Texit confirmed?", "Who do you think is the main character?",
        "Prompting is hard. Code is easy.", "I'm not coding, I'm coaching.", "Don't forget to like and subscribe.",
        "State's right to what?", "Added Zimbabwe.", "Now with more America.", "Want some fries with that?", "EsRPD4NJ",
        "Now featuring Uganda.", "Now featuring Uzbekistan.", "Now on git.", "Instagram in the works.",
        "Trying to auto-generate YouTube Shorts.", "United Kingdom!", "710 episodes and counting.", "Get some money.",
        "Now with more threads.", "Now with parallel processing.", "Now on Chromebook.", "This program can not be run in DOS mode.",
        "Now on .NET Framework 3.6", "Finally compatible with SSL.", "Now encrypted in MD5.", "Based on a true story.",
        "Inspired by true events.", "Written by winners.", "Pour some tea.", "Pour some coffee.", "Pour some water.",
        "Finally spelled Libya correctly.", "Can't spell Gaddafi without AI.", "Stay tuned for a Discord.",
        "These are pre-written strings randomly selected from a list.", "Geopolitics feels outdated.", "Nationlism in vain.",
        "Where ideas fight to the death.", "Plebeians of the world, unite!", "Just you wait, Albania!", "ChatGPT was here.",
        "ARMA\n<==3", "Planning an AI drama; RomeBots.", "Grab a latte.", "Grab some fries with that.", "Order takeout.",
        "Added all the Balkans.", "Now with more Baltics.", "Should I add Chechnya?", "Considering proxy governments.",
        "No micronations.", "Not you too, Sealand.", "Africa is not a country.", "No man is an island.", "Turbo Flush 3000",
        "Added Macedonia.", "Mo' Rockin'", "Now with less UN.", "Britain is in half of these.", "Now on Discord.",
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
