using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// landed 상태일때 lock을하면 컨테이너 잠금
// unlocked 상태일때 unlock을하면 컨테이너 해제


public class SpreaderController : BaseController
{
    public Define.TWLockState CurrentTWLockState {get; private set;} = Define.TWLockState.Unlocked;

    private List<Landed> landedSensors;
    private FixedJoint currentJoint;
    private bool isOperating = false;
    public bool IsLanded
    {
        // Sensor들 전부 true 이면 true
        get
        {
            foreach (var sensor in landedSensors)
            {
                if (!sensor.landed_sensor)
                    return false;
            }
            return true;
        }
    }

    public ContainerController TargetContainer
    {
        get
        {
            // 센서를 돌면서 컨테이너가 있는 센서를 찾음
            foreach (var sensor in landedSensors)
            {
                if (sensor.containerController != null)
                {
                    return sensor.containerController;
                }
            }
            return null;
        }
    }

    public override bool Init()
    {
        if( !base.Init())
            return false;
        landedSensors = new List<Landed>( GetComponentsInChildren<Landed>() );
        Debug.Log($"SpreaderController Init : found {landedSensors.Count} landed sensors.");

        return true;
    }


    public void UpdateTwistLockLogic(bool isLockSignal, bool isUnlockSignal)
    {
        // Safety Guard 1: 이미 동작(잠금/해제) 처리를 수행 중이라면 신호 무시
        if (isOperating) return;

        // Safety Guard 2: 신호 유효성 검사 (동시 입력 무시, 입력 없음 무시)
        if (isLockSignal == isUnlockSignal) return;

        // Logic: 상태에 따른 분기 처리
        if (isLockSignal)
        {
            // 이미 잠겨있거나, 착지하지 않았다면 무시 (Fail-Safe)
            if (CurrentTWLockState == Define.TWLockState.Locked) return;
            if (!IsLanded)  return; 
                

            ExecuteLock();
        }
        else if (isUnlockSignal)
        {
            // 이미 풀려있다면 무시
            if (CurrentTWLockState == Define.TWLockState.Unlocked) return;

            ExecuteUnlock();
        }
    }


    private void ExecuteLock()
    {
        isOperating = true;
        if (TargetContainer != null && IsLanded)
        {
            

            CurrentTWLockState = Define.TWLockState.Locked;
            // TODO : TargetContainer와 스프레더를 연결하는 로직 추가  joint?
            // TODO : TargetContainer한테 소속정보 알려주기?
            ContainerController target = TargetContainer;
            Rigidbody targetRb = target.GetComponent<Rigidbody>();

            if (currentJoint != null) Destroy(currentJoint);

            currentJoint = gameObject.AddComponent<FixedJoint>();
            currentJoint.connectedBody = targetRb;

            currentJoint.breakForce = Mathf.Infinity; // 무한한 힘으로 버팀 (필요 시 수치 조절하여 사고 시뮬레이션 가능)
            currentJoint.breakTorque = Mathf.Infinity;
            currentJoint.enableCollision = false; // 결합된 상태에서는 서로 충돌 계산 무시



            Debug.Log($"TwistLock: Container locked {TargetContainer.name}.");
        }
        else
        {
            Debug.Log("TwistLock : No container to lock or container is not landed.");
        }
        isOperating = false;
    }

    private void ExecuteUnlock()
    {
        isOperating = true;
        if (CurrentTWLockState == Define.TWLockState.Locked)
        {
            CurrentTWLockState = Define.TWLockState.Unlocked;
            // TODO : TargetContainer와 스프레더의 연결 해제 로직 추가

            if (currentJoint != null)
            {
                Destroy(currentJoint);
                currentJoint = null;
            }
            Debug.Log("TwistLock: Container unlocked.");
        }
        else
        {
            Debug.Log("TwistLock: No container is currently locked.");
        }
        isOperating = false;
    }
}
