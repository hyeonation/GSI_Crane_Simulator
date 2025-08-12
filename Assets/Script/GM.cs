using UnityEngine;
using System;
using System.Collections.Generic;

// GameManager, GameMaster
public class GM : MonoBehaviour
{

    // command with PLC
    public bool cmdWithPLC = false;
    public List<string> listIP;
    [HideInInspector] public static string[] nameCranes, nameTrucks;

    [Header("Container_Preset")]
    public static short bay = 16;
    public static short row = 5;
    public static short tier = 5;

    public const float yard_start_val = 8.32f;
    public const float yard_x_interval = 2.840f;
    public const float yard_y_interval = 2.83f;
    public const float yard_z_interval = 12.96f;
    public static short num_containers = 100;

    // command data
    public static float[] cmdGantryVelBWD, cmdGantryVelFWD, cmdTrolleyVel, cmdSpreaderVel;
    public static float[] cmdMM0Vel, cmdMM1Vel, cmdMM2Vel, cmdMM3Vel;
    public static bool[] cmd20ft, cmd40ft, cmd45ft, cmdTwlLock, cmdTwlUnlock;

    void Awake()
    {
        // init variables
        InitVar();

        Application.targetFrameRate = 30;
    }

    void InitVar()
    {
        // Crane name
        GameObject crane = GameObject.Find("Crane");
        nameCranes = new string[crane.transform.childCount];
        for (int i = 0; i < crane.transform.childCount; i++)
        {
            nameCranes[i] = crane.transform.GetChild(i).name;
        }

        // Truck name
        GameObject truck = GameObject.Find("Truck");
        nameTrucks = new string[truck.transform.childCount];
        for (int i = 0; i < truck.transform.childCount; i++)
        {
            nameTrucks[i] = truck.transform.GetChild(i).name;
        }

        // ip

        // Read DB array
        cmdGantryVelFWD = new float[nameCranes.Length];
        cmdGantryVelBWD = new float[nameCranes.Length];
        cmdTrolleyVel = new float[nameCranes.Length];
        cmdSpreaderVel = new float[nameCranes.Length];
        cmdMM0Vel = new float[nameCranes.Length];
        cmdMM1Vel = new float[nameCranes.Length];
        cmdMM2Vel = new float[nameCranes.Length];
        cmdMM3Vel = new float[nameCranes.Length];
        cmd20ft = new bool[nameCranes.Length];
        cmd40ft = new bool[nameCranes.Length];
        cmd45ft = new bool[nameCranes.Length];
        cmdTwlLock = new bool[nameCranes.Length];
        cmdTwlUnlock = new bool[nameCranes.Length];

        // Input_IP = new GameObject[]{
        //     GameObject.Find("Input_IP"),
        //     GameObject.Find("Input_Start_DB"),
        // };
    }

    // void GameStart()
    // {

    //     GameObject canvas = GameObject.Find("Canvas");
    //     canvas.SetActive(false);

    //     plc_comm.Connect_PLC();
    //     run = true;

    // }

}
