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

    const int TruckSpeed = 5;
}