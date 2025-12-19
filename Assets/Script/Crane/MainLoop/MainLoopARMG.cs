
// Organizing data
using System.Diagnostics;
using UnityEngine;

public class MainLoopARMG : MainLoop
{

    // plc constant
    const int shortLength = 2;
    const int charLength = 1;

    // Crane 별 고유 변수
    public override void InitCraneSpecVar()
    {
        GM.readDBNum = 2141;
        GM.readLength = 44;
        GM.writeDBNum = 2131;
        GM.writeStartIdx = 0;
        GM.writeLength = 441;
    }

    public override void ReadPLCdata(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelBWD = 0;
        const int floatStartIdxGantryVelFWD = 4;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 12;
        const int floatStartIdxMM0Vel = 16;
        const int floatStartIdxMM1Vel = 20;
        const int floatStartIdxMM2Vel = 24;
        const int floatStartIdxMM3Vel = 28;

        const int boolStartIdxSprdStatus = 32;
        const int boolStartPoint20ft = 0;
        const int boolStartPoint40ft = 1;
        const int boolStartPoint45ft = 2;

        const int boolStartIdxTwl = 34;
        const int boolStartPointTwllock = 0;
        const int boolStartPointTwlUnlock = 1;

        const int shortStartIdxCam1Index = 36;
        const int shortStartIdxCam2Index = 38;
        const int shortStartIdxCam3Index = 40;
        const int shortStartIdxCam4Index = 42;

        // Read raw data from PLC
        var rawData = GM.plc[iCrane].ReadFromPLC();

        // Read float data
        GM.arrayCraneDataBase[iCrane].ReadData.gantryVelBWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.arrayCraneDataBase[iCrane].ReadData.gantryVelFWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.arrayCraneDataBase[iCrane].ReadData.trolleyVel = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.arrayCraneDataBase[iCrane].ReadData.spreaderVel = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM0Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM0Vel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM1Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM1Vel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM2Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM2Vel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM3Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM3Vel);

        // Read boolean data
        GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on20ft = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPoint20ft);
        GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPoint40ft);
        GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft = CommPLC.ReadBoolData(rawData, boolStartIdxSprdStatus, boolStartPoint45ft);

        GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked = CommPLC.ReadBoolData(rawData, boolStartIdxTwl, boolStartPointTwllock);
        GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked = CommPLC.ReadBoolData(rawData, boolStartIdxTwl, boolStartPointTwlUnlock);

        // read cam index data
        GM.arrayCraneDataBase[iCrane].ReadData.cam1Index = CommPLC.ReadShortData(rawData, shortStartIdxCam1Index);
        GM.arrayCraneDataBase[iCrane].ReadData.cam2Index = CommPLC.ReadShortData(rawData, shortStartIdxCam2Index);
        GM.arrayCraneDataBase[iCrane].ReadData.cam3Index = CommPLC.ReadShortData(rawData, shortStartIdxCam3Index);
        GM.arrayCraneDataBase[iCrane].ReadData.cam4Index = CommPLC.ReadShortData(rawData, shortStartIdxCam4Index);

    }


    public override void WriteUnitydataToPLC(int iCrane)
    {
        craneData = GM.arrayCraneDataBase[iCrane];

        // DB start index

        const int shortStartIdxJobID = 12;
        const int shortStartIdxJobType = 14;

        const int startIdxSRC = 18;
        const int startIdxDST = 118;
        const int startIdxCntrProp = 236;


        const int startIdx_aG_Angle = 302;
        const int startIdx_aG_BPos_x = 306;
        const int startIdx_aG_BPos_z = 310;
        const int startIdx_aG_FPos_x = 314;
        const int startIdx_aG_FPos_z = 318;
        const int startIdx_aT_Pos = 322;
        const int startIdx_aSpreader_Pos_x = 326;
        const int startIdx_aSpreader_Pos_y = 330;
        const int startIdx_aSpreader_Pos_z = 334;
        const int startIdx_aSpreader_Ang_x = 338;
        const int startIdx_aSpreader_Ang_y = 342;
        const int startIdx_aSpreader_Ang_z = 346;
        const int startIdx_MM_1_Pos = 350;
        const int startIdx_MM_2_Pos = 354;
        const int startIdx_MM_3_Pos = 358;
        const int startIdx_MM_4_Pos = 362;


        const int startIdx_SPSS_Stack1_Lidar_Row1 = 390;
        const int startIdx_SPSS_Stack1_Lidar_Row2 = 394;
        const int startIdx_SPSS_Stack1_Lidar_Row3 = 398;
        const int startIdx_SPSS_Stack1_Lidar_Row4 = 402;
        const int startIdx_SPSS_Stack1_Lidar_Row5 = 406;
        const int startIdx_SPSS_Stack1_Lidar_Row6 = 410;
        const int startIdx_SPSS_Stack1_Lidar_Row7 = 414;
        const int startIdx_SPSS_Stack1_Lidar_Row8 = 418;
        const int startIdx_SPSS_Stack1_Lidar_Row9 = 422;

        const int startIdx_SPRD_Status = 426;
        const int startIdx_Truk_Pos_x = 430;
        const int startIdx_Truk_Pos_z = 434;
        const int startIdx_Truk_Angle = 438;

        /////////////////////
        /// 
        // Determine task info
        if (GM.listTaskInfo.Count != 0)
        {
            TaskInfo taskInfo = GM.listTaskInfo[0];

            GM.plc[iCrane].WriteShort(taskInfo.jobID, shortStartIdxJobID);
            GM.plc[iCrane].WriteShort(taskInfo.jobType, shortStartIdxJobType);

            WriteShiftPos(iCrane, taskInfo.srcPos, startIdxSRC, taskInfo.cntrInfoSO);
            WriteShiftPos(iCrane, taskInfo.dstPos, startIdxDST, taskInfo.cntrInfoSO);
            WriteContainerInfo(iCrane, taskInfo.cntrInfoSO, taskInfo.strContainerID, startIdxCntrProp);

            // GM.plc[iCrane].WriteFloat(taskInfo.jobType, shortStartIdxJobType);


            // UnityEngine.Debug.Log($"List Count: {GM.listTaskInfo.Count}, jobID: {taskInfo.jobID}, jobType: {taskInfo.jobType}");


        }




        //TODO : 실제 위치와 PLC에 쓸 위치 offset 보정 
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aG_Angle, startIdx_aG_Angle);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aG_BPos_x + Define.OffsetRMGCTrolleyX, startIdx_aG_BPos_x);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aG_BPos_z + Define.OffsetRMGCGantryZ, startIdx_aG_BPos_z);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aG_FPos_x + Define.OffsetRMGCTrolleyX, startIdx_aG_FPos_x);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aG_FPos_z + Define.OffsetRMGCGantryZ, startIdx_aG_FPos_z);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aT_Pos + Define.OffsetRMGCTrolleyX, startIdx_aT_Pos);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aSpreader_Pos_x, startIdx_aSpreader_Pos_x);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aSpreader_Pos_y + Define.OffsetRMGCHoistY, startIdx_aSpreader_Pos_y);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aSpreader_Pos_z, startIdx_aSpreader_Pos_z);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aSpreader_Ang_x, startIdx_aSpreader_Ang_x);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aSpreader_Ang_y, startIdx_aSpreader_Ang_y);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.aSpreader_Ang_z, startIdx_aSpreader_Ang_z);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.MM_1_Pos, startIdx_MM_1_Pos);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.MM_2_Pos, startIdx_MM_2_Pos);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.MM_3_Pos, startIdx_MM_3_Pos);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.MM_4_Pos, startIdx_MM_4_Pos);

        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row1, startIdx_SPSS_Stack1_Lidar_Row1);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row2, startIdx_SPSS_Stack1_Lidar_Row2);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row3, startIdx_SPSS_Stack1_Lidar_Row3);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row4, startIdx_SPSS_Stack1_Lidar_Row4);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row5, startIdx_SPSS_Stack1_Lidar_Row5);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row6, startIdx_SPSS_Stack1_Lidar_Row6);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row7, startIdx_SPSS_Stack1_Lidar_Row7);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row8, startIdx_SPSS_Stack1_Lidar_Row8);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.SPSS_Stack1_Lidar_Row9, startIdx_SPSS_Stack1_Lidar_Row9);

        GM.plc[iCrane].BoolByteInit();
        GM.plc[iCrane].WriteBool(craneData.WriteData.Landed, 0);
        GM.plc[iCrane].WriteBool(craneData.WriteData.Rope_Slack, 1);
        GM.plc[iCrane].WriteBool(craneData.WriteData.Tw_Locked, 2);
        GM.plc[iCrane].WriteBool(craneData.WriteData.Tw_Unlocked, 3);
        GM.plc[iCrane].WriteBool(craneData.WriteData.On20ft, 4);
        GM.plc[iCrane].WriteBool(craneData.WriteData.On40ft, 5);
        GM.plc[iCrane].WriteBool(craneData.WriteData.On45ft, 6);
        GM.plc[iCrane].WriteBoolByte(startIdx_SPRD_Status);

        GM.plc[iCrane].WriteFloat(craneData.WriteData.Truk_Pos_x, startIdx_Truk_Pos_x);
        GM.plc[iCrane].WriteFloat(craneData.WriteData.Truk_Pos_z, startIdx_Truk_Pos_z);
        // write to PLC
        GM.plc[iCrane].WriteToPLC();
    }

    public void WriteShiftPos(int iCrane, ContainerPosition cntrPos, int startIdxCntrPos, ContainerInfoSO cntrInfoSO)
    {
        // init start index
        int startIdx = startIdxCntrPos;

        // constant
        const int blockMapLength = 8;   // short(2) + short(2) + float(4) = 8

        // block
        GM.plc[iCrane].WriteShort(cntrPos.block, startIdx);
        startIdx += shortLength;

        // bay
        // x10 Converting
        short convBay = (short)(Util.ConvertIndexToBay(cntrPos.bay) * 10);
        GM.plc[iCrane].WriteShort(convBay, startIdx);
        startIdx += shortLength;

        // row
        // x10 Converting
        short convRow = (short)((cntrPos.row + 1) * 10);
        GM.plc[iCrane].WriteShort(convRow, startIdx);
        startIdx += shortLength;

        // tier
        GM.plc[iCrane].WriteShort(cntrPos.tier, startIdx);
        startIdx += shortLength;

        //// Block Map
        // Block Map start index
        startIdx += blockMapLength * cntrPos.row;

        // row num
        GM.plc[iCrane].WriteShort(convRow, startIdx);
        startIdx += shortLength;

        // tier num
        GM.plc[iCrane].WriteShort(cntrPos.tier, startIdx);
        startIdx += shortLength;

        // max height
        float maxHeight = cntrPos.tier * cntrInfoSO.height;
        GM.plc[iCrane].WriteFloat(maxHeight, startIdx);
    }

    public void WriteContainerInfo(int iCrane, ContainerInfoSO cntrInfoSO, string strContainerID, int startIdxCtProp)
    {
        // init start index
        int startIdx = startIdxCtProp;

        // size
        GM.plc[iCrane].WriteShort(cntrInfoSO.size, startIdx);
        startIdx += shortLength;

        // type
        GM.plc[iCrane].WriteShort(cntrInfoSO.type, startIdx);
        startIdx += shortLength;

        // position
        // -1: Undefined, 1: Front, 2: Center, 3: Rear
        GM.plc[iCrane].WriteShort(2, startIdx);
        startIdx += shortLength;
        startIdx += shortLength;    // number

        // container code
        int idxContainerID = GM.FindContainerIndex(strContainerID);
        byte[] byteContainerID = GM.stackProfile.listID[idxContainerID];
        for (int i = 0; i < byteContainerID.Length; i++)
            GM.plc[iCrane].WriteByte(byteContainerID[i], startIdx + (charLength * i));
    }
}
