using UnityEngine;
using System.Collections.Generic;

public class PLCManager
{
    public static PLCManager Instance;
    [SerializeField] private GameObject cranePrefab; 


    // plc 연결 시작
    public void StartPLCConnections()
    {
        // TODO : 상황에 맞는 DB 번호 및 길이로 변경 필요 임시 하드코딩
            GM.readDBNum = 2141;
            GM.readLength = 44;
            GM.writeDBNum = 2131;
            GM.writeLength = 441;


       var settings = GM.settingParams; // GM 의존성
        if (!settings.cmdWithPLC || settings.listIP == null) return;

        // IP 개수만큼 생성 및 주입
        for (int i = 0; i < settings.listIP.Count; i++)
        {
            Vector3 pos = GM.cranePOS[i];

            string addressablePath = Define.AddressablePath_RMGC;

            switch (Managers.Scene.CurrentSceneType)
            {
                case Define.SceneType.RMGC:
                    addressablePath = Define.AddressablePath_RMGC;  
                    break;
                case Define.SceneType.RTGC:
                    addressablePath = Define.AddressablePath_RTGC;
                    break;
                case Define.SceneType.QC:
                    addressablePath = Define.AddressablePath_QC;
                    break;
                case Define.SceneType.Test:
                    addressablePath = Define.AddressablePath_RMGCTEST;
                    break;
                default:
                    addressablePath = Define.AddressablePath_RMGC;  
                    break;
            }
            GameObject crane = GameObject.Find("Crane");
            CranePLCController cranePLCController = Managers.Object.Spawn<CranePLCController>(pos, addressablePath,crane.transform);
            cranePLCController.gameObject.name = $"{GM.craneTypeStr}{i + 1}";
            
            // 초기화 호출
            
            cranePLCController.Initialize(
                settings.listIP[i], 
                GM.readDBNum, GM.readLength, 
                GM.writeDBNum, GM.writeLength
            );
        }
    }
}