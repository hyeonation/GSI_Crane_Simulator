using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Line : MonoBehaviour
{

    //numbers
    public GameObject Line_prefabs;
    [HideInInspector] public float Number_z_interval = 3.24f;

    float x_interval = GM.yard_x_interval;
    float z_interval = GM.yard_z_interval;

    void Start()
    {
        SpawnLines();
    }

    void SpawnLines()
    {
        int ij = 0;
        for (int i = 0; i < GM.stackProfile.lengthRow; i++)
        {
            for (int j = 0; j < GM.stackProfile.lengthBay; j++)
            {
                Vector3 spawnPosition = new Vector3(i * x_interval, 0, j * z_interval);                 // position
                spawnPosition += transform.position;                                                    // Offset
                GameObject newObject = Instantiate(Line_prefabs, spawnPosition, Quaternion.identity);   // create

                newObject.name = $"line_{ij++}";            // Object Name
                newObject.transform.SetParent(transform);   // set parent
            }
        }
    }
}