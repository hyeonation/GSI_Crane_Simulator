using System.Collections.Generic;
using UnityEngine;

public class DrawingARMG : DrawingCrane
{

    private const int TARGET_DISPLAY_INDEX = 2;

    public override void SetCameraViewport(int viewportIdxNow, int camIdx)
    {
        // 데이터 검증 (Index Out of Range 방지)
        if (camIdx < 0 || camIdx >= Define.RMGCCameraIndex.Count)
        {
            Debug.LogError($"[DrawingARMG] Invalid Camera Index: {camIdx}");
            return;
        }

        string targetCamName = Define.RMGCCameraIndex[camIdx];

        CameraController targetCam = null;
        CameraController previousCam = viewPortCams[viewportIdxNow];

        // 타겟과 기존 카메라를 모두 탐색
        if (listCameraController != null)
        {
            for (int i = 0; i < listCameraController.Count; i++)
            {
                var camTemp = listCameraController[i];
                if (camTemp == null) continue;

                // 타겟 카메라 찾기
                if (camTemp.camName == targetCamName)
                {
                    targetCam = camTemp;
                    break;
                }
            }
        }
        // 3. 기존 카메라 비활성화 (새로 켤 카메라와 다를 경우에만)
        if (previousCam != null && previousCam != targetCam)
        {
            previousCam.SetDepth(-1);
            previousCam.CameraOff();
        }

        // 4. 신규 카메라 활성화 및 설정
        if (targetCam != null)
        {
            targetCam.SetScreenRect(viewportIdxNow);
            targetCam.CameraOn();
            targetCam.SetDepth(1);
            targetCam.SetTargetDisplay(TARGET_DISPLAY_INDEX);
            viewPortCams[viewportIdxNow] = targetCam;
        }
        else
        {
            Debug.LogWarning($"[DrawingARMG] Could not find camera with name: {targetCamName}");
        }
    }

    protected override void OnCraneSelectedChange()
    {
        base.OnCraneSelectedChange();
        // Define.RMGCCameraIndex camera Off
        if (isSelectedCrane)
        {
            foreach (var camCtrl in listCameraController)
            {
                if (camCtrl != null && Define.RMGCCameraIndex.ContainsValue(camCtrl.camName))
                {
                    camCtrl.SetDepth(-1);
                    camCtrl.CameraOff();
                }
            }

            // Defalut Camera On
            for (int i = 0; i < Define.DefaultRMGCCameraNames.Length; i++)
            {
                string defaultCamName = Define.DefaultRMGCCameraNames[i];
                foreach (var camCtrl in listCameraController)
                {
                    if (camCtrl != null && camCtrl.camName == defaultCamName)
                    {
                        camCtrl.SetScreenRect(i);
                        camCtrl.CameraOn();
                        camCtrl.SetDepth(1);
                        camCtrl.SetTargetDisplay(TARGET_DISPLAY_INDEX);
                        viewPortCams[i] = camCtrl;
                        break;
                    }
                }
            }
        }


    }
}