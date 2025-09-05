using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;


public class LiDAR3DIJobParallelForMain : MonoBehaviour
{
    [Header("Scan")]
    public Vector3 scanOffset = Vector3.zero;

    [Header("Job Tuning")]
    [Tooltip("IJobParallelFor batch size (inner-loop). 32~256 권장")]
    public int innerloopBatchCount = 128;

    [Tooltip("RaycastCommand.ScheduleBatch 의 minCommandsPerJob. 32~256 권장")]
    public int minCommandsPerJob = 64;

    [Header("Debug/Output (옵션)")]
    public bool debugDraw = false;
    public int debugDrawEveryNFrames = 10;
    public bool fillManagedListForDebug = false; // 꼭 필요할 때만 true (GC 유발)

    [Header("Save Settings (직접 호출 권장)")]
    public bool saveToFile = false; // 매 프레임 저장은 비권장
    public string fileName;

    // Native 컨테이너들 (Persistent)
    NativeArray<RaycastCommand> _commands;
    NativeArray<RaycastHit> _hits;
    NativeArray<float3> _points; // 최종 포인트 (관리 영역으로 복사하지 않음)

    // (선택) 외부 시스템용 임시 List – 필요할 때만 채움
    public List<Vector3> pointCloud = new();

    // 해상도/시야 설정 캐시
    int hSteps, vSteps, totalSteps;
    float hStartDeg, vStartDeg; // deg
    float hStartRad, vStartRad; // rad
    float resH_Rad, resV_Rad;   // rad

    int _frame; // 디버그 드로잉 주기용

    // ========================= 라이프사이클 =========================
    void OnEnable()
    {
        RecalculateScanGrid();
        AllocateIfNeeded();
    }

    void OnDisable()
    {
        DisposeIfCreated();
    }

    void OnDestroy()
    {
        DisposeIfCreated();
    }

    void RecalculateScanGrid()
    {
        // 외부 설정에서 가져오기 (원 코드와 동일한 GM 사용)
        float fovH = GM.settingParams.lidarFovHorizontal_deg;
        float fovV = GM.settingParams.lidarFovVertical_deg;
        float resH = GM.settingParams.lidarResHorizontal_deg;
        float resV = GM.settingParams.lidarResVertical_deg;

        hStartDeg = -fovH * 0.5f;
        vStartDeg = -fovV * 0.5f;

        hSteps = Mathf.CeilToInt(fovH / resH);
        vSteps = Mathf.CeilToInt(fovV / resV);
        totalSteps = Mathf.Max(1, hSteps * vSteps);

        // 라디안 캐시 (Burst 친화적)
        hStartRad = math.radians(hStartDeg);
        vStartRad = math.radians(vStartDeg);
        resH_Rad = math.radians(resH);
        resV_Rad = math.radians(resV);
    }

    void AllocateIfNeeded()
    {
        if (!_commands.IsCreated || _commands.Length != totalSteps)
        {
            DisposeIfCreated();

            _commands = new NativeArray<RaycastCommand>(totalSteps, Allocator.Persistent);
            _hits = new NativeArray<RaycastHit>(totalSteps, Allocator.Persistent);
            _points = new NativeArray<float3>(totalSteps, Allocator.Persistent);
        }

        // 관리 리스트는 디버그/저장 시에만 사용. 용량만 미리 확보
        if (pointCloud.Capacity < totalSteps)
            pointCloud.Capacity = totalSteps;
    }

    void DisposeIfCreated()
    {
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
        if (_points.IsCreated) _points.Dispose();
    }

    // ========================= 잡 정의 =========================
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct SetRaycastJob : IJobParallelFor
    {
        public Vector3 origin;
        public Quaternion rotation; // 월드 로테이션 (한 번만 곱함)
        public float maxDistance;

        // 라디안 단위 각도 정보
        public float resV_Rad, resH_Rad;
        public float hStart_Rad, vStart_Rad;
        public int hSteps;

        [WriteOnly] public NativeArray<RaycastCommand> commands;
        public int layerMask;

        public void Execute(int index)
        {
            int v = index / hSteps;
            int h = index % hSteps;

            float vAng = vStart_Rad + v * resV_Rad;
            float hAng = hStart_Rad + h * resH_Rad;

            math.sincos(vAng, out float sV, out float cV);
            math.sincos(hAng, out float sH, out float cH);

            // 로컬 기준(센서) 방향 (Quaternion.Euler/AngleAxis 없이 계산)
            // Vector3.right(1,0,0)을 Z-축으로 v 회전 후, Yaw(h)
            Vector3 localDir = new Vector3(cV * cH, sV, -cV * sH);
            Vector3 worldDir = rotation * localDir;

            commands[index] = new RaycastCommand(origin, worldDir, maxDistance, layerMask);
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct CollectPointsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> hits;
        [ReadOnly] public NativeArray<RaycastCommand> commands;
        public float maxDistance;

        [WriteOnly] public NativeArray<float3> points;

        public void Execute(int i)
        {
            // Burst 호환을 위해 collider 체크 대신 distance 사용 (미스 시 0)
            bool hasHit = hits[i].distance > 0f;
            if (hasHit)
            {
                Vector3 p = hits[i].point;
                points[i] = new float3(p.x, p.y, p.z);
            }
            else
            {
                Vector3 from = commands[i].from;
                Vector3 dir = commands[i].direction;
                Vector3 p = from + dir * maxDistance;
                points[i] = new float3(p.x, p.y, p.z);
            }
        }
    }

    // ========================= 메인 루프 =========================

    void Start()
    {
        RecalculateScanGrid();

        // gameObject 접근 위해 Start에서 실행
        fileName = $"LiDAR_PointCloud_{gameObject.name}.txt";
    }

    void Update()
    {

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // 런타임 설정 변경 반영
        AllocateIfNeeded();

        var origin = transform.position + scanOffset;
        var rot = transform.rotation;
        float maxDist = GM.settingParams.lidarMaxDistance_m;

        var setJob = new SetRaycastJob
        {
            origin = origin,
            rotation = rot,
            maxDistance = maxDist,
            resV_Rad = resV_Rad,
            resH_Rad = resH_Rad,
            hStart_Rad = hStartRad,
            vStart_Rad = vStartRad,
            hSteps = hSteps,
            commands = _commands,
            layerMask = ~0
        };

        // 1) RaycastCommand 생성
        var setHandle = setJob.Schedule(totalSteps, math.clamp(innerloopBatchCount, 32, 1024));

        // 2) 물리 캐스팅 (올바른 minCommandsPerJob 사용!)
        int minCmdPerJob = math.clamp(minCommandsPerJob, 32, 256);
        var rayHandle = RaycastCommand.ScheduleBatch(_commands, _hits, minCmdPerJob, setHandle);

        // 3) 결과 수집 (포인트 계산)
        var collectJob = new CollectPointsJob
        {
            hits = _hits,
            commands = _commands,
            maxDistance = maxDist,
            points = _points
        };
        var collectHandle = collectJob.Schedule(totalSteps, math.clamp(innerloopBatchCount, 32, 1024), rayHandle);

        JobHandle.ScheduleBatchedJobs();
        collectHandle.Complete();

        // ======= 디버그/출력 (옵션) =======
        if (debugDraw && (++_frame % math.max(1, debugDrawEveryNFrames) == 0))
        {
            Vector3 o = origin;
            for (int i = 0; i < totalSteps; ++i)
            {
                var p = _points[i];
                UnityEngine.Debug.DrawLine(o, new Vector3(p.x, p.y, p.z), Color.cyan, 0.02f, false);
            }
        }

        if (fillManagedListForDebug)
        {
            // 꼭 필요할 때만 관리 영역으로 복사 (GC/복사 비용 발생)
            pointCloud.Clear();
            for (int i = 0; i < totalSteps; ++i)
            {
                var p = _points[i];
                pointCloud.Add(new Vector3(p.x, p.y, p.z));
            }
        }

        if (saveToFile)
        {
            // 매 프레임 저장은 비권장! 필요 시 외부에서 한번만 호출하도록 설계하세요.
            saveToFile = false; // 실수 방지: 한 번만 저장
            // SavePointsToFile(fileName);
            SavePointsToFileBinary(fileName);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"{gameObject.name} 코드 실행 시간: {stopwatch.ElapsedMilliseconds} ms, Time.deltaTime : {Time.deltaTime} s");
    }

    // ========================= 유틸 =========================
    public NativeArray<float3> GetPointsNative() => _points; // ReadOnly로만 사용하세요.

    public void SavePointsToFile(string path)
    {
        try
        {
            using var sw = new StreamWriter(path);
            for (int i = 0; i < totalSteps; ++i)
            {
                var p = _points[i];
                sw.WriteLine($"{p.x} {p.y} {p.z}");
            }
            UnityEngine.Debug.Log($"Saved {totalSteps} points → {path}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"SavePointsToFile failed: {e.Message}");
        }
    }

    public void SavePointsToFileBinary(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1 << 20);
            using var bw = new BinaryWriter(fs);

            bw.Write(totalSteps); // 헤더: 포인트 개수 (int, little-endian)

            for (int i = 0; i < totalSteps; ++i)
            {
                var p = _points[i];
                bw.Write(p.x);
                bw.Write(p.y);
                bw.Write(p.z);
            }

            UnityEngine.Debug.Log($"Saved {totalSteps} points (binary) → {path}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"SavePointsToFileBinary failed: {e.Message}");
        }
    }

    // public void SavePointsToFileBinaryFast(string path)
    // {
    //     try
    //     {
    //         // 1) float3 배열을 float 배열로 재해석 (메모리 공유, 복사 없음)
    //         var floats = _points.Reinterpret<float>(UnsafeUtility.SizeOf<float3>());

    //         // 2) float 배열을 byte 배열로 재해석 (역시 복사 없음)
    //         var bytesNative = floats.Reinterpret<byte>(UnsafeUtility.SizeOf<float>());

    //         // 3) 헤더(포인트 개수) + 본문(좌표들)을 한 번에 파일로
    //         //    헤더는 별도로 붙여야 하므로 최종 바이트 배열이 한 번 필요
    //         int payloadBytes = bytesNative.Length;
    //         int totalBytes = sizeof(int) + payloadBytes; // 4바이트 헤더 + 데이터
    //         var managed = new byte[totalBytes];

    //         // 3-1) 헤더 쓰기 (Little Endian)
    //         System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(
    //             new Span<byte>(managed, 0, sizeof(int)), totalSteps);

    //         // 3-2) 본문 복사 (네이티브 → 매니지드, 한 번만)
    //         bytesNative.CopyTo(new NativeArray<byte>(managed, Allocator.Temp));

    //         // 4) 디스크 기록 (한 번에)
    //         File.WriteAllBytes(path, managed);

    //         UnityEngine.Debug.Log($"Saved {totalSteps} points (binary fast) → {path}");
    //     }
    //     catch (System.Exception e)
    //     {
    //         UnityEngine.Debug.LogError($"SavePointsToFileBinaryFast failed: {e.Message}");
    //     }
    // }
}




// using UnityEngine;
// using System.Collections.Generic;
// using System.IO;
// using System.Collections;
// using System.Diagnostics;
// using System.Threading;
// using UnityEngine.Jobs;
// using Unity.Collections;
// using Unity.Jobs;
// using Unity.Burst;

// public class LiDAR3DIJobParallelForMain : MonoBehaviour
// {

//     public Vector3 scanOffset = Vector3.zero;

//     [Header("Save Settings")]
//     public bool saveToFile = false;
//     public string fileName = "LiDAR_PointCloud.txt";
//     NativeArray<RaycastCommand> _commands;
//     NativeArray<RaycastHit> _hits;
//     public List<Vector3> pointCloud = new();
//     int hSteps, vSteps, totalSteps;
//     float hStart, vStart;

//     public int batchsize = 1;
//     public int minCommandsPerJob = 256;     // standard range : 32 ~ 256. totalSteps 보다 작아야 병렬 처리됨.

//     [BurstCompile(CompileSynchronously = true)]
//     public struct SetRaycastJob : IJobParallelFor
//     {
//         // constants
//         public Vector3 origin;
//         public Quaternion rotation;
//         public float maxDistance;
//         public float resVertical_deg, resHorizontal_deg;
//         public int hSteps;
//         public float hStart, vStart;
//         public NativeArray<RaycastCommand> RaycastCommands;
//         public LayerMask LayerMask;

//         [System.Obsolete]
//         public void Execute(int index)
//         {
//             int v = index / hSteps;
//             int h = index % hSteps;

//             float vAngle = vStart + v * resVertical_deg;
//             Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
//             Vector3 vDir = vRot * Vector3.right;

//             float hAngle = hStart + h * resHorizontal_deg;
//             Quaternion hRot = Quaternion.AngleAxis(hAngle, Vector3.up);
//             Vector3 dir = hRot * vDir;

//             RaycastCommands[index] = new RaycastCommand(origin, rotation * dir, maxDistance, LayerMask);
//         }
//     }


//     // =================================
//     // [3] 라이프사이클: 생성/해제 타이밍
//     // =================================

//     void OnEnable()
//     {
//         // 컴포넌트 활성화 시점에 컨테이너 생성
//         AllocateIfNeeded();
//     }

//     void OnDisable()
//     {
//         // 비활성화/파괴 시점에 꼭 해제 (메모리 누수 & Safety 에러 방지)
//         DisposeIfCreated();
//     }

//     // cols/rows가 바뀌었을 수 있으므로, 필요하면 (재)할당
//     void AllocateIfNeeded()
//     {

//         // 아직 미생성 또는 크기가 다르면 새로 잡음
//         if (!_commands.IsCreated || _commands.Length != totalSteps)
//         {
//             // 기존 것이 있으면 먼저 해제
//             DisposeIfCreated();

//             // ★ 핵심: 잡이 프레임을 넘어 사용하므로 Allocator.Persistent 사용
//             // - TempJob는 4프레임 이내 해제가 원칙이며, 넘어가면 Safety 에러
//             _commands = new NativeArray<RaycastCommand>(totalSteps, Allocator.Persistent);
//             _hits = new NativeArray<RaycastHit>(totalSteps, Allocator.Persistent);

//             // 참고: 메모리 계산
//             // RaycastCommand(대략 수십 바이트) * rayCount + RaycastHit(대략 수십 바이트) * rayCount
//             // cols/rows가 큰 경우 GC/메모리 부담 고려
//         }
//     }

//     // 네이티브 컨테이너 해제(생성됐을 때만)
//     void DisposeIfCreated()
//     {
//         // IsCreated == true 일 때만 Dispose 가능
//         if (_commands.IsCreated) _commands.Dispose();
//         if (_hits.IsCreated) _hits.Dispose();
//     }

//     void Start()
//     {
//         hStart = -GM.settingParams.lidarFovHorizontal_deg / 2f;
//         vStart = -GM.settingParams.lidarFovVertical_deg / 2f;

//         hSteps = Mathf.CeilToInt(GM.settingParams.lidarFovHorizontal_deg / GM.settingParams.lidarResHorizontal_deg);
//         vSteps = Mathf.CeilToInt(GM.settingParams.lidarFovVertical_deg / GM.settingParams.lidarResVertical_deg);
//         totalSteps = hSteps * vSteps;

//         pointCloud.Capacity = totalSteps;
//     }

//     void Update()
//     {

//         Stopwatch stopwatch = new Stopwatch();
//         stopwatch.Start();

//         // 인스펙터에서 런타임 조정 시 반영
//         AllocateIfNeeded();
//         pointCloud.Clear();

//         var setRaycastJob = new SetRaycastJob()
//         {
//             origin = transform.position + scanOffset,
//             rotation = transform.rotation,
//             maxDistance = GM.settingParams.lidarMaxDistance_m,
//             resVertical_deg = GM.settingParams.lidarResVertical_deg,
//             resHorizontal_deg = GM.settingParams.lidarResHorizontal_deg,
//             hSteps = hSteps,
//             hStart = hStart,
//             vStart = vStart,
//             RaycastCommands = _commands,
//             LayerMask = ~0 // All layers
//         };

//         var setHandle = setRaycastJob.Schedule(totalSteps, batchsize);
//         var rayHandle = RaycastCommand.ScheduleBatch(_commands, _hits, minCommandsPerJob, setHandle);
//         rayHandle.Complete();

//         // Drawing and storing results
//         for (int i = 0; i < totalSteps; ++i)
//         {
//             if (_hits[i].collider != null)
//             {
//                 pointCloud.Add(_hits[i].point);
//                 // UnityEngine.Debug.DrawLine(transform.position, _hits[i].point, Color.blue, 0.1f);
//             }
//             else
//             {
//                 pointCloud.Add(transform.position + (_commands[i].direction * GM.settingParams.lidarMaxDistance_m));
//                 // UnityEngine.Debug.DrawLine(transform.position, transform.position + (_commands[i].direction * GM.settingParams.lidarMaxDistance_m), Color.red, 0.1f);
//             }
//         }

//         stopwatch.Stop();
//         UnityEngine.Debug.Log($"{gameObject.name} 코드 실행 시간: {stopwatch.ElapsedMilliseconds} ms, Time.deltaTime : {Time.deltaTime} s,  batch/total : {batchsize}/{totalSteps}");
//     }
// }
