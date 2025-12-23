using UnityEngine;

public class ContainerSensor : MonoBehaviour
{
    private ContainerController _controller;

    [Header("Layer Settings")]
    [SerializeField]
    private LayerMask targetLayers; // Truck, Ground, Container 레이어를 인스펙터에서 체크하세요.

    [Header("Status")]
    public bool landed_sensor = false;
    public GameObject contactedUnderObject = null;

    public void InitSensor(ContainerController controller)
    {
        _controller = controller;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 비트 연산을 통한 레이어 검증 (Truck, Ground, Container만 통과)
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            landed_sensor = true;
            contactedUnderObject = other.gameObject;

            // 컨트롤러에 상태 동기화
            _controller?.SyncSensorState(contactedUnderObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 2. 나가는 오브젝트가 현재 감지 중인 오브젝트일 경우에만 초기화
        if (contactedUnderObject == other.gameObject)
        {
            landed_sensor = false;
            contactedUnderObject = null;

            _controller?.SyncSensorState(null);
        }
    }
}