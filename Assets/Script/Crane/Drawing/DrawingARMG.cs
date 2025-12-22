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

    protected override void WriteToPLC()
    {
        base.WriteToPLC();

        //TODO : SPSS_Stack1_Lidar_Row1~9 현재 위치 bay의 row별 컨테이너 높이  임시
        craneData.WriteData.SPSS_Stack1_Lidar_Row1 = GM.stackProfile.arrTier[0, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row2 = GM.stackProfile.arrTier[1, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row3 = GM.stackProfile.arrTier[2, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row4 = GM.stackProfile.arrTier[3, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row5 = GM.stackProfile.arrTier[4, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row6 = GM.stackProfile.arrTier[5, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row7 = GM.stackProfile.arrTier[6, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row8 = GM.stackProfile.arrTier[7, CurrentBay];
        craneData.WriteData.SPSS_Stack1_Lidar_Row9 = GM.stackProfile.arrTier[8, CurrentBay];


        GM.arrayCraneDataBase[iSelf] = craneData;
    }
}