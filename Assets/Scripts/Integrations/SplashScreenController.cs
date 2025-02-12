using System.Collections;
using TMPro;
using UnityEngine;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private float _titleDuration = 5f;
    private float _splashDuration = 2f;
    private string[] _splashes;

    public void Configure(SplashScreenConfigs c)
    {
        _splashes = c.Splashes;

        ChatManager.Instance.OnIntermission += StartSplashScreen;

        SoccerGameSource.Instance.OnMatchStart += ToggleSplashScreensOff;
        SoccerGameSource.Instance.OnMatchEnd += ToggleSplashScreensOn;
    }

    private void ToggleSplashScreensOn()
    {
        _text.gameObject.SetActive(false);
    }

    private void ToggleSplashScreensOff()
    {
        _text.gameObject.SetActive(true);
    }

    private void Awake()
    {
        ConfigManager.Instance.RegisterConfig(typeof(SplashScreenConfigs), "splash", (config) => Configure((SplashScreenConfigs) config));
    }

    private IEnumerator StartSplashScreen(Chat chat)
    {
        if (_text.gameObject.activeSelf)
        {
            _text.text = string.Empty;
            yield return FadeOut();

            if (_splashes.Length > 0)
            {
                _text.text = _splashes[Random.Range(0, _splashes.Length)];
                yield return FadeIn();

                yield return new WaitForSeconds(_splashDuration);
                yield return FadeOut();
            }

            _text.text = chat.Title;
            yield return FadeIn();

            yield return new WaitForSeconds(_titleDuration);
            yield return FadeOut();
        }
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