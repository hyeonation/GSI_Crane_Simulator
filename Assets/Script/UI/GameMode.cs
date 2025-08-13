/// <summary>
/// 현재 선택한 시뮬레이터 모드(Keyboard / PLC)를
/// 어디서나 접근 가능하도록 보관하는 정적 클래스.
/// - 씬 전환 후에도 유지됩니다(정적 필드 특성).
/// - 필요 시 PlayerPrefs 등으로 영구 저장 로직을 추가하세요.
/// </summary>
public static class GameMode
{
    /// <summary>
    /// 지원 모드 열거형
    /// </summary>
    public enum Mode { Keyboard, PLC }

    /// <summary>
    /// 현재 모드 (기본값은 Keyboard)
    /// setter를 private으로 제한하여 통제된 Set 메서드로만 변경
    /// </summary>
    public static Mode Current { get; private set; } = Mode.Keyboard;

    /// <summary>
    /// 모드 변경용 유일한 진입점.
    /// 추후 이벤트 브로드캐스트, 로깅 등을 한 곳에서 처리 가능.
    /// </summary>
    public static void Set(Mode m)
    {
        Current = m;

        // 예) 모드 변경 이벤트를 추가하고 싶다면:
        // OnModeChanged?.Invoke(m);
        // PlayerPrefs.SetInt("GameMode", (int)m);
    }

    // public static event System.Action<Mode> OnModeChanged;
}
