using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)] // CanvasScaler 이후 실행
public class SafeAreaFitter : MonoBehaviour
{
    public enum WidthMode { Ratio, FixedPixels }            // 비율로 맞출지, 고정 픽셀로 맞출지
    public enum Basis { ScreenWidth, SafeAreaWidth }    // 비율 기준: 전체 화면 or 세이프에어리어

    [Header("Width Setting")]
    public WidthMode widthMode = WidthMode.FixedPixels;

    [Range(0f, 1f)]
    public float widthRatio = 0.55f;        // Ratio 모드일 때: 화면(또는 세이프)에 대한 비율
    public float fixedWidthPixels = 989f;  // FixedPixels 모드일 때: 목표 폭(px)

    public Basis ratioBasis = Basis.ScreenWidth; // 비율 기준 (요구사항: "전체 화면 width 비율"이면 ScreenWidth)

    RectTransform rt;
    Canvas rootCanvas;

    Rect lastSafe;
    int lastW, lastH;
    float lastSF;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        ApplyIfNeeded(force: true);
    }

    void Update()
    {
        ApplyIfNeeded();
    }

    void ApplyIfNeeded(bool force = false)
    {
        if (!rt) return;

        int w = Screen.width;
        int h = Screen.height;
        Rect safe = Screen.safeArea;
        float sf = rootCanvas ? Mathf.Max(0.0001f, rootCanvas.scaleFactor) : 1f;

        // 변경 감지: 화면 크기/세이프/스케일팩터
        if (!force &&
            w == lastW && h == lastH &&
            safe == lastSafe &&
            Mathf.Approximately(sf, lastSF))
            return;

        lastW = w; lastH = h; lastSafe = safe; lastSF = sf;

        // 가드: 아직 화면 정보가 준비되지 않은 타이밍(도메인 리로드 직후 등)
        if (w <= 0 || h <= 0)
        {
            SetFullStretch();
            return;
        }

        // 세이프 영역이 0인 경우 → 전체 화면으로 대체
        if (safe.width <= 0f || safe.height <= 0f)
            safe = new Rect(0, 0, w, h);

        // 1) 앵커를 세이프에어리어에 맞춤 (가로/세로 모두 정규화)
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;
        min.x /= w; min.y /= h;
        max.x /= w; max.y /= h;

        // NaN/Inf/범위 가드
        if (IsBad(min) || IsBad(max))
        {
            SetFullStretch();
            return;
        }
        min = Vector2.Max(Vector2.zero, min);
        max = Vector2.Min(Vector2.one, max);

        rt.anchorMin = min;
        rt.anchorMax = max;

        // 2) 목표 폭을 "픽셀" 기준으로 계산
        //    - 부모(=세이프에어리어)가 더 좁으면 그 안에서 클램프
        float parentWidthPx = safe.width; // 세이프에어리어 실제 픽셀 폭
        float desiredWidthPx;

        if (widthMode == WidthMode.Ratio)
        {
            float basisPx = (ratioBasis == Basis.ScreenWidth) ? w : safe.width; // 요구사항: 전체 화면 기준이면 w 사용
            desiredWidthPx = Mathf.Clamp01(widthRatio) * basisPx;
        }
        else // FixedPixels
        {
            desiredWidthPx = Mathf.Max(0f, fixedWidthPixels);
        }

        // 세이프에어리어보다 클 수 없으니 클램프
        desiredWidthPx = Mathf.Min(desiredWidthPx, parentWidthPx);

        // 3) 좌/우 마진(픽셀)을 계산한 뒤 → "캔버스 단위"로 변환
        //    핵심: height가 변하면 CanvasScaler의 scaleFactor가 변하지만,
        //          우리는 픽셀→캔버스단위로 나눠주므로 실제 보이는 픽셀 폭은 고정됨.
        float marginPx = (parentWidthPx - desiredWidthPx) * 0.5f; // 좌우 동일 마진(px)
        float marginUnits = marginPx / sf;                        // Canvas 단위

        // 수직 방향은 세이프에어리어에 꽉 차게 유지
        rt.offsetMin = new Vector2(marginUnits, 0f);
        rt.offsetMax = new Vector2(-marginUnits, 0f);
    }

    static bool IsBad(Vector2 v) =>
        float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.x) || float.IsInfinity(v.y);

    void SetFullStretch()
    {
        if (!rt) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
