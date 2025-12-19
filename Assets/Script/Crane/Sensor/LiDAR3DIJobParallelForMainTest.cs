using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LiDAR3DIJobParallelForMainTest : MonoBehaviour
{
    [Header("LiDAR Hardware Settings")]
    [Range(1f, 360f)] public float lidarFovHorizontal_deg = 90f;
    [Range(1f, 180f)] public float lidarFovVertical_deg = 42.4f;
    
    [Tooltip("해상도가 낮을수록(0.1 등) 점의 개수가 기하급수적으로 늘어납니다.")]
    [Range(0.1f, 10f)] public float lidarResHorizontal_deg = 0.175f;
    [Range(0.1f, 10f)] public float lidarResVertical_deg = 0.33f;
    
    public float lidarMaxDistance_m = 90f;

    [Header("Job Tuning")]
    public int innerloopBatchCount = 64;
    public int minCommandsPerJob = 128;

    [Header("Performance Optimization")]
    public bool deferJobCompletion = true;

    [Header("Visualization (Mesh)")]
    [Tooltip("체크 해제 시 연산만 수행하고 그리지는 않습니다 (성능 절약).")]
    public bool drawPoints = true;

    [Header("Save Settings")]
    public bool saveTrigger = false;
    string fileName;

    // Native Containers
    NativeArray<RaycastCommand> _commands;
    NativeArray<RaycastHit> _hits;
    NativeArray<float3> _points;

    // Job Handles
    JobHandle _jobHandle;
    bool _isJobScheduled = false;

    // Caches
    int hSteps, vSteps, totalSteps;
    float hStartRad, vStartRad;
    float resH_Rad, resV_Rad;

    // Visualization
    Mesh _mesh;
    MeshFilter _meshFilter;
    int[] _indices; // 메쉬 포인트 인덱스 캐싱

    // ========================= 라이프사이클 =========================

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        
        // 동적 메쉬 초기화
        _mesh = new Mesh
        {
            name = "LiDAR Point Cloud",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // 65535개 이상의 점을 지원하기 위해
        };
        _meshFilter.sharedMesh = _mesh;

        if (string.IsNullOrEmpty(fileName))
            fileName = $"LiDAR_{gameObject.name}.txt";

        ValidateParameters();
        RecalculateScanGrid();
        AllocateMemory();
    }

    void OnDestroy()
    {
        // 잡이 스케줄링 된 상태에서 종료되면 에러가 발생하므로 대기 후 해제
        if (_isJobScheduled)
        {
            _jobHandle.Complete();
        }
        DisposeMemory();
    }

    // 인스펙터 값이 변경되면 재계산 (Editor Only)
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ValidateParameters();
            // 런타임 중 변경 시 다음 프레임 루프에서 재할당됨
        }
    }

    void ValidateParameters()
    {
        // 안전 장치: 해상도가 너무 작으면 다운될 수 있음
        lidarResHorizontal_deg = Mathf.Max(0.05f, lidarResHorizontal_deg);
        lidarResVertical_deg = Mathf.Max(0.05f, lidarResVertical_deg);
    }

    public void RecalculateScanGrid()
    {
        hSteps = Mathf.CeilToInt(lidarFovHorizontal_deg / lidarResHorizontal_deg);
        vSteps = Mathf.CeilToInt(lidarFovVertical_deg / lidarResVertical_deg);
        totalSteps = Mathf.Max(1, hSteps * vSteps);

        float hStartDeg = -lidarFovHorizontal_deg * 0.5f;
        float vStartDeg = -lidarFovVertical_deg * 0.5f;

        hStartRad = math.radians(hStartDeg);
        vStartRad = math.radians(vStartDeg);
        resH_Rad = math.radians(lidarResHorizontal_deg);
        resV_Rad = math.radians(lidarResVertical_deg);

        // 인덱스 배열 미리 생성 (Mesh Topology용)
        if (_indices == null || _indices.Length != totalSteps)
        {
            _indices = new int[totalSteps];
            for (int i = 0; i < totalSteps; i++) _indices[i] = i;
        }
    }

    void AllocateMemory()
    {
        if (_isJobScheduled) _jobHandle.Complete();
        DisposeMemory();

        _commands = new NativeArray<RaycastCommand>(totalSteps, Allocator.Persistent);
        _hits = new NativeArray<RaycastHit>(totalSteps, Allocator.Persistent);
        _points = new NativeArray<float3>(totalSteps, Allocator.Persistent);
    }

    void DisposeMemory()
    {
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
        if (_points.IsCreated) _points.Dispose();
    }

    // ========================= 메인 루프 =========================

    void Update()
    {
        // 1. 이전 프레임의 잡 완료 처리
        if (_isJobScheduled)
        {
            _jobHandle.Complete();
            _isJobScheduled = false;
            
            // 결과 처리 (시각화 및 저장)
            ProcessResults();
        }

        // 2. 해상도 변경 등에 따른 재할당 체크
        if (!_commands.IsCreated || _commands.Length != totalSteps)
        {
            RecalculateScanGrid();
            AllocateMemory();
        }

        // 3. 다음 잡 스케줄링
        ScheduleJobs();

        // 4. 즉시 완료 모드일 경우 바로 대기 (지연 처리 안 함)
        if (!deferJobCompletion)
        {
            _jobHandle.Complete();
            _isJobScheduled = false;
            ProcessResults();
        }
    }

    void ScheduleJobs()
    {
        var origin = transform.position;
        var rot = transform.rotation;

        var setJob = new SetRaycastJob
        {
            origin = origin,
            rotation = rot,
            maxDistance = lidarMaxDistance_m,
            resV_Rad = resV_Rad,
            resH_Rad = resH_Rad,
            hStart_Rad = hStartRad,
            vStart_Rad = vStartRad,
            hSteps = hSteps,
            commands = _commands,
            layerMask = ~0 // 필요시 레이어 마스크 수정
        };

        var setHandle = setJob.Schedule(totalSteps, innerloopBatchCount);
        
        // RaycastBatch 실행 (물리 연산)
        var rayHandle = RaycastCommand.ScheduleBatch(_commands, _hits, minCommandsPerJob, setHandle);

        // [수정됨] 월드 좌표 -> 로컬 좌표 변환을 위해 origin과 역회전(inverse rotation) 값을 전달
        var collectJob = new CollectPointsJob
        {
            hits = _hits,
            commands = _commands,
            maxDistance = lidarMaxDistance_m,
            points = _points,
            lidarOrigin = origin,                // [추가] 라이다 원점
            lidarRotationInverse = math.inverse(rot) // [추가] 라이다 회전의 역행렬 (쿼터니언)
        };

        _jobHandle = collectJob.Schedule(totalSteps, innerloopBatchCount, rayHandle);
        
        JobHandle.ScheduleBatchedJobs();
        _isJobScheduled = true;
    }

    // ========================= 결과 처리 (Mesh Update) =========================
    
    void ProcessResults()
    {
        // 1. Mesh 업데이트 (가장 빠른 방법)
        if (drawPoints && _mesh != null)
        {
            // NativeArray를 바로 Mesh 데이터로 사용 (Unity 2019.3+)
            // float3는 Vector3와 메모리 레이아웃이 같으므로 재해석 가능
            _mesh.SetVertices(_points.Reinterpret<Vector3>());
            
            // 점의 개수가 바뀌었을 수 있으므로 인덱스 업데이트
            if (_mesh.GetIndexCount(0) != totalSteps)
            {
                 // 인덱스 배열 크기가 안맞으면 재조정
                 if (_indices.Length != totalSteps) {
                     _indices = new int[totalSteps];
                     for(int k=0; k<totalSteps; k++) _indices[k] = k;
                 }
                 _mesh.SetIndices(_indices, MeshTopology.Points, 0);
            }
            
            // 바운드 재계산 (카메라 밖에서도 보이게 하려면 수동 설정 권장)
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
        }

        // 2. 파일 저장 트리거
        if (saveTrigger)
        {
            saveTrigger = false;
            SavePointsAsyncWrapper();
        }
    }

    // ========================= 비동기 저장 (Async) =========================

    async void SavePointsAsyncWrapper()
    {
        UnityEngine.Debug.Log($"[LiDAR] Saving {totalSteps} points (Async start)...");

        // 메인 스레드 데이터 복사
        Vector3[] dataToSave = new Vector3[totalSteps];
        _points.Reinterpret<Vector3>().CopyTo(dataToSave);

        // 별도 스레드에서 I/O 수행
        await Task.Run(() => WriteFileAsync(fileName, dataToSave));

        UnityEngine.Debug.Log($"[LiDAR] Save Complete -> {fileName}");
    }

    // 기존 WriteFileAsync 함수를 이 코드로 덮어쓰세요.
    void WriteFileAsync(string path, Vector3[] data)
    {
        try
        {
            // 텍스트 파일 쓰기 도구 (StreamWriter) 생성
            using (var sw = new StreamWriter(path))
            {

                // 데이터 개수만큼 반복
                foreach (var p in data)
                {
                    // 문자열 보간($)을 사용해 "X, Y, Z" 형태로 한 줄씩 기록
                    // 예: 12.5, 3.4, 0.5
                    sw.WriteLine($"{p.x}, {p.y}, {p.z}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LiDAR] Error saving file: {e.Message}");
        }
    }

    // ========================= Burst Jobs =========================

    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct SetRaycastJob : IJobParallelFor
    {
        [ReadOnly] public Vector3 origin;
        [ReadOnly] public Quaternion rotation;
        public float maxDistance;
        public float resV_Rad, resH_Rad;
        public float hStart_Rad, vStart_Rad;
        public int hSteps;
        public int layerMask;

        [WriteOnly] public NativeArray<RaycastCommand> commands;

        public void Execute(int index)
        {
            int v = index / hSteps;
            int h = index % hSteps;

            // 라디안 계산
            float vAng = vStart_Rad + (v * resV_Rad);
            float hAng = hStart_Rad + (h * resH_Rad);

            // 삼각함수 연산 (Burst 최적화)
            math.sincos(vAng, out float sV, out float cV);
            math.sincos(hAng, out float sH, out float cH);

            // 구면 좌표계 -> 직교 좌표계
            Vector3 localDir = new Vector3(cV * sH, sV, cV * cH);
            Vector3 worldDir = math.mul(rotation, localDir);

            // [수정된 부분] Unity 최신 버전 대응 (QueryParameters 사용)
            // 에러 원인: 구버전 생성자 new RaycastCommand(origin, worldDir, maxDistance, layerMask) 가 삭제됨
            
            // 1. 레이캐스트 설정값(레이어 마스크 등)을 미리 묶습니다.
            // 인자 순서: (layerMask, hitMultipleFaces, hitTriggers, hitBackfaces)
            var queryParams = new QueryParameters(layerMask, false, QueryTriggerInteraction.UseGlobal, false);

            // 2. 설정값을 포함하여 명령을 생성합니다.
            commands[index] = new RaycastCommand(origin, worldDir, queryParams, maxDistance);
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct CollectPointsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> hits;
        [ReadOnly] public NativeArray<RaycastCommand> commands;
        public float maxDistance;

        // [추가] 월드 -> 로컬 변환을 위한 데이터
        public float3 lidarOrigin;
        public quaternion lidarRotationInverse;

        [WriteOnly] public NativeArray<float3> points;

        public void Execute(int i)
        {
            float3 worldPoint;

            // 충돌 여부 확인
            if (hits[i].distance > 0f)
            {
                worldPoint = hits[i].point;
            }
            else
            {
                // 미충돌 시 레이 끝부분 계산
                var cmd = commands[i];
                worldPoint = cmd.from + (cmd.direction * maxDistance);
            }

            // [수정됨] 월드 좌표를 라이다 기준 로컬 좌표로 변환
            // 공식: Local = InverseRotation * (World - Origin)
            float3 relativePos = worldPoint - lidarOrigin;
            points[i] = math.mul(lidarRotationInverse, relativePos);
        }
    }
}