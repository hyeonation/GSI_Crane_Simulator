using UnityEngine;

[CreateAssetMenu(fileName = "Container Info",
                 menuName = "Scriptable Object/Container Info",
                 order = int.MaxValue)]
public class ContainerInfoSO : ScriptableObject
{
    //// Container ID string
    // GM.listContainerID 접근할 때
    // 미리 index를 추출해서 접근하는 게 아니라 ID로 접근한다.
    // 작업 생성 중 다른 작업이 완료되어 컨테이너가 삭제되었을 때 버그 발생할 수 있음.
    public string strContainerID;

    //// Type
    // 0: Height_0, 1: Height_8_6, 2: Height_9_6
    public short type = 2;

    //// Size
    // -1: Undefined, 1: Container_20ft, 2: Container_40ft, 3: Container_45ft, 19: Moving, 0: Empty
    public short size = 2;

    public float height = 2.85f;
}