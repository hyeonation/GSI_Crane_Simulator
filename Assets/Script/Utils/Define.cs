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
        QC
        
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

    public enum ContainerPosition
    {
        OnYardTruck,
        OnETruck,
        OnYard,
        OnShip
    }

    public const int SCREEN_WIDTH = 1920;
    public const int SCREEN_HEIGHT = 1080;


    // RMGC offset values
    public const float OffsetRMGCGantryZ = 2581.3f;
    public const float OffsetRMGCTrolleyX = 14.1f;
    public const float OffsetRMGCHoistY = 0f;

    public static readonly Rect[] screenRects = new Rect[]
    {
        new Rect(0.0f, 0.5f, 0.5f, 0.5f), // Slot 0
        new Rect(0.5f, 0.5f, 0.5f, 0.5f), // Slot 1
        new Rect(0.0f, 0.0f, 0.5f, 0.5f), // Slot 2
        new Rect(0.5f, 0.0f, 0.5f, 0.5f)  // Slot 3
    };

    public static Dictionary<int,string> RMGCCameraIndex = new Dictionary<int, string>()
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
    

    const int TruckSpeed = 5;
}