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
        var rawData = GM.plc[iCrane].ReadFromPLC();

        // Read float data
        GM.arrayCraneDataBase[iCrane].ReadData.gantryVelFWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.arrayCraneDataBase[iCrane].ReadData.gantryVelBWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.arrayCraneDataBase[iCrane].ReadData.trolleyVel = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.arrayCraneDataBase[iCrane].ReadData.spreaderVel = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);

        // Read boolean data
        GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);
        GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on20ft = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint20Ft);
        GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint40Ft);
        GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft = CommPLC.ReadBoolData(rawData, boolStartIdxFeet, boolStartPoint45Ft);
    }

    public override void WriteUnitydataToPLC(int iCrane)
    {

        /// Mi 
        GM.plc[iCrane].BoolByteInit();      // init boolByte
        GM.plc[iCrane].WriteBool(mi.up, 0);
        GM.plc[iCrane].WriteBool(mi.down, 1);
        GM.plc[iCrane].WriteBool(mi.forward, 2);
        GM.plc[iCrane].WriteBool(mi.backward, 3);
        GM.plc[iCrane].WriteBool(mi.left, 4);
        GM.plc[iCrane].WriteBool(mi.right, 5);
        GM.plc[iCrane].WriteBool(mi.sprdSingle, 6);
        GM.plc[iCrane].WriteBool(mi.sprdTwin, 7);
        GM.plc[iCrane].WriteBoolByte(0); // write at index 0

        // int
        GM.plc[iCrane].WriteShort(-123, 216);


        // word
        GM.plc[iCrane].WriteShort(-1231, 230);

        // char
        GM.plc[iCrane].WriteChar('d', 200);


        float testFloat = 123.0f;
        GM.plc[iCrane].WriteFloat(testFloat, 84);

        // byte boolByte = 0;  // init
        // CommPLC.WriteBool(true, 0, boolByte);
        // CommPLC.WriteBool(false, 1, boolByte);
        // CommPLC.WriteBool(false, 2, boolByte);

        // plc[iCrane].WriteByte(boolByte, 204);

        // write to PLC
        GM.plc[iCrane].WriteToPLC();

    }
}
