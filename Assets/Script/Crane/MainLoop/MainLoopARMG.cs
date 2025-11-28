
// Organizing data
public class MainLoopARMG : MainLoop
{

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
