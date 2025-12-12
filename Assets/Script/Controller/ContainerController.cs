using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : BaseController
{
    public ContainerInfoSO feet;
    public Color color;

    // GM.listContainerID 접근할 때
    // 미리 index를 추출해서 접근하는 게 아니라 ID로 접근한다.
    // 작업 생성 중 다른 작업이 완료되어 컨테이너가 삭제되었을 때 버그 발생할 수 있음.
    public string strContainerID;

    public void SetInfo(string strContainerID)
    {
        this.strContainerID = strContainerID;
    }

    
}
