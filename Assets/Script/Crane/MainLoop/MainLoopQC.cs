using System;
using UnityEngine;

// Organizing data
public class MainLoopQC : MainLoop
{
    // Inspector Write values
    [Header("Write values")]
    public Mi mi;
    public Hoist hoist;
    public Trolley trolley;
    public Gantry gantry;
    public Boom boom;
    public Spreader spreader;
    public Protection protection;
    public GACS gacs;

    public override void ReadPLCdata(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelBWD = 14;
        const int floatStartIdxGantryVelFWD = 14;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 2;

        const int boolStartIdxTwistLock = 28;
        const int boolStartPointTwlLock = 0;
        const int boolStartPointTwlUnlock = 1;

        const int boolStartIdxFeet = 22;
        const int boolStartPoint20Ft = 1;
        const int boolStartPoint40Ft = 2;
        const int boolStartPoint45Ft = 3;

        // Read raw data from PLC
        var rawData = plc[iCrane].ReadFromPLC();

        // Read float data
        GM.cmdGantryVelFWD[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.cmdGantryVelBWD[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.cmdTrolleyVel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.cmdSpreaderVel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);

        // Read boolean data
        GM.cmdTwlLock[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.cmdTwlUnlock[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);

        GM.cmd20ft[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint20Ft);
        GM.cmd40ft[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint40Ft);
        GM.cmd45ft[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint45Ft);
    }

    public override void WriteUnitydataToPLC(int iCrane)
    {
        // Mi 

        // int
        plc[iCrane].WriteShort(-123, 216);


        // word
        plc[iCrane].WriteShort(-1231, 230);

        // char
        plc[iCrane].WriteChar('d', 200);


        float testFloat = 123.0f;
        int startIdx = 0;
        plc[iCrane].WriteFloat(testFloat, 84);

        // byte boolByte = 0;  // init
        // CommPLC.WriteBool(true, 0, boolByte);
        // CommPLC.WriteBool(false, 1, boolByte);
        // CommPLC.WriteBool(false, 2, boolByte);

        // plc[iCrane].WriteByte(boolByte, 204);

        // write to PLC
        plc[iCrane].WriteToPLC();

    }
}

////////////////////////////////////

[Serializable]
public struct Mi
{
    public bool up;
    public bool down;
    public bool forward;
    public bool backward;
    public bool left;
    public bool right;
    public bool sprdSingle;
    public bool sprdTwin;
    public bool sprd20Ft;
    public bool sprd40Ft;
    public bool sprd45Ft;
    public bool sprdTwinExpand;
    public bool sprdTwinRetract;
    public bool flipLsLeftUp;
    public bool flipLsLeftDn;
    public bool flipSsLeftUp;
    public bool flipSsLeftDn;
    public bool flipSsRightUp;
    public bool flipSsRightDn;
    public bool flipLsRightUp;
    public bool flipLsRightDn;
    public bool tlLock;
    public bool tlUnlock;
    public bool trimLeft;
    public bool trimRight;
    public bool listInBoard;
    public bool listOutBoard;
    public bool skewCw;
    public bool skewCcw;
    public bool tlsStore1;
    public bool tlsStore2;
    public bool tlsSet1;
    public bool tlsSet2;
}

//////////////////////////////////// Hoist
[Serializable]
public struct Hoist
{
    public CamLimitHoist camLimit;
    public float drvSpdFbk;
    public float position;
}

[Serializable]
public struct CamLimitHoist
{
    public bool upperESTOP;
    public bool upperEndStop;
    public bool upperSlowDown;
    public bool posSync;
    public bool silbeamClear;
    public bool dockEndStop;
    public bool lowerSlowDown;
    public bool lowerESTOP;
    public bool overLoad;
    public bool eccenLoad;
}

//////////////////////////////////// Trolley

[Serializable]
public struct Trolley
{
    public CamLimitTrolley camLimit;
    public LimitSwitchTrolley limitSwitch;
    public Prx prx;
    public float drvSpdFbk;
    public float position;
}

[Serializable]
public struct CamLimitTrolley
{
    public bool fwdEstop;
    public bool fwdEndStop;
    public bool fwdSlowDown;
    public bool overWater;
    public bool seaSilbeamRevStop;
    public bool seaSilbeamFwdStop;
    public bool fwdEndStopAtBoom;
    public bool syncPos;
    public bool parkingPos;
    public bool revSlowDown;
    public bool revEndStop;
    public bool revEstop;
}

[Serializable]
public struct LimitSwitchTrolley
{
    public bool fwdEndStop;
}

[Serializable]
public struct Prx
{
    public bool fwdSlowDown;
    public bool revSlowDown;
    public bool parked;
}

//////////////////////////////////// Gantry
[Serializable]
public struct Gantry
{
    public AntiCollision antiCollision;
    public CamLimitGantry camLimitGantry;
    public Proximity proximity;
    public LimitSwitchGantry limitSwitch;
    public float drvSpdFbk;
    public float position;
}

[Serializable]
public struct AntiCollision
{
    public bool left;
    public bool leftSlowDown;
    public bool right;
    public bool rightSlowDown;
}

[Serializable]
public struct CamLimitGantry
{
    public bool CrBypass;
}

[Serializable]
public struct Proximity
{
    public bool CrLeft;
    public bool CrRight;
}

[Serializable]
public struct LimitSwitchGantry
{
    public bool CrLeftEnd;
    public bool CrRightEnd;
}

//////////////////////////////////// Boom

[Serializable]
public struct Boom
{
    public CamLimitBoom camLimit;
    public ProximityBoom proximity;
    public AntiCollisionBoom antiCollision;
    public float drvSpdFbk;

}

[Serializable]
public struct CamLimitBoom
{
    public bool upperESTOP;
    public bool upperNormalStop;
    public bool stowed;
    public bool upperSlowDown;
    public bool jamReCalibration;
    public bool lowerSlowDown;
    public bool inWorkPos;
    public bool ropeSlack;
    public bool finalLowerStop;
}

[Serializable]
public struct ProximityBoom
{
    public bool hingeRight;
    public bool hingeLeft;
}

[Serializable]
public struct AntiCollisionBoom
{
    public bool left;
    public bool right;
}

//////////////////////////////////// Spreader

[Serializable]
public struct Spreader
{
    public LandedSpreader landed;
    public TlLocked tlLocked;
    public TlUnlocked tlUnlocked;
    public Flipper flipper;
    public Beacon beacon;

    public bool single;
    public bool twin;
    public bool size20ft;
    public bool size40ft;
    public bool size45ft;
    public float twinGap;

}

[Serializable]
public struct LandedSpreader
{
    public bool cornerLsLeft;
    public bool cornerSsLeft;
    public bool cornerSsRight;
    public bool cornerLsRight;
    public bool centerLsLeft;
    public bool centerSsLeft;
    public bool centerSsRight;
    public bool centerLsRight;

}

[Serializable]
public struct TlLocked
{
    public bool cornerLsLeft;
    public bool cornerSsLeft;
    public bool cornerSsRight;
    public bool cornerLsRight;
    public bool centerLsLeft;
    public bool centerSsLeft;
    public bool centerSsRight;
    public bool centerLsRight;

}

[Serializable]
public struct TlUnlocked
{
    public bool cornerLsLeft;
    public bool cornerSsLeft;
    public bool cornerSsRight;
    public bool cornerLsRight;
    public bool centerLsLeft;
    public bool centerSsLeft;
    public bool centerSsRight;
    public bool centerLsRight;

}

[Serializable]
public struct Flipper
{
    public bool LsLeftUp;
    public bool LsLeftDown;
    public bool SsLeftUp;
    public bool SsLeftDown;
    public bool SsRightUp;
    public bool SsRightDown;
    public bool LsRightUp;
    public bool LsRightDown;

}

[Serializable]
public struct Beacon
{
    public float tirm;
    public float list;
    public float skew;
    public float swayAngle;
}


//////////////////////////////////// Protection
[Serializable]
public struct Protection
{
    public OCR OCR;
    public CPS CPS;
    public SPSS SPSS;
}
[Serializable]
public struct HeartBeat
{
    public int heartBeat;
}

//////////////////////////////////// Protection - OCR
[Serializable]
public struct OCR
{
    public HeartBeat OCRSystem;
    public OcrCmd cmd;
    public OcrSts sts;
    public OcrSv sv;
    public OcrPv pv;
}

[Serializable]
public struct OcrCmd
{

}

[Serializable]
public struct OcrSts
{
    public CamError camError;
    public CNRS CNRS;
    public CDRS CDRS;
    public OcrYT YT;
}

[Serializable]
public struct CamError
{
    public bool SsSilbeamLeft;
    public bool SsSilbeamRight;
    public bool LsSilbeamLeft;
    public bool LsSilbeamRight;
    public bool leftPortalFront;
    public bool leftPortalRear;
    public bool rightPortalFront;
    public bool rightPortalRear;
    public bool cabinEntrance;
}

[Serializable]
public struct CNRS
{
    public bool CntrDetectionOk;
    public bool CntrDetectionFail;
}
[Serializable]
public struct CDRS
{
    public bool doorDirFwd;
    public bool doorDirBwd;
    public bool doorDirFail;
    public bool doorOpened;
}

[Serializable]
public struct OcrYT
{
    public bool headSafe;

}

[Serializable]
public struct OcrSv
{
}

[Serializable]
public struct OcrPv
{
    public Cntr Cntr1;
    public Cntr Cntr2;
    public OcrTruck truck;
}

[Serializable]
public class Cntr
{
    public int type;
    public char[] number = new char[11];
}

[Serializable]
public class OcrTruck
{
    public int selectedLane;
    public char[] number = new char[11];
}

//////////////////////////////////// Protection - CPS
[Serializable]
public struct CPS
{
    public HeartBeat CpsPC;
    public CpsSts sts;
    public CpsSv sv;
    public CpsPv pv;
}


[Serializable]
public struct CpsCmd
{

}

[Serializable]
public struct CpsSts
{
    public CpsScanner scanner;
    public Cps CPS;
}

[Serializable]
public struct CpsSv
{

}

[Serializable]
public struct CpsPv
{
    public CpsTruck truck;
    public int CntrTwinGap;
}

[Serializable]
public struct CpsScanner
{
    public bool enabled;
    public bool commFault;
    public bool invalidData;
    public bool cleaning;
    public bool PtzEnabled;
    public bool PtzCommError;
    public bool PtzDataError;
    public bool Led1Enabled;
    public bool Led1CommError;
    public bool Led1DataError;
    public bool Led2Enabled;
    public bool Led2CommError;
    public bool Led2DataError;
}

[Serializable]
public struct Cps
{
    public bool truckParked;
    public bool truckLandable;
    public bool truckHeadSafe;
    public bool truckChassisSafe;
    public bool Cntr20FtFront;
    public bool Cntr20FtRear;
    public bool Cntr40Ft;
    public bool Cntr45Ft;
    public bool CntrTwin20Ft;
    public bool CntrSmall;
    public bool shipLoadBlocked;
    public bool shipUnloadBlocked;
    public bool riskZone;
    public bool truckExist;
    public bool truckEnteredLeft;
    public bool truckEnteredRight;
}

[Serializable]
public struct CpsTruck
{
    public float xDev;
    public float yDev;
    public float skew;

    public int selectedLane;
}





//////////////////////////////////// Protection - SPSS
/// <summary>
[Serializable]
public struct SpssHeartBeat
{
    public int heartBeat;
    public short softVersion;

}
/// </summary>
[Serializable]
public struct SPSS
{
    public SpssHeartBeat Spss;
    public SpssCmd cmd;
    public SpssSts sts;
    public SpssSv sv;
    public SpssPv pv;
}


[Serializable]
public struct SpssCmd
{
}

[Serializable]
public struct SpssSts
{
    public SpssScanner scanner;
    public SpssTarget target;
    public ValidHeight validheight;
}

[Serializable]
public struct SpssScanner
{
    public bool trolleyLsCommFault;
    public bool trolleyLsError;
    public bool trolleyLsInvalid;
    public bool trolleyLsEnabled;
    public bool trolleyLsCalibOn;
    public bool trolleySsCommFault;
    public bool trolleySsError;
    public bool trolleySsInvalid;
    public bool trolleySsEnabled;
    public bool trolleySsCalibOn;
    public bool gantryLeftCommFault;
    public bool gantryLeftError;
    public bool gantryLeftInvalid;
    public bool gantryLeftEnabled;
    public bool gantryLeftCalibOn;
    public bool gantryRightCommFault;
    public bool gantryRightError;
    public bool gantryRightInvalid;
    public bool gantryRightEnabled;
    public bool gantryRightCalibOn;

}

[Serializable]
public struct SpssTarget
{
    public bool CntrNone;
    public bool Cntr20Ft;
    public bool Cntr40Ft;
    public bool Cntr45Ft;
    public bool CntrTwin20Ft;
    public bool truckTrolleyLsClear;
    public bool truckTrolleySsClear;
    public bool truckGantryLeftClear;
    public bool truckGantryRightClear;
    public bool truckChassisClear;
    public bool truckDirectionFwd;
    public bool truckHeadSafe;
}


[Serializable]
public class ValidHeight
{
    public float[] apron = new float[12];
    public float[] vessel = new float[25];
}

[Serializable]
public struct SpssSv
{

}

[Serializable]
public struct SpssPv
{
    public StackProfile stackProfile;
    public SpssPvTarget target;
    public SpssAntiCollision anticollision;
}

[Serializable]
public class StackProfile
{
    public float[] apronHeight = new float[12];
    public float[] vesselHeight = new float[25];
    public float[] vesselRowPos = new float[25];
}

[Serializable]
public struct SpssPvTarget
{
    public float LsDev;
    public float SsDev;
    public float LeftDev;
    public float RightDev;
    public float gantryPosDev;
    public float trim;
    public float list;
    public float skew;
    public int twinGap;

}
[Serializable]
public struct SpssAntiCollision
{
    public float LsCollisionDist;
    public float SsCollisionDist;

}


[Serializable]
public struct GACS
{
    public GACSValues LsLeft;
    public GACSValues SsLeft;
    public GACSValues SsRight;
    public GACSValues LsRight;
}

[Serializable]
public struct GACSValues
{
    public bool clear20m;
    public bool clear15m;
    public bool clear10m;
    public bool clear5m;
    public bool clear3m;

}