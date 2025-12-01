
// Organizing data
public class MainLoopARMG : MainLoop
{
    // Crane 별 고유 변수
    public override void InitCraneSpecVar()
    {
        GM.readDBNum = 2130;
        GM.readLength = 302;
        GM.writeDBNum = 2140;
        GM.writeStartIdx = 0;
        GM.writeLength = 178;
    }

    public override void WriteUnitydataToPLC(int iCrane)
    {

        // Row, Bay 는 *10 으로 Converting

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
