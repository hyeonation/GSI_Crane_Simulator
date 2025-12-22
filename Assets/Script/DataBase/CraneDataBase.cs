using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public struct CraneDataBase
{
    // // read
    // public float readGantryVelBWD, readGantryVelFWD, readTrolleyVel, readSpreaderVel;
    // public float readMM0Vel, readMM1Vel, readMM2Vel, readMM3Vel;
    // public bool read20ft, read40ft, read45ft, readTwlLock, readTwlUnlock;
    // public short readCam1, readCam2, readCam3, readCam4;


    // // write
    // public float write_aG_Angle, write_aG_BPos_x,write_aG_BPos_z,write_aG_FPos_x, write_aG_FPos_z;
    // public float write_aT_Pos;
    // public float write_aSpreader_Pos_x, write_aSpreader_Pos_y, write_aSpreader_Pos_z;
    // public float write_aSpreader_Ang_x, write_aaSpreader_Ang_y, write_aSpreader_Ang_z;
    // public float write_MM_1_Pos, write_MM_2_Pos, write_MM_3_Pos, write_MM_4_Pos;
    // public float write_ALS_1, write_ALS_2, write_ALS_3, write_ALS_4;

    public Write WriteData;
    public Read ReadData;

}


public struct Write
{
    public float aG_Angle;
    public float aG_BPos_x;
    public float aG_BPos_z;
    public float aG_FPos_x;
    public float aG_FPos_z;
    public float aT_Pos;
    public float aSpreader_Pos_x;
    public float aSpreader_Pos_y;
    public float aSpreader_Pos_z;
    public float aSpreader_Ang_x;
    public float aSpreader_Ang_y;
    public float aSpreader_Ang_z;
    public float MM_1_Pos;
    public float MM_2_Pos;
    public float MM_3_Pos;
    public float MM_4_Pos;
    public float ALS_1;
    public float ALS_2;
    public float ALS_3;
    public float ALS_4;
    public float ALS_5;
    public float ALS_6;
    public float SPSS_Stack1_Lidar_Row1;
    public float SPSS_Stack1_Lidar_Row2;
    public float SPSS_Stack1_Lidar_Row3;
    public float SPSS_Stack1_Lidar_Row4;
    public float SPSS_Stack1_Lidar_Row5;
    public float SPSS_Stack1_Lidar_Row6;
    public float SPSS_Stack1_Lidar_Row7;
    public float SPSS_Stack1_Lidar_Row8;
    public float SPSS_Stack1_Lidar_Row9;
    public bool Landed;
    public bool Rope_Slack;
    public bool Tw_Locked;
    public bool Tw_Unlocked;
    public bool On20ft;
    public bool On40ft;
    public bool On45ft;
    public float Truk_Pos_x;
    public float Truk_Pos_z;
    public float Truk_Angle;
    public bool Truck_Status_Trailer_up;
    public bool Truck_Status_Trailer_down;
}

public struct Read
{
    public float gantryVelBWD;
    public float gantryVelFWD;
    public float trolleyVel;
    public float spreaderVel;
    public SprdStatus sprdStatus;
    public TwlStatus twlStatus;
    public float MM0Vel;
    public float MM1Vel;
    public float MM2Vel;
    public float MM3Vel;

    public short cam1Index;
    public short cam2Index;
    public short cam3Index;
    public short cam4Index;



    public PTZCamera pTZCamera;

}
public struct PTZCamera
{
    public bool Select_Cam;
    public bool PanLeft;
    public bool PanRight;
    public bool TiltUp;
    public bool TiltDown;
    public bool CW;
    public bool CCW;
    public bool ZoomIn;
    public bool ZoomOut;
}


public struct SprdStatus
{
    public bool landed;
    public bool rope_Slack;
    public bool tw_Locked;
    public bool tw_Unlocked;
    public bool on20ft;
    public bool on40ft;
    public bool on45ft;
}


public struct TwlStatus
{
    public bool locked;
    public bool unlocked;
}