using System.Collections.Generic;
using UnityEngine;

public class DrawingARMG : DrawingCrane
{
    // 매직 넘버 제거: 유지보수를 위해 상수로 관리
    private const int TARGET_DISPLAY_INDEX = 2;

    public override void SetCameraViewport(int viewport, int camIdx)
    {
        // 데이터 검증 (Index Out of Range 방지)
        if (camIdx < 0 || camIdx >= Define.RMGCCameraIndex.Count)
        {
            Debug.LogError($"[DrawingARMG] Invalid Camera Index: {camIdx}");
            return;
        }

        string targetCamName = Define.RMGCCameraIndex[camIdx];
        
        CameraController targetCam = null;
        CameraController previousCam = null;

        // 타겟과 기존 카메라를 모두 탐색
        if (listCameraController != null)
        {
            for (int i = 0; i < listCameraController.Count; i++)
            {
                var currentCam = listCameraController[i];
                if (currentCam == null) continue;

                // 타겟 카메라 찾기
                if (currentCam.camName == targetCamName)
                {
                    targetCam = currentCam;
                }

                // 해당 뷰포트와 디스플레이를 점유 중인 기존 카메라 찾기
                if (currentCam.viewportIdx == viewport && currentCam.targetDisplayIdx == TARGET_DISPLAY_INDEX)
                {
                    previousCam = currentCam;
                }
            }
        }

        // 3. 기존 카메라 비활성화 (새로 켤 카메라와 다를 경우에만)
        if (previousCam != null && previousCam != targetCam)
        {
            previousCam.CameraOff();
            // Debug.Log($"Set {previousCam.camName} off from viewport {viewport}");
        }

        // 4. 신규 카메라 활성화 및 설정
        if (targetCam != null)
        {
            targetCam.SetScreenRect(viewport);
            targetCam.CameraOn();
            targetCam.SetTargetDisplay(TARGET_DISPLAY_INDEX);
            // Debug.Log($"Set {targetCam.camName} to viewport {viewport}");
        }
        else
        {
            Debug.LogWarning($"[DrawingARMG] Could not find camera with name: {targetCamName}");
        }
    }
}