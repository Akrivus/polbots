using System.Collections;
using TMPro;
using UnityEngine;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private string[] _splashes;

    public void Configure(SplashConfigs c)
    {
        _splashes = c.Splashes;

        ChatManager.Instance.OnIntermission += StartSplashScreen;
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(SplashConfigs), "splash", (config) => Configure((SplashConfigs) config));
    }

    private IEnumerator StartSplashScreen(Chat chat)
    {
        _text.text = string.Empty;
        yield return FadeOut();

        if (_splashes.Length > 0)
        {
            _text.text = _splashes[Random.Range(0, _splashes.Length)];
            yield return FadeIn();

            yield return new WaitForSeconds(2.0f);
            yield return FadeOut();
        }

        _text.text = chat.Title;
        yield return FadeIn();

        yield return new WaitForSeconds(3.0f);
        yield return FadeOut();
    }

    private IEnumerator FadeIn()
    {
        var c = _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0);
        var t = 0.0f;

        while (t < 1.0f)
        {
            _text.color = new Color(c.r, c.g, c.b, t += Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FadeOut()
    {
        var c = _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1);
        var t = 1.0f;

        while (t > 0.0f)
        {
            _text.color = new Color(c.r, c.g, c.b, t -= Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}