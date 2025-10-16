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

        float testFloat = 123.0f;
        int startIdx = 0;
        plc[iCrane].WriteFloat(testFloat, startIdx);

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