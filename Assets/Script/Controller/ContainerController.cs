using UnityEngine;

public class ContainerController : BaseController
{
    [Header("Data Settings")]
    public ContainerInfoSO feet;
    public Color color;
    public string strContainerID;
    public Define.ContainerHolderType currentHolder = Define.ContainerHolderType.None;

    private ContainerSensor _containerSensor;

    [Header("Debug Monitor")]
    public GameObject contactedUnderObject;

    public override bool Init()
    {
        if (!base.Init())
            return false;

        if (_containerSensor == null)
            _containerSensor = GetComponentInChildren<ContainerSensor>();

        if (_containerSensor != null)
            _containerSensor.InitSensor(this);

        return true;
    }

    public void SetInfo(string strContainerID)
    {
        this.strContainerID = strContainerID;
    }

    public void SyncSensorState(GameObject target)
    {
        contactedUnderObject = target;
    }
}