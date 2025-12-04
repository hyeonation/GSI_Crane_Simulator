using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Yard_Infra : MonoBehaviour
{

    //numbers
    public GameObject[] Number_Prefabs, Line_prefabs;
    List<Vector3> spawnedPositions = new List<Vector3>();
    [HideInInspector] public float Number_z_interval = 3.24f;

    float start_val = 0;
    float x_interval = GM.yard_x_interval;
    float z_interval = GM.yard_z_interval;

    Vector3 yardOffset;

    void Start()
    {
        yardOffset = transform.position;
        SpawnNumbers();
        SpawnLines();
        // RFID();
    }

    void SpawnNumbers()
    {
        GameObject NumberPrefab = Number_Prefabs[0]; //프리팹 추가
        GameObject folder = GameObject.Find("Number"); //오브젝트 저장 폴더 지정
        Vector3 spawnPosition; //포지션 설정

        for (int i = 0; i < 66; i++)
        {
            GameObject newObject = Instantiate(NumberPrefab); //오브젝트 생성
            newObject.GetComponent<TextMeshPro>().text = $"{i}"; //오브젝트 내용 변경
            spawnPosition = new Vector3(-5f, 0.2f, -8.45f + i * Number_z_interval); //오브젝트 포지션 설정
            spawnPosition += yardOffset;
            newObject.transform.position = spawnPosition; //오브젝트에 설정한 포지션 넣기

            newObject.name = $"{i}"; // 이름 설정
            newObject.transform.SetParent(folder.transform); //폴더에 저장

        }
    }
    void SpawnLines()
    {
        Vector3 spawnPosition;
        GameObject folder = GameObject.Find("Line"); //오브젝트 저장 폴더 지정

        int ij = 0;
        for (int i = 0; i < GM.stackProfile.lengthRow; i++)
        {
            for (int j = 0; j < GM.stackProfile.lengthBay; j++)
            {
                spawnPosition = new Vector3((i * x_interval) + start_val, 3.51f, (j * z_interval) + 7.75f);
                // spawnPosition += yardOffset;
                spawnedPositions.Add(spawnPosition);
                GameObject newObject = Instantiate(Line_prefabs[0], spawnPosition, Quaternion.identity);

                // 이름 설정
                newObject.name = $"line_{ij}";
                ij++;

                // 폴더에 저장
                newObject.transform.SetParent(folder.transform);
            }
        }
    }
    void RFID()
    {
        GameObject NumberPrefab = Number_Prefabs[1]; //프리팹 추가
        GameObject folder = GameObject.Find("RFID"); //오브젝트 저장 폴더 지정
        Vector3 spawnPosition; //포지션 설정
        Quaternion Set_Rotation; //Rotation 설정

        for (int i = 0; i < 66; i++)
        {
            GameObject newObject = Instantiate(NumberPrefab); //오브젝트 생성

            spawnPosition = new Vector3(-1f, 0.2f, -8.45f + i * Number_z_interval); //오브젝트 포지션 설정
            newObject.transform.position = spawnPosition; //오브젝트에 설정한 포지션 넣기

            Set_Rotation = Quaternion.Euler(0, 0, newObject.transform.eulerAngles.z * -1);
            newObject.transform.rotation = Set_Rotation; //오브젝트에 설정한 로테이션 넣기

            newObject.name = $"{i}"; // 이름 설정
            newObject.transform.SetParent(folder.transform); //폴더에 저장

            GameObject newObject2 = Instantiate(NumberPrefab);
            spawnPosition = new Vector3(24.5f, 0.2f, -8.45f + i * Number_z_interval); //오브젝트 포지션 설정
            newObject2.transform.position = spawnPosition; //오브젝트에 설정한 포지션 넣기



            newObject2.name = $"{i}"; // 이름 설정
            newObject2.transform.SetParent(folder.transform); //폴더에 저장

        }
    }

}