using Newtonsoft.Json;
using PolBol.Models;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TextToSpeechGenerator : MonoBehaviour
{
    public IEnumerator Generate(StoreNode node, int stacks = 0)
    {
        if (stacks > 3) yield break;
        var url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={ApiKeys.GOOGLE}";
        var json = JsonConvert.SerializeObject(new Request()
        {
            Input = new TextInput()
            {
                Text = ChatNode.regex.Replace(node.Text, " ")
            },
            AudioConfig = new AudioConfig(),
            Voice = new Voice(node.Country)
        });

        WWW www;
        yield return www = new WWW(url, Encoding.UTF8.GetBytes(json),
            new Dictionary<string, string>()
            {
                { "Content-Type", "application/json" }
            });
        if (www.error != null)
        {
            yield return new WaitForSeconds(1);
            yield return Generate(node, ++stacks);
        }

        var output = JsonConvert.DeserializeObject<Output>(www.text);
        node.Speech = output.AudioContent;
    }
}

namespace PolBol.Models
{
    class Request
    {
        [JsonProperty("input")]
        public TextInput Input { get; set; }

        [JsonProperty("audioConfig")]
        public AudioConfig AudioConfig { get; set; }

        [JsonProperty("voice")]
        public Voice Voice { get; set; }
    }

    class TextInput
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    class AudioConfig
    {
        [JsonProperty("audioEncoding")]
        public string AudioEncoding { get; set; } = "LINEAR16";

        [JsonProperty("sampleRateHertz")]
        public float SampleRate { get; set; } = 24000;

        [JsonProperty("volumeGainDb")]
        public float VolumeGain { get; set; } = 0;

        [JsonProperty("pitch")]
        public float Pitch { get; set; } = 0;

        [JsonProperty("speakingRate")]
        public float SpeakingRate { get; set; } = 1;
    }

    class Voice
    {

        [JsonProperty("name")]
        public string Name { get; set; } = "en-US-Standard-D";

        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; } = "en-US";

        public Voice(Country country)
        {
            if (country == null)
                return;
            Name = country.Voice;
        }
    }

    class Output
    {
        [JsonProperty("audioContent")]
        public string AudioContent { get; set; }
    }
}