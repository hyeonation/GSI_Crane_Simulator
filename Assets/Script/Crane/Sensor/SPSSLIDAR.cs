using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System.IO;
using System.Threading.Tasks;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SPSSLIDAR : MonoBehaviour
{
    public enum CoordinateSystem { Local, World }

    [Header("LiDAR Hardware Settings")]
    [Range(1f, 360f)] public float lidarFovHorizontal_deg = 90f;
    [Range(1f, 180f)] public float lidarFovVertical_deg = 42.4f;
    [Range(0.05f, 5f)] public float lidarResHorizontal_deg = 0.5f;
    [Range(0.05f, 5f)] public float lidarResVertical_deg = 0.5f;
    public float lidarMaxDistance_m = 100f;

    [Header("Noise Settings")]
    public bool useNoise = true;
    [Range(0f, 0.2f)] public float noiseIntensity = 0.02f; // 거리 대비 오차 비율

    [Header("Sampling & Output")]
    public float scanRate = 10f;
    public CoordinateSystem saveCoordinate = CoordinateSystem.Local;
    private float _lastScanTime;

    [Header("Collision & Performance")]
    public LayerMask detectionLayer = 1;
    public bool drawPoints = true;
    public bool saveTrigger = false;

    private NativeArray<RaycastCommand> _commands;
    private NativeArray<RaycastHit> _hits;
    private NativeArray<float3> _points;
    private JobHandle _jobHandle;
    private bool _isJobScheduled = false;
    private bool _isSaving = false;
    private bool _needsReinit = false;

    private int _hSteps, _vSteps, _totalSteps;
    private Mesh _mesh;
    private int[] _indices;

    // SPSSLIDAR 클래스 내부에 추가
    public NativeArray<float3> GetPoints() => _points;
    public int TotalPoints => _totalSteps;
    public bool IsDataReady => _commands.IsCreated && !_isJobScheduled;

    void Awake()
    {
        _mesh = new Mesh { name = "LiDAR_Cloud", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    void Start() => ForceReinitialize();

    void OnValidate() { if (Application.isPlaying) _needsReinit = true; }

    private void ForceReinitialize()
    {
        if (_isJobScheduled) { _jobHandle.Complete(); _isJobScheduled = false; }
        DisposeMemory();

        _hSteps = Mathf.Max(1, Mathf.CeilToInt(lidarFovHorizontal_deg / lidarResHorizontal_deg));
        _vSteps = Mathf.Max(1, Mathf.CeilToInt(lidarFovVertical_deg / lidarResVertical_deg));
        _totalSteps = _hSteps * _vSteps;

        _indices = new int[_totalSteps];
        for (int i = 0; i < _totalSteps; i++) _indices[i] = i;

        _commands = new NativeArray<RaycastCommand>(_totalSteps, Allocator.Persistent);
        _hits = new NativeArray<RaycastHit>(_totalSteps, Allocator.Persistent);
        _points = new NativeArray<float3>(_totalSteps, Allocator.Persistent);

        _needsReinit = false;
        Debug.Log($"<color=cyan>[LiDAR]</color> Reinitialized. Steps: {_totalSteps}");
    }

    void Update()
    {
        if (_needsReinit) ForceReinitialize();
        if (!_commands.IsCreated) return;

        if (scanRate > 0 && Time.time < _lastScanTime + (1f / scanRate)) return;

        if (_isJobScheduled)
        {
            _jobHandle.Complete();
            _isJobScheduled = false;
            if (drawPoints) UpdateMesh();
            if (saveTrigger) SaveData();
        }

        _lastScanTime = Time.time;
        ScheduleLidarJobs();
    }

    private void ScheduleLidarJobs()
    {
        var setJob = new SetRaycastJob
        {
            origin = transform.position,
            rotation = transform.rotation,
            maxDistance = lidarMaxDistance_m,
            resV_Rad = math.radians(lidarResVertical_deg),
            resH_Rad = math.radians(lidarResHorizontal_deg),
            hStart_Rad = math.radians(-lidarFovHorizontal_deg * 0.5f),
            vStart_Rad = math.radians(-lidarFovVertical_deg * 0.5f),
            hSteps = _hSteps,
            layerMask = detectionLayer,
            commands = _commands
        };

        _jobHandle = setJob.Schedule(_totalSteps, 64);
        _jobHandle = RaycastCommand.ScheduleBatch(_commands, _hits, 128, _jobHandle);

        var collectJob = new CollectPointsJob
        {
            hits = _hits,
            commands = _commands,
            maxDistance = lidarMaxDistance_m,
            points = _points,
            lidarOrigin = transform.position,
            lidarRotationInverse = math.inverse(transform.rotation),
            useNoise = useNoise,
            noiseIntensity = noiseIntensity,
            seed = (uint)(Time.frameCount + 1)
        };

        _jobHandle = collectJob.Schedule(_totalSteps, 64, _jobHandle);
        _isJobScheduled = true;
    }

    private void UpdateMesh()
    {
        if (!_points.IsCreated) return;
        _mesh.SetVertices(_points.Reinterpret<Vector3>());
        if (_mesh.GetIndexCount(0) != _totalSteps)
            _mesh.SetIndices(_indices, MeshTopology.Points, 0);
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2000f);
    }

    private void SaveData()
    {
        if (_isSaving || !_points.IsCreated) return;
        saveTrigger = false;
        _isSaving = true;

        Vector3[] dataCopy = new Vector3[_totalSteps];
        _points.Reinterpret<Vector3>().CopyTo(dataCopy);

        string path = Path.Combine(Application.persistentDataPath, $"LiDAR_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
        Task.Run(() =>
        {
            try
            {
                using (var sw = new StreamWriter(path))
                {
                    sw.WriteLine("X,Y,Z");
                    foreach (var p in dataCopy) sw.WriteLine($"{p.x:F4},{p.y:F4},{p.z:F4}");
                }
                Debug.Log($"Saved: {path}");
            }
            finally { _isSaving = false; }
        });
    }

    void OnDestroy()
    {
        if (_isJobScheduled) _jobHandle.Complete();
        DisposeMemory();
    }

    private void DisposeMemory()
    {
        if (_commands.IsCreated) _commands.Dispose();
        if (_hits.IsCreated) _hits.Dispose();
        if (_points.IsCreated) _points.Dispose();
    }

    [BurstCompile]
    struct SetRaycastJob : IJobParallelFor
    {
        public float3 origin; public quaternion rotation;
        public float maxDistance, resV_Rad, resH_Rad, hStart_Rad, vStart_Rad;
        public int hSteps, layerMask;
        [WriteOnly] public NativeArray<RaycastCommand> commands;

        public void Execute(int i)
        {
            float vAng = vStart_Rad + (i / hSteps * resV_Rad);
            float hAng = hStart_Rad + (i % hSteps * resH_Rad);
            float cV = math.cos(vAng);
            float3 dir = math.mul(rotation, new float3(cV * math.sin(hAng), math.sin(vAng), cV * math.cos(hAng)));
            commands[i] = new RaycastCommand(origin, dir, new QueryParameters(layerMask, false, QueryTriggerInteraction.Ignore, false), maxDistance);
        }
    }

    [BurstCompile]
    struct CollectPointsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RaycastHit> hits;
        [ReadOnly] public NativeArray<RaycastCommand> commands;
        public float maxDistance;
        public float3 lidarOrigin; public quaternion lidarRotationInverse;
        public bool useNoise;
        public float noiseIntensity;
        public uint seed;
        public NativeArray<float3> points;

        public void Execute(int i)
        {
            float3 dir = commands[i].direction;
            float distance = hits[i].distance > 0 ? hits[i].distance : maxDistance;

            if (useNoise)
            {
                // Index와 Seed를 조합하여 고유한 난수 생성
                var rand = new Unity.Mathematics.Random(seed + (uint)i);
                // 거리에 비례하는 노이즈 적용 (가우시안 분포 근사)
                float noise = rand.NextFloat(-1f, 1f) * noiseIntensity * (distance / maxDistance);
                distance += noise;
            }

            float3 worldPos = (float3)commands[i].from + (dir * distance);
            points[i] = math.mul(lidarRotationInverse, worldPos - lidarOrigin);
        }
    }
}