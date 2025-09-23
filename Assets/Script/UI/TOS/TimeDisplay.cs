using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스
using System;

public class TimeDisplay : MonoBehaviour
{
    TextMeshProUGUI timeText; // TMP UI에 연결할 변수
    private bool showColon = true;

    const string formatColon = "yy'/'MM'/'dd HH:mm";
    const string formatNoColon = "yy'/'MM'/'dd HH mm";
    const string formatStd = "yy'/'MM'/'dd HH:mm:ss";

    void Start()
    {
        timeText = gameObject.GetComponent<TextMeshProUGUI>();
        InvokeRepeating(nameof(ToggleColon), 1f, 1f); // 1초마다 토글
    }

    void Update()
    {
        GM.dateTimeNow = DateTime.Now;
        string format = showColon ? formatColon : formatNoColon;
        timeText.text = GM.dateTimeNow.ToString(format);
    }

    public string TimeNowToString()
    {
        return GM.dateTimeNow.ToString(formatStd);
    }

    void ToggleColon()
    {
        showColon = !showColon;
    }
}
