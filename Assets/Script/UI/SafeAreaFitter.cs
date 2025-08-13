using UnityEngine;

/// <summary>
/// 기기의 안전 영역(Screen.safeArea)에 맞춰
/// 이 오브젝트(RectTransform)의 앵커를 자동으로 조정해 주는 컴포넌트.
/// - iOS/안드로이드 노치, 펀치홀, 소프트 내비게이션 바 등 UI가 가려지는 영역을 피합니다.
/// - [ExecuteAlways] 덕분에 에디터/플레이 모드 모두에서 즉시 반영됩니다.
/// - 최상위 컨테이너(예: SafeAreaRoot Panel)에 붙여 사용하는 것을 권장합니다.
/// </summary>
[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    // 대상 RectTransform 캐시 (매 프레임 GetComponent 방지)
    private RectTransform rt;
    // 마지막으로 적용했던 safeArea를 저장해 불필요한 재계산 방지
    private Rect lastSafe;

    private void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        Apply(); // 활성화 시 한 번 즉시 적용
    }

    private void Update()
    {
        // 해상도 회전/변경, 창 크기 조절 등으로 safeArea가 바뀌면 다시 적용
        if (Screen.safeArea != lastSafe)
            Apply();
    }

    /// <summary>
    /// 현재 기기의 안전 영역을 0~1 정규화 앵커로 변환하여
    /// RectTransform.anchorMin/anchorMax에 적용합니다.
    /// offset(픽셀) 값은 0으로 초기화하여 앵커만으로 레이아웃을 맞춥니다.
    /// </summary>
    private void Apply()
    {
        if (!rt) return;

        // 픽셀 좌표계에서의 안전 영역
        Rect safe = Screen.safeArea;
        lastSafe = safe;

        // 안전 영역의 최소/최대 모서리(좌하/우상) 픽셀값
        Vector2 min = safe.position;
        Vector2 max = safe.position + safe.size;

        // 화면 크기로 나누어 0~1의 앵커 좌표로 정규화
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;

        // 앵커 적용: 이 패널은 항상 안전 영역 안에 꽉 차게 됩니다.
        rt.anchorMin = min;
        rt.anchorMax = max;

        // 앵커 기반 레이아웃을 위해 오프셋 제거
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 여백 만들기
        float margin = Screen.width / 2.5f; // 여백
        rt.offsetMin = new Vector2(margin, 0);    // 예시: 왼쪽 여백을 1/4 화면 너비로 설정
        rt.offsetMax = new Vector2(-margin, 0); // 오른쪽 여백도 동일하게 설정
    }
}
