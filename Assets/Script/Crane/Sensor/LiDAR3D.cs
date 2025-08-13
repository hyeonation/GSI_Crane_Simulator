using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LiDAR3D : MonoBehaviour
{
    [Header("LiDAR Settings")]
    public float maxDistance = 10f;
    public float horizontalFOV = 90f; // 수평 시야각 Field of View
    public float verticalFOV = 20f;   // 수직 시야각
    public float horizontalResolution = 1f; // 수평 각도 간격(도)
    public float verticalResolution = 1f;   // 수직 각도 간격(도)

    public Vector3 scanOffset = Vector3.zero;

    [Header("Noise Settings")]
    public float noiseStdDev = 0.01f; // 노이즈 표준편차 (미터 단위)

    [Header("Save Settings")]
    public bool saveToFile = false;
    public string fileName = "LiDAR_PointCloud.txt";

    private List<Vector3> pointCloud = new List<Vector3>();

    void Update()
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

        int hSteps = Mathf.CeilToInt(horizontalFOV / horizontalResolution);
        int vSteps = Mathf.CeilToInt(verticalFOV / verticalResolution);

        float hStart = -horizontalFOV / 2f;
        float vStart = -verticalFOV / 2f;

        for (int v = 0; v < vSteps; v++)
        {
            float vAngle = vStart + v * verticalResolution;
            Quaternion vRot = Quaternion.Euler(0, 0, vAngle);
            Vector3 vDir = vRot * Vector3.right;

            for (int h = 0; h < hSteps; h++)
            {
                float hAngle = hStart + h * horizontalResolution;
                Quaternion hRot = Quaternion.AngleAxis(hAngle, Vector3.up);
                Vector3 dir = hRot * vDir;

                if (Physics.Raycast(origin, transform.rotation * dir, out RaycastHit hit, maxDistance))
                {
                    Vector3 noisyPoint = hit.point + Random.insideUnitSphere * noiseStdDev;
                    pointCloud.Add(noisyPoint);
                    Debug.DrawLine(origin, noisyPoint, Color.green, 0.1f);
                }
                else
                {
                    Debug.DrawRay(origin, transform.rotation * dir * maxDistance, Color.red, 0.1f);
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
        Debug.Log($"LiDAR point cloud saved to: {path}");
    }
}