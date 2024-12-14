using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareScreenUIManager : MonoBehaviour
{
    [SerializeField]
    private UIGridLayoutGroup _gridLayoutGroup;

    [SerializeField]
    private GameObject _shareScreenPrefab;

    [SerializeField]
    private int maxVideoScreens = 12;

    [SerializeField]
    private int minVideoScreens = 3;

    private TextMeshProUGUI _text;

    private void Start()
    {
        _text = _shareScreenPrefab
            .GetComponentInChildren<TextMeshProUGUI>();
        ShareScreenOff();
    }

    public void ShareScreenOn()
    {
        SetShareScreen(
            GridLayoutGroup.Corner.UpperRight,
            GridLayoutGroup.Axis.Vertical,
            TextAnchor.MiddleRight,
            minVideoScreens);
    }

    public void ShareScreenOff()
    {
        SetShareScreen(
            GridLayoutGroup.Corner.UpperLeft,
            GridLayoutGroup.Axis.Horizontal,
            TextAnchor.MiddleCenter,
            maxVideoScreens);
    }

    private void SetShareScreen(GridLayoutGroup.Corner corner, GridLayoutGroup.Axis axis, TextAnchor alignment, int childCount)
    {
        _gridLayoutGroup.startCorner = corner;
        _gridLayoutGroup.startAxis = axis;
        _gridLayoutGroup.childAlignment = alignment;
        _gridLayoutGroup.MaxChildren = childCount;
        _gridLayoutGroup.UpdateChildren();

        switch (axis)
        {
            case GridLayoutGroup.Axis.Horizontal:
                _gridLayoutGroup.SetLayoutHorizontal();
                _shareScreenPrefab.SetActive(false);
                break;
            case GridLayoutGroup.Axis.Vertical:
                _gridLayoutGroup.SetLayoutVertical();
                _shareScreenPrefab.SetActive(true);
                break;
        }
    }

    public void SetShareScreenInfo(float time, string homeTeam, int homeScore, string awayTeam, int awayScore)
    {
        var minutes = Mathf.FloorToInt(time / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        var minutesString = minutes < 10 ? $"0{minutes}" : minutes.ToString();
        var secondsString = seconds < 10 ? $"0{seconds}" : seconds.ToString();
        var timeString = $"{minutesString}:{secondsString}";

        _text.text = $"<size=300%><b>{timeString}<b></size>\r\n" +
            $"{homeTeam} - {awayTeam}\r\n" +
            $"<size=150%><b>{homeScore} - {awayScore}</b></size>";
    }
}
