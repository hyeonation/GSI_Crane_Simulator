// using UnityEngine;
// using Unity.Collections;
// using Unity.Mathematics;
// using Unity.Jobs;

// public class SPSSManager : MonoBehaviour
// {
//     [Header("Attached Sensors")]
//     public SPSSLIDAR[] sensors;

//     // 통합 데이터 버퍼
//     private NativeArray<float3> _allPoints;
//     private int _totalCapacity;

//     void Start()
//     {
//         // 자식 오브젝트에서 센서 자동 탐색 및 초기화
//         if (sensors == null || sensors.Length == 0)
//             sensors = GetComponentsInChildren<SPSSLIDAR>();

//         ReallocateBuffer();
//     }

//     private void ReallocateBuffer()
//     {
//         if (_allPoints.IsCreated) _allPoints.Dispose();

//         _totalCapacity = 0;
//         foreach (var sensor in sensors)
//         {
//             // 각 센서의 해상도에 따른 전체 포인트 수 합산
//             _totalCapacity += sensor.TotalPoints;
//         }

//         if (_totalCapacity > 0)
//         {
//             _allPoints = new NativeArray<float3>(_totalCapacity, Allocator.Persistent);
//             Debug.Log($"<color=green>[SPSSManager]</color> Global Buffer Allocated: {_totalCapacity} points.");
//         }
//     }

//     void LateUpdate()
//     {
//         if (sensors == null || sensors.Length == 0) return;

//         int currentOffset = 0;
//         bool anyDataUpdated = false;

//         for (int i = 0; i < sensors.Length; i++)
//         {
//             // 센서의 Job이 완료되었고 데이터가 유효한지 확인
//             if (sensors[i].IsDataReady)
//             {
//                 var sensorPoints = sensors[i].GetPoints();
//                 if (sensorPoints.IsCreated)
//                 {
//                     // NativeArray.Copy를 사용하여 메모리 고속 복사 (병렬 처리 가능하나 여기서는 순차 복사)
//                     NativeArray<float3>.Copy(sensorPoints, 0, _allPoints, currentOffset, sensorPoints.Length);
//                     anyDataUpdated = true;
//                 }
//             }
//             currentOffset += sensors[i].TotalPoints;
//         }

//         if (anyDataUpdated)
//         {
//             OnDataIntegrated(_allPoints);
//         }
//     }

//     private void OnDataIntegrated(NativeArray<float3> integratedData)
//     {
//         // 여기서 통합된 데이터(integratedData)를 활용한 후속 처리를 수행합니다.
//         // 예: AI 인지 시스템 전달, 슬램(SLAM) 데이터 구축 등
//         // Debug.Log($"Integrated {integratedData.Length} points.");
//     }

//     void OnDestroy()
//     {
//         if (_allPoints.IsCreated) _allPoints.Dispose();
//     }

//     // 센서 설정이 변경되었을 때 버퍼를 재설정하기 위한 Public 메서드
//     public void RefreshManager() => ReallocateBuffer();
// }