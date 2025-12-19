using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CollectLiDARData : MonoBehaviour
{
    [SerializeField]
    public List<LiDAR3DIJobParallelForMain> liDARObjects;
    [SerializeField]
    Transform spreader;
    
    [SerializeField]
    bool saveTriggered = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) || saveTriggered)
        {
            saveTriggered = false;
            // create folder
            string guid = AssetDatabase.CreateFolder("Assets/SPSS_Data", $"H_{spreader.position.y}");
            // string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            string newFolderPath = $"Assets/SPSS_Data/H_{spreader.position.y}";

            // delay 1 second to ensure folder is created
            // System.Threading.Thread.Sleep(1000);
            
            foreach (var liDAR in liDARObjects)
            {
                liDAR.NewFolderPath = newFolderPath;
                liDAR.saveToFile = true;
            }
        }
    }
}