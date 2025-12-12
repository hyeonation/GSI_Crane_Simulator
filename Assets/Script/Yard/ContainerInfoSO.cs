using UnityEngine;

[CreateAssetMenu(fileName = "Container Info",
                 menuName = "Scriptable Object/Container Info",
                 order = int.MaxValue)]
public class ContainerInfoSO : ScriptableObject
{
    //// Container ID string
    //// Type
    // 0: Height_0, 1: Height_8_6, 2: Height_9_6
    public short type = 2;

    //// Size
    // -1: Undefined, 1: Container_20ft, 2: Container_40ft, 3: Container_45ft, 19: Moving, 0: Empty
    public short size = 2;

    public float height = 2.85f;
}