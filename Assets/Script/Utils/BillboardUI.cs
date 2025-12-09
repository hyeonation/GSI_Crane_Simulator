using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    // 추적할 카메라의 Transform (주로 Main Camera를 할당)
    [SerializeField]
    private Transform _targetCamera;

    private void Start()
    {
        // Target Camera가 할당되지 않은 경우, 메인 카메라를 자동으로 찾습니다.
        if (_targetCamera == null)
        {
            // Unity의 성능 최적화를 위해 Start에서 한 번만 찾아 캐싱(Caching)합니다.
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                _targetCamera = mainCam.transform;
            }
            else
            {
                Debug.LogError("씬에 Main Camera 태그가 지정된 카메라가 없습니다.");
                enabled = false; // 스크립트 비활성화
                return;
            }
        }
    }

    private void LateUpdate()
    {
     

        if (_targetCamera != null)
        {

            transform.LookAt(_targetCamera);
            transform.rotation = _targetCamera.rotation; // 이 코드가 World Space UI 빌보딩의 가장 일반적인 형태입니다.
        }
    }
}