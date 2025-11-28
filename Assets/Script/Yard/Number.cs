using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Number : MonoBehaviour
{

    //numbers
    public GameObject NumberPrefab;
    [HideInInspector] public float Number_z_interval = 3.24f;

    void Start()
    {
        SpawnNumbers();
    }

    void SpawnNumbers()
    {
        for (int i = 0; i < 66; i++)
        {
            GameObject newObject = Instantiate(NumberPrefab);                           // Create
            newObject.GetComponent<TextMeshPro>().text = $"{i}";                        // Numbering
            newObject.transform.position = new Vector3(0, 0, i * Number_z_interval);    // Position
            newObject.transform.position += transform.position;                         // Offset

            newObject.name = $"{i}";                    // Object Name
            newObject.transform.SetParent(transform);   // set parent

        }
    }
}