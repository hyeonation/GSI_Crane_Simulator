using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;


public class LiDAR3DIJobParallelForMain : MonoBehaviour
{

    public Vector3 scanOffset = Vector3.zero;

    [Header("Save Settings")]
    public bool saveToFile = false;
    public string fileName = "LiDAR_PointCloud.txt";
    NativeArray<RaycastCommand> _commands;
    NativeArray<RaycastHit> _hits;
    public List<Vector3> pointCloud = new();
    int hSteps, vSteps, totalSteps;
    float hStart, vStart;

    [BurstCompile(CompileSynchronously = true)]
    public struct SetRaycastJob : IJobParallelFor
    {
        // constants
        public Vector3 origin;
        public Quaternion rotation;
        public float maxDistance;
        public float resVertical_deg, resHorizontal_deg;
        public int hSteps;
        public float hStart, vStart;
        public NativeArray<RaycastCommand> RaycastCommands;
        public LayerMask LayerMask;

        [System.Obsolete]
        public void Execute(int index)
        {
            int v = index / hSteps;
            int h = index % hSteps;

            float vAngle = vStart + v * resVertical_deg;
            Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
            Vector3 vDir = vRot * Vector3.right;

            float hAngle = hStart + h * resHorizontal_deg;
            Quaternion hRot = Quaternion.AngleAxis(hAngle, Vector3.up);
            Vector3 dir = hRot * vDir;

            RaycastCommands[index] = new RaycastCommand(origin, rotation * dir, maxDistance, LayerMask);
        }
    }


    // =================================
    // [3] 라이프사이클: 생성/해제 타이밍
    // =================================

    void OnEnable()
    {
        // 컴포넌트 활성화 시점에 컨테이너 생성
        AllocateIfNeeded();
    }

    void OnDisable()
    {
        // 비활성화/파괴 시점에 꼭 해제 (메모리 누수 & Safety 에러 방지)
        DisposeIfCreated();
    }

    // cols/rows가 바뀌었을 수 있으므로, 필요하면 (재)할당
    void AllocateIfNeeded()
    {

        // 아직 미생성 또는 크기가 다르면 새로 잡음
        if (!_commands.IsCreated || _commands.Length != totalSteps)
        {
            // 기존 것이 있으면 먼저 해제
            DisposeIfCreated();

            // ★ 핵심: 잡이 프레임을 넘어 사용하므로 Allocator.Persistent 사용
            // - TempJob는 4프레임 이내 해제가 원칙이며, 넘어가면 Safety 에러
            _commands = new NativeArray<RaycastCommand>(totalSteps, Allocator.Persistent);
            _hits = new NativeArray<RaycastHit>(totalSteps, Allocator.Persistent);

            // 참고: 메모리 계산
            // RaycastCommand(대략 수십 바이트) * rayCount + RaycastHit(대략 수십 바이트) * rayCount
            // cols/rows가 큰 경우 GC/메모리 부담 고려
        }
    }

    // 네이티브 컨테이너 해제(생성됐을 때만)
    void DisposeIfCreated()
    {
        // IsCreated == true 일 때만 Dispose 가능
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
    }

    void Start()
    {
        hStart = -GM.settingParams.lidarFovHorizontal_deg / 2f;
        vStart = -GM.settingParams.lidarFovVertical_deg / 2f;

        hSteps = Mathf.CeilToInt(GM.settingParams.lidarFovHorizontal_deg / GM.settingParams.lidarResHorizontal_deg);
        vSteps = Mathf.CeilToInt(GM.settingParams.lidarFovVertical_deg / GM.settingParams.lidarResVertical_deg);
        totalSteps = hSteps * vSteps;
    }

    void Update()
    {
        // 인스펙터에서 런타임 조정 시 반영
        AllocateIfNeeded();
        pointCloud.Clear();

        var setRaycastJob = new SetRaycastJob()
        {
            origin = transform.position + scanOffset,
            rotation = transform.rotation,
            maxDistance = GM.settingParams.lidarMaxDistance_m,
            resVertical_deg = GM.settingParams.lidarResVertical_deg,
            resHorizontal_deg = GM.settingParams.lidarResHorizontal_deg,
            hSteps = hSteps,
            hStart = hStart,
            vStart = vStart,
            RaycastCommands = _commands,
            LayerMask = ~0 // All layers
        };

        var setHandle = setRaycastJob.Schedule(totalSteps, 1);
        var rayHandle = RaycastCommand.ScheduleBatch(_commands, _hits, totalSteps, setHandle);
        rayHandle.Complete();

        // Drawing and storing results
        for (int i = 0; i < totalSteps; ++i)
        {
            if (_hits[i].collider != null)
            {
                pointCloud.Add(_hits[i].point);
                // UnityEngine.Debug.DrawLine(transform.position, _hits[i].point, Color.blue, 0.1f);
            }
            else
            {
                pointCloud.Add(transform.position + (_commands[i].direction * GM.settingParams.lidarMaxDistance_m));
                // UnityEngine.Debug.DrawLine(transform.position, transform.position + (_commands[i].direction * GM.settingParams.lidarMaxDistance_m), Color.red, 0.1f);
            }
        }
    }
}
