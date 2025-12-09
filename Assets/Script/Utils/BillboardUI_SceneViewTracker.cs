using UnityEngine;
#if UNITY_EDITOR // Unity Editor 네임스페이스는 Editor 환경에서만 유효
using UnityEditor;
#endif

// [ExecuteInEditMode] 어트리뷰트가 핵심입니다.
// 게임 실행 중(Runtime)은 물론, Editor Mode에서도 LateUpdate를 실행시킵니다.
[ExecuteInEditMode]
public class BillboardUI_SceneViewTracker : MonoBehaviour
{
    private void LateUpdate()
    {
        // **UNITY_EDITOR 환경에서만 실행**
        // 빌드된 최종 게임 파일에서는 이 코드가 완전히 제외됩니다.
#if UNITY_EDITOR

        // SceneView 인스턴스를 가져옵니다. 
        // lastActiveSceneView는 현재 가장 활성화된 Scene 뷰 창을 의미합니다.
        Camera sceneCamera = SceneView.lastActiveSceneView?.camera;

        // 씬 뷰 카메라가 유효한지 확인합니다.
        if (sceneCamera != null)
        {
            transform.rotation = sceneCamera.transform.rotation;
        }

#endif
    }
}