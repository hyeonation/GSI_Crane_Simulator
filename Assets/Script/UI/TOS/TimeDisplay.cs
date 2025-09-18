using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스
using System;

public class TimeDisplay : MonoBehaviour
{
    TextMeshProUGUI timeText; // TMP UI에 연결할 변수
    private bool showColon = true;

    void Start()
    {
        timeText = gameObject.GetComponent<TextMeshProUGUI>();
        InvokeRepeating(nameof(ToggleColon), 1f, 1f); // 1초마다 토글
    }

    void Update()
    {
        GM.dateTimeNow = DateTime.Now;
        string format = showColon ? "yy'/'MM'/'dd HH:mm" : "yy'/'MM'/'dd HH mm";
        timeText.text = GM.dateTimeNow.ToString(format);
    }

    void ToggleColon()
    {
        showColon = !showColon;
    }
}
