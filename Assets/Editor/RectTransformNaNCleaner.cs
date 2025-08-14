// Assets/Editor/RectTransformNaNCleaner.cs
#if UNITY_EDITOR
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class RectTransformNaNCleaner
{
    [MenuItem("Tools/UI/Fix NaNs in Selection")]
    public static void FixSelection()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0) { Debug.Log("No selection."); return; }
        int count = 0;
        foreach (var go in objs)
            count += FixAllIn(go);
        Debug.Log($"Fixed {count} RectTransform(s) in selection.");
    }

    [MenuItem("Tools/UI/Fix NaNs in Scene")]
    public static void FixScene()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        int count = 0;
        foreach (var root in roots)
            count += FixAllIn(root);
        Debug.Log($"Fixed {count} RectTransform(s) in scene.");
    }

    static int FixAllIn(GameObject root)
    {
        int count = 0;
        var rts = root.GetComponentsInChildren<RectTransform>(true);
        foreach (var rt in rts)
        {
            if (HasNaN(rt))
            {
                HardReset(rt);
                count++;
            }
        }
        return count;
    }

    [MenuItem("Tools/UI/Fix TextMeshPro margin NaNs in Scene")]
    public static void FixTextMeshProMarginNaN()
    {
        var objs = Selection.gameObjects;
        Debug.Log($"Fixing TextMeshPro margin NaNs in {objs.Length} selected objects.");
        if (objs == null || objs.Length == 0) { Debug.Log("No selection."); return; }
        int count = 0;
        foreach (var obj in objs) {

            var textMeshPros = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"Found {textMeshPros.Length} TextMeshPro components in {obj.name}.");
            foreach (var textMeshPro in textMeshPros)
            {
                Debug.Log($"Checking TextMeshPro: {textMeshPro.gameObject.name}");
                Debug.Log($"Current margin: {textMeshPro.margin}");
                // NaN 값이 있는 경우 0으로 대체
                if (float.IsNaN(textMeshPro.margin.x) || float.IsNaN(textMeshPro.margin.y) ||
                    float.IsNaN(textMeshPro.margin.z) || float.IsNaN(textMeshPro.margin.w))
                {
                    Debug.LogWarning($"TextMeshPro margin NaN found in {textMeshPro.gameObject.name}, resetting to 0.");
                }
                textMeshPro.margin = new Vector4(
                    float.IsNaN(textMeshPro.margin.x) ? 0f : textMeshPro.margin.x,
                    float.IsNaN(textMeshPro.margin.y) ? 0f : textMeshPro.margin.y,
                    float.IsNaN(textMeshPro.margin.z) ? 0f : textMeshPro.margin.z,
                    float.IsNaN(textMeshPro.margin.w) ? 0f : textMeshPro.margin.w
                );
            }

            count++;
        }

        Debug.Log($"Fixed {count} TextMeshPro margin(s) in selection.");
    }

    static bool HasNaN(RectTransform rt)
    {
        return IsBad(rt.anchorMin) || IsBad(rt.anchorMax) || IsBad(rt.anchoredPosition)
            || IsBad(rt.sizeDelta) || IsBad(rt.pivot)
            || IsBad(rt.localScale) || IsBad(rt.localPosition)
            || IsBad(rt.localEulerAngles);
    }

    static bool IsBad(Vector2 v) => float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.x) || float.IsInfinity(v.y);
    static bool IsBad(Vector3 v) => float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)
                                 || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);

    static void HardReset(RectTransform rt)
    {
        // 안전한 기본 상태(풀 스트레치)로 강제 초기화
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        EditorUtility.SetDirty(rt);
    }
}
#endif
