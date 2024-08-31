using Newtonsoft.Json;
using PolBol.Models;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TextToSpeechGenerator : MonoBehaviour
{
    public IEnumerator Generate(StoreNode node)
    {
        var url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={ApiKeys.GOOGLE}";
        var json = JsonConvert.SerializeObject(new Request()
        {
            Input = new TextInput()
            {
                Text = ChatNode.regex.Replace(node.Text, " ")
            },
            AudioConfig = new AudioConfig()
            {
                SpeakingRate = node.Country.SpeakingRate,
                Pitch = node.Country.Pitch
            },
            Voice = new Voice()
            {
                Name = node.Country.Voice,
                LanguageCode = node.Country.Language
            }
        });

        WWW www;
        yield return www = new WWW(url, Encoding.UTF8.GetBytes(json),
            new Dictionary<string, string>()
            {
                { "Content-Type", "application/json" }
            });
        if (www.error != null) yield break;

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
        public string Name { get; set; } = "en-US-Wavenet-D";

        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; } = "en-US";
    }

    class Output
    {
        [JsonProperty("audioContent")]
        public string AudioContent { get; set; }
    }
}