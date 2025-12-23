using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Util;

public class Define
{
    public enum UIEvent
    {
        Click,
        Preseed,
        PointerDown,
        PointerUp,
        BeginDrag,
        Drag,
        EndDrag,
    }

    public enum SceneType
    {
        Unknown = -1,
        StartMenu,
        RMGC,
        RTGC,
        QC,
        Test,


    }

    public enum ControlMode
    {
        PLC,
        Keyboard,
    }

    public enum CraneType
    {
        RTGC,
        RMGC,
        QC,
    }

    public enum ContainerHolderType
    {
        None,   // 초기화 상태 또는 아무에게도 잡히지 않은 상태 (필수)
        Yard,   // 야적장에 적재됨
        Truck,  // 트럭에 실림
        Spreader
    }
    public enum TruckJobType
    {
        None,
        //반입
        Import,
        //반출
        Export
    }

    public enum TWLockState
    {
        Locked,
        Unlocked,
    }

    public const int SCREEN_WIDTH = 1920;
    public const int SCREEN_HEIGHT = 1080;


    // RMGC offset values
    public const float OffsetRMGCGantryZ = 2581.3f;
    public const float OffsetRMGCTrolleyX = 14.1f;
    public const float OffsetRMGCHoistY = 0f;

    public static readonly Rect[] screenRects = new Rect[]
    {
        new Rect(0.0f, 0.5f, 0.5f, 0.5f), // Cam 1
        new Rect(0.5f, 0.5f, 0.5f, 0.5f), // Cam 2
        new Rect(0.0f, 0.0f, 0.5f, 0.5f), // Cam 3
        new Rect(0.5f, 0.0f, 0.5f, 0.5f)  // Cam 4
    };

    public static Dictionary<int, string> RMGCCameraIndex = new Dictionary<int, string>()
    {
        {0, "Gantry1"},
        {1, "Gantry2"},
        {2, "Gantry3"},
        {3, "Gantry4"},
        {4, "Girder1"},
        {5, "Girder2"},
        {6, "MI 20ft 1"},
        {7, "MI 20ft 2"},
        {8, "MI 40ft 1"},
        {9, "MI 40ft 2"},
        {10, "Trolley1"},
        {11, "Trolley2"},
        {12, "Spreader_Camera"},
    };

    public static string[] DefaultRMGCCameraNames = new string[]
    {
        "Gantry1",
        "Trolley2",
        "Gantry2",
        "Trolley1",
    };



    public static float[] Block_8G_Bay_Pos = new float[]
    {
        2581.3F,
        2594.1F,
        2607.1F,
        2620.1F,
        2633.1F,
        2646.1F,
        2659.1F,
        2672.1F,
        2685.1F,
        2698.1F,
        2711.1F,
        2724.1F,
        2737.1F,
        2750.1F,
        2763.1F,
        2776.1F,
        2789.1F,
        2802.1F,
        2815.1F,
        2828.1F,
        2841.1F,
        2854.1F,
        2867.1F,
        2880.1F,

    };

    const int TruckSpeed = 5;

    public const float CameraSpeed = 1;

    #region addressable load path
    public const string AddressablePath_RMGC = "ARMG";
    public const string AddressablePath_RTGC = "ARTG";
    public const string AddressablePath_QC = "QC";

    public const string AddressablePath_RMGCTEST = "ARMGTEST";

    public static readonly string[] containerPrefabs = { "Container_G", "Container_DG", "Container_R", "Container_Y", "Container_W" };
    # endregion

}

