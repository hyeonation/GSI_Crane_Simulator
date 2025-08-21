using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LiDAR3D : MonoBehaviour
{
    public Vector3 scanOffset = Vector3.zero;

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

        int hSteps = Mathf.CeilToInt(GM.settingParams.lidarFovHorizontal_deg / GM.settingParams.lidarResHorizontal_deg);
        int vSteps = Mathf.CeilToInt(GM.settingParams.lidarFovVertical_deg / GM.settingParams.lidarResVertical_deg);

        float hStart = -GM.settingParams.lidarFovHorizontal_deg / 2f;
        float vStart = -GM.settingParams.lidarFovVertical_deg / 2f;

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
                    Vector3 noisyPoint = hit.point + Random.insideUnitSphere * GM.settingParams.lidarNoiseStd;
                    pointCloud.Add(noisyPoint);
                    Debug.DrawLine(origin, noisyPoint, Color.green, 0.1f);
                }
                else
                {
                    Debug.DrawRay(origin, transform.rotation * dir * GM.settingParams.lidarMaxDistance_m, Color.red, 0.1f);
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