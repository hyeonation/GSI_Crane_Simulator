using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Diagnostics;

public class LiDAR3D : MonoBehaviour
{
    public Vector3 scanOffset = Vector3.zero;

    [Header("Save Settings")]
    public bool saveToFile = false;
    public string fileName = "LiDAR_PointCloud.txt";
    public float scanDelay = 5f; // 스캔 간격
    public bool run = true; // 스캔 실행 여부

    private List<Vector3> pointCloud = new List<Vector3>();

    int hSteps, vSteps;

    float hStart, vStart;

    void Start()
    {
        hSteps = Mathf.CeilToInt(GM.settingParams.lidarFovHorizontal_deg / GM.settingParams.lidarResHorizontal_deg);
        vSteps = Mathf.CeilToInt(GM.settingParams.lidarFovVertical_deg / GM.settingParams.lidarResVertical_deg);
        hStart = -GM.settingParams.lidarFovHorizontal_deg / 2f;
        vStart = -GM.settingParams.lidarFovVertical_deg / 2f;

        // 값 확인
        UnityEngine.Debug.Log($"LiDAR3D started with {hSteps} horizontal steps and {vSteps} vertical steps.");

        // hstart와 vStart 값 확인
        UnityEngine.Debug.Log($"Horizontal Start: {hStart}, Vertical Start: {vStart}");
    }


    IEnumerator RunLiDAR()
    {
        while (run)
        {
            yield return new WaitForSeconds(scanDelay); // 초기화 대기

            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();

            pointCloud.Clear();
            Scan();

            if (saveToFile)
            {
                SavePointCloud();
                saveToFile = false; // 한 번만 저장
            }

            // stopwatch.Stop();
            // UnityEngine.Debug.Log("코드 실행 시간: " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }

    // disable the script to stop scanning
    void OnDisable()
    {
        run = false; // 스캔 중지
        pointCloud.Clear(); // 포인트 클라우드 초기화
    }

    // enable the script to start scanning
    void OnEnable()
    {
        run = true; // 스캔 재개
        StartCoroutine(RunLiDAR());
    }

    void Run()
    {
        pointCloud.Clear();
        Scan();

        if (saveToFile)
        {
            SavePointCloud();
            saveToFile = false; // 한 번만 저장
        }
    }

    void Scan()
    {
        Vector3 origin = transform.position + scanOffset;

        for (int v = 0; v < vSteps; v++)
        {
            float vAngle = vStart + v * GM.settingParams.lidarResVertical_deg;
            Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
            Vector3 vDir = vRot * Vector3.right;

            for (int h = 0; h < hSteps; h++)
            {
                float hAngle = hStart + h * GM.settingParams.lidarResHorizontal_deg;
                Quaternion hRot = Quaternion.AngleAxis(hAngle, Vector3.up);
                Vector3 dir = hRot * vDir;

                if (Physics.Raycast(origin, transform.rotation * dir, out RaycastHit hit, GM.settingParams.lidarMaxDistance_m))
                {
                    // Vector3 noisyPoint = hit.point + Random.insideUnitSphere * GM.settingParams.lidarNoiseStd;
                    // pointCloud.Add(noisyPoint);
                    // UnityEngine.Debug.DrawLine(origin, noisyPoint, Color.green, 0.1f);
                }
                else
                {
                    // UnityEngine.Debug.DrawRay(origin, transform.rotation * dir * GM.settingParams.lidarMaxDistance_m, Color.red, 0.1f);
                }
            }
        }
    }

    void SavePointCloud()
    {
        string path = Path.Combine(Application.dataPath, fileName);
        using (StreamWriter writer = new StreamWriter(path, false))
        {
            foreach (var pt in pointCloud)
            {
                writer.WriteLine($"{pt.x:F4},{pt.y:F4},{pt.z:F4}");
            }
        }
        UnityEngine.Debug.Log($"LiDAR point cloud saved to: {path}");
    }
}