using System;
using System.Runtime.InteropServices;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct CranePlcReadData
{
    [FieldOffset(0)]
    public float sG_Vel_Backward;
    [FieldOffset(4)]
    public float sG_Vel_Forward;
    [FieldOffset(8)]
    public float sT_Vel;
    [FieldOffset(12)]
    public float sH_Vel;
    [FieldOffset(16)]
    public float MM_1_Vel;
    [FieldOffset(20)]
    public float MM_2_Vel;
    [FieldOffset(24)]
    public float MM_3_Vel;
    [FieldOffset(28)]
    public float MM_4_Vel;
    [FieldOffset(32)]
    public byte _32_Raw; // Backing for bits
    // Bit 0: 20FT
    public bool _20FT => (_32_Raw & (1 << 0)) != 0;
    // Bit 1: 40FT
    public bool _40FT => (_32_Raw & (1 << 1)) != 0;
    // Bit 2: 45FT
    public bool _45FT => (_32_Raw & (1 << 2)) != 0;
    [FieldOffset(34)]
    public byte _34_Raw; // Backing for bits
    // Bit 0: TL_Lock
    public bool TL_Lock => (_34_Raw & (1 << 0)) != 0;
    // Bit 1: TL_Unlock
    public bool TL_Unlock => (_34_Raw & (1 << 1)) != 0;
    [FieldOffset(36)]
    public short Cam1;
    [FieldOffset(38)]
    public short Cam2;
    [FieldOffset(40)]
    public short Cam3;
    [FieldOffset(42)]
    public short Cam4;
}
