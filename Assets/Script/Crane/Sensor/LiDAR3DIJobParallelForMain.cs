using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using Unity.Jobs;
using System.Diagnostics;
using System.Threading;
using UnityEngine.Jobs;
using Unity.Collections;


public class LiDAR3DIJobParallelForMain : MonoBehaviour {

    public Vector3 scanOffset = Vector3.zero;

    [Header("Save Settings")]
    public bool saveToFile = false;
    public string fileName = "LiDAR_PointCloud.txt";
    NativeArray<RaycastCommand> _commands;
    NativeArray<RaycastHit> _hits;
    public List<Vector3> pointCloud = new();
    int hSteps, vSteps, totalSteps;
    float hStart, vStart;

    public struct SetRaycastJob : IJobParallelFor
    {
        // constants
        public Vector3 origin;
        public Quaternion rotation;
        public float maxDistance;
        public int hSteps;
        public float hStart, vStart;
        public NativeArray<RaycastCommand> RaycastCommands;
        public LayerMask LayerMask;

        [System.Obsolete]
        public void Execute(int index)
        {
            int v = index / hSteps;
            int h = index % hSteps;

            float vAngle = vStart + v * GM.settingParams.lidarResVertical_deg;
            Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
            Vector3 vDir = vRot * Vector3.right;

            float hAngle = hStart + h * GM.settingParams.lidarResHorizontal_deg;
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
            _hits     = new NativeArray<RaycastHit>(totalSteps, Allocator.Persistent);

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
        if (_hits.IsCreated)     _hits.Dispose();
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
            hSteps = hSteps,
            hStart = hStart,
            vStart = vStart,
            RaycastCommands = _commands,
            LayerMask = ~0 // All layers
        };

        var setHandle = setRaycastJob.Schedule(totalSteps, 1);
        var rayHandle = RaycastCommand.ScheduleBatch(_commands, _hits, totalSteps, setHandle);
        rayHandle.Complete();
        // for (int i = 0; i < totalSteps; ++i)
        // {
        //     if (_hits[i].collider != null)
        //     {
        //         pointCloud.Add(_hits[i].point);
        //         // UnityEngine.Debug.DrawLine(transform.position, _hits[i].point, Color.blue, 0.1f);
        //     }
        //     else
        //     {
        //         pointCloud.Add(transform.position + (_commands[i].direction * GM.settingParams.lidarMaxDistance_m));
        //         // UnityEngine.Debug.DrawLine(transform.position, transform.position + (_commands[i].direction * GM.settingParams.lidarMaxDistance_m), Color.red, 0.1f);
        //     }
        // }
    }
}


// public class LiDAR3DIJobParallelForMain : MonoBehaviour {

//     float stepSize = 1f;
//     float distance = 10f;
//     NativeArray<RaycastCommand> _raycastCommands = new NativeArray<RaycastCommand>();
//     LayerMask _layerMask = ~0;
//     int totalRay = 360;
//     NativeArray<RaycastHit> _raycastHits = new NativeArray<RaycastHit>();

//     public struct SetRaycastJob : IJobParallelFor
//     {
//         public Vector3 Diection;
//         public Vector3 Start;
//         public float StepSize;
//         public float Distance;
//         public NativeArray<RaycastCommand> RaycastCommands;
//         public LayerMask LayerMask;

//         [System.Obsolete]
//         public void Execute(int index)
//         {
//             float angle = StepSize * index;
//             float rad = angle * Mathf.Deg2Rad;
//             Diection = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
//             RaycastCommands[index] = new RaycastCommand(Start, Diection, Distance, LayerMask);
//         }
//     }
    
//     void Update()
//     {
//         var setRaycastJob = new SetRaycastJob()
//         {
//             Start = transform.position,
//             StepSize = stepSize,
//             Distance = distance,
//             RaycastCommands = _raycastCommands,
//             LayerMask = _layerMask
//         };

//         var setHandle = setRaycastJob.Schedule(totalRay, 1);
//         var rayHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastHits, totalRay, setHandle);
//         rayHandle.Complete();
//         for (int i = 0; i < totalRay; ++i)
//         {
//             if (_raycastHits[i].collider != null)
//             {
//                 UnityEngine.Debug.DrawLine(transform.position, _raycastHits[i].point, Color.blue, 0.1f);
//             }
//             else
//             {
//                 UnityEngine.Debug.DrawLine(transform.position, transform.position + (_raycastCommands[i].direction* distance), Color.red, 0.1f);
//             }
//         }
//     }
// }





// class ApplyVelocityParallelForSample : MonoBehaviour
// {
//     struct VelocityJob : IJobParallelFor
//     {
//         // Jobs declare all data that will be accessed in the job
//         // By declaring it as read only, multiple jobs are allowed to access the data in parallel
//         [ReadOnly]
//         public NativeArray<Vector3> velocity;

//         // By default containers are assumed to be read &amp; write
//         public NativeArray<Vector3> position;

//         // Delta time must be copied to the job since jobs generally don't have concept of a frame.
//         // The main thread waits for the job same frame or next frame, but the job should do work deterministically
//         // independent on when the job happens to run on the worker threads.
//         public float deltaTime;
//         RaycastHit hit;


//         // The code actually running on the job
//         public void Execute(int i)
//         {
//             // Move the positions based on delta time and velocity
//             position[i] = position[i] + velocity[i] * deltaTime;

//             if (Physics.Raycast(position[i], Vector3.down, out hit, 10f))
//             {
//                 // If the raycast hits something, adjust the position
//                 position[i] = hit.point;
//             }
//         }
//     }

//     public void Update()
//     {
//         var position = new NativeArray<Vector3>(500, Allocator.Persistent);

//         var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
//         for (var i = 0; i < velocity.Length; i++)
//             velocity[i] = new Vector3(0, 10, 0);

//         // Initialize the job data
//         var job = new VelocityJob()
//         {
//             deltaTime = Time.deltaTime,
//             position = position,
//             velocity = velocity
//         };

//         // Schedule a parallel-for job. First parameter is how many for-each iterations to perform.
//         // The second parameter is the batch size,
//         // essentially the no-overhead innerloop that just invokes Execute(i) in a loop.
//         // When there is a lot of work in each iteration then a value of 1 can be sensible.
//         // When there is very little work values of 32 or 64 can make sense.
//         JobHandle jobHandle = job.Schedule(position.Length, 64);

//         // Ensure the job has completed.
//         // It is not recommended to Complete a job immediately,
//         // since that reduces the chance of having other jobs run in parallel with this one.
//         // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
//         jobHandle.Complete();

//         UnityEngine.Debug.Log($"{job.position[456]}, {Time.deltaTime}");

//         // Native arrays must be disposed manually.
//         position.Dispose();
//         velocity.Dispose();
//     }
// }



// // [BurstCompile(CompileSynchronously = true)]
// public class LiDAR3DIJobParallelForMain : MonoBehaviour
// {
//     public Vector3 scanOffset = Vector3.zero;

//     [Header("Save Settings")]
//     public bool saveToFile = false;
//     public string fileName = "LiDAR_PointCloud.txt";
//     public float scanDelay = 5f; // 스캔 간격
//     public bool run = true; // 스캔 실행 여부

//     private NativeArray<Vector3> pointCloud = new NativeArray<Vector3>();

//     int hSteps, vSteps;

//     float hStart, vStart;

//     struct LiDAR3DIJobParallelForTransform : IJobParallelForTransform
//     {
//         public Vector3 scanOffset;

//         [Header("Save Settings")]
//         public bool saveToFile;
//         // public string fileName;
//         public float scanDelay;
//         public bool run;

//         private NativeArray<Vector3> pointCloud;

//         int hSteps, vSteps;

//         float hStart, vStart;

//         // public LiDAR3DIJobParallelForTransform(Vector3 offset, bool save, string file, float delay, bool isRunning)
//         public LiDAR3DIJobParallelForTransform(Vector3 offset, bool save, float delay, bool isRunning, NativeArray<Vector3> pointCloud)
//         {
//             scanOffset = offset;
//             saveToFile = save;
//             // fileName = file;
//             scanDelay = delay;
//             run = isRunning;

//             this.pointCloud = pointCloud;

//             hSteps = Mathf.CeilToInt(GM.settingParams.lidarFovHorizontal_deg / GM.settingParams.lidarResHorizontal_deg);
//             vSteps = Mathf.CeilToInt(GM.settingParams.lidarFovVertical_deg / GM.settingParams.lidarResVertical_deg);
//             hStart = -GM.settingParams.lidarFovHorizontal_deg / 2f;
//             vStart = -GM.settingParams.lidarFovVertical_deg / 2f;
//         }

//         void Scan(TransformAccess transform)
//         {
//             Clear();
//             Vector3 origin = transform.position + scanOffset;

//             int idx = 0;

//             for (int v = 0; v < vSteps; v++)
//             {
//                 float vAngle = vStart + v * GM.settingParams.lidarResVertical_deg;
//                 Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
//                 Vector3 vDir = vRot * Vector3.right;

//                 for (int h = 0; h < hSteps; h++)
//                 {
//                     float hAngle = hStart + h * GM.settingParams.lidarResHorizontal_deg;
//                     Quaternion hRot = Quaternion.AngleAxis(hAngle, Vector3.up);
//                     Vector3 dir = hRot * vDir;

//                     if (Physics.Raycast(origin, transform.rotation * dir, out RaycastHit hit, GM.settingParams.lidarMaxDistance_m))
//                     {
//                         Vector3 noisyPoint = hit.point + Random.insideUnitSphere * GM.settingParams.lidarNoiseStd;
//                         pointCloud[idx++] = noisyPoint;
//                         UnityEngine.Debug.DrawLine(origin, noisyPoint, Color.green, 0.1f);
//                     }
//                     else
//                     {
//                         UnityEngine.Debug.DrawRay(origin, transform.rotation * dir * GM.settingParams.lidarMaxDistance_m, Color.red, 0.1f);
//                     }
//                 }
//             }
//         }

//         // // Save the point cloud to a file
//         // void SavePointCloud()
//         // {
//         //     string path = Path.Combine(Application.dataPath, fileName);
//         //     using (StreamWriter writer = new StreamWriter(path, false))
//         //     {
//         //         foreach (var pt in pointCloud)
//         //         {
//         //             writer.WriteLine($"{pt.x:F4},{pt.y:F4},{pt.z:F4}");
//         //         }
//         //     }
//         //     UnityEngine.Debug.Log($"LiDAR point cloud saved to: {path}");
//         // }

//         public void Execute(int index, TransformAccess transform)
//         {
//             Clear();
//             Scan(transform);

//             // if (saveToFile)
//             // {
//             //     SavePointCloud();
//             //     saveToFile = false; // 한 번만 저장
//             // }

//             // throw new System.NotImplementedException();
//         }

//         public void Clear()
//         {

//             for (int i = 0; i < pointCloud.Length; i++)
//             {
//                 pointCloud[i] = Vector3.zero;
//             }
//         }
//     }

//     void Start()
//     {
//         hSteps = Mathf.CeilToInt(GM.settingParams.lidarFovHorizontal_deg / GM.settingParams.lidarResHorizontal_deg);
//         vSteps = Mathf.CeilToInt(GM.settingParams.lidarFovVertical_deg / GM.settingParams.lidarResVertical_deg);
//         hStart = -GM.settingParams.lidarFovHorizontal_deg / 2f;
//         vStart = -GM.settingParams.lidarFovVertical_deg / 2f;

//         // 값 확인
//         UnityEngine.Debug.Log($"LiDAR3D started with {hSteps} horizontal steps and {vSteps} vertical steps.");

//         // hstart와 vStart 값 확인
//         UnityEngine.Debug.Log($"Horizontal Start: {hStart}, Vertical Start: {vStart}");

//         // Thread thread = new Thread(Run);
//         // thread.IsBackground = true; // 백그라운드 스레드로 설정
//         // thread.Start();
//     }

//     void Update()
//     {
//         TransformAccessArray transforms = new TransformAccessArray(new Transform[] { transform }, 1);
//         // JobHandle jobHandle = new LiDAR3DIJobParallelForTransform(scanOffset, saveToFile, fileName, scanDelay, run).Schedule(transforms);
//         JobHandle jobHandle = new LiDAR3DIJobParallelForTransform(scanOffset, saveToFile, scanDelay, run, pointCloud).Schedule(transforms);
//         jobHandle.Complete();
//         transforms.Dispose();
//     }


//     // IEnumerator RunLiDAR()
//     // {
//     //     while (run)
//     //     {
//     //         yield return new WaitForSeconds(scanDelay); // 초기화 대기

//     //         // Stopwatch stopwatch = new Stopwatch();
//     //         // stopwatch.Start();

//     //         pointCloud.Clear();
//     //         Scan();

//     //         if (saveToFile)
//     //         {
//     //             SavePointCloud();
//     //             saveToFile = false; // 한 번만 저장
//     //         }

//     //         // stopwatch.Stop();
//     //         // UnityEngine.Debug.Log("코드 실행 시간: " + stopwatch.ElapsedMilliseconds + "ms");
//     //     }
//     // }

//     // // disable the script to stop scanning
//     // void OnDisable()
//     // {
//     //     run = false; // 스캔 중지
//     //     pointCloud.Clear(); // 포인트 클라우드 초기화
//     // }

//     // // enable the script to start scanning
//     // void OnEnable()
//     // {
//     //     run = true; // 스캔 재개
//     //     StartCoroutine(RunLiDAR());
//     // }

//     // void Run()
//     // {
//     //     pointCloud.Clear();
//     //     Scan();

//     //     if (saveToFile)
//     //     {
//     //         SavePointCloud();
//     //         saveToFile = false; // 한 번만 저장
//     //     }
//     // }

//     // void Scan()
//     // {
//     //     Vector3 origin = transform.position + scanOffset;

//     //     for (int v = 0; v < vSteps; v++)
//     //     {
//     //         float vAngle = vStart + v * GM.settingParams.lidarResVertical_deg;
//     //         Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
//     //         Vector3 vDir = vRot * Vector3.right;

//     //         for (int h = 0; h < hSteps; h++)
//     //         {
//     //             float hAngle = hStart + h * GM.settingParams.lidarResHorizontal_deg;
//     //             Quaternion hRot = Quaternion.AngleAxis(hAngle, Vector3.up);
//     //             Vector3 dir = hRot * vDir;

//     //             if (Physics.Raycast(origin, transform.rotation * dir, out RaycastHit hit, GM.settingParams.lidarMaxDistance_m))
//     //             {
//     //                 Vector3 noisyPoint = hit.point + Random.insideUnitSphere * GM.settingParams.lidarNoiseStd;
//     //                 pointCloud.Add(noisyPoint);
//     //                 UnityEngine.Debug.DrawLine(origin, noisyPoint, Color.green, 0.1f);
//     //             }
//     //             else
//     //             {
//     //                 UnityEngine.Debug.DrawRay(origin, transform.rotation * dir * GM.settingParams.lidarMaxDistance_m, Color.red, 0.1f);
//     //             }
//     //         }
//     //     }
//     // }

//     // void SavePointCloud()
//     // {
//     //     string path = Path.Combine(Application.dataPath, fileName);
//     //     using (StreamWriter writer = new StreamWriter(path, false))
//     //     {
//     //         foreach (var pt in pointCloud)
//     //         {
//     //             writer.WriteLine($"{pt.x:F4},{pt.y:F4},{pt.z:F4}");
//     //         }
//     //     }
//     //     UnityEngine.Debug.Log($"LiDAR point cloud saved to: {path}");
//     // }
// }