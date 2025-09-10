using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)] // CanvasScaler 등 이후에 실행되도록 지연
public class SafeAreaFitter : MonoBehaviour
{

    const float opaqueScreenWidth = 989f;

    RectTransform rt;
    Rect lastSafe;
    int lastW, lastH;

    private void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        ApplyIfReady(); // 활성화 시 한 번 즉시 적용
    }

    void Update()
    {
        // 화면 크기/세이프 영역 변경 시에만 재적용
        if (Screen.width != lastW || Screen.height != lastH || Screen.safeArea != lastSafe)
            ApplyIfReady();
    }

    void ApplyIfReady()
    {
        if (!rt) return;

        int w = Screen.width;
        int h = Screen.height;

        // 가드: 화면 정보가 아직 준비 안 됨 → 기본 스트레치로 유지
        if (w <= 0 || h <= 0)
        {
            Debug.LogWarning("SafeAreaFitter: Screen dimensions not ready, applying full stretch.");
            SetFullStretch();
            return;
        }

        Rect safe = Screen.safeArea;

        // 일부 환경/타이밍에서 safeArea가 0일 수 있음 → 전체 화면으로 대체
        if (safe.width <= 0f || safe.height <= 0f)
        {
            Debug.LogWarning("SafeAreaFitter: Invalid safe area detected, applying full screen.");
            safe = new Rect(0, 0, w, h);
        }
        lastW = w; lastH = h; lastSafe = safe;

        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;

        // 정규화 (0~1)
        min.x /= w; min.y /= h;
        max.x /= w; max.y /= h;

        // NaN/Infinity 방지 및 범위 클램프
        if (IsBad(min) || IsBad(max))
        {
            Debug.LogWarning("SafeAreaFitter: Invalid safe area detected, applying full stretch.");
            SetFullStretch();
            return;
        }
        min = Vector2.Max(Vector2.zero, min);
        max = Vector2.Min(Vector2.one, max);

        rt.anchorMin = min;
        rt.anchorMax = max;
        // rt.offsetMin = Vector2.zero;
        // rt.offsetMax = Vector2.zero;

        // 여백 만들기
        // Debug.Log(Screen.width);
        // float margin = Screen.width / 2.5f; // 여백
        float margin = opaqueScreenWidth / 2f;  // 여백. 상수로 입력. 해상도 크기에 따라 버튼이 화면 밖으로 벗어나기도 해서.
        rt.offsetMin = new Vector2(margin, 0);    // 예시: 왼쪽 여백을 1/4 화면 너비로 설정
        rt.offsetMax = new Vector2(-margin, 0); // 오른쪽 여백도 동일하게 설정
    }

    static bool IsBad(Vector2 v) =>
        float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.x) || float.IsInfinity(v.y);

    void SetFullStretch()
    {
        // 안전 기본값 (전체 화면)
        if (!rt) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        lastW = Screen.width;
        lastH = Screen.height;
        lastSafe = new Rect(0, 0, Mathf.Max(0, lastW), Mathf.Max(0, lastH));
    }
}